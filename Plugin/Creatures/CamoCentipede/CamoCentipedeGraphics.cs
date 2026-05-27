using RWCustom;
using System;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace ParkourScugPlugin.Creatures;


    public class CamoCentipedeGraphics : CentipedeGraphics
    {
        public float Camouflaged
        {
            get
            {
                if (this.whiteCamoColorAmount == -1f)
                {
                    return 1f;
                }
                return this.whiteCamoColorAmount;
            }
        }
        public CamoCentipedeGraphics(PhysicalObject ow)
        : base(ow)
        {
            cullRange = 200f + ((Centipede)ow).size * 200f;
            centipede = (Centipede)ow;
            Random.State state = Random.state;
            Random.InitState(centipede.abstractCreature.ID.RandomSeed);
            totSegs = base.owner.bodyChunks.Length;
            totalSecondarySegments = base.owner.bodyChunks.Length - 1;
            defaultRotat = Mathf.Lerp(-5f, 5f, Random.value);
            bodyRotations = new Vector2[3, 2];
            for (int i = 0; i < bodyRotations.GetLength(0); i++)
            {
                bodyRotations[i, 0] = Custom.DegToVec(defaultRotat);
                bodyRotations[i, 1] = Custom.DegToVec(defaultRotat);
            }
            if (ModManager.DLCShared && this.centipede.Small && this.centipede.abstractCreature.superSizeMe)
            {
                this.hue = Mathf.Lerp(1f, 1f, Random.value);
                this.saturation = 1f;
            }

                hue = Mathf.Lerp(1f, 1f, Random.value);
                saturation = 0.8f;
            

            legs = new Limb[totSegs, 2];
            legLengths = new float[totSegs];
            for (int l = 0; l < totSegs; l++)
            {
                float num7 = (float)l / (float)(totSegs - 1);
                legLengths[l] = Mathf.Lerp(10f, 25f, Mathf.Sin(num7 * (float)Math.PI));
                legLengths[l] *= Mathf.Lerp(0.5f, 1.5f, centipede.size) * (centipede.Centiwing ? 0.65f : 1f);
                for (int m = 0; m < 2; m++)
                {
                    legs[l, m] = new Limb(this, ow.bodyChunks[l], l * 2 + m, 2f, 0.5f, 0.9f, 7f, 0.8f);
                }
            }

            lastDarkness = -1f;
            whiskers = new GenericBodyPart[2, 2, 2];
            bodyParts = new BodyPart[totSegs * 2 + 8];
            int num8 = 0;
            for (int n = 0; n < totSegs; n++)
            {
                for (int num9 = 0; num9 < 2; num9++)
                {
                    bodyParts[num8] = legs[n, num9];
                    num8++;
                }
            }

            for (int num10 = 0; num10 < 2; num10++)
            {
                for (int num11 = 0; num11 < 2; num11++)
                {
                    for (int num12 = 0; num12 < 2; num12++)
                    {
                        whiskers[num10, num11, num12] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, (num10 == 0) ? base.owner.bodyChunks[0] : base.owner.bodyChunks[base.owner.bodyChunks.Length - 1]);
                        bodyParts[num8] = whiskers[num10, num11, num12];
                        num8++;
                    }
                }
            }

            soundLoop = new ChunkDynamicSoundLoop(centipede.mainBodyChunk);
            Random.state = state;
        }
        public float WhiskeresLength(int part)
        {
            
            if (!this.centipede.Red)
            {
                return ((part == 0) ? 17f : 43f) * Mathf.Lerp(0.5f, 1.5f, this.centipede.size) * (this.centipede.Small ? 0.375f : .5f);
            }
            if (part != 0)
            {
                return 24f;
            }
            return 22f;
        }
        public override void Update()
        {
            base.Update(); 
            for (int l = 0; l < 2; l++)
            {
                Vector2 pos2;
                if (l == 0)
                {
                    pos2 = base.owner.bodyChunks[0].pos;
                }
                else
                {
                    pos2 = base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].pos;
                }
                for (int m = 0; m < 2; m++)
                {
                    for (int n = 0; n < 2; n++)
                    {
                        this.whiskers[l, n, m].Update();
                        this.whiskers[l, n, m].ConnectToPoint(pos2, this.WhiskeresLength(n), false, 0f, new Vector2(0f, 0f), 0f, 0f);
                        this.whiskers[l, n, m].vel += (pos2 + this.WhiskerDir(l, m, n, 1f) * this.WhiskeresLength(n) - this.whiskers[l, n, m].pos) / 30f;
                        this.whiskers[l, n, m].vel += this.WhiskerDir(l, m, n, 1f);
                        GenericBodyPart genericBodyPart = this.whiskers[l, n, m];
                        genericBodyPart.vel.y = genericBodyPart.vel.y - 0.3f;
                        if (this.centipede.Consious && !this.centipede.moving)
                        {
                            this.whiskers[l, n, m].pos += Custom.RNV() * Mathf.Lerp(0.5f, 1.5f, this.centipede.size) * ((this.centipede.bodyDirection == (l == 0)) ? 2f : 0.8f);
                        }
                    }
                }
            }

            if (centipede.dead)
            {
                whiteCamoColorAmount = Mathf.Lerp(whiteCamoColorAmount, 0.3f, 0.01f);
            }
            else
            {
                if (((HealthState)centipede.State).health < 0.6f && Random.value * 1.5f < ((HealthState)centipede.State).health && Random.value < 1f / (centipede.Stunned ? 10f : 40f))
                {
                    whiteGlitchFit = (int)Mathf.Lerp(5f, 40f, (1f - ((HealthState)centipede.State).health) * Random.value);
                }

                if (whiteGlitchFit == 0 && centipede.Stunned && Random.value < 0.05f)
                {
                    whiteGlitchFit = 2;
                }

                if (whiteGlitchFit > 0)
                {
                    whiteGlitchFit--;
                    float f = 1f - ((HealthState)centipede.State).health;
                    if (Random.value < 0.2f)
                    {
                        whiteCamoColorAmountDrag = 1f;
                    }

                    if (Random.value < 0.2f)
                    {
                        whiteCamoColorAmount = 1f;
                    }

                    if (Random.value < 0.5f)
                    {
                        whiteCamoColor = Color.Lerp(whiteCamoColor, new Color(Random.value, Random.value, Random.value), Mathf.Pow(f, 0.2f) * Mathf.Pow(Random.value, 0.1f));
                    }

                    if (Random.value < 1f / 3f)
                    {
                        whitePickUpColor = new Color(Random.value, Random.value, Random.value);
                    }
                }
                else if (showDominance > 0f)
                {
                    whiteDominanceHue += Random.value * Mathf.Pow(showDominance, 2f) * 0.2f;
                    if (whiteDominanceHue > 1f)
                    {
                        whiteDominanceHue -= 1f;
                    }

                    whiteCamoColor = Color.Lerp(whiteCamoColor, Custom.HSL2RGB(whiteDominanceHue, 1f, 0.5f), Mathf.InverseLerp(0.5f, 1f, Mathf.Pow(showDominance, 0.5f)) * Random.value);
                    whiteCamoColorAmount = Mathf.Lerp(whiteCamoColorAmount, 1f - Mathf.Sin(Mathf.InverseLerp(0f, 1.1f, Mathf.Pow(showDominance, 0.5f)) * (float)Math.PI), 0.1f);
                }
                else
                {
                    if (Random.value < 0.1f)
                    {
                        CamoAmountControlled();
                    }

                    whiteCamoColorAmount = Mathf.Clamp(Mathf.Lerp(whiteCamoColorAmount, whiteCamoColorAmountDrag, 0.1f * Random.value), 0.15f, 1f);
                    whiteCamoColor = Color.Lerp(whiteCamoColor, whitePickUpColor, 0.1f);
                }
            }
            
        }
        public void CamoAmountControlled()
        {
            if (!centipede.safariControlled)
            {
                whiteCamoColorAmountDrag = Mathf.Lerp(whiteCamoColorAmountDrag, Mathf.InverseLerp(0.65f, 0.4f, centipede.AI.excitement), Random.value);
            }
            else if (!centipede.inputWithDiagonals.HasValue || !centipede.inputWithDiagonals.Value.thrw)
            {
                whiteCamoColorAmountDrag = 0f;
            }
            else if (centipede.mainBodyChunk.vel.magnitude > 0f)
            {
                whiteCamoColorAmountDrag = Mathf.InverseLerp(0.65f, 0.4f, centipede.mainBodyChunk.vel.magnitude / 5f);
            }
            else
            {
                whiteCamoColorAmountDrag = 1f;
            }
        }
        public Color DynamicBodyColor(float f)
        {
            
                return Color.Lerp(blackColor, whiteCamoColor, whiteCamoColorAmount);
            
        }
        public Color DynamichueColor(float f)
        {

            return Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.3f), whiteCamoColor, whiteCamoColorAmount);

        }
        public void ColorBody(RoomCamera.SpriteLeaser sLeaser, Color col)
        {
            
            for (int i = 0; i < base.owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[SegmentSprite(i)].color = col;
                sLeaser.sprites[TubeSprite].color = col;
                for (int l = 0; l < 2; l++)
                {
                    sLeaser.sprites[LegSprite(i, l, 0)].color = col;
                    sLeaser.sprites[LegSprite(i, l, 1)].color = col;
                    ((VertexColorSprite)sLeaser.sprites[LegSprite(i, l, 1)]).verticeColors[2] = col;
                    ((VertexColorSprite)sLeaser.sprites[LegSprite(i, l, 1)] ).verticeColors[3] = col;
                }
                for (int m = 0; m < 2; m++)
                {
                    for (int n = 0; n < 2; n++)
                    {
                        for (int num11 = 0; num11 < 2; num11++)
                        {
                            
                            for (int num14 = 0; num14 < 4; num14++)
                            {
                                
                                sLeaser.sprites[WhiskerSprite(m, n, num11)].color = col;




                            }
                        }
                    }
                }

            }
        }
        public void Colorhue(RoomCamera.SpriteLeaser sLeaser, Color col)
        {
            for (int j = 0; j < totalSecondarySegments; j++)
            {
            

                sLeaser.sprites[SecondarySegmentSprite(j)].color = col;
            }
            for (int k = 0; k < base.owner.bodyChunks.Length; k++)
            {
                for (int l = 0; l < 2; l++)
                {

                    ((VertexColorSprite)sLeaser.sprites[LegSprite(k, l, 1)]).verticeColors[0] = col;
                    ((VertexColorSprite)sLeaser.sprites[LegSprite(k, l, 1)]).verticeColors[1] = col;
                }
            }
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos); 
            ColorBody(sLeaser, DynamicBodyColor(0f)); 
            Colorhue(sLeaser, DynamichueColor(0f));
            Color color = rCam.PixelColorAtCoordinate(centipede.mainBodyChunk.pos);
            Color color2 = rCam.PixelColorAtCoordinate(centipede.bodyChunks[centipede.HeadIndex].pos);
            Color color3 = rCam.PixelColorAtCoordinate(centipede.bodyChunks[centipede.mainBodyChunkIndex].pos);
            if (color == color2)
            {
                whitePickUpColor = color;
            }
            else if (color2 == color3)
            {
                whitePickUpColor = color2;
            }
            else if (color3 == color)
            {
                whitePickUpColor = color3;
            }
            else
            {
                whitePickUpColor = (color + color2 + color3) / 3f;
            }
            if (whiteCamoColorAmount == -1f)
            {
                whiteCamoColor = whitePickUpColor;
                whiteCamoColorAmount = 1f;
            }

        }

        public Color whiteCamoColor = new Color(0f, 0f, 0f);
        public float whiteCamoColorAmount = -1f;
        public float whiteCamoColorAmountDrag = 1f;
        public Color whitePickUpColor;
        public float showDominance;
        public float whiteDominanceHue;
        public int whiteGlitchFit;

}
