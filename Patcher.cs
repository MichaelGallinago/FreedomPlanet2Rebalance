using System.Reflection;

namespace FP2Rebalance
{
    public class Patcher
    {
        private static FieldInfo GetPlayerField(string name)
        {
            return typeof(FPPlayer).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static void SetPlayerValue(string name, object value, object player = null)
        {
            GetPlayerField(name).SetValue(player, value);
        }

        public static object GetPlayerValue(string name, object player = null) => GetPlayerField(name).GetValue(player);

        public static FPPlayer GetPlayer => FPStage.currentStage.GetPlayerInstance_FPPlayer();

        public static int GetMillaCubeNumber()
        {
            var num = 0;
            FPPlayer fpPlayer = GetPlayer;
            FPBaseObject baseObject = null;
            while (FPStage.ForEach(MillaMasterCube.classID, ref baseObject))
            {
                if (((MillaMasterCube)baseObject).parentObject == fpPlayer) num++;
            }
            return num;
        }
    }
}
