using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class Neera
    {
        public static void Patch()
        {
            if (!Plugin.NeeraRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;

            harmony.PatchAll(typeof(Action_NeeraRune));
            harmony.PatchAll(typeof(AddCrystal));
            harmony.PatchAll(typeof(Action_NeeraEnergyReset));
            harmony.PatchAll(typeof(Action_Neera_GroundMoves));
            harmony.PatchAll(typeof(Action_Neera_AirMoves));
            harmony.PatchAll(typeof(State_Neera_AttackNeutral));
            harmony.PatchAll(typeof(State_Neera_AttackForward));
            harmony.PatchAll(typeof(Action_Hurt));
            harmony.PatchAll(typeof(Action_Guard));
            harmony.PatchAll(typeof(Energy_Restore));

            if (Plugin.TripleShoot.Value)
                harmony.PatchAll(typeof(Action_NeeraProjectile));
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraRune")]
        public class Action_NeeraRune
        {
            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.characterID == FPCharacterID.NEERA && fpPlayer.energy >= 100f && fpPlayer.powerupTimer == 30f)
                {
                    fpPlayer.energyRecoverRateCurrent = -0.3f;
                    fpPlayer.acceleration = fpPlayer.GetPlayerStat_Default_Acceleration() * 2f;
                    fpPlayer.deceleration = fpPlayer.GetPlayerStat_Default_Deceleration() * 2f;
                    fpPlayer.airAceleration = fpPlayer.GetPlayerStat_Default_AirAceleration() * 2f;
                    Patcher.SetPlayerValue("speedMultiplier", 2f + (float)fpPlayer.potions[6] * 0.05f, fpPlayer);
                }
            }
        }

        [HarmonyPatch(typeof(FPSaveManager), "AddCrystal")]
        public class AddCrystal
        {
            static void Postfix(FPPlayer targetPlayer)
            {
                if (targetPlayer.characterID == FPCharacterID.NEERA && targetPlayer.energyRecoverRateCurrent < 0f)
                    targetPlayer.energy += targetPlayer.energyRecoverRate * Plugin.CrystalEnergyBonus.Value;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraEnergyReset")]
        public class Action_NeeraEnergyReset
        {
            static bool Prefix(bool zero = true)
            {
                var fpPlayer = Patcher.GetPlayer;

                if (zero)
                    fpPlayer.Energy_Restore(-50f);

                if (!zero || fpPlayer.energy <= 0f)
                    fpPlayer.energyRecoverRateCurrent = fpPlayer.energyRecoverRate / 4f;

                return false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Neera_GroundMoves")]
        public class Action_Neera_GroundMoves
        {
            private static float savedEnergy = -1f;

            static void Prefix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.energyRecoverRateCurrent >= 0f)
                {
                    savedEnergy = fpPlayer.energy;
                    fpPlayer.energy += 50f;
                }
            }

            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (savedEnergy != -1f)
                {
                    fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;
                    savedEnergy = -1f;
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Neera_AirMoves")]
        public class Action_Neera_AirMoves
        {
            private static float savedEnergy = -1f;

            static void Prefix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.energyRecoverRateCurrent >= 0f)
                {
                    savedEnergy = fpPlayer.energy;
                    fpPlayer.energy += 50f;
                }
            }

            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (savedEnergy != -1f)
                {
                    fpPlayer.energy = fpPlayer.energy == 0 ? savedEnergy - 50f : savedEnergy;
                    savedEnergy = -1f;
                }

                if ((fpPlayer.powerupTimer > 0f || fpPlayer.energyRecoverRateCurrent < 0f) && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward))
                {
                    fpPlayer.velocity.y = Mathf.Max(5f, fpPlayer.velocity.y + 3f);
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackNeutral")]
        public class State_Neera_AttackNeutral
        {
            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (!fpPlayer.onGround && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward))
                    fpPlayer.jumpAbilityFlag = false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackForward")]
        public class State_Neera_AttackForward
        {
            private static float timer = 0;
            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.currentAnimation == "AirAttackDown" && fpPlayer.input.attackPress && fpPlayer.velocity.y > -12f && fpPlayer.energy >= 25f)
                {
                    fpPlayer.Energy_Restore(-25f);
                    fpPlayer.velocity.y = -15f;
                }

                if (fpPlayer.velocity.y <= -12f)
                {
                    fpPlayer.attackPower = -fpPlayer.velocity.y - 2f;
                    if (Mathf.Repeat(timer += FPStage.deltaTime, 4f) < 1f)
                    {
                        FPStage.CreateStageObject(Sparkle.classID, fpPlayer.position.x + Random.Range(-24f, 24f), fpPlayer.position.y + Random.Range(-24f, 24f));
                    }
                }
                else
                {
                    timer = 0;
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraProjectile")]
        public class Action_NeeraProjectile
        {
            static bool Prefix(RuntimeAnimatorController projectileAnimator)
            {
                var fpPlayer = Patcher.GetPlayer;
                float offsetY = 8f;
                float offsetX = 0f;
                if (fpPlayer.currentAnimation == "CrouchAttack_Loop")
                {
                    offsetY = -8f;
                    offsetX = 24f;
                }
                ProjectileBasic projectileBasic;
                float sign = (fpPlayer.direction == FPDirection.FACING_LEFT ? 1 : -1);
                for (var angleOffset = -45f; angleOffset < 90f; angleOffset += 45f)
                {
                    float angle = fpPlayer.angle + angleOffset;
                    projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, fpPlayer.position.x - sign * Mathf.Cos(0.017453292f * fpPlayer.angle) * (80f + offsetX) + Mathf.Sin(0.017453292f * fpPlayer.angle) * offsetY, fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * offsetY - sign * Mathf.Sin(0.017453292f * fpPlayer.angle) * (80f + offsetX));
                    projectileBasic.velocity.x = Mathf.Cos(0.017453292f * angle) * (-20f * sign);
                    projectileBasic.velocity.y = Mathf.Sin(0.017453292f * angle) * (-20f * sign);
                    projectileBasic.animatorController = projectileAnimator;
                    projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                    projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                    projectileBasic.direction = fpPlayer.direction;
                    projectileBasic.angle = angle;
                    projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                    projectileBasic.sfxExplode = null;
                    projectileBasic.parentObject = fpPlayer;
                    projectileBasic.faction = fpPlayer.faction;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Hurt")]
        public class Action_Hurt
        {
            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.guardEffectFlag == true && fpPlayer.characterID == FPCharacterID.NEERA)
                    fpPlayer.Energy_Restore(fpPlayer.energyRecoverRateCurrent < 0f ? Plugin.FocusGuardEnergyBonus.Value : Plugin.GuardEnergyBonus.Value);
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Guard")]
        public class Action_Guard
        {
            static void Prefix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.characterID == FPCharacterID.NEERA && fpPlayer.energyRecoverRateCurrent < 0f)
                {
                    fpPlayer.powerupTimer += 30f;
                    fpPlayer.acceleration = fpPlayer.GetPlayerStat_Default_Acceleration() * 2f;
                    fpPlayer.deceleration = fpPlayer.GetPlayerStat_Default_Deceleration() * 2f;
                    fpPlayer.airAceleration = fpPlayer.GetPlayerStat_Default_AirAceleration() * 2f;
                    Patcher.SetPlayerValue("speedMultiplier", 2f + (float)fpPlayer.potions[6] * 0.05f, fpPlayer);
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Energy_Restore")]
        public class Energy_Restore
        {
            static bool Prefix(float amount)
            {
                var fpPlayer = Patcher.GetPlayer;
                if (amount >= 0f)
                {
                    if (fpPlayer.energy < 100f)
                    {
                        fpPlayer.energy = Mathf.Min(fpPlayer.energy + amount + (fpPlayer.powerupTimer > 0f ? Plugin.AttackEnergyBonus.Value : 0f), 100f);
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
    }
}
