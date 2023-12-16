using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class General
    {
        public static void Patch()
        {
            if (!(Plugin.NeeraRebalance.Value || Plugin.MillaRebalance.Value 
                || Plugin.CarolRebalance.Value || Plugin.LilacRebalance.Value)) return;

            Harmony harmony = Plugin.HarmonyLink;

            harmony.PatchAll(typeof(Start));

            harmony.PatchAll(typeof(State_Collected));
            harmony.PatchAll(typeof(State_CrushKO));
            harmony.PatchAll(typeof(State_KO));

            harmony.PatchAll(typeof(StartHud));
            harmony.PatchAll(typeof(LateUpdate));
        }

        // General
        [HarmonyPatch(typeof(FPPlayer), "Start")]
        public class Start
        {
            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / 
                    (fpPlayer.characterID == FPCharacterID.NEERA ? 4f : 1f);
            }
        }

        [HarmonyPatch(typeof(ItemFuel), "State_Collected")]
        public class State_Collected
        {
            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.hasSpecialItem) return;

                fpPlayer.Action_Jump();
                fpPlayer.hasSpecialItem = true;
                    
                if (fpPlayer.powerupTimer >= 590f)
                {
                    fpPlayer.powerupTimer = 0f;
                    fpPlayer.flashTime = 0f;
                }

                fpPlayer.Action_PlayVoiceArray("ItemGet");
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_CrushKO")]
        public class State_CrushKO
        {
            static void Prefix()
            {
                Patcher.GetPlayer.hasSpecialItem = false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_KO")]
        public class State_KO
        {
            static void Prefix()
            {
                Patcher.GetPlayer.hasSpecialItem = false;
            }
        }

        [HarmonyPatch(typeof(FPHudMaster), "Start")]
        public class StartHud
        {
            static void Postfix(ref FPHudMaster __instance)
            {
                Sprite millaSprite = Plugin.Sprites[Plugin.SpriteId.MillaSpecialItem];
                Sprite neeraSprite = Plugin.Sprites[Plugin.SpriteId.NeeraSpecialItem];
                __instance.hudBike[0].digitFrames = __instance.hudBike[0].digitFrames.AddToArray(millaSprite).AddToArray(neeraSprite);
            }
        }

        [HarmonyPatch(typeof(FPHudMaster), "LateUpdate")]
        public class LateUpdate
        {
            static void Postfix(ref FPHudMaster __instance)
            {
                switch (__instance.targetPlayer.characterID)
                {
                    case FPCharacterID.MILLA when __instance.targetPlayer.hasSpecialItem:
                        __instance.hudBike[0].GetRenderer().enabled = true;
                        __instance.hudBike[0].SetDigitValue(3);
                        break;
                    case FPCharacterID.NEERA when __instance.targetPlayer.hasSpecialItem:
                        __instance.hudBike[0].GetRenderer().enabled = true;
                        __instance.hudBike[0].SetDigitValue(4);
                        break;
                }
            }
        }
    }
}
