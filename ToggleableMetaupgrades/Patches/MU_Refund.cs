using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ToggleableMetaupgrades.Patches;

internal static class UpgradeRefunds
{
    private static MetaUpgradeMenu menuInstance;

    internal static Dictionary<MetaUpgrade, MetaUpgrade[]> dependenciesDict;

    [HarmonyPatch(typeof(MetaUpgradeMenu), "Start")]
    [HarmonyPrefix]
    internal static void Prefix(ref MetaUpgradeMenu __instance)
    {
        menuInstance = __instance;
        menuInstance.UpgradeMenuList.OnItemSelectedDisabled = RefundMetaUpgrade;

    }

    internal static void RefundMetaUpgrade(MenuListItem selectedItem)
    {
        if (selectedItem == null)
        {
            return;
        }
        MetaUpgrade displayedUpgrade = ((MetaUpgradeItemDisplay)selectedItem).DisplayedUpgrade;
        if (displayedUpgrade == null)
        {
            return;
        }
        if (displayedUpgrade.UpgradeType != EMetaUpgradeType.PassiveEffect || !ProgressManager.Instance.HasMetaUpgrade(displayedUpgrade))
        {
            WwiseAudioController.Instance.PostWwiseEventGlobal("Play_SFX_menu_disabled");
            Plugin.Logger.LogInfo("INFO - Couldn't refund meta upgrade " + displayedUpgrade.GetName() + ".");
            return;
        }

        if (IsRequirementForOtherMetaUpgrades(displayedUpgrade))
        {
            RequirementPopup(displayedUpgrade);
            return;
        }

        InitiateRefund(displayedUpgrade);
    }

    private static bool IsRequirementForOtherMetaUpgrades(MetaUpgrade displayedUpgrade)
    {
        return dependenciesDict[displayedUpgrade].Where(x => ProgressManager.Instance.HasMetaUpgrade(x)).Count() > 0;
    }

    private static void RequirementPopup(MetaUpgrade upgrade)
    {
        menuInstance.UpgradeMenuList.SetLocked(true, false);

        PopupController.Instance.Open(Loca.TEXT_FORMAT("Failed to refund meta upgrade {0}. It might be because it is a requirement for another meta upgrade.", new object[]
            {
                upgrade.GetName()
            }), "", delegate ()
            {
                menuInstance.UpgradeMenuList.SetLocked(false, false);
            }, null, true, false, true, null, null, false, true, -1f);
    }

    private static void InitiateRefund(MetaUpgrade upgrade)
    {
        menuInstance.UpgradeMenuList.SetLocked(true, false);

        PopupController.Instance.Open("", Loca.TEXT_FORMAT("Do you want to refund {0}?", new object[]
            {
                ColorCodes.InfoText(upgrade.GetName())
            }), delegate ()
            {
                CompleteRefund(upgrade);
            }, delegate ()
            {
                menuInstance.UpgradeMenuList.SetLocked(false, false);
            }, true, false, true, null, null, false, true, -1f);
    }

    private static void CompleteRefund(MetaUpgrade upgrade)
    {
        Plugin.Logger.LogInfo("INFO - Refunded meta upgrade " + upgrade.GetName());
        if (upgrade.CostCurrency == ECollectibleLoot.Aether)
        {
            ProgressManager.Instance.SpendAetherCrystals(-upgrade.Cost);
        }
        else if (upgrade.CostCurrency == ECollectibleLoot.LurkerTeeth)
        {
            ProgressManager.Instance.SpendLurkerTeeth(-upgrade.Cost);
        }
        CompleteRefundMetaUpgrade(upgrade);

        int selectedIndex = (int)AccessTools.
            Field(typeof(MetaUpgradeMenu), "selectedIndex")
            .GetValue(menuInstance);
        AccessTools.
            Field(typeof(MetaUpgradeMenu), "oldIndex")
            .SetValue(menuInstance, selectedIndex);
        AccessTools.Method(typeof(MetaUpgradeMenu), "UpdateDisplays", [typeof(bool)]).Invoke(menuInstance, [false]);
        PopupController.Instance.Open("", Loca.TEXT_FORMAT("Refunded meta upgrade {0}", new object[]
            {
                ColorCodes.InfoText(upgrade.GetName())
            }), delegate ()
            {
                menuInstance.UpgradeMenuList.SetLocked(false, false);
            }, null, true, false, true, null, null, false, true, -1f);
        return;
    }

    private static void CompleteRefundMetaUpgrade(MetaUpgrade upgrade)
    {
        MetaUpgradeInstance metaUpgradeInstance = ProgressManager.Instance.UnlockedMetaUpgrades.Find((MetaUpgradeInstance instance) => instance.MetaUpgrade.ID == upgrade.ID);
        if (metaUpgradeInstance != null)
        {
            ProgressManager.Instance.UnlockedMetaUpgrades.Remove(metaUpgradeInstance);
        }
        else
        {
            Plugin.Logger.LogError("ERROR - Could not find a MetaUpgradeInstance to remove.");
        }
    }
}
