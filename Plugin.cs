using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FP2Rebalance
{
    [BepInPlugin("com.micg.plugins.fp2.rebalance", "FP2Rebalance", "2.3.0")]
    [BepInProcess("FP2.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> LilacRebalance { get; private set; }
        public static ConfigEntry<int> YellPercent { get; private set; }

        public static ConfigEntry<bool> CarolRebalance { get; private set; }
        public static ConfigEntry<bool> DiscCancel { get; private set; }
        public static ConfigEntry<bool> InertialBike { get; private set; }
        public static ConfigEntry<bool> InertialRolling { get; private set; }
        public static ConfigEntry<bool> AlwaysAccelerate { get; private set; }
        public static ConfigEntry<bool> HoldAttacks { get; private set; }
        public static ConfigEntry<bool> PounceCancel { get; private set; }


        public static ConfigEntry<bool> MillaRebalance { get; private set; }
        public static ConfigEntry<int> CubeLimit { get; private set; }

        public static ConfigEntry<bool> NeeraRebalance{ get; private set; }
        public static ConfigEntry<bool> TripleShoot { get; private set; }
        public static ConfigEntry<int> CrystalEnergyBonus { get; private set; }
        public static ConfigEntry<int> AttackEnergyBonus { get; private set; }
        public static ConfigEntry<int> GuardEnergyBonus { get; private set; }
        public static ConfigEntry<int> FocusGuardEnergyBonus { get; private set; }
        public static ManualLogSource MyLogger { get; private set; }

        public static Dictionary<SpriteId, Sprite> Sprites { get; } = new();
        public enum SpriteId
        {
            MillaSpecialItem = 0,
            NeeraSpecialItem = 1
        }

        public static Harmony HarmonyLink { get; } = new Harmony("com.micg.plugins.fp2.rebalance");

        private void Awake()
        {
            MyLogger = Logger;

            string path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            if (path != null)
            {
                LoadSprites(Path.Combine(path, "FP2Rebalance.assets"));
            }

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

            CarolRebalance = Config.Bind("Carol", "CarolRebalance", true, "Set if you want to enable Carol changes.");
            DiscCancel = Config.Bind("Carol", "DiscCancel", true, "Set if you want Disc Cancel.");
            InertialBike = Config.Bind("Carol", "InertialBike", true, "Set if you want inertial bike physics.");
            InertialRolling = Config.Bind("Carol", "InertialRolling", true, "Set if you want inertial rolling physics.");
            AlwaysAccelerate = Config.Bind("Carol", "AlwaysAccelerate", false, "Set if you want inertial bike physics without holding down.");
            HoldAttacks = Config.Bind("Carol", "AutoAttack", true, "Set if you want to hold attacks (from 1.8.0 only kicks up).");
            PounceCancel = Config.Bind("Carol", "PounceCancel", true, "Set if you want to cancel the pounce changing the direction as in the FP1.");

            MillaRebalance = Config.Bind("Milla", "MillaRebalance", true, "Set if you want to enable Milla changes.");
            CubeLimit = Config.Bind("Milla", "CubeLimit", 1, new ConfigDescription("Set base max number of cubes.", new AcceptableValueRange<int>(0, 100)));

            NeeraRebalance = Config.Bind("Neera", "NeeraRebalance", true, "Set if you want to enable Neera changes.");
            TripleShoot = Config.Bind("Neera", "TripleShoot", true, "Set if you want triple shoot.");
            CrystalEnergyBonus = Config.Bind("Neera", "CrystalEnergyBonus", 45, new ConfigDescription("Set multiplier for extra energy per crystal.", new AcceptableValueRange<int>(0, 100)));
            AttackEnergyBonus = Config.Bind("Neera", "AttackEnergyBonus", 10, new ConfigDescription("Set value for extra energy per attack.", new AcceptableValueRange<int>(0, 100)));
            GuardEnergyBonus = Config.Bind("Neera", "GuardEnergyBonus", 25, new ConfigDescription("Set value for extra energy per guard.", new AcceptableValueRange<int>(0, 100)));
            FocusGuardEnergyBonus = Config.Bind("Neera", "FocusGuardEnergyBonus", 40, new ConfigDescription("Set value for extra energy per guard in focus.", new AcceptableValueRange<int>(0, 100)));
        }

        private void LoadSprites(string assetBundlePath)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);
            Sprites.Add(SpriteId.MillaSpecialItem, LoadSprite(bundle, "assets/sprites/millaspecialitem.png"));
            Sprites.Add(SpriteId.NeeraSpecialItem, LoadSprite(bundle, "assets/sprites/neeraspecialitem.png"));
        }

        private static Sprite LoadSprite(AssetBundle bundle, string path)
        {
            try
            {
                var texture = (Texture2D)bundle.LoadAsset(path);
                return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), 
                    new Vector2(0.5f, 0.5f), 1f, 1, SpriteMeshType.Tight);
            }
            catch (Exception e)
            {
                MyLogger.LogError($"FP2Rebalance: Failed to load sprite {path} due to exception: {e.Message}");
                return null;
            }
        }
    }
}