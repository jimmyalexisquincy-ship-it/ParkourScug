using RWCustom;
using System.Linq;
using UnityEngine;

using namespace ParkourScugPlugin.Creatures;

// Refrenced From https://github.com/Dual-Iron/fisobs/blob/master/examples/mosquitoes/Mosquito.cs
public class ScavengerMercenary : Creature
{
    public readonly ScavengerMercenaryAI AI = null;

    
    public ScavengerMercenary(AbstractCreature acrit) : base(acrit, acrit.world)
    {
        // Add bodyChucks
            
        // Add Scavenger Stats
    }
        
    public override Color ShortCutColor()
    {
        return new Color(.4f, .4f, .4f);
    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule = new ScavengerMercenaryGraphics(this);
        graphicsModule.Reset();
    }


    public override void Update(bool eu)
    {
        base.Update(eu);
    }
}
