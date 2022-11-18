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

            if (Plugin.ControllableWings.Value)
            {
                harmony.PatchAll(typeof(State_Lilac_DragonBoostPt2));
            }

            harmony.PatchAll(typeof(State_Lilac_DragonBoostPt1));

            if (Plugin.WingAsPowerUp.Value)
            {
                harmony.PatchAll(typeof(CollisionCheck));
                harmony.PatchAll(typeof(State_CrushKO));
                harmony.PatchAll(typeof(State_KO));
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt2")]
    public class State_Lilac_DragonBoostPt2
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.up)
            {
                fpPlayer.specialAttackDirection = 0;
            }
            else if (fpPlayer.input.down)
            {
                fpPlayer.specialAttackDirection = 1;
            }
            else if (fpPlayer.input.left)
            {
                fpPlayer.specialAttackDirection = 2;
            }
            else if (fpPlayer.input.right)
            {
                fpPlayer.specialAttackDirection = 2;
            }

            switch (fpPlayer.specialAttackDirection)
            {
                case 0:
                    if (fpPlayer.direction == FPDirection.FACING_LEFT)
                    {
                        fpPlayer.velocity.x = Mathf.Min(Mathf.Min(fpPlayer.recoveryTimer, 0f) * 0.3f - 12f, fpPlayer.recoveryTimer);
                    }
                    else
                    {
                        fpPlayer.velocity.x = Mathf.Max(Mathf.Max(fpPlayer.recoveryTimer, 0f) * 0.3f + 12f, fpPlayer.recoveryTimer);
                    }
                    fpPlayer.velocity.y = 12f;
                    fpPlayer.onGround = false;
                    break;
                case 1:
                    if (fpPlayer.direction == FPDirection.FACING_LEFT)
                    {
                        fpPlayer.velocity.x = Mathf.Min(Mathf.Min(fpPlayer.recoveryTimer, 0f) * 0.3f - 12f, fpPlayer.recoveryTimer);
                    }
                    else
                    {
                        fpPlayer.velocity.x = Mathf.Max(Mathf.Max(fpPlayer.recoveryTimer, 0f) * 0.3f + 12f, fpPlayer.recoveryTimer);
                    }
                    fpPlayer.velocity.y = -12f;
                    fpPlayer.onGround = false;
                    break;
                case 2:
                    if (fpPlayer.onGround)
                    {
                        if (fpPlayer.direction == FPDirection.FACING_LEFT)
                        {
                            fpPlayer.groundVel = Mathf.Min(Mathf.Min(fpPlayer.groundVel, 0f) * 0.5f - 15f, fpPlayer.groundVel);
                        }
                        else
                        {
                            fpPlayer.groundVel = Mathf.Max(Mathf.Max(fpPlayer.groundVel, 0f) * 0.5f + 15f, fpPlayer.groundVel);
                        }
                    }
                    else if (fpPlayer.direction == FPDirection.FACING_LEFT)
                    {
                        fpPlayer.velocity.x = Mathf.Min(Mathf.Min(fpPlayer.velocity.x, 0f) * 0.5f - 15f, fpPlayer.velocity.x);
                        fpPlayer.velocity.y = 0f;
                    }
                    else
                    {
                        fpPlayer.velocity.x = Mathf.Max(Mathf.Max(fpPlayer.velocity.x, 0f) * 0.5f + 15f, fpPlayer.velocity.x);
                        fpPlayer.velocity.y = 0f;
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt1")]
    public class State_Lilac_DragonBoostPt1
    {
        private static AudioClip SavedVoice = null;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (SavedVoice == null) SavedVoice = fpPlayer.vaExtra[0];
            if (fpPlayer.genericTimer < 30f)
            {
                fpPlayer.vaExtra[0] = UnityEngine.Random.Range(1, 100) <= Plugin.YellPercent.Value ? SavedVoice : null;
            }
        }
    }

    [HarmonyPatch(typeof(ItemFuel), "CollisionCheck")]
    public class CollisionCheck
    {
        static void Postfix()
        {
            FPBaseObject fpbaseObject = null;
            while (FPStage.ForEach(FPPlayer.classID, ref fpbaseObject))
            {
                FPPlayer fpPlayer = (FPPlayer)fpbaseObject;
                if (fpPlayer.characterID == FPCharacterID.LILAC && !fpPlayer.hasSpecialItem && fpPlayer.powerupTimer >= 600f)
                {
                    fpPlayer.Action_Jump();
                    fpPlayer.hasSpecialItem = true;
                    fpPlayer.powerupTimer = 0f;
                    fpPlayer.flashTime = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_CrushKO")]
    public class State_CrushKO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
                fpPlayer.hasSpecialItem = false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_KO")]
    public class State_KO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
                fpPlayer.hasSpecialItem = false;
        }
    }
}
