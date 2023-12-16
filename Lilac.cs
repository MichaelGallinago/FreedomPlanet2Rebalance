using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class Lilac
    {
        public static void Patch()
        {
            if (!Plugin.LilacRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;
            
            harmony.PatchAll(typeof(State_Lilac_DragonBoostPt1));
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt1")]
    public class State_Lilac_DragonBoostPt1
    {
        private static AudioClip _savedVoice;

        static void Prefix()
        {
            FPPlayer fpPlayer = Patcher.GetPlayer;
            if (_savedVoice == null) _savedVoice = fpPlayer.vaExtra[0];
            if (fpPlayer.genericTimer < 30f)
            {
                fpPlayer.vaExtra[0] = Random.Range(1, 100) <= Plugin.YellPercent.Value ? _savedVoice : null;
            }
        }
    }
}
