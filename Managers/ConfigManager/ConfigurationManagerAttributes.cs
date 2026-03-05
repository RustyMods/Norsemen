using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace Norsemen;

#nullable enable
public class ConfigurationManagerAttributes
{
    [UsedImplicitly] public int? Order;
    [UsedImplicitly] public bool? Browsable;
    [UsedImplicitly] public string? Category;
    [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
}