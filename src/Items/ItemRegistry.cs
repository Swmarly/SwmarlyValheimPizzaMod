using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SwmarlyValheimPizzaMod.Assets;
using UnityEngine;

namespace SwmarlyValheimPizzaMod.Items
{
    internal static class ItemRegistry
    {
        internal const string PizzaDough = "SwmarlyPizzaDough";
        internal const string Tomato = "SwmarlyTomato";
        internal const string TomatoSeeds = "SwmarlyTomatoSeeds";
        internal const string TomatoSauce = "SwmarlyTomatoSauce";
        internal const string LoxMilker = "SwmarlyLoxMilker";
        internal const string LoxMilk = "SwmarlyLoxMilk";
        internal const string LoxCheese = "SwmarlyLoxCheese";

        internal static GameObject TomatoPrefab { get; private set; } = null!;
        internal static GameObject TomatoSeedsPrefab { get; private set; } = null!;

        private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);

        internal static void Register(PizzaConfiguration configuration)
        {
            RequireBasePrefabs();
            RegisterDough(configuration);
            RegisterTomato(configuration);
            RegisterTomatoSeeds(configuration);
            RegisterTomatoSauce(configuration);
            RegisterLoxMilker();
            RegisterLoxMilk();
            RegisterLoxCheese(configuration);
        }

        private static void RequireBasePrefabs()
        {
            string[] required =
            {
                "BreadDough",
                "Carrot",
                "CarrotSeeds",
                "QueensJam",
                "Cultivator",
                "Honey",
                "LoxPie"
            };

            foreach (string prefabName in required)
            {
                AssetRegistry.Require(prefabName);
            }
        }

        internal static GameObject Get(string name)
        {
            return Prefabs[name];
        }

        private static void RegisterDough(PizzaConfiguration configuration)
        {
            AddItem(
                PizzaDough,
                "BreadDough",
                CreateItemConfig("$item_pizza_dough", "$item_pizza_dough_desc", "piece_preptable", Requirement("BarleyFlour", PizzaConfiguration.Positive(configuration.DoughFlourCost, 12))),
                item => MakeIngredient(item, 20));
        }

        private static void RegisterTomato(PizzaConfiguration configuration)
        {
            TomatoPrefab = AddItemWithoutRecipe(
                Tomato,
                "Carrot",
                item =>
                {
                    MakeIngredient(item, 50);
                    item.m_name = "$item_tomato";
                    item.m_description = "$item_tomato_desc";
                    // Tomatoes are obtained by harvesting the vanilla-style crop. A recipe is
                    // deliberately not added here, so farming remains the source of the item.
                });
        }

        private static void RegisterTomatoSeeds(PizzaConfiguration configuration)
        {
            ItemConfig seedConfig = CreateItemConfig(
                "$item_tomato_seeds",
                "$item_tomato_seeds_desc",
                "piece_preptable",
                Requirement("Carrot", 1),
                Requirement("Barley", 1));
            seedConfig.Amount = 2;

            TomatoSeedsPrefab = AddItem(
                TomatoSeeds,
                "CarrotSeeds",
                seedConfig,
                item => MakeIngredient(item, 50));
        }

        private static void RegisterTomatoSauce(PizzaConfiguration configuration)
        {
            AddItem(
                TomatoSauce,
                "QueensJam",
                CreateItemConfig(
                    "$item_tomato_sauce",
                    "$item_tomato_sauce_desc",
                    "piece_preptable",
                    Requirement(Tomato, PizzaConfiguration.Positive(configuration.SauceTomatoCost, 3)),
                    Requirement("Onion", PizzaConfiguration.Positive(configuration.SauceOnionCost, 1))),
                MakeNonFood);
        }

        private static void RegisterLoxMilker()
        {
            AddItem(
                LoxMilker,
                "Cultivator",
                CreateItemConfig(
                    "$item_lox_milker",
                    "$item_lox_milker_desc",
                    "forge",
                    Requirement("FineWood", 8),
                    Requirement("Iron", 4),
                    Requirement("LeatherScraps", 6)),
                item =>
                {
                    MakeNonFood(item);
                    item.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Tool;
                    item.m_shared.m_maxStackSize = 1;
                });
        }

        private static void RegisterLoxMilk()
        {
            AddItemWithoutRecipe(
                LoxMilk,
                "Honey",
                item =>
                {
                    MakeIngredient(item, 50);
                    item.m_name = "$item_lox_milk";
                    item.m_description = "$item_lox_milk_desc";
                });
        }

        private static void RegisterLoxCheese(PizzaConfiguration configuration)
        {
            AddItem(
                LoxCheese,
                "LoxPie",
                CreateItemConfig(
                    "$item_lox_cheese",
                    "$item_lox_cheese_desc",
                    "piece_preptable",
                    Requirement(LoxMilk, PizzaConfiguration.Positive(configuration.CheeseMilkCost, 2))),
                MakeNonFood);
        }

        private static GameObject AddItem(string name, string basePrefab, ItemConfig config, Action<ItemDrop.ItemData.SharedData> configure)
        {
            try
            {
                CustomItem customItem = new CustomItem(name, basePrefab, config);
                return RegisterItem(name, basePrefab, customItem, configure);
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogError("Could not register item " + name + ": " + exception.Message);
                throw;
            }
        }

        private static GameObject AddItemWithoutRecipe(string name, string basePrefab, Action<ItemDrop.ItemData.SharedData> configure)
        {
            try
            {
                CustomItem customItem = new CustomItem(name, basePrefab);
                return RegisterItem(name, basePrefab, customItem, configure);
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogError("Could not register item " + name + ": " + exception.Message);
                throw;
            }
        }

        private static GameObject RegisterItem(string name, string basePrefab, CustomItem customItem, Action<ItemDrop.ItemData.SharedData> configure)
        {
                if (!ItemManager.Instance.AddItem(customItem))
                {
                    throw new InvalidOperationException("Jotunn rejected the item registration.");
                }

                GameObject prefab = customItem.ItemPrefab;
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop == null)
                {
                    throw new InvalidOperationException("The registered prefab has no ItemDrop component.");
                }

                configure(drop.m_itemData.m_shared);
                Prefabs.Add(name, prefab);
                PizzaPlugin.Log.LogInfo("Registered item " + name + " from " + basePrefab + ".");
                return prefab;
        }

        private static ItemConfig CreateItemConfig(string name, string description, string? station, params RequirementConfig[] requirements)
        {
            ItemConfig config = new ItemConfig
            {
                Name = name,
                Description = description,
                CraftingStation = station
            };

            config.Requirements = requirements;
            return config;
        }

        private static RequirementConfig Requirement(string itemName, int amount)
        {
            return new RequirementConfig(itemName, amount, 0, true);
        }

        private static void MakeIngredient(ItemDrop.ItemData.SharedData shared, int maxStack)
        {
            MakeNonFood(shared);
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_maxStackSize = maxStack;
        }

        private static void MakeNonFood(ItemDrop.ItemData.SharedData shared)
        {
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_food = 0f;
            shared.m_foodStamina = 0f;
            shared.m_foodEitr = 0f;
            shared.m_foodRegen = 0f;
            shared.m_foodBurnTime = 0f;
            shared.m_consumeStatusEffect = null;
        }
    }
}
