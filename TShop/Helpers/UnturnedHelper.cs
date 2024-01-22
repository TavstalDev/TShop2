using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Core.Logging;
using SDG.Unturned;
using System;
using System.Linq;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop.Helpers
{
    public class UnturnedHelper
    {
        public static string GetItemIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
                ItemAsset i = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                FileServerFolder GithubElement = TShop.Instance.Config.ItemFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (GithubElement == null)
                    return TShop.Instance.Config.DefaultProductIconUrl;
                string filename = id + ".png";
                return GithubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                TShop.Logger.Log("Error in " + voidName + "(): ");
                TShop.Logger.Log(e);
            }
            return TShop.Instance.Config.DefaultProductIconUrl;
        }

        public static string GetVehicleIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
                VehicleAsset i = Assets.find(EAssetType.VEHICLE, id) as VehicleAsset;
                FileServerFolder GithubElement = TShop.Instance.Config.VehicleFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (GithubElement == null)
                    return TShop.Instance.Config.DefaultProductIconUrl;
                string filename = id + ".png";
                return GithubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                TShop.Logger.Log("Error in " + voidName + "(): ");
                TShop.Logger.Log(e);
            }
            return TShop.Instance.Config.DefaultProductIconUrl;
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
