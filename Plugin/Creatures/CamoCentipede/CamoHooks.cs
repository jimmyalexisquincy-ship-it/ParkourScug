using System;
using UnityEngine;

namespace ParkourScugPlugin.Creatures
{
    internal static class CamoHooks
    {
        internal static void Apply()
        {
            On.Player.SlugcatGrab += OnPlayerSlugcatGrab; 
            On.Player.Grabability += OnPlayerGrabability;
            On.Player.IsCreatureLegalToHoldWithoutStun += OnPlayerIsCreatureLegalToHoldWithoutStun;
        }

        public static Player.ObjectGrabability OnPlayerGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is Weapon)
            {
                return orig(self, obj);
            }
            else
            {
                
                if (obj is Fly)
                {

                    return orig(self, obj);
                }
                else
                {
                    
                    if (obj is Hazer)
                    {
                        return orig(self, obj);
                    }
                    else
                    {
                        
                        if (obj is TubeWorm)
                        {
                            return orig(self, obj);
                        }
                        else if (obj is JokeRifle)
                        {
                            return orig(self, obj);
                        }
                        else
                        {
                            if (obj is CamoCentipede && ((CamoCentipede)obj).Template.type == CreatureTemplateType.SmallCamoCentipede)
                            {
                                return Player.ObjectGrabability.OneHand;
                            }
                        }

                    }
                }
            }

            return orig(self, obj);
        }
        public static void OnPlayerSlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            orig(self, obj, graspUsed);
            Debug.Log("Hello?");
            if (obj is CamoCentipede && ((CamoCentipede)obj).Template.type == CreatureTemplateType.SmallCamoCentipede)
            {
                self.Grab(obj, graspUsed, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, false, true);
                Debug.Log("This works?");
            }
            
        }
        public static bool OnPlayerIsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, global::Player self, global::Creature grabCheck)
        {
            return (grabCheck is Creature && (grabCheck as Creature).Template.type == CreatureTemplateType.SmallCamoCentipede)||orig(self, grabCheck);
        }
        internal static bool Small(Func<Centipede, bool> orig, Centipede self) => self.abstractCreature.creatureTemplate.type == CreatureTemplateType.SmallCamoCentipede || orig(self);
        internal static bool Edible(Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.SmallCamoCentipede || orig(self);
        internal static bool AutomaticPickUp(Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.SmallCamoCentipede || orig(self);

    }
}
