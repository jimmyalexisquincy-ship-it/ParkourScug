using System.Collections.Generic;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;

namespace ParkourScugPlugin.Creatures;

sealed class SmallCamoCentipedeCritob : Critob
{
    internal SmallCamoCentipedeCritob() : base(CreatureTemplateType.SmallCamoCentipede)
    {
        base.Icon = new SimpleIcon("Kill_Centipede1", new Color(1f, 1f, 1f));

        base.SandboxPerformanceCost = new SandboxPerformanceCost(1f, 0.7f);
        base.RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.SmallCamoCentipede, null, 0);
        base.ShelterDanger = ShelterDanger.Safe;
        CamoHooks.Apply();
    }
    

    public override int ExpeditionScore()
    {
        return 2;
    }


    public override Color DevtoolsMapColor(AbstractCreature acrit)
    {
        return new Color(1f, 1f, 1f);
    }

    public override string DevtoolsMapName(AbstractCreature acrit)
    {
        return "SCC";
    }

    public override IEnumerable<string> WorldFileAliases()
    {
        return new string[]
        {
                "SmallCamoCentipede",
                "Small Camo Centipede"
        };
    }



    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new RoomAttractivenessPanel.Category[]
        {
                RoomAttractivenessPanel.Category.LikesInside,
                RoomAttractivenessPanel.Category.Dark
        };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate creatureTemplate = new CreatureFormula(CreatureTemplate.Type.SmallCentipede, base.Type, "SmallCamoCentipede")
        {
            
            DefaultRelationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new AttackResist
            {
                Base = 1f,
            },

            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.SmallCentipede),
        }.IntoTemplate();
        
        return creatureTemplate;
    }

    public override void EstablishRelationships()
    {
        var relationships = new Relationships(base.Type);

        relationships.Ignores(base.Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
    {
        return new CentipedeAI(acrit, acrit.world);
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit)
    {
        return new CamoCentipede(acrit, acrit.world);
    }

    public override void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat)
    {
        canEatMeat = true;
    }

    public override CreatureState CreateState(AbstractCreature acrit)
    {
        return new CamoCentipede.CentipedeState(acrit);
    }

    public override void LoadResources(RainWorld rainWorld){ }


    public override CreatureTemplate.Type ArenaFallback()
    {
        return CreatureTemplate.Type.SmallCentipede;
    }
}
