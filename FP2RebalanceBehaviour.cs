using UnityEngine;
using HarmonyLib;
using static MelonLoader.MelonLogger;

namespace FP2Rebalance.MichaelGallinago
{
    public class FP2RebalanceBehaviour : MonoBehaviour
    {
        /*
        private FPPlayer FP2RChar = null;

        public void Update()
        {
            if (FPStage.currentStage != null)
            {
                if (FP2RChar == null)
                {
                    FP2RChar = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }
            }
        }
        */
    }

    /*
    [HarmonyPatch(typeof(FPPlayer), "Start")]
    public class Patch_Start
    {
        static void Prefix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            fpplayer.hasSpecialItem = true;
        }
    }
    */

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt2")]
    public class Patch_State_Lilac_DragonBoostPt2
    {
        static void Postfix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpplayer.state == new FPObjectState(fpplayer.State_Lilac_DragonBoostPt2) && fpplayer.currentAnimation == "Wings_Loop")
            {
                fpplayer.health = FPCommon.RoundToQuantumWithinErrorThreshold(fpplayer.health - FPStage.deltaTime / 70f, 0.5f);
            }
        }
    }

    [HarmonyPatch(typeof(ItemFuel), "CollisionCheck")]
    public class Patch_ItemFuel
    {
        static void Postfix()
        {
            FPBaseObject fpbaseObject = null;
            while (FPStage.ForEach(FPPlayer.classID, ref fpbaseObject))
            {
                FPPlayer fpplayer = (FPPlayer)fpbaseObject;
                if (fpplayer.characterID == FPCharacterID.LILAC && !fpplayer.hasSpecialItem && fpplayer.powerupTimer >= 600f)
                {
                    fpplayer.Action_Jump();
                    fpplayer.hasSpecialItem = true;
                    fpplayer.powerupTimer = 0f;
                    fpplayer.flashTime = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class Patch_State_Carol_JumpDiscWarp
    {
        private static Vector2 savedVelocity;

        static void Prefix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            savedVelocity = fpplayer.velocity;
        }

        static void Postfix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpplayer.genericTimer <= 0f && fpplayer.guardTime <= 0f && fpplayer.input.guardHold)
            {
                fpplayer.velocity = savedVelocity * 1.2f;
                fpplayer.hitStun = 0f;

                if (Mathf.Abs(fpplayer.velocity.x) > 12f)
                {
                    fpplayer.SetPlayerAnimation("GuardAirFast", 0f, 0f, false, true);
                }
                else
                {
                    fpplayer.SetPlayerAnimation("GuardAir", 0f, 0f, false, true);
                }
                fpplayer.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpplayer.velocity.x * 0.05f)));
                fpplayer.childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpplayer.velocity.x * 0.05f)));
                fpplayer.Action_Guard(0f);
                fpplayer.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, fpplayer.position.x, fpplayer.position.y);
                guardFlash.parentObject = fpplayer;
                fpplayer.Action_StopSound();
                FPAudio.PlaySfx(15);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces")]
    public class Patch_ApplyGroundForces
    {
        static void Postfix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpplayer.state == new FPObjectState(fpplayer.State_Carol_Roll))
            {
                fpplayer.groundVel *= 1f - Mathf.Sin(fpplayer.groundAngle * 0.017453292f) / 32f * ((fpplayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
            }
            else if (fpplayer.characterID == FPCharacterID.BIKECAROL && fpplayer.currentAnimation == "Crouching")
            {
                var acceleration = 1f - Mathf.Sin(fpplayer.groundAngle * 0.017453292f) / 32f * ((fpplayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
                fpplayer.groundVel *= acceleration * (acceleration > 1f ? 1.004f: 0.998f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_CrushKO")]
    public class Patch_State_CrushKO
    {
        static void Prefix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpplayer.characterID == FPCharacterID.LILAC)
            {
                fpplayer.hasSpecialItem = false;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_KO")]
    public class Patch_State_KO
    {
        static void Prefix()
        {
            var fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
            if (fpplayer.characterID == FPCharacterID.LILAC)
            {
                fpplayer.hasSpecialItem = false;
            }
        }
    }

    /*
    [HarmonyPatch(typeof(ClassName), "MethodName")]
    public class Patch_MethodName
    {
        static bool Prefix()
        {
        }
        static void Postfix()
        {
        }
    }
    */
}