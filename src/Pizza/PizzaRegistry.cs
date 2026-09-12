using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SwmarlyValheimPizzaMod.Assets;
using SwmarlyValheimPizzaMod.Config;
using UnityEngine;

namespace SwmarlyValheimPizzaMod.Pizza
{
    internal static class PizzaRegistry
    {
        private sealed class PizzaDefinition
        {
            internal string Id = string.Empty;
            internal string Name = string.Empty;
            internal string Description = string.Empty;
            internal float Health;
            internal float Stamina;
            internal float Eitr;
            internal float Healing;
            internal float Duration;
            internal string[] Toppings = Array.Empty<string>();
        }

        private sealed class PizzaStats
        {
            internal PizzaFoodConfiguration Values = null!;
        }

        internal static void Register(PizzaConfiguration configuration)
        {
            AssetRegistry.Require("piece_preptable");
            AssetRegistry.Require("piece_oven");
            foreach (PizzaDefinition definition in Definitions())
            {
                try
                {
                    PizzaStats stats = BindStats(definition);
                    RegisterOne(definition, stats, configuration);
                }
                catch (Exception exception)
                {
                    PizzaPlugin.Log.LogError("Could not register pizza '" + definition.Id + "': " + exception);
                }
            }
        }

        private static void RegisterOne(PizzaDefinition definition, PizzaStats stats, PizzaConfiguration configuration)
        {
            PizzaLocalization.AddPizza(definition.Id, definition.Name, definition.Description);

            string uncookedName = definition.Id + "PizzaUncooked";
            string cookedName = definition.Id + "Pizza";
            List<RequirementConfig> ingredients = new List<RequirementConfig>
            {
                Requirement(ItemRegistry.PizzaDough, 1),
                Requirement(ItemRegistry.TomatoSauce, 1),
                Requirement(ItemRegistry.LoxCheese, 1)
            };

            foreach (string topping in definition.Toppings)
            {
                if (AssetRegistry.Find(topping) == null)
                {
                    throw new InvalidOperationException("Required topping prefab '" + topping + "' was not found.");
                }

                ingredients.Add(Requirement(topping, 1));
            }

            ItemConfig uncookedConfig = new ItemConfig
            {
                Name = "$item_" + definition.Id + "_uncooked",
                Description = "$item_" + definition.Id + "_uncooked_desc",
                CraftingStation = "piece_preptable",
                Requirements = ingredients.ToArray()
            };

            CustomItem uncooked = new CustomItem(uncookedName, "LoxPieUncooked", uncookedConfig);
            if (!ItemManager.Instance.AddItem(uncooked))
            {
                throw new InvalidOperationException("Jotunn rejected the uncooked pizza item.");
            }

            CustomItem cooked = new CustomItem(cookedName, "LoxPie");
            if (!ItemManager.Instance.AddItem(cooked))
            {
                throw new InvalidOperationException("Jotunn rejected the finished pizza item.");
            }

            ConfigureUncooked(uncooked.ItemPrefab);
            ConfigureFinished(cooked.ItemPrefab, definition, stats);
            RegisterStoneOvenConversion(uncookedName, cookedName, configuration.StoneOvenCookTimeSeconds.Value);

            PizzaPlugin.Log.LogInfo("Registered " + definition.Name + ", including Food Preparation Table recipe and Stone Oven conversion.");
        }

        private static void ConfigureUncooked(GameObject prefab)
        {
            ItemDrop? drop = prefab.GetComponent<ItemDrop>();
            if (drop == null)
            {
                throw new InvalidOperationException("Uncooked pizza prefab has no ItemDrop.");
            }

            ItemDrop.ItemData.SharedData shared = drop.m_itemData.m_shared;
            shared.m_food = 0f;
            shared.m_foodStamina = 0f;
            shared.m_foodEitr = 0f;
            shared.m_foodRegen = 0f;
            shared.m_foodBurnTime = 0f;
            shared.m_consumeStatusEffect = null;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
        }

        private static void ConfigureFinished(GameObject prefab, PizzaDefinition definition, PizzaStats stats)
        {
            ItemDrop? drop = prefab.GetComponent<ItemDrop>();
            if (drop == null)
            {
                throw new InvalidOperationException("Finished pizza prefab has no ItemDrop.");
            }

            ItemDrop.ItemData.SharedData shared = drop.m_itemData.m_shared;
            shared.m_name = "$item_" + definition.Id;
            shared.m_description = "$item_" + definition.Id + "_desc";
            shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            shared.m_consumeStatusEffect = null;
            shared.m_food = Math.Max(0f, stats.Values.Health.Value);
            shared.m_foodStamina = Math.Max(0f, stats.Values.Stamina.Value);
            shared.m_foodEitr = Math.Max(0f, stats.Values.Eitr.Value);
            shared.m_foodRegen = Math.Max(0f, stats.Values.Healing.Value);
            shared.m_foodBurnTime = Math.Max(1f, stats.Values.DurationSeconds.Value);
        }

        private static void RegisterStoneOvenConversion(string fromItem, string toItem, float configuredCookTime)
        {
            // Jotunn's conversion API requires a concrete duration. A zero config value
            // therefore falls back to the conservative vanilla pizza duration.
            float cookTime = configuredCookTime > 0f ? configuredCookTime : 10f;
            CookingConversionConfig conversion = new CookingConversionConfig
            {
                Station = "piece_oven",
                FromItem = fromItem,
                ToItem = toItem,
                CookTime = cookTime
            };

            if (!ItemManager.Instance.AddItemConversion(new CustomItemConversion(conversion)))
            {
                throw new InvalidOperationException("Jotunn rejected the Stone Oven conversion for " + fromItem + ".");
            }
        }

        private static PizzaStats BindStats(PizzaDefinition definition)
        {
            string section = "Pizza - " + definition.Name;
            return new PizzaStats
            {
                Values = new PizzaFoodConfiguration(PizzaPlugin.Config, section, definition.Health, definition.Stamina, definition.Eitr, definition.Healing, definition.Duration)
            };
        }

        private static RequirementConfig Requirement(string itemName, int amount)
        {
            return new RequirementConfig(itemName, amount, 0, true);
        }

        private static IEnumerable<PizzaDefinition> Definitions()
        {
            yield return Definition("Cheese", "Cheese Pizza", "A warm, balanced slice.", 55, 55, 0, 4, 1500);
            yield return Definition("Mushroom", "Mushroom Pizza", "Earthy and filling.", 45, 75, 0, 4, 1500, "Mushroom");
            yield return Definition("Onion", "Onion Pizza", "Sweet onion and bright tomato.", 40, 90, 0, 4, 1500, "Onion");
            yield return Definition("Veggie", "Veggie Pizza", "A garden on a crust.", 55, 105, 0, 5, 1800, "Carrot", "Turnip", "Onion", "Mushroom");
            yield return Definition("Hunters", "Hunter's Pizza", "A hunter's hearty reward.", 90, 65, 0, 5, 1800, "BoarMeat", "DeerMeat", "Mushroom");
            yield return Definition("Sausage", "Sausage Pizza", "A smoky, savory classic.", 100, 55, 0, 5, 1800, "Sausages", "Onion");
            yield return Definition("Wolf", "Wolf Pizza", "A cold-mountain feast.", 115, 55, 0, 6, 1800, "WolfMeat", "Onion");
            yield return Definition("Lox", "Lox Pizza", "A massive Plains-inspired meal.", 125, 45, 0, 6, 1800, "LoxMeat");
            yield return Definition("LoxCloudberry", "Lox & Cloudberry Pizza", "An ancient culinary disagreement, reborn in Valheim.", 105, 75, 0, 6, 1800, "LoxMeat", "Cloudberry");
            yield return Definition("Serpent", "Serpent Pizza", "A dangerous sea-born delicacy.", 135, 60, 0, 7, 1800, "SerpentMeat", "Onion");
            yield return Definition("Fisherman", "Fisherman's Pizza", "A fresh catch with a savory finish.", 75, 90, 0, 5, 1800, "FishRaw", "Onion");
            yield return Definition("Mistlands", "Mistlands Pizza", "Strange food from the mist.", 80, 55, 35, 5, 1800, "SeekerMeat", "JotunPuffs");
            yield return Definition("Mage", "Mage Pizza", "A slice for hungry spellcasters.", 45, 55, 75, 5, 1800, "Magecap", "JotunPuffs", "RoyalJelly");
            yield return Definition("Ashlands", "Ashlands Pizza", "A hot, high-end meal from the burning south.", 145, 70, 0, 7, 1800, "AsksvinMeat", "Fiddleheadfern");
        }

        private static PizzaDefinition Definition(string id, string name, string description, float health, float stamina, float eitr, float healing, float duration, params string[] toppings)
        {
            return new PizzaDefinition
            {
                Id = id,
                Name = name,
                Description = description,
                Health = health,
                Stamina = stamina,
                Eitr = eitr,
                Healing = healing,
                Duration = duration,
                Toppings = toppings
            };
        }
    }
}
