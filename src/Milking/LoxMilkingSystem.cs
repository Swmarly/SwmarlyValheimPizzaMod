using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace SwmarlyValheimPizzaMod.Milking
{
    internal static class LoxMilkingSystem
    {
        private const string CooldownKey = "swmarly_pizza_lox_milk_time";
        private static readonly HashSet<ZDOID> PendingRequests = new HashSet<ZDOID>();
        private static CustomRPC? MilkRpc;
        private static bool initialized;

        private enum Result : byte
        {
            Success,
            Cooldown,
            Untamed,
            InvalidTarget,
            TooFar,
            NoSpace,
            MissingTool
        }

        internal static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            new Harmony(PizzaPlugin.Guid + ".milking").PatchAll(typeof(LoxMilkingSystem).Assembly);
            MilkRpc = NetworkManager.Instance.AddRPC(PizzaPlugin.Guid + ".Milk", ServerReceive, ClientReceive);
            PizzaPlugin.Log.LogInfo("Lox milking initialized with server-side ZDO cooldown persistence.");
        }

        [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
        private static class TameableInteractPatch
        {
            private static bool Prefix(Tameable __instance, Humanoid character, bool hold, bool alt, ref bool __result)
            {
                if (hold || alt || !(character is Player player) || !IsHoldingMilker(player))
                {
                    return true;
                }

                RequestMilk(player, __instance.gameObject);
                __result = true;
                return false;
            }
        }

        private static bool IsHoldingMilker(Player player)
        {
            ItemDrop.ItemData? item = player.m_rightItem;
            return item != null && item.m_dropPrefab != null && item.m_dropPrefab.name == Items.ItemRegistry.LoxMilker;
        }

        private static void RequestMilk(Player player, GameObject target)
        {
            ZNetView? view = target.GetComponent<ZNetView>();
            if (view == null || view.GetZDO() == null || !view.IsValid())
            {
                Show("$pizza_milk_invalid_target");
                return;
            }

            ZDOID id = view.GetZDO().m_uid;
            if (!PendingRequests.Add(id))
            {
                return;
            }

            try
            {
                ZPackage package = new ZPackage();
                package.Write(id);
                MilkRpc!.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
            }
            catch (Exception exception)
            {
                PendingRequests.Remove(id);
                PizzaPlugin.Log.LogWarning("Could not send lox milking request: " + exception.Message);
            }
        }

        private static IEnumerator ServerReceive(long sender, ZPackage package)
        {
            if (ZNet.instance == null || !ZNet.instance.IsServer())
            {
                yield break;
            }

            Result result = Result.InvalidTarget;
            try
            {
                ZDOID targetId = package.ReadZDOID();
                ZNetPeer? peer = ZNet.instance.m_peers.FirstOrDefault(candidate => candidate.m_uid == sender);
                ZNetView? playerView = peer == null ? null : ZNetScene.instance.FindInstance(peer.m_characterID);
                Player? player = playerView == null ? null : playerView.GetComponent<Player>();
                ZNetView? targetView = ZNetScene.instance.FindInstance(targetId);
                Tameable? tameable = targetView == null ? null : targetView.GetComponent<Tameable>();

                if (player == null || tameable == null || !IsLox(targetView!.gameObject))
                {
                    result = Result.InvalidTarget;
                }
                else if (!IsHoldingMilker(player))
                {
                    result = Result.MissingTool;
                }
                else if (!tameable.m_tamed)
                {
                    result = Result.Untamed;
                }
                else if (Vector3.Distance(player.transform.position, tameable.transform.position) > 6f)
                {
                    result = Result.TooFar;
                }
                else
                {
                    ZDO zdo = targetView.GetZDO();
                    double now = ZNet.instance.GetTimeSeconds();
                    double lastMilk = zdo.GetDouble(CooldownKey, double.MinValue);
                    double cooldown = Math.Max(0f, PizzaPlugin.Configuration.MilkingCooldownMinutes.Value) * 75.0;

                    if (now - lastMilk < cooldown)
                    {
                        result = Result.Cooldown;
                    }
                    else if (!TryAddMilk(player, Math.Max(1, PizzaPlugin.Configuration.MilkYield.Value)))
                    {
                        result = Result.NoSpace;
                    }
                    else
                    {
                        zdo.Set(CooldownKey, now);
                        result = Result.Success;
                    }
                }
            }
            catch (Exception exception)
            {
                PizzaPlugin.Log.LogError("Lox milking request failed safely: " + exception);
                result = Result.InvalidTarget;
            }

            ZPackage response = new ZPackage();
            response.Write((byte)result);
            MilkRpc!.SendPackage(sender, response);
            yield break;
        }

        private static IEnumerator ClientReceive(long sender, ZPackage package)
        {
            Result result = (Result)package.ReadByte();
            PendingRequests.Clear();
            switch (result)
            {
                case Result.Success:
                    Show("$pizza_milk_success");
                    break;
                case Result.Cooldown:
                    Show("$pizza_milk_cooldown");
                    break;
                case Result.Untamed:
                    Show("$pizza_milk_untamed");
                    break;
                case Result.TooFar:
                    Show("$pizza_milk_too_far");
                    break;
                case Result.NoSpace:
                    Show("$pizza_milk_no_space");
                    break;
                default:
                    Show("$pizza_milk_invalid_target");
                    break;
            }

            yield break;
        }

        private static bool IsLox(GameObject target)
        {
            string name = target.name.Replace("(Clone)", string.Empty);
            return name.Equals("Lox", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryAddMilk(Player player, int amount)
        {
            Inventory inventory = player.GetInventory();
            MethodInfo[] methods = typeof(Inventory).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "AddItem")
                .OrderByDescending(method => method.GetParameters().Length)
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                object?[] arguments = BuildAddItemArguments(parameters, player, amount);
                if (arguments == null)
                {
                    continue;
                }

                try
                {
                    object? result = method.Invoke(inventory, arguments);
                    if (method.ReturnType == typeof(bool))
                    {
                        return result is bool success && success;
                    }

                    if (method.ReturnType == typeof(ItemDrop.ItemData))
                    {
                        return result != null;
                    }

                    return true;
                }
                catch (TargetInvocationException exception)
                {
                    PizzaPlugin.Log.LogWarning("Inventory.AddItem rejected Lox Milk: " + exception.InnerException?.Message);
                }
            }

            PizzaPlugin.Log.LogError("No compatible Inventory.AddItem overload was found for Valheim 1.0.");
            return false;
        }

        private static object?[]? BuildAddItemArguments(ParameterInfo[] parameters, Player player, int amount)
        {
            object?[] arguments = new object?[parameters.Length];
            int intIndex = 0;
            bool prefabAssigned = false;

            for (int index = 0; index < parameters.Length; index++)
            {
                Type type = parameters[index].ParameterType;
                if (type == typeof(string))
                {
                    arguments[index] = !prefabAssigned ? Items.ItemRegistry.LoxMilk : player.GetPlayerName();
                    prefabAssigned = true;
                }
                else if (type == typeof(int))
                {
                    arguments[index] = intIndex++ == 0 ? amount : intIndex == 2 ? 1 : 0;
                }
                else if (type == typeof(long))
                {
                    arguments[index] = player.GetPlayerID();
                }
                else if (type == typeof(bool))
                {
                    arguments[index] = false;
                }
                else if (type == typeof(GameObject))
                {
                    GameObject? prefab = ObjectDB.instance == null ? null : ObjectDB.instance.GetItemPrefab(Items.ItemRegistry.LoxMilk);
                    if (prefab == null)
                    {
                        return null;
                    }

                    arguments[index] = prefab;
                    prefabAssigned = true;
                }
                else if (type.IsEnum)
                {
                    arguments[index] = Activator.CreateInstance(type);
                }
                else
                {
                    return null;
                }
            }

            return prefabAssigned ? arguments : null;
        }

        private static void Show(string token)
        {
            if (MessageHud.instance != null)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, token, 0, null, false);
            }
        }
    }
}
