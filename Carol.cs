using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public static class Carol
    {
        public static void Patch()
        {
            if (!Plugin.CarolRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;

            if (Plugin.DiscCancel.Value)
            {
                harmony.PatchAll(typeof(State_Carol_JumpDiscWarp));
            }

            if (Plugin.InertialBike.Value || Plugin.InertialRolling.Value)
            {
                harmony.PatchAll(typeof(ApplyGroundForces));
            }

            harmony.PatchAll(typeof(Action_Carol_AirMoves));
            if (Plugin.HoldAttacks.Value)
            {
                harmony.PatchAll(typeof(Action_Carol_GroundMoves));
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class State_Carol_JumpDiscWarp
    {
        static bool Prefix()
        {
            FPPlayer fpPlayer = Patcher.GetPlayer;
            float virtualGenericTime = fpPlayer.genericTimer;
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
                        float num3 = 1f - (fpPlayer.carolJumpDisc.maxDistanceFromParent - num2) 
                            / fpPlayer.carolJumpDisc.maxDistanceFromParent;
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

            if (!(virtualGenericTime <= 0f) || !(fpPlayer.guardTime <= 0f) || !fpPlayer.input.guardHold) return true;
            fpPlayer.state = fpPlayer.State_InAir;
            fpPlayer.carolJumpDisc.parentHasWarpedSuccessfully = true;
            fpPlayer.genericTimer = 0;
            fpPlayer.velocity *= 1.2f;
            fpPlayer.SetPlayerAnimation("GuardAir");
            fpPlayer.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
            if (fpPlayer.childAnimator != null)
            {
                fpPlayer.childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
            }
            FPAudio.PlaySfx(15);
            fpPlayer.Action_Guard();
            fpPlayer.Action_ShadowGuard();
            var guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, 
                fpPlayer.position.x, fpPlayer.position.y);
            guardFlash.parentObject = fpPlayer;
            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_AirMoves")]
    public class Action_Carol_AirMoves
    {
        private static FPDirection _direction;
        private static string _previousAnimation;
        static void Prefix()
        {
            FPPlayer fpPlayer = Patcher.GetPlayer;

            _previousAnimation = fpPlayer.currentAnimation;

            if (!Plugin.HoldAttacks.Value) return;

            if (!fpPlayer.input.up || !fpPlayer.input.attackHold || fpPlayer.input.attackPress ||
                fpPlayer.state == fpPlayer.State_Carol_Punch) return;
            fpPlayer.SetPlayerAnimation("AttackUp");
            fpPlayer.nextAttack = 1;

            fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
            fpPlayer.state = fpPlayer.State_Carol_Punch;
        }

        static void Postfix()
        {
            if (!Plugin.PounceCancel.Value) return;

            FPPlayer fpPlayer = Patcher.GetPlayer;

            if (fpPlayer.currentAnimation != "Pounce") return;

            if (_previousAnimation != fpPlayer.currentAnimation)
            {
                _direction = fpPlayer.direction;
            }

            if (_direction == fpPlayer.direction) return;
            fpPlayer.SetPlayerAnimation("GuardAir", 0f, 0f);
            fpPlayer.jumpAbilityFlag = false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_GroundMoves")]
    public class Action_Carol_GroundMoves
    {
        static bool Prefix()
        {
            FPPlayer fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input is not { attackHold: true, attackPress: false }
                || fpPlayer.state == fpPlayer.State_Carol_Punch
                || fpPlayer.hasSpecialItem && fpPlayer.carolJumpDisc != null
                && !fpPlayer.carolJumpDisc.parentIsWarping && !fpPlayer.carolJumpDisc.parentHasWarpedSuccessfully
                || fpPlayer.input.guardHold && fpPlayer.Action_Carol_JumpDiscWarp(fpPlayer.carolJumpDisc)
                || fpPlayer.characterID == FPCharacterID.CAROL && fpPlayer.state == fpPlayer.State_Crouching
                && fpPlayer.input.jumpPress) return true;
            
            fpPlayer.animator.SetSpeed(1f);
            if (fpPlayer.state == fpPlayer.State_Crouching || (fpPlayer.onGround && fpPlayer.input.down 
                    && fpPlayer.groundVel == 0f) || !fpPlayer.input.up) return true;
            fpPlayer.SetPlayerAnimation("AttackUp");
            if (fpPlayer.characterID == FPCharacterID.CAROL)
            {
                fpPlayer.EqualizeAirVelocityToGroundVelocity();
                fpPlayer.onGround = false;
                fpPlayer.velocity.x += 7f * FPCommon.GetCleanCos((fpPlayer.groundAngle + 90f) * 0.017453292f);
                fpPlayer.velocity.y += 7f * FPCommon.GetCleanSin((fpPlayer.groundAngle + 90f) * 0.017453292f);
                fpPlayer.velocity += fpPlayer.platformVelocity;
                fpPlayer.platformVelocity = new Vector2(0f, 0f);
            }
            fpPlayer.nextAttack = 1;

            fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
            fpPlayer.state = fpPlayer.State_Carol_Punch;

            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces")]
    public class ApplyGroundForces
    {
        static void Postfix()
        {
            FPPlayer fpPlayer = Patcher.GetPlayer;
            if (Plugin.InertialRolling.Value && fpPlayer.state == fpPlayer.State_Carol_Roll)
            {
                fpPlayer.groundVel *= 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f 
                    * (fpPlayer.groundVel > 0f ? 1f : -1f) * FPStage.deltaTime;
            }
            else if (Plugin.InertialBike.Value && fpPlayer.characterID == FPCharacterID.BIKECAROL 
                && (Plugin.AlwaysAccelerate.Value || fpPlayer.currentAnimation == "Crouching"))
            {
                float acceleration = 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f 
                    * (fpPlayer.groundVel > 0f ? 1f : -1f) * FPStage.deltaTime;
                fpPlayer.groundVel *= acceleration * (acceleration > 1f ? 1.004f : 0.998f);
            }
        }
    }
}
