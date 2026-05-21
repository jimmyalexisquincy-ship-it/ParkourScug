using IL.MoreSlugcats; // wawa
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ParkourScugPlugin.RevenantAbilities
{
    public class MirosKillerProgram : MechanicalProgram
    {
        protected override Color ProgramColor => new ColorByte(190, 0, 130);

        
        public override void InitializeHologram()
        {
            base.InitializeHologram();
            hologram = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Sphere, new Vector3(0f, 0f, 0f), 0f, 0f);
            hologram.subShapes.Add(new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Ribbon, new Vector3(0f, 0f, 0f), 20f, 20f));
            hologram.subShapes.Add(new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Sphere, new Vector3(0f, 0f, 0f), 20f, 20f));
        }
    }
    public class HunterProgram : MechanicalProgram
    {
        private int originalThrowingSkill;
        protected override Color ProgramColor => new ColorByte(250, 70, 70);

        
        public override void InitializeHologram()
        {
            base.InitializeHologram();
            hologram = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Sphere, new Vector3(0f, 0f, 0f), 0f, 0f);
            hologram.subShapes.Add(new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Shell, new Vector3(0f, 0f, 0f), 20f, 20f));
            //hologram.subShapes.Add(new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Ribbon, new Vector3(0f, 0f, 0f), 20f, 10f));
            hologram.subShapes.Add(new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Diamond, new Vector3(0f, 0f, 0f), 10f, 10f));
        }
        protected override void ConnectToPlayer()
        {
            originalThrowingSkill = player.slugcatStats.throwingSkill;
            player.slugcatStats.throwingSkill = 3;
            player.spearOnBack = new Player.SpearOnBack(player);
            playerData.betterSlide = false;
        }
        protected override void DisconnectFromPlayer()
        {
            player.slugcatStats.throwingSkill = originalThrowingSkill;
            player.spearOnBack = null;
            originalThrowingSkill = -1;
            playerData.betterSlide = true;
        }
        protected override void Tick()
        {
            base.Tick();
        }
        protected override void PlayerTick()
        {
            if (UnityEngine.Random.value < 0.01f)
            {
                Color color = ColorByte(135, 107, 31);
                player.room.AddObject(new Explosion.ExplosionLight(player.firstChunk.pos, 120f, 1f, 30, color));
                player.room.PlaySound(SoundID.Death_Lightning_Spark_Object, player.firstChunk.pos, 0.15f, Mathf.Lerp(1f, 1.4f, UnityEngine.Random.value));
            }
        }
        protected override void PlayerJump()
        {
            player.bodyChunks[0].vel.y *= 1.5f;
            player.bodyChunks[1].vel.y *= 1.5f;
            player.standing = true;
            player.room.AddObject(new ExplosionSpikes(player.room, player.bodyChunks[1].pos + new Vector2(0f, 0f - player.bodyChunks[1].rad), 2, 7f, 5f, 5.5f, 20f, new Color(1f, 1f, 1f, 0.5f)));
        }
        protected override void Throw(Creature.Grasp grasp)
        {
            if (PlayerInput.y == 1)
            {
                player.animation = Player.AnimationIndex.Flip;
            }
            else if (PlayerInput.y == -1)
            {
                player.animation = Player.AnimationIndex.Flip;
            }
        }


        public HunterProgram() : base() { }
        public HunterProgram(Player player) : base(player) { }
        public HunterProgram(Creature creature) : base(creature) { }
        public override string ToString() { return "ScavHunter"; }
    }


    public abstract class MechanicalProgram
    {
        protected static Color ColorByte(int r, int g, int b) => new Color(r / 256f, g / 256f, b / 256f);
        public static string GetProgramName(MechanicalProgram program)
        {
            if (program == null) return "Empty";
            return program.ToString() + ".Exe";
        }


        public bool inPlayer;
        public NSHSwarmer.Shape hologram;
        public bool showHologram = false;
        protected Player.InputPackage PlayerInput => player.input[0];
        protected bool abstracted = false;
        protected Player player;
        protected ParkourScugData playerData;
        protected Creature creature;
        protected DataPearl pearl;
        public PhysicalObject Owner
        {
            get
            {
                if (!abstracted) { return creature as PhysicalObject; }
                else if (pearl != null) { return pearl as PhysicalObject; }
                else
                {
                    Custom.LogWarning("No owner for program: " + ToString());
                    return null;
                }
            }
            set
            {
                if (value is Player)
                {
                    ConnectPlayer(value as Player);
                }
                else if (value is Creature)
                {
                    creature = value as Creature;
                    inPlayer = false;
                    abstracted = false;
                }
                else if (value is DataPearl)
                {
                    pearl = value as DataPearl;
                    inPlayer = false;
                    abstracted = true;
                }
                else
                {
                    Custom.LogWarning("Invalid owner for program: " + ToString());
                }
            }
        }


        public MechanicalProgram()
        {
            inPlayer = false;
            abstracted = true;
        }
        public MechanicalProgram(DataPearl pearl)
        {
            this.pearl = pearl;
            inPlayer = false;
            abstracted = true;
            Initialize();
        }
        public MechanicalProgram(Creature creature)
        {
            this.creature = creature;
            inPlayer = false;
            Initialize();
        }
        public MechanicalProgram(Player player)
        {
            ConnectPlayer(player);
            Initialize();
        }
        public void ConnectPlayer(Player player)
        {
            this.player = player;
            creature = player;
            inPlayer = true;
            playerData = ParkourScugPlugin.GetParkourScugData(player);
            abstracted = false;
            ConnectToPlayer();
        }
        public void Disconnect()
        {
            if (inPlayer)
            {
                DisconnectFromPlayer();
            }
            player = null;
            creature = null;
            inPlayer = false;
            abstracted = true;
            showHologram = false;
        }


        public void Update()
        {
            UpdateAnimation();
            if (abstracted || creature == null) { return; }
            Tick();
            if (inPlayer)
            {
                PlayerTick();
                if (player.input[0].spec)
                {
                    UseAbility();
                }
            }
            else
            {
                CreatureTick();
            }
        }
        public FSprite[] GetHologramSprites(RoomCamera rCam)
        {
            if (hologram != null)
            {
                FSprite[] sprites = new FSprite[hologram.LinesCount];
                for (int num = hologram.LinesCount - 1; num >= 0; num--)
                {
                    sprites[num] = new FSprite("pixel");
                    sprites[num].anchorY = 0f;
                    sprites[num].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
                    sprites[num].color = ProgramColor;
                }
                return sprites;
            }
            return null;
        }
        public void Jump()
        {
            if (inPlayer) PlayerJump();
        }
        public void ThrowObject(Creature.Grasp grasp)
        {
            Throw(grasp);
        }
        public void FadeHologram(float fade)
        {
            holoFade = fade;
        }
        public virtual void DrawHologram(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int sprite)
        {
            Vector2 vector = Vector2.Lerp(Owner.firstChunk.lastPos, Owner.firstChunk.pos, timeStacker);
            Vector2 vector3 = new Vector3(1f, 0f);
            float pointsWeight = 1f;
            Vector2 pointsVec = vector;
            float maxDist = 1f;
            float num3 = Custom.SCurve(Mathf.Lerp(lastHoloFade, holoFade, timeStacker), 0.65f);
            num3 *= hologram.Fade.SmoothValue(timeStacker);
            hologram.Draw(sLeaser, rCam, timeStacker, vector, camPos, ref sprite, Custom.VecToDeg(Vector3.Slerp(vector3, new Vector2(0f, 1f), 0.51f)), Mathf.Lerp(lastHoloErrors, holoErrors, timeStacker), num3, shakeErr: false, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
        }
        private void Initialize()
        {            
            InitializeHologram();
        }
        private float holoErrors = 0f;
        private int hologramCounter = 0;
        private float holoFade = 0f;
        private float[,] directionsPower = new float[12, 3];
        private float lastHoloErrors = 0f;
        private float lastHoloFade = 0f;
        private bool wantToShowHologram = true;
        private void UpdateAnimation()
        {
            // Hologram stuff (stolen from NSH)
            if (showHologram && hologram != null)
            {
                if (wantToShowHologram) hologramCounter++;
                else hologramCounter--;
                hologramCounter = Custom.IntClamp(hologramCounter, 0, 160);
                lastHoloFade = holoFade;
                lastHoloErrors = holoErrors; // Please remove holo errors
                for (int j = 0; j < directionsPower.GetLength(0); j++)
                {
                    directionsPower[j, 1] = directionsPower[j, 0];
                    directionsPower[j, 0] = Custom.LerpAndTick(directionsPower[j, 0], directionsPower[j, 2], 0.03f, 1f / 15f);
                    directionsPower[j, 2] = 0f;
                }
                if (hologramCounter > 60 && wantToShowHologram)
                {
                    holoFade = Custom.LerpAndTick(holoFade, Mathf.InverseLerp(60f, 160f, hologramCounter), 0.06f, 1f / 60f);
                }
                else
                {
                    holoFade = Mathf.Max(0f, holoFade - 1f / 12f);
                }
                hologram.Update(false || UnityEngine.Random.value < 0.05f, holoErrors, holoFade, Owner.firstChunk.pos - Owner.firstChunk.lastPos, 0f, ref directionsPower);
            }
        }
        private void HologramInit()
        {
            if (Owner == null) throw new Exception("No owner asigned to program: " + ToString());
            holoFade = 1f;
        }
        public virtual void InitializeHologram() { HologramInit(); }
        protected virtual void PlayerJump() { }
        protected virtual void ConnectToPlayer() { }
        protected virtual void DisconnectFromPlayer() { }
        protected virtual void Tick() { }
        protected virtual void PlayerTick() { }
        protected virtual void CreatureTick() { }
        protected virtual void UseAbility() { }
        protected virtual void Throw(Creature.Grasp grasp) { }

        protected virtual Color ProgramColor => Color.white;
    }


    public class ProgramedPearl
    {
        public DataPearl pearl;
        public MechanicalProgram program = null;
        public bool needsGraphicalUpdate = false;
        public bool HasProgram => program != null;


        public ProgramedPearl(DataPearl pearl)
        {
            this.pearl = pearl;
        }
        public void DownloadProgram(MechanicalProgram program)
        {
            if (program == null)
            {
                this.program = null;
                Custom.LogImportant("Pearl Did Not Downloaded");
                return;
            }
            this.program = program;
            ParkourScugPlugin.GetAbstractObjectData(pearl.abstractPhysicalObject).program = program;
            program.Disconnect();
            program.Owner = pearl;
            Custom.LogImportant("Pearl Downloaded " + MechanicalProgram.GetProgramName(program));
            RefreshHologram();
        }
        public MechanicalProgram ExportProgram()
        {
            if (!HasProgram)
            {
                Custom.LogImportant("Pearl Did Not Export ");
                return null;
            }
            MechanicalProgram export = this.program;
            this.program = null;
            ParkourScugPlugin.GetAbstractObjectData(pearl.abstractPhysicalObject).program = null;
            Custom.LogImportant("Pearl Exported " + MechanicalProgram.GetProgramName(export));
            RefreshHologram();
            return export;
        }

        public void RefreshHologram()
        {
            needsGraphicalUpdate = true;
            if (HasProgram)
            {
                program.InitializeHologram();
                program.showHologram = true;
            }
            else
            {

            }
        }
        public void FadeHologram(float fade)
        {
            if (HasProgram)
            {
                program.FadeHologram(fade);
            }
        }


        public void Update()
        {
            if (HasProgram)
            {
                program.Update();
            }
            else
            {
                if (ParkourScugPlugin.GetAbstractObjectData(pearl.abstractPhysicalObject).program != null)
                {
                    DownloadProgram(ParkourScugPlugin.GetAbstractObjectData(pearl.abstractPhysicalObject).program);
                }
            }
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!HasProgram) { return; }
            if (program.hologram != null && program.showHologram)
            {
                FSprite[] newSprites = program.GetHologramSprites(rCam);
                FSprite[] oldSprites = sLeaser.sprites;
                sLeaser.sprites = new FSprite[sLeaser.sprites.Length + program.hologram.LinesCount];
                for (int i = 0; i < oldSprites.Length; i++)
                {
                    sLeaser.sprites[i] = oldSprites[i];
                }
                for (int j = 0; j < program.hologram.LinesCount; j++)
                {
                    sLeaser.sprites[oldSprites.Length + j] = newSprites[j];
                }
                pearl.AddToContainer(sLeaser, rCam, null);
            }
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (needsGraphicalUpdate)
            {
                needsGraphicalUpdate = false;
                if (HasProgram)
                {
                    InitiateSprites(sLeaser, rCam);
                }
                else
                {
                    sLeaser.RemoveAllSpritesFromContainer();
                    pearl.InitiateSprites(sLeaser, rCam);
                }
            }
            if (!HasProgram) { return; }
            if (program.hologram != null && program.showHologram)
            {
                program.DrawHologram(sLeaser, rCam, timeStacker, camPos, sLeaser.sprites.Length - program.hologram.LinesCount);
            }
        }
    }
}
