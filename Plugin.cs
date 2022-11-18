using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace FP2Rebalance
{
    [BepInPlugin("com.micg.plugins.fp2.rebalance", "FP2Rebalance", "1.8.0")]
    [BepInProcess("FP2.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> LilacRebalance { get; private set; }
        public static ConfigEntry<int> YellPercent { get; private set; }
        public static ConfigEntry<bool> WingAsPowerUp { get; private set; }
        public static ConfigEntry<bool> ControllableWings { get; private set; }

        public static ConfigEntry<bool> CarolRebalance { get; private set; }
        public static ConfigEntry<bool> DiscCansel { get; private set; }
        public static ConfigEntry<bool> InertialBike { get; private set; }
        public static ConfigEntry<bool> InertialRolling { get; private set; }
        public static ConfigEntry<bool> AlwaysAccelerate { get; private set; }
        public static ConfigEntry<bool> HoldAttacks { get; private set; }

        public static ConfigEntry<bool> MillaRebalance { get; private set; }
        public static ConfigEntry<bool> LimitedAutoAttack { get; private set; }
        public static ConfigEntry<float> FireRangeMultiplier { get; private set; }
        public static ConfigEntry<int> CubeLimit { get; private set; }

        public static ConfigEntry<bool> NeeraRebalance{ get; private set; }
        public static ConfigEntry<bool> TripleShoot { get; private set; }
        public static ConfigEntry<int> CrystalEnergyBonus { get; private set; }
        public static ConfigEntry<int> AttackEnergyBonus { get; private set; }
        public static ConfigEntry<int> GuardEnergyBonus { get; private set; }
        public static ConfigEntry<int> FocusGuardEnergyBonus { get; private set; }

        public static Harmony HarmonyLink { get; } = new Harmony("com.micg.plugins.fp2.rebalance");

        private void Awake()
        {
            LoadConfig();
            Lilac.Patch();
            Carol.Patch();
            Neera.Patch();
            Milla.Patch();
            General.Patch();
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
            HoldAttacks = Config.Bind("Carol", "AutoAttack", false, "Set if you want to hold attacks (from 1.8.0 only kicks up).");

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
    }
}