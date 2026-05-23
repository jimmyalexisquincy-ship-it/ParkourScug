using RWCustom;
using System.Linq;
using UnityEngine;
using static CreatureTemplate.Relationship.Type;

namespace ParkourScugPlugin.Creatures

public class ScavengerMercenaryAI : ScavengerAI
{
    public ScavengerMercenary scavenger;
    
    
    public MosquitoAI(AbstractCreature acrit, ScavengerMercenary crit) : base(acrit, acrit.world)
    {
        scavenger = crit;
        crit.AI = this;
    }
}
