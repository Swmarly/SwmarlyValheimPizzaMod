using System;
using System.Collections.Generic;
using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SwmarlyValheimPizzaMod.Assets;
using UnityEngine;

namespace SwmarlyValheimPizzaMod.Crops
{
    internal static class TomatoCropSystem
    {
        private const float GrowTimeSeconds = 1800f;

        internal static void Register(PizzaConfiguration configuration)
        {
            try
            {
                GameObject tomatoPlant = CreatePickablePlant("SwmarlyTomatoPlant", "Pickable_Carrot", ItemRegistry.TomatoPrefab, 1);
                GameObject seedPlant = CreatePickablePlant("SwmarlySeedTomatoPlant", "Pickable_SeedCarrot", ItemRegistry.TomatoSeedsPrefab, 2);

                RegisterSapling(
                    "sapling_tomato",
                    "sapling_carrot",
                    "$piece_sapling_tomato",
                    "$piece_sapling_tomato_desc",
                    ItemRegistry.TomatoSeeds,
                    tomatoPlant);

                RegisterSapling(
                    "sapling_seedtomato",
                    "sapling_seedcarrot",
                    "$piece_sapling_seedtomato",
                    "$piece_sapling_seedtomato_desc",
                    ItemRegistry.Tomato,
                    seedPlant);

                PizzaPlugin.Log.LogInfo("Registered tomato cultivator crops using vanilla Plant and Pickable components.");
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogError("Tomato crop registration failed: " + exception);
                throw;
            }
        }

        private static GameObject CreatePickablePlant(string name, string basePrefab, GameObject itemPrefab, int amount)
        {
            GameObject prefab = PrefabManager.Instance.CreateClonedPrefab(name, AssetRegistry.Require(basePrefab));
            if (prefab == null)
            {
                throw new InvalidOperationException("Vanilla crop prefab '" + basePrefab + "' was not found.");
            }

            Pickable pickable = prefab.GetComponent<Pickable>();
            if (pickable == null)
            {
                throw new InvalidOperationException("Vanilla crop prefab '" + basePrefab + "' has no Pickable component.");
            }

            SetField(pickable, "m_itemPrefab", itemPrefab);
            SetField(pickable, "m_amount", amount);
            PrefabManager.Instance.AddPrefab(prefab);
            return prefab;
        }

        private static void RegisterSapling(
            string name,
            string basePrefab,
            string displayName,
            string description,
            string seedRequirement,
            GameObject grownPrefab)
        {
            PieceConfig config = new PieceConfig
            {
                PieceTable = PieceTables.Cultivator,
                Name = displayName,
                Description = description,
                Requirements = new[] { new RequirementConfig(seedRequirement, 1, 0, true) }
            };

            CustomPiece customPiece = new CustomPiece(name, basePrefab, config);
            if (!PieceManager.Instance.AddPiece(customPiece))
            {
                throw new InvalidOperationException("Jotunn rejected cultivator piece '" + name + "'.");
            }

            Plant plant = customPiece.PiecePrefab.GetComponent<Plant>();
            if (plant == null)
            {
                throw new InvalidOperationException("Sapling '" + basePrefab + "' has no Plant component.");
            }

            SetField(plant, "m_name", displayName);
            SetField(plant, "m_growTime", GrowTimeSeconds);
            SetField(plant, "m_growRadius", 1f);
            SetField(plant, "m_needCultivatedGround", true);
            SetField(plant, "m_destroyIfCantGrow", true);
            SetGrownPrefabs(plant, grownPrefab);
        }

        private static void SetGrownPrefabs(Plant plant, GameObject grownPrefab)
        {
            FieldInfo? field = AccessTools.Field(typeof(Plant), "m_grownPrefabs");
            if (field == null)
            {
                throw new MissingFieldException(typeof(Plant).FullName, "m_grownPrefabs");
            }

            if (field.FieldType == typeof(GameObject[]))
            {
                field.SetValue(plant, new[] { grownPrefab });
            }
            else if (field.FieldType.IsAssignableFrom(typeof(List<GameObject>)))
            {
                field.SetValue(plant, new List<GameObject> { grownPrefab });
            }
            else if (field.FieldType.IsAssignableFrom(typeof(GameObject)))
            {
                field.SetValue(plant, grownPrefab);
            }
            else
            {
                throw new InvalidOperationException("Plant.m_grownPrefabs has unsupported type " + field.FieldType.FullName + ".");
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo? field = AccessTools.Field(target.GetType(), fieldName);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().FullName, fieldName);
            }

            field.SetValue(target, value);
        }
    }
}
