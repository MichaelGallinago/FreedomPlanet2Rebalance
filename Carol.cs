using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class Carol
    {
        public static void Patch()
        {
            if (!Plugin.CarolRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;

            if (Plugin.DiscCansel.Value)
            {
                harmony.PatchAll(typeof(State_Carol_JumpDiscWarp));
            }

            if (Plugin.InertialBike.Value || Plugin.InertialRolling.Value)
            {
                harmony.PatchAll(typeof(ApplyGroundForces));
            }

            if (Plugin.HoldAttacks.Value)
            {
                harmony.PatchAll(typeof(Action_Carol_AirMoves));
                harmony.PatchAll(typeof(Action_Carol_GroundMoves));
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class State_Carol_JumpDiscWarp
    {
        static bool Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            var virtualGenericTime = fpPlayer.genericTimer;
            if (virtualGenericTime > 0f)
            {
                if (fpPlayer.carolJumpDisc != null && fpPlayer.hitStun <= 0f)
                {
                    Vector2 rhs = fpPlayer.carolJumpDisc.position - fpPlayer.position;
                    if (Vector2.Dot(fpPlayer.velocity, rhs) < -0.001f)
                    {
                        virtualGenericTime = 0f;
                    }
                    else
                    {
                        float magnitude = rhs.magnitude;
                        float num = fpPlayer.velocity.magnitude;
                        float num2 = magnitude / virtualGenericTime * 25f;
                        float num3 = 1f - (fpPlayer.carolJumpDisc.maxDistanceFromParent - num2) / fpPlayer.carolJumpDisc.maxDistanceFromParent;
                        float num4 = Mathf.Clamp(num2 / (25f * num3), 4f, 16f);

                        if (num4 > num)
                        {
                            num = num4;
                        }

                        if (num * FPStage.deltaTime >= magnitude)
                        {
                            virtualGenericTime = 0f;
                        }
                        else
                        {
                            virtualGenericTime -= FPStage.deltaTime;
                        }
                    }
                }
                else
                {
                    virtualGenericTime = 0f;
                }
            }
            if (virtualGenericTime <= 0f && fpPlayer.guardTime <= 0f && fpPlayer.input.guardHold)
            {
                fpPlayer.state = new FPObjectState(fpPlayer.State_InAir);
                fpPlayer.genericTimer = 0;
                fpPlayer.velocity *= 1.2f;
                fpPlayer.SetPlayerAnimation("GuardAir", 0f, 0f, false, true);
                fpPlayer.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                if (fpPlayer.childAnimator != null)
                {
                    fpPlayer.childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                }
                FPAudio.PlaySfx(15);
                fpPlayer.Action_Guard(0f);
                fpPlayer.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, fpPlayer.position.x, fpPlayer.position.y);
                guardFlash.parentObject = fpPlayer;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_AirMoves")]
    public class Action_Carol_AirMoves
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.up && fpPlayer.input.attackHold && !fpPlayer.input.attackPress && fpPlayer.state != new FPObjectState(fpPlayer.State_Carol_Punch))
            {
                fpPlayer.SetPlayerAnimation("AttackUp", 0f, 0f, false, true);
                fpPlayer.nextAttack = 1;

                fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);
                //Patcher.SetPlayerValue("combo", false);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_GroundMoves")]
    public class Action_Carol_GroundMoves
    {
        static bool Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.attackHold && !fpPlayer.input.attackPress && fpPlayer.state != new FPObjectState(fpPlayer.State_Carol_Punch)
                && !(fpPlayer.hasSpecialItem && fpPlayer.carolJumpDisc != null && !fpPlayer.carolJumpDisc.parentIsWarping && !fpPlayer.carolJumpDisc.parentHasWarpedSuccessfully)
                && !(fpPlayer.input.guardHold && fpPlayer.Action_Carol_JumpDiscWarp(fpPlayer.carolJumpDisc))
                && !(fpPlayer.characterID == FPCharacterID.CAROL && fpPlayer.state == new FPObjectState(fpPlayer.State_Crouching) && fpPlayer.input.jumpPress)
                )
            {
                fpPlayer.animator.SetSpeed(1f);
                if (!(fpPlayer.state == new FPObjectState(fpPlayer.State_Crouching) || (fpPlayer.onGround && fpPlayer.input.down && fpPlayer.groundVel == 0f)) && fpPlayer.input.up)
                {
                    fpPlayer.SetPlayerAnimation("AttackUp", 0f, 0f, false, true);
                    if (fpPlayer.characterID == FPCharacterID.CAROL)
                    {
                        fpPlayer.EqualizeAirVelocityToGroundVelocity();
                        fpPlayer.onGround = false;
                        fpPlayer.velocity.x = fpPlayer.velocity.x + 7f * FPCommon.GetCleanCos((fpPlayer.groundAngle + 90f) * 0.017453292f);
                        fpPlayer.velocity.y = fpPlayer.velocity.y + 7f * FPCommon.GetCleanSin((fpPlayer.groundAngle + 90f) * 0.017453292f);
                        fpPlayer.velocity += fpPlayer.platformVelocity;
                        fpPlayer.platformVelocity = new Vector2(0f, 0f);
                    }
                    fpPlayer.nextAttack = 1;

                    fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                    fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);
                    //Patcher.SetPlayerValue("combo", false);
                    return false;
                } 
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces")]
    public class ApplyGroundForces
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (Plugin.InertialRolling.Value && fpPlayer.state == new FPObjectState(fpPlayer.State_Carol_Roll))
            {
                fpPlayer.groundVel *= 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
            }
            else if (Plugin.InertialBike.Value && fpPlayer.characterID == FPCharacterID.BIKECAROL && (Plugin.AlwaysAccelerate.Value || fpPlayer.currentAnimation == "Crouching"))
            {
                var acceleration = 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
                fpPlayer.groundVel *= acceleration * (acceleration > 1f ? 1.004f : 0.998f);
            }
        }
    }
}
