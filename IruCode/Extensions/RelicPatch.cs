using HarmonyLib;
using Iru.IruCode.Cards.Ancient;
using Iru.IruCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Iru.IruCode.Extensions;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class IruTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is PhotonRifle)
        {
            __result = ModelDb.Relic<PhotonBuster>().ToMutable();
        }
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class IruArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<Cards.Basic.PhotonRifle>().Id] = ModelDb.Card<PhotonCannon>();
    }
}

[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
public static class IruDustyTomeSetupPatch
{
    [HarmonyPostfix]
    public static void Postfix(DustyTome __instance, Player player)
    {
        if (player.Character is not Character.Iru)
            return;

        __instance.AncientCard = ModelDb.Card<HyperBurst>().Id;
    }
}


[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
public static class IruDustyTomePatch
{
    [HarmonyPrefix]
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.Owner?.Character is not Character.Iru)
            return;
        
        __instance.AncientCard = ModelDb.Card<HyperBurst>().Id;
    }
}