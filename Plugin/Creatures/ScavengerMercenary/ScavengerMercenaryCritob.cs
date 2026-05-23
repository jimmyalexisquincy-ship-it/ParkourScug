using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace ParkourScugPlugin.Creatures
{
    // Refrenced From https://github.com/Dual-Iron/fisobs/blob/master/examples/mosquitoes/MosquitoCritob.cs
    public class ScavengerMercenaryCritob : Critob
    {
        public static readonly CreatureTemplate.Type ScavengerMercenary = new CreatureTemplate.Type("ScavengerMercenary", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID ScavengerMercenaryUnlock = new MultiplayerUnlocks.SandboxUnlockID("ScavengerMercenary", true);

        public ScavengerMercenaryCritob() : base(ScavengerMercenary)
        {
            LoadedPerformanceCost = 20f;
            SandboxPerformanceCost = new SandboxPerformanceCost(linear: 0.6f, exponential: 0.1f);
            ShelterDanger = ShelterDanger.Safe;
            CreatureName = "Scavenger Mercenary";

            RegisterUnlock(killScore: KillScore.Configurable(2), ScavengerMercenaryUnlock, data: 0);
        }
        public override CreatureTemplate CreateTemplate()
        {
            // My fisobs friend... do the thing (i've never done this before)
            CreatureTemplate t = new CreatureFormula(this)
            {
                // Add stuff here
            }.IntoTemplate();

            // Add stuff here too

            return t;
        }
        // Do this too. (i ran out of time)
        public override void EstablishRelationships()
        {
            throw new System.NotImplementedException();
        }
        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            throw new System.NotImplementedException();
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            throw new System.NotImplementedException();
        }
    }
}
