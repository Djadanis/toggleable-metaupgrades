using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ToggleableMetaupgrades.Patches;

internal static class MetaUpgradeDependencies
{
    private static Dictionary<MetaUpgrade, MetaUpgrade[]> dependencies = new();

    [HarmonyPatch(typeof(ProgressManager), "Awake")]
    [HarmonyPostfix]
    internal static void Postfix()
    {
        GetDependencies();
    }

    private static void GetDependencies()
    {
        List<MetaUpgrade> metaUpgrades =
        [
            .. Resources.FindObjectsOfTypeAll<MetaUpgrade>(),
        ];

        foreach (var metaUpgrade in metaUpgrades)
        {
            dependencies[metaUpgrade] = metaUpgrades
                .Where(x => x.PrerequirementUpgrades
                    .Any(z => z.GetComponent<MetaUpgrade>().ID == metaUpgrade.ID))
                .ToArray();
        }

        UpgradeRefunds.dependenciesDict = dependencies;
    }
}