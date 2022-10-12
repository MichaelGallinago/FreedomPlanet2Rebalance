using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;
using MelonLoader;
using static Ferr.Path2D;

namespace FP2Rebalance.MichaelGallinago
{
    public class FP2RebalanceBehaviour : MonoBehaviour
    {
        /*
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

    class Patcher
    {
        private static Type playerType = typeof(FPPlayer);

        public static FieldInfo GetPlayerField(string name) => playerType.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

        public static void SetPlayerValue(string name, object value, object player = null) => GetPlayerField(name).SetValue(player, value);

        public static object GetPlayerValue(string name, object player = null) => GetPlayerField(name).GetValue(player);

        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();
    }

    [HarmonyPatch(typeof(FPPlayer), "Start")]
    public class Patch_Start
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / ((fpPlayer.characterID == FPCharacterID.NEERA) ? 4f : 1f);
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_NeeraRune")]
    public class Patch_Action_NeeraRune
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.NEERA && fpPlayer.energy >= 100f && fpPlayer.powerupTimer == 30f)
            {
                if (FPStage.prevGameSpeed == 1f)
                {
                    FPStage.SetGameSpeed(0.7f);
                }
                fpPlayer.energyRecoverRateCurrent = -0.3f;
            }
        }
    }

    [HarmonyPatch(typeof(FPSaveManager), "AddCrystal")]
    public class Patch_AddCrystal
    {
        static void Postfix(FPPlayer targetPlayer)
        {
            if (targetPlayer.characterID == FPCharacterID.NEERA && targetPlayer.energyRecoverRateCurrent < 0f)
            {
                targetPlayer.energy += targetPlayer.energyRecoverRate * 45f;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Energy_Restore")]
    public class Patch_Energy_Restore
    {
        static bool Prefix(float amount)
        {
            var fpPlayer = Patcher.GetPlayer;
            if (amount >= 0f)
            {
                if (fpPlayer.energy < 100f)
                {
                    fpPlayer.energy = Mathf.Min(fpPlayer.energy + amount + (fpPlayer.powerupTimer > 0f ? 10f : 0f), 100f);
                    if (fpPlayer.energy >= 100f)
                    {
                        fpPlayer.Effect_Regen_Sparkle();
                        return false;
                    }
                }
            }
            else
            {
                fpPlayer.energy = Mathf.Max(fpPlayer.energy + amount, 0f);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_NeeraEnergyReset")]
    public class Patch_Action_NeeraEnergyReset
    {
        static bool Prefix(bool zero = true)
        {
            var fpPlayer = Patcher.GetPlayer;
            if (zero)
            {
                fpPlayer.Energy_Restore(-50f);
            }
            if (!zero || fpPlayer.energy <= 0f)
            {
                if (FPStage.prevGameSpeed == 0.7f)
                {
                    FPStage.SetGameSpeed(1f);
                }
                fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / 4f;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Neera_GroundMoves")]
    public class Patch_Action_Neera_GroundMoves
    {
        private static float savedEnergy;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedEnergy = fpPlayer.energy;
            fpPlayer.energy += 50f;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Neera_AirMoves")]
    public class Patch_Action_Neera_AirMoves
    {
        private static float savedEnergy;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedEnergy = fpPlayer.energy;
            fpPlayer.energy += 50f;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;

            if ((fpPlayer.powerupTimer > 0f || fpPlayer.energyRecoverRateCurrent < 0f) && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward))
            {
                fpPlayer.velocity.y = Mathf.Max(5f, fpPlayer.velocity.y + 2f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackNeutral")]
    public class Patch_State_Neera_AttackNeutral
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (!fpPlayer.onGround && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward) && (fpPlayer.powerupTimer > 0f || fpPlayer.energyRecoverRateCurrent < 0f))
            {
                fpPlayer.jumpAbilityFlag = false;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackForward")]
    public class Patch_State_Neera_AttackForward
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.currentAnimation == "AirAttackDown" && fpPlayer.input.attackPress)
            {
                fpPlayer.velocity.y = -12f;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Hurt")]
    public class Patch_Action_Hurt
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.guardEffectFlag == true && fpPlayer.characterID == FPCharacterID.NEERA)
            {
                fpPlayer.Energy_Restore((fpPlayer.energyRecoverRateCurrent < 0f) ? 40f : 25f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_Lilac_DragonBoostPt2")]
    public class Patch_State_Lilac_DragonBoostPt2
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.currentAnimation == "Wings_Loop" && fpPlayer.state == new FPObjectState(fpPlayer.State_Lilac_DragonBoostPt2))
            {
                if (fpPlayer.powerupTimer <= 0f)
                {
                    fpPlayer.health = FPCommon.RoundToQuantumWithinErrorThreshold(fpPlayer.health - FPStage.deltaTime / 70f, 0.5f);
                }
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

    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class Patch_State_Carol_JumpDiscWarp
    {
        private static Vector2 savedVelocity;

        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            savedVelocity = fpPlayer.velocity;
        }

        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.genericTimer <= 0f && fpPlayer.guardTime <= 0f && fpPlayer.input.guardHold)
            {
                fpPlayer.velocity = savedVelocity * 1.2f;
                fpPlayer.hitStun = 0f;

                if (Mathf.Abs(fpPlayer.velocity.x) > 12f)
                {
                    fpPlayer.SetPlayerAnimation("GuardAirFast", 0f, 0f, false, true);
                }
                else
                {
                    fpPlayer.SetPlayerAnimation("GuardAir", 0f, 0f, false, true);
                }
                fpPlayer.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                fpPlayer.childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(fpPlayer.velocity.x * 0.05f)));
                fpPlayer.Action_Guard(0f);
                fpPlayer.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, fpPlayer.position.x, fpPlayer.position.y);
                guardFlash.parentObject = fpPlayer;
                fpPlayer.Action_StopSound();
                FPAudio.PlaySfx(15);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_AirMoves")]
    public class Patch_Action_Carol_AirMoves
    {
        static bool Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.attackHold && !(fpPlayer.input.attackPress || fpPlayer.state == new FPObjectState(fpPlayer.State_Carol_Punch)
                || fpPlayer.currentAnimation == "Rolling" || fpPlayer.currentAnimation == "Pounce" || fpPlayer.input.down))
            {
                if (fpPlayer.input.up)
                {
                    fpPlayer.SetPlayerAnimation("AttackUp", 0f, 0f, false, true);
                    if (fpPlayer.characterID == FPCharacterID.CAROL)
                    {
                        float SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostThreshold = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostThreshold");
                        float SINGLE_CarolUpAttack_VelocityYBoost_MaxBoostThreshold = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_MaxBoostThreshold");
                        float SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostMultiplier = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostMultiplier");
                        float SINGLE_CarolUpAttack_VelocityYBoost_FinalValue = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_FinalValue");
                        float SINGLE_CarolUpAttack_VelocityYBoost_DecayFinalValue = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_DecayFinalValue");
                        float SINGLE_CarolUpAttack_VelocityYBoost_DecayDecrement = (float)Patcher.GetPlayerValue("SINGLE_CarolUpAttack_VelocityYBoost_DecayDecrement");
                        float carolUpAttack_VelocityYBoost_Current = (float)Patcher.GetPlayerValue("carolUpAttack_VelocityYBoost_Current", fpPlayer);
                        float carolUpAttack_VelocityYBoost_DecayCurrent = (float)Patcher.GetPlayerValue("carolUpAttack_VelocityYBoost_DecayCurrent", fpPlayer);

                        if (fpPlayer.velocity.y < SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostThreshold)
                        {
                            fpPlayer.velocity.y = Mathf.Min(SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostThreshold, fpPlayer.velocity.y + carolUpAttack_VelocityYBoost_Current * SINGLE_CarolUpAttack_VelocityYBoost_IncreasedBoostMultiplier);
                        }
                        if (fpPlayer.velocity.y < SINGLE_CarolUpAttack_VelocityYBoost_MaxBoostThreshold)
                        {
                            fpPlayer.velocity.y = Mathf.Min(SINGLE_CarolUpAttack_VelocityYBoost_MaxBoostThreshold, fpPlayer.velocity.y + carolUpAttack_VelocityYBoost_Current);
                        }
                        Patcher.SetPlayerValue("carolUpAttack_VelocityYBoost_Current", Mathf.Max(SINGLE_CarolUpAttack_VelocityYBoost_FinalValue, carolUpAttack_VelocityYBoost_Current - carolUpAttack_VelocityYBoost_DecayCurrent), fpPlayer);
                        if (carolUpAttack_VelocityYBoost_DecayCurrent > SINGLE_CarolUpAttack_VelocityYBoost_DecayFinalValue)
                        {
                            Patcher.SetPlayerValue("carolUpAttack_VelocityYBoost_DecayCurrent", Mathf.Max(SINGLE_CarolUpAttack_VelocityYBoost_DecayFinalValue, carolUpAttack_VelocityYBoost_DecayCurrent - SINGLE_CarolUpAttack_VelocityYBoost_DecayDecrement), fpPlayer);
                        }
                    }
                    fpPlayer.nextAttack = 1;
                }
                else if (fpPlayer.nextAttack > 1 && fpPlayer.nextAttack < 4)
                {
                    fpPlayer.SetPlayerAnimation("Punch" + fpPlayer.nextAttack, 0f, 0f, false);
                    fpPlayer.nextAttack ++;
                }
                else
                {
                    fpPlayer.SetPlayerAnimation("Punch1", 0f, 0f, false);
                    fpPlayer.nextAttack = 2;
                }

                fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);
                //Patcher.GetPlayerField("combo").SetValue(null, false);
                Patcher.SetPlayerValue("combo", false);
                return false;
            }
            return true;
        }
    }

    /*
    [HarmonyPatch(typeof(FPPlayer), "Action_Carol_GroundMoves")]
    public class Patch_Action_Carol_GroundMoves
    {
        static bool Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.attackHold && !(fpPlayer.input.attackPress
                || fpPlayer.state == new FPObjectState(fpPlayer.State_Carol_Punch)
                || fpPlayer.currentAnimation == "Rolling" || fpPlayer.currentAnimation == "Pounce"
                || fpPlayer.input.down || fpPlayer.input.up || fpPlayer.nextAttack <= 1 || fpPlayer.nextAttack >= 4))
            {
                fpPlayer.SetPlayerAnimation("Punch" + fpPlayer.nextAttack, 0f, 0f, false, true);
                fpPlayer.nextAttack++;
            }
            fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
            fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);

            Type exampleType = typeof(FPPlayer);
            FieldInfo combo = exampleType.GetField("combo", BindingFlags.NonPublic | BindingFlags.Instance);
            combo.SetValue(null, false);
            return false;
        }
    }*/

    [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces")]
    public class Patch_ApplyGroundForces
    {
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.state == new FPObjectState(fpPlayer.State_Carol_Roll))
            {
                fpPlayer.groundVel *= 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
            }
            else if (fpPlayer.characterID == FPCharacterID.BIKECAROL && fpPlayer.currentAnimation == "Crouching")
            {
                var acceleration = 1f - Mathf.Sin(fpPlayer.groundAngle * 0.017453292f) / 32f * ((fpPlayer.groundVel > 0f) ? 1f : -1f) * FPStage.deltaTime;
                fpPlayer.groundVel *= acceleration * (acceleration > 1f ? 1.004f: 0.998f);
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_CrushKO")]
    public class Patch_State_CrushKO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
            {
                fpPlayer.hasSpecialItem = false;
            }
        }
    }

    [HarmonyPatch(typeof(FPPlayer), "State_KO")]
    public class Patch_State_KO
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.characterID == FPCharacterID.LILAC)
            {
                fpPlayer.hasSpecialItem = false;
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