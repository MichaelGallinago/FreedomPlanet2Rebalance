using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FP2Rebalance
{
    [BepInPlugin("com.micg.plugins.fp2.rebalance", "FP2Rebalance", "1.6.3")]
    [BepInProcess("FP2.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> LilacRebalance;
        public static ConfigEntry<int> YellPercent;
        public static ConfigEntry<bool> WingAsPowerUp;
        public static ConfigEntry<bool> ControllableWings;

        public static ConfigEntry<bool> CarolRebalance;
        public static ConfigEntry<bool> DiscCansel;
        public static ConfigEntry<bool> InertialBike;
        public static ConfigEntry<bool> InertialRolling;
        public static ConfigEntry<bool> AlwaysAccelerate;
        public static ConfigEntry<bool> HoldAttacks;

        public static ConfigEntry<bool> MillaRebalance;
        public static ConfigEntry<bool> LimitedAutoAttack;
        public static ConfigEntry<float> FireRangeMultiplier;
        public static ConfigEntry<int> CubeLimit;

        public static ConfigEntry<bool> NeeraRebalance;
        public static ConfigEntry<bool> TripleShoot;
        public static ConfigEntry<int> CrystalEnergyBonus;
        public static ConfigEntry<int> AttackEnergyBonus;
        public static ConfigEntry<int> GuardEnergyBonus;
        public static ConfigEntry<int> FocusGuardEnergyBonus;

        private static Harmony PluginLink = new Harmony("com.micg.plugins.fp2.rebalance");

        private void Awake()
        {
            LoadConfig();
            PatchLilac();
            PatchCarol();
            PatchMilla();
            PatchNeera();
            PatchGeneral();
        }

        private void LoadConfig()
        {
            LilacRebalance = Config.Bind("Lilac", "LilacRebalance", true, "Set if you want to enable Lilac changes.");
            YellPercent = Config.Bind("Lilac", "YellChance", 10, new ConfigDescription("Set the percent of how often you want Lilac to scream when super boosting. 0 = Never, 100 = Every time.", new AcceptableValueRange<int>(0, 100)));
            WingAsPowerUp = Config.Bind("Lilac", "WingsAsPowerUp", true, "Set if you want the wings to be a powerup you need to find on the stage.");
            ControllableWings = Config.Bind("Lilac", "ControllableWings", true, "Set if you want to control the wings.");

            CarolRebalance = Config.Bind("Carol", "CarolRebalance", true, "Set if you want to enable Carol changes.");
            DiscCansel = Config.Bind("Carol", "DiscCansel", true, "Set if you want Disc Cansel.");
            InertialBike = Config.Bind("Carol", "InertialBike", true, "Set if you want inertial bike physics.");
            InertialRolling = Config.Bind("Carol", "InertialRolling", true, "Set if you want inertial rolling physics.");
            AlwaysAccelerate = Config.Bind("Carol", "AlwaysAccelerate", false, "Set if you want inertial bike physics without holding down.");
            HoldAttacks = Config.Bind("Carol", "AutoAttack", false, "Set if you want to hold attacks.");

            MillaRebalance = Config.Bind("Milla", "MillaRebalance", true, "Set if you want to enable Milla changes.");
            LimitedAutoAttack = Config.Bind("Milla", "LimitedAutoAttack", true, "Set if you want that auto attack to not work without cubes.");
            FireRangeMultiplier = Config.Bind("Milla", "FireRangeMultiplier", 1f, new ConfigDescription("Set multiplier for shoot range.", new AcceptableValueRange<float>(0f, 100f)));
            CubeLimit = Config.Bind("Milla", "CubeLimit", 3, new ConfigDescription("Set multiplier for shoot range.", new AcceptableValueRange<int>(0, 100)));

            NeeraRebalance = Config.Bind("Neera", "NeeraRebalance", true, "Set if you want to enable Neera changes.");
            TripleShoot = Config.Bind("Neera", "TripleShoot", true, "Set if you want triple shoot.");
            CrystalEnergyBonus = Config.Bind("Neera", "CrystalEnergyBonus", 45, new ConfigDescription("Set multiplier for extra energy per crystal.", new AcceptableValueRange<int>(0, 100)));
            AttackEnergyBonus = Config.Bind("Neera", "AttackEnergyBonus", 10, new ConfigDescription("Set value for extra energy per attack.", new AcceptableValueRange<int>(0, 100)));
            GuardEnergyBonus = Config.Bind("Neera", "GuardEnergyBonus", 25, new ConfigDescription("Set value for extra energy per guard.", new AcceptableValueRange<int>(0, 100)));
            FocusGuardEnergyBonus = Config.Bind("Neera", "FocusGuardEnergyBonus", 40, new ConfigDescription("Set value for extra energy per guard in focus.", new AcceptableValueRange<int>(0, 100)));
        }

        private void PatchLilac()
        {
            if (!LilacRebalance.Value) return;

            if (ControllableWings.Value)
            {
                PluginLink.PatchAll(typeof(State_Lilac_DragonBoostPt2));
            }

            PluginLink.PatchAll(typeof(State_Lilac_DragonBoostPt1));

            if (WingAsPowerUp.Value)
            {
                PluginLink.PatchAll(typeof(CollisionCheck));
                PluginLink.PatchAll(typeof(State_CrushKO));
                PluginLink.PatchAll(typeof(State_KO));
            }
        }

        private void PatchCarol()
        {
            if (!CarolRebalance.Value) return;

            if (DiscCansel.Value)
            {
                PluginLink.PatchAll(typeof(State_Carol_JumpDiscWarp));
            }

            if (InertialBike.Value || InertialRolling.Value)
            {
                PluginLink.PatchAll(typeof(ApplyGroundForces));
            }

            if (HoldAttacks.Value)
            {
                PluginLink.PatchAll(typeof(Action_Carol_AirMoves));
                PluginLink.PatchAll(typeof(Action_Carol_GroundMoves));
            }
        }

        private void PatchMilla()
        {
            if (!MillaRebalance.Value) return;

            PluginLink.PatchAll(typeof(Action_MillaCubeSpawn));
            PluginLink.PatchAll(typeof(ObjectCreated));
            PluginLink.PatchAll(typeof(State_Milla_Shield));
        }

        private void PatchNeera()
        {
            if (!NeeraRebalance.Value) return;

            PluginLink.PatchAll(typeof(Action_NeeraRune));
            PluginLink.PatchAll(typeof(AddCrystal));
            PluginLink.PatchAll(typeof(Action_NeeraEnergyReset));
            PluginLink.PatchAll(typeof(Action_Neera_GroundMoves));
            PluginLink.PatchAll(typeof(Action_Neera_AirMoves));
            PluginLink.PatchAll(typeof(State_Neera_AttackNeutral));
            PluginLink.PatchAll(typeof(State_Neera_AttackForward));
            PluginLink.PatchAll(typeof(Action_Hurt));
            PluginLink.PatchAll(typeof(Action_Guard));
            PluginLink.PatchAll(typeof(Energy_Restore));

            if (TripleShoot.Value)
                PluginLink.PatchAll(typeof(Action_NeeraProjectile));
        }

        private void PatchGeneral()
        {
            if (!(NeeraRebalance.Value || MillaRebalance.Value || CarolRebalance.Value || LilacRebalance.Value)) return;

            PluginLink.PatchAll(typeof(Start));
        }
    }

    public class Patcher
    {
        public static FieldInfo GetPlayerField(string name) => typeof(FPPlayer).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

        public static void SetPlayerValue(string name, object value, object player = null) => GetPlayerField(name).SetValue(player, value);

        public static object GetPlayerValue(string name, object player = null) => GetPlayerField(name).GetValue(player);

        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();

        public static int GetMillaCubeNumber()
        {
            int num = 0;
            var fpPlayer = GetPlayer;
            FPBaseObject fpbaseObject = null;
            while (FPStage.ForEach(MillaMasterCube.classID, ref fpbaseObject))
            {
                if (((MillaMasterCube)fpbaseObject).parentObject == fpPlayer) num++;
            }
            return num;
        }
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

    // Lilac
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

        /*
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            /*
            if (fpPlayer.currentAnimation == "Wings_Loop" && fpPlayer.state == new FPObjectState(fpPlayer.State_Lilac_DragonBoostPt2))
            {
                if (fpPlayer.powerupTimer <= 0f)
                {
                    fpPlayer.health = FPCommon.RoundToQuantumWithinErrorThreshold(fpPlayer.health - FPStage.deltaTime / 70f, 0.5f);
                }
            }
            */
            /*
            if (fpPlayer.state == new FPObjectState(fpPlayer.State_Lilac_Glide))
            {

            }
        }
        */
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

    // Carol
    [HarmonyPatch(typeof(FPPlayer), "State_Carol_JumpDiscWarp")]
    public class State_Carol_JumpDiscWarp
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
    public class Action_Carol_AirMoves
    {
        static void Prefix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.input.attackHold && !fpPlayer.input.attackPress && fpPlayer.state != new FPObjectState(fpPlayer.State_Carol_Punch))
            {
                if (fpPlayer.input.up)
                {
                    fpPlayer.SetPlayerAnimation("AttackUp", 0f, 0f, false, true);
                    fpPlayer.nextAttack = 1;
                }
                else if (fpPlayer.nextAttack > 1 && fpPlayer.nextAttack < 4)
                {
                    fpPlayer.SetPlayerAnimation("Punch" + fpPlayer.nextAttack, 0f, 0f, false);
                    fpPlayer.nextAttack++;
                }
                else
                {
                    fpPlayer.SetPlayerAnimation("Punch1", 0f, 0f, false);
                    fpPlayer.nextAttack = 2;
                }

                fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);
                Patcher.SetPlayerValue("combo", false);
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
                if (fpPlayer.state == new FPObjectState(fpPlayer.State_Crouching) || (fpPlayer.onGround && fpPlayer.input.down && fpPlayer.groundVel == 0f))
                {
                    fpPlayer.SetPlayerAnimation("CrouchAttack", 0f, 0f, false, true);
                    fpPlayer.genericTimer = -20f;
                }
                else if (fpPlayer.input.up)
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
                }
                else if (fpPlayer.nextAttack > 1 && fpPlayer.nextAttack < 4)
                {
                    fpPlayer.SetPlayerAnimation("Punch" + fpPlayer.nextAttack, 0f, 0f, false);
                    fpPlayer.nextAttack++;
                }
                else
                {
                    fpPlayer.SetPlayerAnimation("Punch1", 0f, 0f, false);
                    fpPlayer.nextAttack = 2;
                }

                fpPlayer.idleTimer = -fpPlayer.fightStanceTime;
                fpPlayer.state = new FPObjectState(fpPlayer.State_Carol_Punch);
                Patcher.SetPlayerValue("combo", false);
                return false;
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

    // Milla
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

    // Neera
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
        static void Postfix()
        {
            var fpPlayer = Patcher.GetPlayer;
            if (fpPlayer.currentAnimation == "AirAttackDown" && fpPlayer.input.attackPress)
                fpPlayer.velocity.y = -12f;
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