using MoreSlugcats;
using System;
using System.Globalization;
using UnityEngine;

namespace ParkourScugPlugin.Creatures;

    
    public class CamoCentipede : Centipede
    {
        public bool Small;
        //It is not seen by other creatures when invisible.
        public override float VisibilityBonus
        {
            get
            {
                float visibilityBonus = base.VisibilityBonus;
                CamoCentipedeGraphics centipedeGraphics = base.graphicsModule as CamoCentipedeGraphics;
                if (centipedeGraphics == null)
                {
                    return visibilityBonus;
                }
                if (visibilityBonus == 0f)
                {
                    return -centipedeGraphics.Camouflaged;
                }
                float num = Vector3.Distance(RWCustom.Custom.RGB2Vec3(centipedeGraphics.whiteCamoColor), RWCustom.Custom.RGB2Vec3(this.mudColor));
                num = Mathf.Pow(Mathf.Clamp01(num), 0.25f);
                return Mathf.Lerp(-centipedeGraphics.Camouflaged, visibilityBonus, num);
            }
        }
        public static float GeneratorSize(AbstractCreature abstrCrit)
        {
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(abstrCrit.ID.RandomSeed);
            float result = Mathf.Lerp(0f, 1f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
            if (abstrCrit.creatureTemplate.type == CreatureTemplateType.SmallCamoCentipede)
            {
                result = 0f;
            }

            UnityEngine.Random.state = state;
            if (abstrCrit.creatureTemplate.type == CreatureTemplateType.CamoCentipede && abstrCrit.spawnData != null && abstrCrit.spawnData.Length > 2)
            {
                string s = abstrCrit.spawnData.Substring(1, abstrCrit.spawnData.Length - 2);
                try
                {
                    result = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }

            return result;
        }
        public CamoCentipede(AbstractCreature abstractCreature, World world)
        : base(abstractCreature, world)
        {
            Small = abstractCreature.creatureTemplate.type == CreatureTemplateType.SmallCamoCentipede;
            size = GeneratorSize(abstractCreature); 
            if (Small)
            {
                bites = 5;
            }
            if (!CentiState.meatInitated)
            {
                
                    abstractCreature.state.meatLeft = Mathf.RoundToInt(Mathf.Lerp(2.3f, 7f, size));
                

                CentiState.meatInitated = true;
            }

            base.bodyChunks = new BodyChunk[Red ? 18 : (Small ? 5 : ((int)Mathf.Lerp(7f, 17f, size)))];
            for (int i = 0; i < base.bodyChunks.Length; i++)
            {
                float num = (float)i / (float)(base.bodyChunks.Length - 1);
                float num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, size), Mathf.Lerp(4f, 6.5f, size), Mathf.Pow(Mathf.Clamp(Mathf.Sin((float)Math.PI * num), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, size)));
                

                if (Small)
                {
                    num2 = Mathf.Lerp(1.5f, 3f, Mathf.Pow(Mathf.Clamp(Mathf.Sin((float)Math.PI * num), 0f, 1f), 0.5f));
                }

                base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), num2, Mathf.Lerp(3f / 70f, 11f / 34f, Mathf.Pow(size, 1.4f)));
                base.bodyChunks[i].loudness = 0f;
            }


            base.mainBodyChunkIndex = base.bodyChunks.Length / 2; 
            if (!Small && (CentiState.shells == null || CentiState.shells.Length != base.bodyChunks.Length))
            {
                CentiState.shells = new bool[base.bodyChunks.Length];
                for (int k = 0; k < CentiState.shells.Length; k++)
                {
                    CentiState.shells[k] = UnityEngine.Random.value < 0.985f;
                }
            }


            bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * (base.bodyChunks.Length - 1) / 2];
            int num3 = 0;
            for (int l = 0; l < base.bodyChunks.Length; l++)
            {
                for (int m = l + 1; m < base.bodyChunks.Length; m++)
                {
                    float num4 = 0f;
                    if (AquaCenti)
                    {
                        num4 = 0.7f;
                    }

                    bodyChunkConnections[num3] = new BodyChunkConnection(base.bodyChunks[l], base.bodyChunks[m], base.bodyChunks[l].rad + base.bodyChunks[m].rad, BodyChunkConnection.Type.Push, 1f - num4, -1f);
                    num3++;
                }
            }
            for (int k = 0; k < CentiState.shells.Length; k++)
            {
                CentiState.shells[k] = false;
            }
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            bounce = 0.1f;
            surfaceFriction = 0.4f;
            collisionLayer = 1;
            base.waterFriction = 0.96f;
            base.buoyancy = (1.05f);
            collisionRange = 150f;
        }
        public override void InitiateGraphicsModule()
        {
            if (base.graphicsModule == null)
            {
                base.graphicsModule = new CamoCentipedeGraphics(this);
            }
        }
        public override void Update(bool eu)
        {
            this.visionDirection = !this.visionDirection;
            if (this.grabbedBy.Count > 0 && !base.dead && this.Small)
            {
                this.shockCharge += 0.016666668f;
                for (int i = 0; i < base.bodyChunks.Length; i++)
                {
                    base.bodyChunks[i].vel += RWCustom.Custom.RNV() * (UnityEngine.Random.value * 2f);
                }
                if (this.shockCharge >= 1f)
                {
                    Creature grabber = this.grabbedBy[0].grabber;
                    bool flag = ModManager.MSC && this.grabbedBy[0].grabber is Player && (this.grabbedBy[0].grabber as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint;
                    this.Shock(this.grabbedBy[0].grabber);
                    if (flag)
                    {
                        (grabber as Player).SaintStagger(680);
                    }
                    this.Stun(14);
                    for (int j = 0; j < base.bodyChunks.Length; j++)
                    {
                        base.bodyChunks[j].vel += RWCustom.Custom.RNV() * (UnityEngine.Random.value * 7f);
                    }
                }
            }
            base.Update(eu);
        }

    }
