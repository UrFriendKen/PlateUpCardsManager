using HarmonyLib;
using Kitchen;
using KitchenData;
using System;
using System.Reflection;

namespace KitchenCardsManager.Patches
{
    [HarmonyPatch]
    static class HandleNewParameterChange_Patch
    {
        public static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(HandleNewParameterChange), t => t.Name.Contains($"c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        [HarmonyPrefix]
        static void OriginalLambdaBody_Prefix(CNewParameterChange change)
        {
            Main.LogInfo("HandleNewParameterChange Patch");
            if (GameData.Main.TryGet(change.ID, out UnlockCard unlockCard) &&
                unlockCard.Effects.Count > change.Index &&
                unlockCard.Effects[change.Index] is ParameterEffect parameterEffect)
            {
                Main.LogInfo($"Handling Parameter Effect for {unlockCard.Name}:");
                if (parameterEffect.Parameters.MinimumGroupSize != 0)
                    Main.LogInfo($"\tMinimum Group Size = {parameterEffect.Parameters.MinimumGroupSize}");
                if (parameterEffect.Parameters.MaximumGroupSize != 0)
                    Main.LogInfo($"\tMaximum Group Size = {parameterEffect.Parameters.MaximumGroupSize}");
            }
        }
    }
}
