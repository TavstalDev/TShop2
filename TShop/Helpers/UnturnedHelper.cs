using System;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using UnityEngine;
using SDG.Unturned;
using System.Collections;
using Rocket.Core.Permissions;
using Rocket.API.Serialisation;
using Rocket.Core.Commands;
using System.Text.RegularExpressions;
using Rocket.Unturned.Events;
using System.IO;
using UnityEngine.Networking;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Random = System.Random;
using Tavstal.TShop.Compability;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using static SDG.Unturned.ItemCurrencyAsset;
using Tavstal.TShop.Helpers;

namespace Tavstal.TShop.Managers
{
    public class UnturnedHelper
    {
        private static Random random;
        private static object syncObj = new object();
        private static TShop pluginMain => TShop.Instance;
        private static TShopConfiguration pluginConfig => TShop.Instance.Configuration.Instance;

        public static int GenerateRandomNumber(int min, int max)
        {
            lock (syncObj)
            {
                if (random == null)
                    random = new Random(); // Or exception...
                return random.Next(min, max);
            }
        }

        public static int GenerateRandomNumber(int max)
        {
            lock (syncObj)
            {
                if (random == null)
                    random = new Random(); // Or exception...
                return random.Next(0, max);
            }
        }

        public static double GenerateRandomNumber(double min, double max)
        {
            lock (syncObj)
            {
                if (random == null)
                    random = new Random(); // Or exception...
                return (random.NextDouble() * Math.Abs(max - min)) + min;
            }
        }

        public static double GenerateRandomNumber(double max)
        {
            lock (syncObj)
            {
                if (random == null)
                    random = new Random(); // Or exception...
                return (random.NextDouble() * Math.Abs(max));
            }
        }

        private static string Translate(bool addPrefix, string key, params object[] args) => TShop.Instance.Translate(addPrefix, key, args);

        public static void ServerSendChatMessage(string text, string icon = null, SteamPlayer fromPlayer = null, SteamPlayer toPlayer = null, EChatMode mode = EChatMode.GLOBAL)
        => ChatManager.serverSendMessage(text, Color.white, fromPlayer, toPlayer, mode, icon, true);

        public static void SendCommandReply(object toPlayer, string translation, params object[] args)
        {
            string icon = "";
            if (toPlayer is SteamPlayer steamPlayer)
                ServerSendChatMessage(FormatHelper.FormatTextV2(Translate(true, translation, args)), icon, null, steamPlayer, EChatMode.GLOBAL);
            else
                LoggerHelper.LogRichCommand(Translate(false, translation, args));
        }

        public static void SendChatMessage(SteamPlayer toPlayer, string translation, params object[] args)
        {
            string icon = "";
            ServerSendChatMessage(FormatHelper.FormatTextV2(Translate(true, translation, args)), icon, null, toPlayer, EChatMode.GLOBAL);
        }

        public static void SendChatMessage(string translation, params object[] args)
        {
            string icon = "";
            ServerSendChatMessage(Translate(true, translation, args), icon, null, null, EChatMode.GLOBAL);
        }

        public static void SendChatMessageUntranslated(SteamPlayer toPlayer, string text)
        {
            string icon = "";
            ServerSendChatMessage(text, icon, null, toPlayer, EChatMode.GLOBAL);
        }

        public static List<ItemAsset> GetItemAssets()
        {
            Asset[] assets = null;
            List<ItemAsset> values = new List<ItemAsset>();

            assets = Assets.find(EAssetType.ITEM);

            foreach (Asset a in assets)
            {
                values.Add((ItemAsset)a);
            }
            return values;
        }

        public static List<VehicleAsset> GetVehicleAssets()
        {
            Asset[] assets = null;
            List<VehicleAsset> values = new List<VehicleAsset>();

            assets = Assets.find(EAssetType.VEHICLE);

            foreach (Asset a in assets)
            {
                values.Add((VehicleAsset)a);
            }
            return values;
        }

        public static ItemAsset FindItemAsset(string name)
        {
            ItemAsset asset = null;
            foreach (ItemAsset a in GetItemAssets())
            {
                if (a.itemName != null && a.itemName.Length > 0)
                {
                    if (a.itemName.ContainsIgnoreCase(name))
                    {
                        asset = a;
                        break;
                    }
                }
            }

            return asset;
        }

        public static VehicleAsset FindVehicleAsset(string name)
        {
            VehicleAsset asset = null;
            foreach (VehicleAsset a in GetVehicleAssets())
            {
                if (a.vehicleName != null && a.vehicleName.Length > 0)
                {
                    if (a.vehicleName.ContainsIgnoreCase(name))
                    {
                        asset = a;
                        break;
                    }
                }
            }

            return asset;
        }

        public static string GetItemIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
                ItemAsset i = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                GithubFolders GithubElement = pluginConfig.GithubItemFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (GithubElement == null)
                    return pluginConfig.DefaultProductIconUrl;
                string filename = id + ".png";
                return GithubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                Logger.Log("Error in " + voidName + "(): ");
                Logger.Log(e);
            }
            return pluginConfig.DefaultProductIconUrl;
        }

        public static string GetVehicleIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
                ItemAsset i = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                GithubFolders GithubElement = pluginConfig.GithubVehicleFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (GithubElement == null)
                    return pluginConfig.DefaultProductIconUrl;
                string filename = id + ".png";
                return GithubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                Logger.Log("Error in " + voidName + "(): ");
                Logger.Log(e);
            }
            return pluginConfig.DefaultProductIconUrl;
        }

        public static string GetItemsInJson()
        {
            return JArray.FromObject(TShop.Database.GetItems()).ToString(Formatting.None);
        }

        public static string GetVehiclesInJson()
        {
            return JArray.FromObject(TShop.Database.GetVehicles()).ToString(Formatting.None);
        }
    }
}
