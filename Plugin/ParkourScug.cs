#pragma warning disable IDE1006

using System; // wawawa
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

namespace ParkourScugPlugin
{
    public class ParkourScugData
    {
        public Player player;
        private Room room => player.room;
        private Player.InputPackage input => player.input[0];
        private PlayerGraphics GetPlayerGraphics() => (player.graphicsModule as PlayerGraphics);

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

        public int superRollPounce = 0;
        public float superRollPounceEnterVelocity = 0;

        public int bellySlideExitCounter = 0;

        public int ceilingHangCounter = 0;

        public bool betterPoleSlide = false;

        private bool oddTick = false;
        private Player.AnimationIndex previousAnimation = Player.AnimationIndex.None;
        private Vector2 previousVelocity = Vector2.zero;


        public ParkourScugData(Player player)
        {
            this.player = player;
            animationData = new ParkourScugAnimation(this);
        }
        public virtual void ParkourScugTick(On.Player.orig_Update orig, bool eu)
        {
            AnimationTick();
            MovementTick();
            orig(player, eu);

            ///Custom.LogImportant("Var Tracker: " + player.slideUpPole);
        }
        private void MovementTick()
        {
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
            if (!(input.x == 0) && player.IsTileSolid(0, input.x, 0) && player.IsTileSolid(1, input.x, 0) && wallLatchCounter < 200)
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
            if (player.animation == Player.AnimationIndex.ClimbOnBeam && previousAnimation != Player.AnimationIndex.ClimbOnBeam)
            {
                player.slideUpPole = (int)(-previousVelocity.y < Math.Abs(previousVelocity.x) ? Mathf.Sqrt(previousVelocity.magnitude) + 6 : 0) * 2; betterPoleSlide = true;
                player.firstChunk.vel.x = 0.0f;
            }

            // Better pole climb
            if (betterPoleSlide)
            {
                if (player.slideUpPole > 10)
                {
                    if (oddTick) player.slideUpPole += 1;
                    if (player.animation == Player.AnimationIndex.GetUpToBeamTip && input.y == 1 && slideUpPole > 30)
                    {
                        player.animation = Player.AnimationIndex.None;
                        player.firstChunk.vel.y += Mathf.Sqrt(player.slideUpPole - 10) + 5;
                    }
                }
                else
                {
                    player.slideUpPole = 0;
                    betterPoleSlide = false;
                }
            }

            /* ------------------------------ Belly Slide Mechanics ------------------------------ */

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


            /* ------------------------------ Final Variable Updates ------------------------------ */

            oddTick = !oddTick;
            previousAnimation = player.animation;
            previousVelocity = player.firstChunk.vel;
        }
        private void AnimationTick()
        {
            
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
