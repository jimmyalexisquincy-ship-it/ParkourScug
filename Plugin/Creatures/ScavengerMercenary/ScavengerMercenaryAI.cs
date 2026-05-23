using RWCustom;
using System.Linq;
using UnityEngine;
using static CreatureTemplate.Relationship.Type;

namespace ParkourScugPlugin.Creatures
{
    public class ScavengerMercenaryAI : ScavengerAI
    {
        public ScavengerMercenary scavengerMerc;


        public ScavengerMercenaryAI(AbstractCreature acrit, ScavengerMercenary crit) : base(acrit, acrit.world)
        {
            scavengerMerc = crit;
            crit.AI = this;
        }
    }

}