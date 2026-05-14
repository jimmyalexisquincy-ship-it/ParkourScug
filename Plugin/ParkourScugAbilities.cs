using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ParkourScugPlugin.RevenantAbilities
{
    public class HunterAbility : MechanicalAbility
    {
        private int originalThrowingSkill;


        protected override void ConnectToPlayer()
        {
            originalThrowingSkill = player.slugcatStats.throwingSkill;
            player.slugcatStats.throwingSkill = 3;
        }
        protected override void DisconnectFromPlayer()
        {
            player.slugcatStats.throwingSkill = originalThrowingSkill;
            originalThrowingSkill = -1;
        }


        public HunterAbility() : base() { }
        public HunterAbility(Player player) : base(player) { }
        public HunterAbility(Creature creature) : base(creature) { }
    }


    public abstract class MechanicalAbility
    {
        public bool inPlayer;
        protected bool abstracted;
        protected Player player;
        protected Creature creature;
        public Creature Owner
        {
            get
            {
                return creature;
            }
            set
            {
                if (value is Player)
                {
                    ConnectPlayer(value as Player);
                }
                else
                {
                    creature = value;
                    inPlayer = false;
                }
            }
        }


        public MechanicalAbility()
        {
            inPlayer = false;
            abstracted = true;
        }
        public MechanicalAbility(Creature creature)
        {
            this.creature = creature;
            inPlayer = false;
        }
        public MechanicalAbility(Player player)
        {
            ConnectPlayer(player);
        }
        public void ConnectPlayer(Player player)
        {
            this.player = player;
            creature = player;
            inPlayer = true;
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


        public void Update()
        {
            Tick();
            if (player.input[0].spec)
            {
                UseAbility();
            }
        }
        protected virtual void ConnectToPlayer() { }
        protected virtual void DisconnectFromPlayer() { }
        protected virtual void Tick() { }
        protected virtual void UseAbility() { }
    }
}
