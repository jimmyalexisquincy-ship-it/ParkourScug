using BepInEx; //wawawawawawa
using BepInEx.Logging;
using IL.MoreSlugcats;
using On.Watcher;
using RWCustom;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using ParkourScugPlugin.RevenantAbilities;

namespace ParkourScugPlugin
{
    [BepInPlugin("com.flyingfishbone.ParkourScug", "Parkour Scug", "0.1")]
    [BepInDependency("slime-cubed.slugbase")]
    public class ParkourScugPlugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public static SlugBase.SlugBaseCharacter RevenantSlugBaseCharacter => SlugBase.PlayerManager.GetCustomPlayer("revenant");

        public static ParkourScugData GetParkourScugData(Player player) => PlayerExtensionData.GetValue(player, k => new ParkourScugData(player));
        private static readonly ConditionalWeakTable<Player, ParkourScugData> PlayerExtensionData = new ConditionalWeakTable<Player, ParkourScugData>();
        public static ProgramedPearl GetPearlData(DataPearl pearl) => PearlExtensionData.GetValue(pearl, k => new ProgramedPearl(pearl));
        private static readonly ConditionalWeakTable<DataPearl, ProgramedPearl> PearlExtensionData = new ConditionalWeakTable<DataPearl, ProgramedPearl>();
        public static AbstractObjectExtensionData GetAbstractObjectData(AbstractPhysicalObject obj) => AbstractPhysicalObjectExtensionData.GetValue(obj, k => new AbstractObjectExtensionData(obj));
        private static readonly ConditionalWeakTable<AbstractPhysicalObject, AbstractObjectExtensionData> AbstractPhysicalObjectExtensionData = new ConditionalWeakTable<AbstractPhysicalObject, AbstractObjectExtensionData>();


        private bool IsParkourScug(Player player) { return player.SlugCatClass.ToString().Equals("revenant"); }

        public void OnEnable()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource("   ParkourScugPlugin");

            On.Player.Update += PlayerUpdateTick;
            On.SlugcatHand.EngageInMovement += SlugcatHandEngageInMovement;
            On.Player.ThrowObject += PlayerThrow;
            On.DataPearl.Update += DataPearl_Update;
            On.DataPearl.InitiateSprites += DataPearl_InitiateSprites;
            On.DataPearl.DrawSprites += DataPearl_DrawSprites;

            logger.LogInfo("Parkour Scug plugin loaded!");
        }

        private void PlayerUpdateTick(On.Player.orig_Update orig, Player player, bool eu)
        {
            if (!IsParkourScug(player))
            {
                orig(player, eu);
                return;
            }

            GetParkourScugData(player).ParkourScugTick(orig, eu);
        }

        private bool SlugcatHandEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand hand)
        {
            Player player = hand.connection.owner as Player;
            if (!IsParkourScug(player))
            {
                return orig(hand);
            }

            return GetParkourScugData(player).animationData.OnHandEngageInMovement(orig, hand);
        }
        private void PlayerThrow(On.Player.orig_ThrowObject orig, Player player, int grasp, bool eu)
        {
            if (!IsParkourScug(player))
            {
                orig(player, grasp, eu);
                return;
            }

            GetParkourScugData(player).ThrowObject(orig, grasp, eu);
        }
        private void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl pearl, bool eu)
        {
            GetPearlData(pearl).Update();
            orig(pearl, eu);
        }
        private void DataPearl_InitiateSprites(On.DataPearl.orig_InitiateSprites orig, DataPearl pearl, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(pearl, sLeaser, rCam);
            GetPearlData(pearl).InitiateSprites(sLeaser, rCam);
        }
        private void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl pearl, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(pearl, sLeaser, rCam, timeStacker, camPos);
            GetPearlData(pearl).DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}
