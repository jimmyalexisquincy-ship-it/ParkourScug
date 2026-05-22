#pragma warning disable IDE1006

using System; // wawawawawawawawa
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ParkourScugPlugin.ParkourScugPlugin;
using RWCustom;
using IL.Menu.Remix;
using IL.MoreSlugcats;
using ParkourScugPlugin.ParkourScug;
using JetBrains.Annotations;
using ParkourScugPlugin.RevenantAbilities;
using System.Reflection;
using IL.Menu;
using BepInEx;

namespace ParkourScugPlugin
{
    public class ParkourScugData
    {
        public Player player;
        public MechanicalProgram program;
        private Room room => player.room;
        private bool HasProgram => program != null;
        private bool playerInitialized = false;
        private Player.InputPackage input => player.input[0];
        private PlayerGraphics GetPlayerGraphics() => (player.graphicsModule as PlayerGraphics);

        public Player.Grasp FirstGrasp
        {
            get
            {
                if (player.grasps[0] == null) return player.grasps[1];
                else return player.grasps[0];
            }
            set
            {
                if (player.grasps[0] == null) player.grasps[1] = value;
                else player.grasps[0] = value;
            }
        }

        public ParkourScugAnimation animationData;
        public ParkourScugAnimationIndex playerAnimation
        {
            get
            {
                if (customPlayerAnimationState == Player.AnimationIndex.None) return player.animation as ParkourScugAnimationIndex;
                else return customPlayerAnimationState;
            }
            set
            {
                customPlayerAnimationState = Player.AnimationIndex.None as ParkourScugAnimationIndex;
                if (value is ParkourScugAnimationIndex)
                {
                    if (value == ParkourScugAnimationIndex.HangOnCeiling)
                    {
                        customPlayerAnimationState = ParkourScugAnimationIndex.HangOnCeiling;
                        player.animation = Player.AnimationIndex.HangFromBeam;
                    }
                    else if (value == ParkourScugAnimationIndex.WallLatch)
                    {
                        customPlayerAnimationState = ParkourScugAnimationIndex.WallLatch;
                        player.animation = Player.AnimationIndex.None;
                    }
                    else
                    {
                        player.animation = value;
                    }
                }
                else
                {
                    player.animation = value;
                }
            }
        }
        private ParkourScugAnimationIndex customPlayerAnimationState;

        public int wallLatchCounter = 0;
        public int wallLatchExitCounter = 0;
        public int uploadCounter = 0;

        public int superRollPounce = 0;
        public float superRollPounceEnterVelocity = 0;

        public int bellySlideExitCounter = 0;

        public int ceilingHangCounter = 0;

        public bool betterPoleSlide = false;

        private bool oddTick = false;
        private Player.AnimationIndex previousAnimation = Player.AnimationIndex.None;
        private Vector2 previousVelocity = Vector2.zero;

        public bool betterSlide = true;
        public bool canMaul = false;


        public ParkourScugData(Player player)
        {
            this.player = player;
            animationData = new ParkourScugAnimation(this);
        }
        public virtual void ParkourScugTick(On.Player.orig_Update orig, bool eu)
        {
            if (!playerInitialized)
            {
                playerInitialized = true;
                InitializePlayer();
            }
            AnimationTick();
            AbilityTick();
            MovementTick();
            orig(player, eu);

            ///Custom.LogImportant("Var Tracker: " + player.slideUpPole);
        }
        private void InitializePlayer()
        {
            float rand = UnityEngine.Random.value;
            if (rand > .5f)
            {
                program = new MirosKillerProgram(player);
            }
            else
            {
                program = new HunterProgram(player);
            }
        }
        private void AbilityTick()
        {
            if (!canMaul) { player.maulTimer = 0; }
            if (FirstGrasp?.grabbed is DataPearl && input.y == 1 && input.spec)
            {
                DataPearl pearl = FirstGrasp.grabbed as DataPearl;
                ProgramedPearl pearlData = GetPearlData(pearl);
                if (uploadCounter == 0)
                {
                    room.PlaySound(SoundID.Centipede_Electric_Charge_LOOP, player.firstChunk);
                }
                if (uploadCounter >= 100)
                {
                    uploadCounter = 0;
                    room.PlaySound(MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Karma_Pitch_Discovery, player.firstChunk);
                    player.SaintStagger(120);
                    room.PlaySound(SoundID.Centipede_Shock, player.firstChunk);
                    for (int i = 0; i < 6; i++)
                    {
                        room.AddObject(new Spark(player.firstChunk.pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
                    }

                    MechanicalProgram pearlProgram = pearlData.ExportProgram();
                    pearlData.DownloadProgram(program);
                    if (pearlProgram == null)
                    {
                        program = null;
                        Custom.LogImportant("Player Did Not Download!");
                    }
                    else
                    {
                        program = pearlProgram;
                        program.ConnectPlayer(player);
                        Custom.LogImportant("Player Downloaded " + MechanicalProgram.GetProgramName(program));
                    }
                }
                else
                {
                    uploadCounter++;
                    pearlData.FadeHologram(1f - (uploadCounter / 100f));
                }
            }
            else
            {
                uploadCounter = 0;
            }


            if (!HasProgram) return;
            program.Update();
        }
        private void MovementTick()
        {
            bool flag;
            /* ------------------------------ Smaller Modifiers ------------------------------ */

            player.initSlideCounter++;
            if (player.animation == Player.AnimationIndex.Roll && player.rollCounter > 10)
            {
                player.rollCounter = 25;
            }


            /* ------------------------------ New Mechanics --------------------------- */

            // Ceiling hang
            if (player.IsTileSolid(0, 0, 1) && !player.IsTileSolid(1, 0, -1) && player.animation == Player.AnimationIndex.None && input.y > 0)
            {
                playerAnimation = ParkourScugAnimationIndex.HangOnCeiling;
                ceilingHangCounter++;
                player.firstChunk.vel.y += ceilingHangCounter * 0.01f * Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value);
                if (ceilingHangCounter > 200 && UnityEngine.Random.value < (ceilingHangCounter - 200) * 0.01f)
                {
                    player.stun = 5;
                }
            }
            else if (playerAnimation == ParkourScugAnimationIndex.HangOnCeiling)
            {
                playerAnimation = Player.AnimationIndex.None as ParkourScugAnimationIndex;
                ceilingHangCounter = 0;
            }

            // Wall Latch
            flag = player.animation == Player.AnimationIndex.None && player.standing && !(input.x == 0);
            flag &= player.IsTileSolid(0, input.x, 0) && player.IsTileSolid(1, input.x, 0);
            flag &= !(player.IsTileSolid(0, -input.x, 0) || player.IsTileSolid(1, -input.x, 0));
            if (flag && wallLatchCounter < 200)
            {
                playerAnimation = ParkourScugAnimationIndex.WallLatch;
                wallLatchCounter++;
                wallLatchExitCounter = 5;
                if (wallLatchCounter == 1)
                {
                    player.firstChunk.vel.y += 5f;
                    room.PlaySound(SoundID.Shelter_Little_Hatch_Close, player.firstChunk, false, 2f, 1.3f);
                }
                else if (wallLatchCounter < 100)
                {
                    player.firstChunk.vel.y = player.firstChunk.vel.y / 2f;
                    player.firstChunk.vel.y += 1f;
                }
                else if (wallLatchCounter < 150)
                {
                    player.firstChunk.vel.y = 0f;
                }
                else
                {
                    wallLatchCounter = 300;
                }
            }
            else
            {
                if (playerAnimation == ParkourScugAnimationIndex.WallLatch)
                {
                    playerAnimation = Player.AnimationIndex.None as ParkourScugAnimationIndex;
                }
                if (wallLatchExitCounter > 0)
                {
                    wallLatchExitCounter--;
                    if (input.jmp && !player.input[1].jmp && wallLatchCounter < 100)
                    {
                        previousVelocity = new Vector2(0f, 10f);
                        player.animation = Player.AnimationIndex.Roll;
                        if (input.y == -1) player.rollCounter = 25;
                        else player.rollCounter = 0;
                        wallLatchExitCounter = 0;
                        wallLatchCounter += 200;
                    }
                }
                if (wallLatchCounter > 0)
                {
                    if (player.IsTileSolid(1, 0, -1)) wallLatchCounter = 0;
                    else if (player.animation != Player.AnimationIndex.None && player.animation != Player.AnimationIndex.Roll) wallLatchCounter = 0;
                    else wallLatchCounter--;
                }
            }


            /* ------------------------------ Momentum Mechanics --------------------------- */

            // Instant roll pounce
            if (player.animation == Player.AnimationIndex.Roll && previousAnimation != Player.AnimationIndex.Roll)
            {
                superRollPounceEnterVelocity = previousVelocity.magnitude;
            }
            if (player.animation == Player.AnimationIndex.Roll && player.input[0].jmp && (!player.input[1].jmp))
            {
                superRollPounceEnterVelocity /= player.rollCounter + 3.0f;
                if (player.rollCounter < 7)
                {
                    BoostEffect();
                    BoostDir(10.0f, 20.0f);
                    player.animation = Player.AnimationIndex.None;
                    player.standing = true;
                    superRollPounce = 5;
                }
            }
            if (superRollPounce > 0)
            {
                superRollPounce--;
                player.firstChunk.vel.y += superRollPounceEnterVelocity;
            }

            // Slide into beam momentum
            if (player.animation == Player.AnimationIndex.ClimbOnBeam && previousAnimation != Player.AnimationIndex.ClimbOnBeam && (player.canJump == 0 || !player.standing))
            {
                player.bodyChunks[0].vel.x = 0.0f;
                player.bodyChunks[0].vel.y += 10.0f;
                player.bodyChunks[1].vel.x = 0.0f;
                if ( -previousVelocity.y < Math.Abs(previousVelocity.x) )
                {
                    player.slideUpPole = (int) (Mathf.Abs(previousVelocity.x) + Mathf.Sqrt(previousVelocity.y));
                    betterPoleSlide = true;
                }
            }

            // Better pole climb
            if (betterPoleSlide)
            {
                if (player.animation == Player.AnimationIndex.GetUpToBeamTip)
                {
                    player.animation = Player.AnimationIndex.None;
                    player.firstChunk.vel.y += Mathf.Sqrt(player.slideUpPole - 10) + 5;
                    BoostEffect();
                }
                else if (player.slideUpPole > 10)
                {
                    if (oddTick) player.slideUpPole += 1;
                }
                else
                {
                    player.slideUpPole = 0;
                    betterPoleSlide = false;
                }
            }

            /* ------------------------------ Belly Slide Mechanics ------------------------------ */

            if (betterSlide)
            {
                if (player.animation == Player.AnimationIndex.Roll && previousAnimation != Player.AnimationIndex.Roll && input.jmp && bellySlideExitCounter > 10)
                {
                    player.animation = Player.AnimationIndex.BellySlide;
                    player.rollCounter = 0;
                    player.allowRoll = -1;
                }

                if (player.animation == Player.AnimationIndex.BellySlide)
                {
                    bellySlideExitCounter = 0;

                    int rollCounter = player.rollCounter; /// For some reason, rollCounter is used for the belly slide. the slideCounter var is actually the turn around.
                    Vector2 feetPos = player.bodyChunks[1].pos;

                    if (rollCounter >= 0 && rollCounter < 3)
                    {
                        BoostDir((input.jmp ? 24.0f : 12.0f), -5.5f);

                        if (rollCounter == 1)
                        {
                            BoostEffect();
                        }
                    }
                    if (rollCounter >= 3 && rollCounter < 20)
                    {
                        player.rollCounter = 14;
                        BoostDir(4.0f, 0.0f);

                        room.AddObject(new Spark(
                            feetPos,
                            Custom.DegToVec((-input.x) * Mathf.Lerp(30f, 70f, UnityEngine.Random.value)) * Mathf.Lerp(6f, 8f, UnityEngine.Random.value),
                            new Color(1f, 0.8f, 0.5f),
                            null, 6, 11
                            ));
                        room.PlaySound(SoundID.Cyan_Lizard_Prepare_Small_Jump, feetPos, 1.0f, 3.0f);
                    }

                    if (input.jmp)
                    {
                        player.longBellySlide = true;
                    }
                    else
                    {
                        player.longBellySlide = false;
                    }
                }
                else if (bellySlideExitCounter < 20)
                {
                    bellySlideExitCounter++;

                    // Larger Coyote Frames
                    if (bellySlideExitCounter < 10)
                    {
                        player.canJump += 1;
                    }
                }
            }


            /* ------------------------------ Final Variable Updates ------------------------------ */

            oddTick = !oddTick;
            previousAnimation = player.animation;
            previousVelocity = player.firstChunk.vel;
        }
        private void AnimationTick()
        {

        }
        public void Jump(On.Player.orig_Jump orig)
        {
            orig(player);
            if (HasProgram)
            {
                program.Jump();
            }
        }
        public void ThrowObject(On.Player.orig_ThrowObject orig, int grasp, bool eu)
        {
            if (HasProgram)
            {
                program.ThrowObject(player.grasps[grasp]);
            }
            orig(player, grasp, eu);
        }

        private void Boost(Vector2 vel) { player.firstChunk.vel += vel; }
        private void Boost(float x, float y) { Boost(new Vector2(x, y)); }
        private void BoostDir(float x, float y) { Boost(x * input.x, y); }

        public void BoostEffect()
        {
            Vector2 feetPos = player.bodyChunks[1].pos;
            Color color = new Color(1f, 0.8f, 0.5f);
            for (int i = 0; i < 9; i++) room.AddObject(new Spark(feetPos, 3.0f * Custom.DegToVec((-input.x) * Mathf.Lerp(30f, 70f, UnityEngine.Random.value)) * Mathf.Lerp(6f, 11f, UnityEngine.Random.value), color, null, 6, 11));
            room.AddObject(new Explosion.ExplosionLight(feetPos, 100.0f, 0.5f, 6, color));
            room.PlaySound(MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, feetPos, 2.0f, 1.3f);
        }
    }
}
