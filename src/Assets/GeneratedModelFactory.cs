using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwmarlyValheimPizzaMod.Assets
{
    /// <summary>
    /// Creates small low-poly item models at runtime so the mod does not require a
    /// Unity AssetBundle or proprietary source art to be functional.
    /// </summary>
    internal static class GeneratedModelFactory
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();

        internal static void ApplyIngredient(GameObject prefab, string itemId)
        {
            try
            {
                switch (itemId)
                {
                    case Items.ItemRegistry.PizzaDough:
                        ApplyDough(prefab);
                        break;
                    case Items.ItemRegistry.Tomato:
                        ApplyTomato(prefab);
                        break;
                    case Items.ItemRegistry.TomatoSeeds:
                        ApplySeeds(prefab);
                        break;
                    case Items.ItemRegistry.TomatoSauce:
                        ApplySauce(prefab);
                        break;
                    case Items.ItemRegistry.LoxMilker:
                        ApplyMilker(prefab);
                        break;
                    case Items.ItemRegistry.LoxMilk:
                        ApplyMilk(prefab);
                        break;
                    case Items.ItemRegistry.LoxCheese:
                        ApplyCheese(prefab);
                        break;
                }
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogWarning("Generated model for item '" + itemId + "' was skipped: " + exception.Message);
            }
        }

        internal static void ApplyPizza(GameObject prefab, string pizzaId, bool cooked)
        {
            try
            {
                GameObject model = Begin(prefab, pizzaId + (cooked ? "CookedModel" : "UncookedModel"), 0.62f);
                Material crust = RequireMaterial(prefab, cooked ? new Color(0.45f, 0.19f, 0.07f) : new Color(0.72f, 0.48f, 0.24f));
                Material baseMaterial = RequireMaterial(prefab, cooked ? new Color(0.82f, 0.53f, 0.18f) : new Color(0.91f, 0.72f, 0.42f));
                Material sauce = RequireMaterial(prefab, cooked ? new Color(0.58f, 0.06f, 0.025f) : new Color(0.75f, 0.12f, 0.04f));
                Material cheese = RequireMaterial(prefab, cooked ? new Color(1f, 0.69f, 0.12f) : new Color(1f, 0.87f, 0.42f));

                AddPrimitive(model, PrimitiveType.Cylinder, "Crust", new Vector3(0f, 0.09f, 0f), new Vector3(1f, 0.10f, 1f), crust, Vector3.zero);
                AddPrimitive(model, PrimitiveType.Cylinder, "Dough", new Vector3(0f, 0.18f, 0f), new Vector3(0.91f, 0.055f, 0.91f), baseMaterial, Vector3.zero);
                AddPrimitive(model, PrimitiveType.Cylinder, "TomatoSauce", new Vector3(0f, 0.235f, 0f), new Vector3(0.82f, 0.025f, 0.82f), sauce, Vector3.zero);
                AddPrimitive(model, PrimitiveType.Cylinder, "Cheese", new Vector3(0f, 0.27f, 0f), new Vector3(0.78f, 0.024f, 0.78f), cheese, Vector3.zero);

                Color[] toppingColors = GetToppingColors(pizzaId, cooked);
                for (int index = 0; index < toppingColors.Length; index++)
                {
                    float angle = (Mathf.PI * 2f * index) / toppingColors.Length;
                    float radius = 0.47f - (index % 2) * 0.12f;
                    Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0.32f, Mathf.Sin(angle) * radius);
                    Material topping = RequireMaterial(prefab, toppingColors[index]);
                    AddPrimitive(model, PrimitiveType.Sphere, "Topping" + index, position, new Vector3(0.14f, 0.045f, 0.14f), topping, Vector3.zero);
                }

                Finish(prefab, model);
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogWarning("Generated model for pizza '" + pizzaId + (cooked ? "'" : " (uncooked)'") + " was skipped: " + exception.Message);
            }
        }

        private static void ApplyDough(GameObject prefab)
        {
            GameObject model = Begin(prefab, "PizzaDoughModel", 0.55f);
            Material dough = RequireMaterial(prefab, new Color(0.78f, 0.53f, 0.29f));
            Material scoring = RequireMaterial(prefab, new Color(0.49f, 0.25f, 0.10f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Dough", new Vector3(0f, 0.08f, 0f), new Vector3(0.95f, 0.16f, 0.95f), dough, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cube, "ScoreA", new Vector3(-0.18f, 0.245f, 0f), new Vector3(0.06f, 0.015f, 0.54f), scoring, new Vector3(0f, 0f, -25f));
            AddPrimitive(model, PrimitiveType.Cube, "ScoreB", new Vector3(0.18f, 0.245f, 0f), new Vector3(0.06f, 0.015f, 0.54f), scoring, new Vector3(0f, 0f, 25f));
            Finish(prefab, model);
        }

        private static void ApplyTomato(GameObject prefab)
        {
            GameObject model = Begin(prefab, "TomatoModel", 0.42f);
            Material red = RequireMaterial(prefab, new Color(0.82f, 0.08f, 0.035f));
            Material green = RequireMaterial(prefab, new Color(0.12f, 0.36f, 0.08f));
            AddPrimitive(model, PrimitiveType.Sphere, "Tomato", new Vector3(0f, 0.20f, 0f), new Vector3(0.82f, 0.72f, 0.82f), red, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "Calyx", new Vector3(0f, 0.53f, 0f), new Vector3(0.24f, 0.05f, 0.24f), green, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "Stem", new Vector3(0f, 0.62f, 0f), new Vector3(0.10f, 0.16f, 0.10f), green, Vector3.zero);
            Finish(prefab, model);
        }

        private static void ApplySeeds(GameObject prefab)
        {
            GameObject model = Begin(prefab, "TomatoSeedsModel", 0.46f);
            Material cloth = RequireMaterial(prefab, new Color(0.56f, 0.35f, 0.16f));
            Material tie = RequireMaterial(prefab, new Color(0.22f, 0.13f, 0.06f));
            Material seed = RequireMaterial(prefab, new Color(0.92f, 0.75f, 0.38f));
            AddPrimitive(model, PrimitiveType.Cube, "SeedPouch", new Vector3(0f, 0.20f, 0f), new Vector3(0.72f, 0.48f, 0.28f), cloth, new Vector3(0f, 0f, -8f));
            AddPrimitive(model, PrimitiveType.Cylinder, "PouchTie", new Vector3(0f, 0.46f, 0f), new Vector3(0.22f, 0.035f, 0.22f), tie, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Sphere, "Seed", new Vector3(-0.18f, 0.31f, -0.17f), new Vector3(0.07f, 0.035f, 0.07f), seed, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Sphere, "Seed2", new Vector3(0.02f, 0.33f, -0.17f), new Vector3(0.07f, 0.035f, 0.07f), seed, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Sphere, "Seed3", new Vector3(0.20f, 0.29f, -0.17f), new Vector3(0.07f, 0.035f, 0.07f), seed, Vector3.zero);
            Finish(prefab, model);
        }

        private static void ApplySauce(GameObject prefab)
        {
            GameObject model = Begin(prefab, "TomatoSauceModel", 0.44f);
            Material jar = RequireMaterial(prefab, new Color(0.52f, 0.08f, 0.035f));
            Material sauce = RequireMaterial(prefab, new Color(0.82f, 0.12f, 0.035f));
            Material lid = RequireMaterial(prefab, new Color(0.72f, 0.56f, 0.26f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Jar", new Vector3(0f, 0.23f, 0f), new Vector3(0.62f, 0.42f, 0.62f), jar, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "Sauce", new Vector3(0f, 0.28f, -0.16f), new Vector3(0.48f, 0.25f, 0.05f), sauce, new Vector3(90f, 0f, 0f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Lid", new Vector3(0f, 0.68f, 0f), new Vector3(0.67f, 0.08f, 0.67f), lid, Vector3.zero);
            Finish(prefab, model);
        }

        private static void ApplyMilker(GameObject prefab)
        {
            GameObject model = Begin(prefab, "LoxMilkerModel", 0.48f);
            Material wood = RequireMaterial(prefab, new Color(0.43f, 0.22f, 0.09f));
            Material iron = RequireMaterial(prefab, new Color(0.27f, 0.31f, 0.32f));
            Material leather = RequireMaterial(prefab, new Color(0.20f, 0.10f, 0.045f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Pail", new Vector3(0f, 0.22f, 0f), new Vector3(0.58f, 0.34f, 0.58f), iron, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "WoodenGrip", new Vector3(0f, 0.48f, 0f), new Vector3(0.21f, 0.24f, 0.21f), wood, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cube, "Handle", new Vector3(0f, 0.72f, 0f), new Vector3(0.08f, 0.38f, 0.08f), leather, new Vector3(0f, 0f, 25f));
            AddPrimitive(model, PrimitiveType.Sphere, "Nozzle", new Vector3(0f, 0.51f, 0.20f), new Vector3(0.12f, 0.10f, 0.12f), iron, Vector3.zero);
            Finish(prefab, model);
        }

        private static void ApplyMilk(GameObject prefab)
        {
            GameObject model = Begin(prefab, "LoxMilkModel", 0.45f);
            Material glass = RequireMaterial(prefab, new Color(0.84f, 0.92f, 0.94f));
            Material milk = RequireMaterial(prefab, new Color(1f, 0.98f, 0.86f));
            Material cork = RequireMaterial(prefab, new Color(0.46f, 0.27f, 0.11f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Bottle", new Vector3(0f, 0.25f, 0f), new Vector3(0.52f, 0.42f, 0.52f), glass, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "Milk", new Vector3(0f, 0.28f, -0.18f), new Vector3(0.39f, 0.30f, 0.035f), milk, new Vector3(90f, 0f, 0f));
            AddPrimitive(model, PrimitiveType.Cylinder, "Neck", new Vector3(0f, 0.70f, 0f), new Vector3(0.27f, 0.20f, 0.27f), glass, Vector3.zero);
            AddPrimitive(model, PrimitiveType.Cylinder, "Cork", new Vector3(0f, 0.89f, 0f), new Vector3(0.30f, 0.08f, 0.30f), cork, Vector3.zero);
            Finish(prefab, model);
        }

        private static void ApplyCheese(GameObject prefab)
        {
            GameObject model = Begin(prefab, "LoxCheeseModel", 0.48f);
            Material rind = RequireMaterial(prefab, new Color(0.58f, 0.31f, 0.07f));
            Material cheese = RequireMaterial(prefab, new Color(1f, 0.67f, 0.16f));
            AddMesh(model, "CheeseWedge", CreateWedgeMesh(), new Vector3(0f, 0.22f, 0f), new Vector3(0.82f, 0.62f, 0.72f), rind);
            AddPrimitive(model, PrimitiveType.Cube, "CheeseFace", new Vector3(0f, 0.25f, -0.37f), new Vector3(0.62f, 0.38f, 0.025f), cheese, Vector3.zero);
            Finish(prefab, model);
        }

        private static GameObject Begin(GameObject prefab, string name, float scale)
        {
            GameObject model = new GameObject(name);
            model.transform.SetParent(prefab.transform, false);
            model.transform.localScale = Vector3.one * scale;
            return model;
        }

        private static void Finish(GameObject prefab, GameObject model)
        {
            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.transform.IsChildOf(model.transform))
                {
                    renderer.enabled = false;
                }
            }

            PizzaPlugin.Log.LogInfo("Applied generated low-poly model '" + model.name + "'.");
        }

        private static GameObject AddPrimitive(GameObject parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent.transform, false);
            primitive.transform.localPosition = position;
            primitive.transform.localRotation = Quaternion.Euler(rotation);
            primitive.transform.localScale = scale;

            foreach (Collider collider in primitive.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer? renderer = primitive.GetComponent<Renderer>();
            if (renderer == null)
            {
                throw new InvalidOperationException("Generated primitive '" + name + "' has no renderer.");
            }

            renderer.sharedMaterial = material;
            return primitive;
        }

        private static GameObject AddMesh(GameObject parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Material material)
        {
            GameObject model = new GameObject(name);
            model.transform.SetParent(parent.transform, false);
            model.transform.localPosition = position;
            model.transform.localScale = scale;
            MeshFilter filter = model.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = model.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return model;
        }

        private static Material RequireMaterial(GameObject prefab, Color color)
        {
            if (Materials.TryGetValue(color, out Material? cached) && cached)
            {
                return cached;
            }

            Shader? shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Legacy Shaders/Diffuse");
            Material? material = shader == null ? CloneTemplateMaterial(prefab) : new Material(shader);
            if (material == null)
            {
                throw new InvalidOperationException("No compatible Unity material or shader was available.");
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            Materials[color] = material;
            return material;
        }

        private static Material? CloneTemplateMaterial(GameObject prefab)
        {
            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.sharedMaterial != null)
                {
                    return new Material(renderer.sharedMaterial);
                }
            }

            return null;
        }

        private static Color[] GetToppingColors(string pizzaId, bool cooked)
        {
            Color mushroom = cooked ? new Color(0.43f, 0.23f, 0.10f) : new Color(0.70f, 0.48f, 0.27f);
            Color onion = cooked ? new Color(0.72f, 0.34f, 0.56f) : new Color(0.88f, 0.64f, 0.80f);
            Color green = cooked ? new Color(0.18f, 0.42f, 0.10f) : new Color(0.35f, 0.62f, 0.18f);
            Color meat = cooked ? new Color(0.46f, 0.11f, 0.045f) : new Color(0.66f, 0.28f, 0.10f);
            Color lox = cooked ? new Color(0.88f, 0.29f, 0.12f) : new Color(0.97f, 0.50f, 0.22f);
            Color berry = new Color(0.28f, 0.10f, 0.40f);
            Color mist = new Color(0.34f, 0.16f, 0.56f);
            Color ash = new Color(0.16f, 0.19f, 0.08f);

            switch (pizzaId)
            {
                case "Mushroom": return new[] { mushroom, mushroom, mushroom, cheeseColor(cooked) };
                case "Onion": return new[] { onion, onion, onion, cheeseColor(cooked) };
                case "Veggie": return new[] { green, new Color(0.90f, 0.40f, 0.08f), onion, mushroom, green, onion };
                case "Hunters": return new[] { meat, new Color(0.30f, 0.16f, 0.08f), mushroom, meat, mushroom };
                case "Sausage": return new[] { meat, meat, onion, meat, onion };
                case "Wolf": return new[] { new Color(0.17f, 0.12f, 0.10f), meat, onion, meat, onion };
                case "Lox": return new[] { lox, lox, lox, lox, cheeseColor(cooked) };
                case "LoxCloudberry": return new[] { lox, lox, berry, berry, lox };
                case "Serpent": return new[] { new Color(0.08f, 0.33f, 0.32f), meat, onion, new Color(0.08f, 0.33f, 0.32f) };
                case "Fisherman": return new[] { new Color(0.18f, 0.42f, 0.60f), onion, new Color(0.18f, 0.42f, 0.60f), onion };
                case "Mistlands": return new[] { mist, mist, new Color(0.76f, 0.68f, 0.18f), mist, new Color(0.76f, 0.68f, 0.18f) };
                case "Mage": return new[] { new Color(0.16f, 0.25f, 0.65f), mist, new Color(0.74f, 0.68f, 0.22f), mist, new Color(0.16f, 0.25f, 0.65f) };
                case "Ashlands": return new[] { meat, ash, new Color(0.82f, 0.21f, 0.04f), ash, meat };
                default: return new[] { cheeseColor(cooked), cheeseColor(cooked), cheeseColor(cooked) };
            }
        }

        private static Color cheeseColor(bool cooked)
        {
            return cooked ? new Color(1f, 0.69f, 0.12f) : new Color(1f, 0.87f, 0.42f);
        }

        private static Mesh CreateWedgeMesh()
        {
            Mesh mesh = new Mesh { name = "SwmarlyLoxCheeseWedge" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f)
            };
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 1, 3, 0, 3, 5, 0, 5, 4,
                4, 5, 7, 4, 7, 6, 2, 6, 7, 2, 7, 3,
                0, 4, 6, 0, 6, 2, 1, 7, 5, 1, 3, 7
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
