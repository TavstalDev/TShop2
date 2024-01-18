using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDG.Unturned;
using System;
using System.Linq;
using Tavstal.TShop.Compability;

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
