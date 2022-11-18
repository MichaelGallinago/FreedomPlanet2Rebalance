using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class General
    {
        public static void Patch()
        {
            if (!(Plugin.NeeraRebalance.Value || Plugin.MillaRebalance.Value || Plugin.CarolRebalance.Value || Plugin.LilacRebalance.Value)) return;

            Plugin.HarmonyLink.PatchAll(typeof(Start));
        }

        // General
        [HarmonyPatch(typeof(FPPlayer), "Start")]
        public class Start
        {
            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;

                if (!Plugin.WingAsPowerUp.Value && fpPlayer.characterID == FPCharacterID.LILAC)
                    fpPlayer.hasSpecialItem = true;

                fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / ((fpPlayer.characterID == FPCharacterID.NEERA) ? 4f : 1f);
            }
        }
    }
}
