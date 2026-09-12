using System;
using BepInEx.Configuration;
using Jotunn.Utils;

namespace SwmarlyValheimPizzaMod.Config;

internal sealed class PizzaConfiguration
{
    public ConfigEntry<int> DoughFlourCost { get; }
    public ConfigEntry<int> SauceTomatoCost { get; }
    public ConfigEntry<int> SauceOnionCost { get; }
    public ConfigEntry<float> MilkingCooldownMinutes { get; }
    public ConfigEntry<int> MilkYield { get; }
    public ConfigEntry<int> CheeseMilkCost { get; }
    public ConfigEntry<float> StoneOvenCookTimeSeconds { get; }

    public PizzaConfiguration(ConfigFile config)
    {
        DoughFlourCost = config.Bind("Ingredients", "PizzaDoughBarleyFlour", 12, ServerDescription("Barley Flour required for one Pizza Dough."));
        SauceTomatoCost = config.Bind("Ingredients", "TomatoSauceTomatoes", 3, ServerDescription("Tomatoes required for one Tomato Sauce."));
        SauceOnionCost = config.Bind("Ingredients", "TomatoSauceOnion", 1, ServerDescription("Onion required for one Tomato Sauce."));
        CheeseMilkCost = config.Bind("Ingredients", "LoxCheeseMilk", 2, ServerDescription("Lox Milk required for one Lox Cheese."));

        MilkingCooldownMinutes = config.Bind("Lox Milking", "CooldownInGameMinutes", 25f, ServerDescription("Per-Lox cooldown in Valheim in-game minutes."));
        MilkYield = config.Bind("Lox Milking", "MilkYield", 2, ServerDescription("Lox Milk granted by a successful milking."));

        StoneOvenCookTimeSeconds = config.Bind("Cooking", "StoneOvenCookTimeSeconds", 10f, ServerDescription("Stone Oven cook time for pizzas. Set to 0 to use the safe fallback."));
    }

    public static int Positive(ConfigEntry<int> entry, int fallback) => Math.Max(1, entry.Value > 0 ? entry.Value : fallback);
    public static float NonNegative(ConfigEntry<float> entry, float fallback) => entry.Value >= 0 ? entry.Value : fallback;

    private static ConfigDescription ServerDescription(string description)
    {
        return new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdminOnly = true });
    }
}

internal sealed class PizzaFoodConfiguration
{
    public ConfigEntry<float> Health { get; }
    public ConfigEntry<float> Stamina { get; }
    public ConfigEntry<float> Eitr { get; }
    public ConfigEntry<float> Healing { get; }
    public ConfigEntry<float> DurationSeconds { get; }

    public PizzaFoodConfiguration(ConfigFile config, string section, float health, float stamina, float eitr, float healing, float durationSeconds)
    {
        Health = config.Bind(section, "Health", health, ServerDescription("Health restored by this pizza."));
        Stamina = config.Bind(section, "Stamina", stamina, ServerDescription("Stamina restored by this pizza."));
        Eitr = config.Bind(section, "Eitr", eitr, ServerDescription("Eitr restored by this pizza."));
        Healing = config.Bind(section, "Healing", healing, ServerDescription("Health regeneration per food tick."));
        DurationSeconds = config.Bind(section, "DurationSeconds", durationSeconds, ServerDescription("Food duration in seconds."));
    }

    private static ConfigDescription ServerDescription(string description)
    {
        return new ConfigDescription(description, null, new ConfigurationManagerAttributes { IsAdminOnly = true });
    }
}
