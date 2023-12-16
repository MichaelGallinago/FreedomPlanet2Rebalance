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
            harmony.PatchAll(typeof(Action_NeeraSlam));
            harmony.PatchAll(typeof(Action_NeeraFreezeRay));
            harmony.PatchAll(typeof(Action_NeeraFreeze));

            if (Plugin.TripleShoot.Value)
                harmony.PatchAll(typeof(Action_NeeraProjectile));
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraRune")]
        public class Action_NeeraRune
        {
            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.characterID != FPCharacterID.NEERA || !(fpPlayer.energy >= 100f) ||
                    fpPlayer.powerupTimer != 30f) return;
                fpPlayer.energyRecoverRateCurrent = fpPlayer.hasSpecialItem ? -0.2f : -0.3f;
                fpPlayer.acceleration = fpPlayer.GetPlayerStat_Default_Acceleration() * 2f;
                fpPlayer.deceleration = fpPlayer.GetPlayerStat_Default_Deceleration() * 2f;
                fpPlayer.airAceleration = fpPlayer.GetPlayerStat_Default_AirAceleration() * 2f;
                Patcher.SetPlayerValue("speedMultiplier", 2f + fpPlayer.potions[6] * 0.05f, fpPlayer);
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
                FPPlayer fpPlayer = Patcher.GetPlayer;

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
            private static float _savedEnergy = -1f;
            private static string _lastAnimation;

            static void Prefix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                _lastAnimation = fpPlayer.currentAnimation;

                if (!(fpPlayer.energyRecoverRateCurrent >= 0f)) return;
                _savedEnergy = fpPlayer.energy;
                fpPlayer.energy += 50f;
            }

            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                if (_lastAnimation != fpPlayer.currentAnimation && fpPlayer.currentAnimation == "SpecialUp")
                {
                    Action_NeeraFreezeRay.IsFocused = fpPlayer.energyRecoverRateCurrent < 0f;
                }

                if (_savedEnergy == -1f) return;
                fpPlayer.energy = fpPlayer.energy == 0 ? _savedEnergy - 50f : _savedEnergy;
                _savedEnergy = -1f;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Neera_AirMoves")]
        public class Action_Neera_AirMoves
        {
            private static float _savedEnergy = -1f;
            private static string _lastAnimation;

            static void Prefix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                _lastAnimation = fpPlayer.currentAnimation;

                if (!(fpPlayer.energyRecoverRateCurrent >= 0f)) return;
                _savedEnergy = fpPlayer.energy;
                fpPlayer.energy += 50f;
            }

            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                if (_lastAnimation != fpPlayer.currentAnimation && fpPlayer.currentAnimation == "AirSpecialUp")
                {
                    Action_NeeraFreezeRay.IsFocused = fpPlayer.energyRecoverRateCurrent < 0f;
                }

                if (_savedEnergy != -1f)
                {
                    fpPlayer.energy = fpPlayer.energy == 0 ? _savedEnergy - 50f : _savedEnergy;
                    _savedEnergy = -1f;
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
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (!fpPlayer.onGround && fpPlayer.state == new FPObjectState(fpPlayer.State_Neera_AttackForward))
                    fpPlayer.jumpAbilityFlag = false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_Neera_AttackForward")]
        public class State_Neera_AttackForward
        {
            private static float _timer;
            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.currentAnimation == "AirAttackDown" && fpPlayer.input.attackPress && fpPlayer.velocity.y > -12f && fpPlayer.energy >= 25f)
                {
                    FPCamera.stageCamera.screenShake = Mathf.Max(FPCamera.stageCamera.screenShake, 10f);
                    fpPlayer.Energy_Restore(-25f);
                    fpPlayer.velocity.y = -15f;
                }

                if (fpPlayer.velocity.y <= -12f)
                {
                    fpPlayer.attackPower = -fpPlayer.velocity.y - 2f;
                    fpPlayer.invincibilityTime = Mathf.Max(2f, fpPlayer.invincibilityTime);
                    if (Mathf.Repeat(_timer += FPStage.deltaTime, 4f) < 1f)
                    {
                        FPStage.CreateStageObject(Sparkle.classID, fpPlayer.position.x + Random.Range(-24f, 24f), fpPlayer.position.y + Random.Range(-24f, 24f));
                    }
                }
                else
                {
                    _timer = 0;
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraProjectile")]
        public class Action_NeeraProjectile
        {
            static bool Prefix(RuntimeAnimatorController projectileAnimator)
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                var offsetY = 8f;
                var offsetX = 0f;
                if (fpPlayer.currentAnimation == "CrouchAttack_Loop")
                {
                    offsetY = -8f;
                    offsetX = 24f;
                }

                float sign = (fpPlayer.direction == FPDirection.FACING_LEFT ? 1 : -1);
                for (float angleOffset = -45f; angleOffset < 90f; angleOffset += 45f)
                {
                    float angle = fpPlayer.angle + angleOffset;
                    var projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID,
                        fpPlayer.position.x - sign * Mathf.Cos(0.017453292f * fpPlayer.angle) * (80f + offsetX)
                        + Mathf.Sin(0.017453292f * fpPlayer.angle) * offsetY,
                        fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * offsetY
                        - sign * Mathf.Sin(0.017453292f * fpPlayer.angle) * (80f + offsetX));
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
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.guardEffectFlag && fpPlayer.characterID == FPCharacterID.NEERA)
                    fpPlayer.Energy_Restore(fpPlayer.energyRecoverRateCurrent < 0f ? Plugin.FocusGuardEnergyBonus.Value : Plugin.GuardEnergyBonus.Value);
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_Guard")]
        public class Action_Guard
        {
            static void Prefix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.characterID != FPCharacterID.NEERA || !(fpPlayer.energyRecoverRateCurrent < 0f)) return;
                fpPlayer.powerupTimer += 30f;
                fpPlayer.acceleration = fpPlayer.GetPlayerStat_Default_Acceleration() * 2f;
                fpPlayer.deceleration = fpPlayer.GetPlayerStat_Default_Deceleration() * 2f;
                fpPlayer.airAceleration = fpPlayer.GetPlayerStat_Default_AirAceleration() * 2f;
                Patcher.SetPlayerValue("speedMultiplier", 2f + fpPlayer.potions[6] * 0.05f, fpPlayer);
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Energy_Restore")]
        public class Energy_Restore
        {
            static bool Prefix(float amount)
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                if (amount >= 0f)
                {
                    if (!(fpPlayer.energy < 100f)) return false;
                    fpPlayer.energy = Mathf.Min(fpPlayer.energy + amount +
                        (fpPlayer.powerupTimer > 0f ? Plugin.AttackEnergyBonus.Value : 0f), 100f);
                    if (!(fpPlayer.energy >= 100f)) return false;
                    fpPlayer.Effect_Regen_Sparkle();
                    return false;
                }

                fpPlayer.energy = Mathf.Max(fpPlayer.energy + amount, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraSlam")]
        public class Action_NeeraSlam
        {
            private static bool IsFocused { get; set; }

            static void Prefix()
            {
                IsFocused = Patcher.GetPlayer.energyRecoverRateCurrent < 0f;
            }

            static void Postfix(ref RuntimeAnimatorController projectileAnimator)
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                if (!fpPlayer.hasSpecialItem || !IsFocused) return;

                const float num = 40f;
                const float num2 = -12f;
                for (var i = 4; i < 8; i++)
                {
                    float num3;
                    float num4;
                    if (fpPlayer.onGround)
                    {
                        num3 = Mathf.Cos(0.017453292f * (i * 15f + 25f)) * (8f - i * 2f);
                        num4 = Mathf.Sin(0.017453292f * (i * 15f + 25f)) * (9f + i % 2 * 3f);
                    }
                    else
                    {
                        num3 = Mathf.Cos(0.017453292f * (i * 15f + 25f)) * (8f - i * 2f);
                        num4 = Mathf.Sin(0.017453292f * (i * 15f + 25f)) * (9f + i % 2 * 3f) - 3f;
                    }
                    ProjectileBasic projectileBasic;
                    if (fpPlayer.direction == FPDirection.FACING_LEFT)
                    {
                        projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, fpPlayer.position.x - Mathf.Cos(0.017453292f * fpPlayer.angle) * num + Mathf.Sin(0.017453292f * fpPlayer.angle) * num2, fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * num2 - Mathf.Sin(0.017453292f * fpPlayer.angle) * num);
                        projectileBasic.velocity.x = Mathf.Cos(0.017453292f * fpPlayer.angle) * -num3 - Mathf.Sin(0.017453292f * fpPlayer.angle) * num4;
                        projectileBasic.velocity.y = Mathf.Sin(0.017453292f * fpPlayer.angle) * -num3 + Mathf.Cos(0.017453292f * fpPlayer.angle) * num4;
                        ProjectileBasic projectileBasic2 = projectileBasic;
                        projectileBasic2.position.x -= 16f;
                        if (FPCollision.CheckTerrainCircleThroughPlatforms(projectileBasic, 8f, false))
                        {
                            ProjectileBasic projectileBasic3 = projectileBasic;
                            projectileBasic3.velocity.x *= -2f;
                        }
                        ProjectileBasic projectileBasic4 = projectileBasic;
                        projectileBasic4.position.x += 16f;
                    }
                    else
                    {
                        projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, 
                            fpPlayer.position.x + Mathf.Cos(0.017453292f * fpPlayer.angle) * num + Mathf.Sin(0.017453292f * fpPlayer.angle) * num2, 
                            fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * num2 + Mathf.Sin(0.017453292f * fpPlayer.angle) * num);
                        projectileBasic.velocity.x = Mathf.Cos(0.017453292f * fpPlayer.angle) * num3 - Mathf.Sin(0.017453292f * fpPlayer.angle) * num4;
                        projectileBasic.velocity.y = Mathf.Sin(0.017453292f * fpPlayer.angle) * num3 + Mathf.Cos(0.017453292f * fpPlayer.angle) * num4;
                        ProjectileBasic projectileBasic5 = projectileBasic;
                        projectileBasic5.position.x += 16f;
                        if (FPCollision.CheckTerrainCircleThroughPlatforms(projectileBasic, 8f, false))
                        {
                            projectileBasic.velocity.x *= -2f;
                        }
                        ProjectileBasic projectileBasic7 = projectileBasic;
                        projectileBasic7.position.x -= 16f;
                    }
                    projectileBasic.animatorController = projectileAnimator;
                    projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                    projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                    projectileBasic.direction = fpPlayer.direction;
                    projectileBasic.angle = fpPlayer.angle;
                    projectileBasic.explodeType = FPExplodeType.ICEWALL;
                    projectileBasic.parentObject = fpPlayer;
                    projectileBasic.faction = fpPlayer.faction;
                    projectileBasic.timeBeforeCollisions = 8f;
                }
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraFreezeRay")]
        public class Action_NeeraFreezeRay
        {
            public static bool IsFocused;

            static void Postfix(ref RuntimeAnimatorController projectileAnimator)
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                if (!fpPlayer.hasSpecialItem || !IsFocused) return;

                fpPlayer.Action_ScreenShake(6f);
                CreateProjectile(projectileAnimator, fpPlayer, 30f);
                CreateProjectile(projectileAnimator, fpPlayer, -30f);
            }

            static void CreateProjectile(RuntimeAnimatorController projectileAnimator, FPPlayer fpPlayer, float offsetAngle)
            {
                ProjectileBasic projectileBasic = (ProjectileBasic)FPStage.CreateStageObject(
                    ProjectileBasic.classID, fpPlayer.position.x, fpPlayer.position.y);
                projectileBasic.velocity.x = Mathf.Sin(0.017453292f * (fpPlayer.angle + offsetAngle)) * -16f;
                projectileBasic.velocity.y = Mathf.Cos(0.017453292f * (fpPlayer.angle + offsetAngle)) * 16f;
                projectileBasic.animatorController = projectileAnimator;
                projectileBasic.animator = projectileBasic.GetComponent<Animator>();
                projectileBasic.animator.runtimeAnimatorController = projectileBasic.animatorController;
                projectileBasic.direction = fpPlayer.direction;
                projectileBasic.angle = fpPlayer.angle + offsetAngle;
                projectileBasic.explodeType = FPExplodeType.WHITEBURST;
                projectileBasic.sfxExplode = null;
                projectileBasic.parentObject = fpPlayer;
                projectileBasic.faction = fpPlayer.faction;
                projectileBasic.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_NeeraFreeze")]
        public class Action_NeeraFreeze
        {
            private static bool _isFocused;

            static void Prefix()
            {
                _isFocused = Patcher.GetPlayer.energyRecoverRateCurrent < 0f;
            }

            static void Postfix()
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;

                if (!fpPlayer.hasSpecialItem || !_isFocused) return;

                NeeraFreeze neeraFreeze;
                float offsetAngle;
                if (fpPlayer.direction == FPDirection.FACING_LEFT)
                {
                    offsetAngle = -30f;
                    neeraFreeze = (NeeraFreeze)FPStage.CreateStageObject(NeeraFreeze.classID, 
                        fpPlayer.position.x - Mathf.Cos(0.017453292f * fpPlayer.angle) * 32f + Mathf.Sin(0.017453292f * fpPlayer.angle) * 8f, 
                        fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * 8f - Mathf.Sin(0.017453292f * fpPlayer.angle) * 32f);
                    neeraFreeze.velocity.x = Mathf.Cos(0.017453292f * (fpPlayer.angle + offsetAngle)) * -8f;
                    neeraFreeze.velocity.y = Mathf.Sin(0.017453292f * (fpPlayer.angle + offsetAngle)) * -8f;
                    NeeraFreeze neeraFreeze2 = neeraFreeze;
                    neeraFreeze2.velocity.x += Mathf.Min(0f, fpPlayer.velocity.x * 0.5f);
                }
                else
                {
                    offsetAngle = 30f;
                    neeraFreeze = (NeeraFreeze)FPStage.CreateStageObject(NeeraFreeze.classID, 
                        fpPlayer.position.x + Mathf.Cos(0.017453292f * fpPlayer.angle) * 32f + Mathf.Sin(0.017453292f * fpPlayer.angle) * 8f, 
                        fpPlayer.position.y + Mathf.Cos(0.017453292f * fpPlayer.angle) * 8f + Mathf.Sin(0.017453292f * fpPlayer.angle) * 32f);
                    neeraFreeze.velocity.x = Mathf.Cos(0.017453292f * (fpPlayer.angle + offsetAngle)) * 8f;
                    neeraFreeze.velocity.y = Mathf.Sin(0.017453292f * (fpPlayer.angle + offsetAngle)) * 8f;
                    NeeraFreeze neeraFreeze3 = neeraFreeze;
                    neeraFreeze3.velocity.x += Mathf.Max(0f, fpPlayer.velocity.x * 0.5f);
                }
                neeraFreeze.direction = fpPlayer.direction;
                neeraFreeze.angle = fpPlayer.angle + offsetAngle;
                neeraFreeze.gravityStrength = 0f;
                neeraFreeze.explodeTimer = 120f;
                if (fpPlayer.IsPowerupActive(FPPowerup.LONG_SPECIALS))
                {
                    neeraFreeze.explodeTimer = 240f;
                }
                neeraFreeze.parentObject = fpPlayer;
                neeraFreeze.faction = fpPlayer.faction;
            }
        }
    }
}
