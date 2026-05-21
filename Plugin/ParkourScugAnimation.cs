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


        public ParkourScugAnimation(ParkourScugData playerData)
        {
            this.playerData = playerData;
            player = playerData.player;
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