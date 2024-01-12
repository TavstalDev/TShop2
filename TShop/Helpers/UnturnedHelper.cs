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

using static SDG.Unturned.ItemCurrencyAsset;
using Tavstal.TShop.Helpers;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop.Helpers
{
    public class UnturnedHelper
    {
        private static TShop pluginMain => TShop.Instance;
        private static TShopConfiguration pluginConfig => TShop.Instance.Config;

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
                TShop.Logger.Log("Error in " + voidName + "(): ");
                TShop.Logger.Log(e);
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
                TShop.Logger.Log("Error in " + voidName + "(): ");
                TShop.Logger.Log(e);
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
