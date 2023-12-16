using HarmonyLib;

namespace FP2Rebalance
{
    public static class Milla
    {
        public static void Patch()
        {
            if (!Plugin.MillaRebalance.Value) return;
            Harmony harmony = Plugin.HarmonyLink;

            harmony.PatchAll(typeof(Action_MillaCubeSpawn));
        }

        [HarmonyPatch(typeof(FPPlayer), "Action_MillaCubeSpawn")]
        public class Action_MillaCubeSpawn
        {
            static bool Prefix(bool spawnWeakerCube = false)
            {
                FPPlayer fpPlayer = Patcher.GetPlayer;
                int cubeLimit = Plugin.CubeLimit.Value 
                    + (fpPlayer.hasSpecialItem ? 1 : 0) + (fpPlayer.shieldHealth > 0 ? 1 : 0);
                int num = Patcher.GetMillaCubeNumber();

                MillaMasterCube millaCubeCurrent = fpPlayer.GetMillaCubeCurrent();
                if (num < cubeLimit)
                {
                    var millaMasterCube = (MillaMasterCube)FPStage.CreateStageObject(
                        MillaMasterCube.classID, fpPlayer.position.x, fpPlayer.position.y);
                    
                    fpPlayer.AddMillaCube(millaMasterCube);
                    if (spawnWeakerCube)
                    {
                        millaMasterCube.SetEnergyReduced();
                    }
                    else
                    {
                        millaMasterCube.SetEnergyFull();
                    }
                }
                else
                {
                    millaCubeCurrent.SetEnergyFull();
                }
                fpPlayer.Action_PlaySoundUninterruptable(fpPlayer.sfxMillaCubeSpawn);
                return false;
            }
        }
    }
}
