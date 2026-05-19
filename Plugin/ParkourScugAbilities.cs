using IL.MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ParkourScugPlugin.RevenantAbilities
{
    public class HunterProgram : MechanicalProgram
    {
        private int originalThrowingSkill;
        protected override Color ProgramColor => new Color(.8f, .1f, .1f);

        
        public override void InitializeHologram()
        {
            hologram = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Main, Owner.firstChunk.pos, 20f, 20f);
            hologram.owner = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Cube, hologram.pos, 20f, 20f);
        }
        protected override void ConnectToPlayer()
        {
            originalThrowingSkill = player.slugcatStats.throwingSkill;
            player.slugcatStats.throwingSkill = 3;
            player.spearOnBack = new Player.SpearOnBack(player);
        }
        protected override void DisconnectFromPlayer()
        {
            player.slugcatStats.throwingSkill = originalThrowingSkill;
            player.spearOnBack = null;
            originalThrowingSkill = -1;
        }
        protected override void Tick()
        {
            base.Tick();
        }
        protected override void PlayerTick()
        {
            if (UnityEngine.Random.value < 0.01f)
            {
                player.room.AddObject(new Explosion.ExplosionLight(player.firstChunk.pos, 120f, 1f, 30, new Color(135 / 256f, 107 / 256f, 31 / 256f)));
                player.room.PlaySound(SoundID.Death_Lightning_Spark_Object, player.firstChunk.pos, 0.2f, 1f);
            }
        }
        protected override void Throw(Creature.Grasp grasp)
        {
            if (PlayerInput.y == 1)
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
        }


        private float holoErrors = 0f;
        private int hologramCounter = 0;
        private float holoFade = 0f;
        private float[,] directionsPower = new float[12, 3];
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
                }
                return sprites;
            }
            return null;
        }
        public void ThrowObject(Creature.Grasp grasp)
        {
            Throw(grasp);
        }
        private void Initialize()
        {
            InitializeHologram();
        }
        private void UpdateAnimation()
        {
            // Hologram stuff (stolen from NSH)
            if (showHologram && hologram != null)
            {
                for (int j = 0; j < directionsPower.GetLength(0); j++)
                {
                    directionsPower[j, 1] = directionsPower[j, 0];
                    directionsPower[j, 0] = Custom.LerpAndTick(directionsPower[j, 0], directionsPower[j, 2], 0.03f, 1f / 15f);
                    directionsPower[j, 2] = 0f;
                }
                if (UnityEngine.Random.value < 0.05f)
                {
                    holoErrors = Mathf.Max(0f, holoErrors - UnityEngine.Random.value / 7f);
                }
                if (hologramCounter > 60 && Owner.grabbedBy?.Count == 0)
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
        public virtual void InitializeHologram() { }
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
            program.Disconnect();
            Custom.LogImportant("Pearl Downloaded " + MechanicalProgram.GetProgramName(program));
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
            Custom.LogImportant("Pearl Exported " + MechanicalProgram.GetProgramName(export));
            return export;
        }

        public void RefreshHologram()
        {
            program?.InitializeHologram();
        }


        public void Update()
        {

        }
    }
}
