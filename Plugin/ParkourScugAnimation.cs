using ParkourScugPlugin.RevenantAbilities; //wa
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ParkourScugPlugin.ParkourScug
{
    public class ParkourScugAnimationIndex : Player.AnimationIndex
    {
        public static readonly ParkourScugAnimationIndex HangOnCeiling = new ParkourScugAnimationIndex("HangOnCeiling", register: true);
        public static readonly ParkourScugAnimationIndex WallLatch = new ParkourScugAnimationIndex("WallLatch", register: true);
        public ParkourScugAnimationIndex(string value, bool register = false)
            : base(value, register)
        {
        }
    }
    public class ParkourScugAnimation
    {
        public ParkourScugData playerData;
        public Player player;
        public PlayerGraphics graphicsModule;
        private readonly List<FSprite> spriteAdder;
        // Rework the entirity of AddSprites
        public void AddSprites(FSprite[] sprites) { foreach (FSprite sprite in sprites) spriteAdder.Add(sprite); }
        public void AddSprites(List<FSprite> sprites) { foreach (FSprite sprite in sprites) spriteAdder.Add(sprite); }


        public ParkourScugAnimation(ParkourScugData playerData)
        {
            this.playerData = playerData;
            player = playerData.player;
            graphicsModule = player.graphicsModule as PlayerGraphics;
            spriteAdder = new List<FSprite>();
        }


        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (playerData.program != null && playerData.program.showHologram)
            {
                playerData.program.InitializeHologram();
            }
            if (spriteAdder.Count == 0) return;
            FSprite[] oldSprites = sLeaser.sprites;
            sLeaser.sprites = new FSprite[oldSprites.Length + spriteAdder.Count];
            for (int i = 0; i < oldSprites.Length; i++)
            {
                sLeaser.sprites[i] = oldSprites[i];
            }
            for (int i = 0; i < spriteAdder.Count; i++)
            {
                sLeaser.sprites[i + oldSprites.Length] = spriteAdder[i];
            }
            spriteAdder.Clear();
            graphicsModule.AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (spriteAdder.Count > 0)
            {
                ParkourScugPlugin.logger.LogInfo("Initiating additional player sprites");
                try
                {
                    sLeaser.RemoveAllSpritesFromContainer();
                    graphicsModule.InitiateSprites(sLeaser, rCam);
                    ParkourScugPlugin.logger.LogMessage("Finished initiating additional player sprites");
                }
                catch (Exception e)
                {
                    ParkourScugPlugin.logger.LogError("Error initiating added player sprites: " + e);
                }
            }
            else if (playerData.program != null && playerData.program.showHologram)
            {
                MechanicalProgram program = playerData.program;
                program.DrawHologram(sLeaser, rCam, timeStacker, camPos, sLeaser.sprites.Length - program.hologram.LinesCount);
            }
        }


        public bool OnHandEngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand hand)
        {
            if (playerData.playerAnimation == ParkourScugAnimationIndex.HangOnCeiling)
            {
                int ceilingHangCount = playerData.ceilingHangCounter;

                RWCustom.Custom.LogImportant("wawa");
                hand.pos.y = player.room.MiddleOfTile(player.bodyChunks[0].pos).y + 8f;
                hand.absoluteHuntPos = new Vector2(player.room.MiddleOfTile(player.bodyChunks[0].pos).x, player.room.MiddleOfTile(player.bodyChunks[0].pos).y);
                hand.absoluteHuntPos.y -= 1f;
                hand.absoluteHuntPos.x += ((hand.limbNumber == 0) ? (-1f) : 1f) * (10f + 3f * Mathf.Sin((float)Math.PI * 2f * (float)player.animationFrame / 20f));
                hand.retract = false;
                hand.mode = SlugcatHand.Mode.HuntAbsolutePosition;
                if (UnityEngine.Random.value < ceilingHangCount * 0.001f) player.Blink(5);
                return false;
            }
            else
            {
                return orig(hand);
            }
        }
    }
}