using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

namespace SwmarlyValheimPizzaMod.Localization;

internal static class PizzaLocalization
{
    public static CustomLocalization Register()
    {
        CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
        localization.AddTranslation("English", new Dictionary<string, string>
        {
            ["item_pizza_dough"] = "Pizza Dough",
            ["item_pizza_dough_desc"] = "A soft dough base, ready for sauce and toppings.",
            ["item_tomato"] = "Tomato",
            ["item_tomato_desc"] = "A bright, juicy crop grown in cultivated soil.",
            ["item_tomato_seeds"] = "Tomato Seeds",
            ["item_tomato_seeds_desc"] = "Plant them with a Cultivator to grow tomatoes.",
            ["item_tomato_sauce"] = "Tomato Sauce",
            ["item_tomato_sauce_desc"] = "A rich sauce made from tomatoes and onion.",
            ["item_lox_milker"] = "Lox Milker",
            ["item_lox_milker_desc"] = "Use on a tamed Lox to collect its milk.",
            ["item_lox_milk"] = "Lox Milk",
            ["item_lox_milk_desc"] = "Rich milk collected from a tamed Lox.",
            ["item_lox_cheese"] = "Lox Cheese",
            ["item_lox_cheese_desc"] = "Dense cheese made from Lox Milk.",
            ["pizza_milk_success"] = "You milk the Lox.",
            ["pizza_milk_cooldown"] = "This Lox has already been milked recently.",
            ["pizza_milk_untamed"] = "The Lox must be tamed first.",
            ["pizza_milk_invalid_target"] = "The Lox Milker can only be used on a Lox.",
            ["pizza_milk_too_far"] = "You are too far away to milk that Lox.",
            ["pizza_milk_no_space"] = "You do not have room for Lox Milk.",
            ["pizza_oven_ready"] = "Ready for the Stone Oven.",
            ["piece_sapling_tomato"] = "Tomato",
            ["piece_sapling_tomato_desc"] = "Plant a tomato seed in cultivated soil.",
            ["piece_sapling_seedtomato"] = "Seed Tomato",
            ["piece_sapling_seedtomato_desc"] = "Grow this plant to harvest Tomato Seeds.",
        });
        return localization;
    }

    public static void AddPizza(string id, string displayName, string description)
    {
        PizzaPlugin.Localization.AddTranslation("English", new Dictionary<string, string>
        {
            [$"item_{id}_uncooked"] = $"Uncooked {displayName}",
            [$"item_{id}_uncooked_desc"] = "Prepared at the Food Preparation Table. Ready for the Stone Oven.",
            [$"item_{id}"] = displayName,
            [$"item_{id}_desc"] = description,
        });
    }
}
