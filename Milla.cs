using HarmonyLib;
using UnityEngine;

namespace FP2Rebalance
{
    public class Milla
    {
        public static void Patch()
        {
            if (!Plugin.MillaRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;

            harmony.PatchAll(typeof(Action_MillaCubeSpawn));
            harmony.PatchAll(typeof(ObjectCreated));
            harmony.PatchAll(typeof(State_Milla_Shield));
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_MillaCubeSpawn")]
        public class Action_MillaCubeSpawn
        {
            static bool Prefix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (fpPlayer.state == new FPObjectState(fpPlayer.State_Milla_ShieldRelease) && (bool)Patcher.GetPlayerValue("useSpecialItem", fpPlayer))
                {
                    fpPlayer.millaCubeEnergy = 50f;
                }
                else
                {
                    fpPlayer.millaCubeEnergy = 100f;
                }

                if (fpPlayer.cubeSpawned)
                {
                    int num = Patcher.GetMillaCubeNumber();
                    if (num < Plugin.CubeLimit.Value)
                    {
                        MillaMasterCube millaMasterCube = (MillaMasterCube)FPStage.CreateStageObject(MillaMasterCube.classID, fpPlayer.position.x, fpPlayer.position.y);
                        millaMasterCube.parentObject = fpPlayer;
                        millaMasterCube.cubeSpawnID = num;
                        millaMasterCube.floatStep = (float)num * -30f;
                    }
                }
                else if (Plugin.CubeLimit.Value > 0)
                {
                    MillaMasterCube millaMasterCube = (MillaMasterCube)FPStage.CreateStageObject(MillaMasterCube.classID, fpPlayer.position.x, fpPlayer.position.y);
                    millaMasterCube.parentObject = fpPlayer;
                    fpPlayer.cubeSpawned = true;
                }

                fpPlayer.Action_PlaySoundUninterruptable(fpPlayer.sfxMillaCubeSpawn);
                return false;
            }
        }

        [HarmonyPatch(typeof(MillaCube), "ObjectCreated")]
        public class ObjectCreated
        {
            static void Postfix(ref float ___explodeTimer)
            {
                var fpPlayer = Patcher.GetPlayer;
                ___explodeTimer = 15f + (Patcher.GetMillaCubeNumber() * 3f + fpPlayer.millaCubeEnergy / 33f) * Plugin.FireRangeMultiplier.Value;
            }
        }

        [HarmonyPatch(typeof(FPPlayer), "State_Milla_Shield")]
        public class State_Milla_Shield
        {
            private static float spawnBulletTimer = 0f;
            private static bool savedInput;
            static void Prefix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (Plugin.LimitedAutoAttack.Value && Patcher.GetMillaCubeNumber() == 0) return;

                savedInput = fpPlayer.input.specialHold;
                fpPlayer.input.specialHold = true;
            }

            static void Postfix()
            {
                var fpPlayer = Patcher.GetPlayer;
                if (Plugin.LimitedAutoAttack.Value && Patcher.GetMillaCubeNumber() == 0) return;

                fpPlayer.input.specialHold = savedInput;
                spawnBulletTimer += FPStage.deltaTime;
                if (spawnBulletTimer > 5f - Mathf.Min(Patcher.GetMillaCubeNumber(), 3f) + (fpPlayer.onGround ? 0f : 3f) && !(fpPlayer.input.specialHold && fpPlayer.input.attackHold))
                {
                    spawnBulletTimer = 0f;
                    if (fpPlayer.onGround)
                    {
                        string text = fpPlayer.currentAnimation;
                        fpPlayer.currentAnimation = string.Empty;
                        fpPlayer.animator.Play(text, -1, 0f);
                        fpPlayer.SetPlayerAnimation(text, 0f, 0f, false, true);
                    }
                    FPBaseObject fpbaseObject = null;
                    while (FPStage.ForEach(MillaShield.classID, ref fpbaseObject))
                    {
                        MillaShield millaShield = (MillaShield)fpbaseObject;
                        if (millaShield.parentObject == fpPlayer)
                        {
                            millaShield.burst = true;
                        }
                    }
                    fpPlayer.directionLockScriptside = true;
                    fpPlayer.state = new FPObjectState(fpPlayer.State_Milla_ShieldRelease);
                    fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                    fpPlayer.Action_PlaySoundUninterruptable(fpPlayer.sfxMillaShieldFire);
                }
            }
        }
    }
}
