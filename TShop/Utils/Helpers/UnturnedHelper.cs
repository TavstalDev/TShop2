using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop.Helpers
{
    /// <summary>
    /// Provides helper methods for Unturned-related functionality.
    /// </summary>
    public static class UnturnedHelper
    {
        /// <summary>
        /// Retrieves the icon for the specified item ID.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <returns>
        /// The icon associated with the specified item ID.
        /// </returns>
        public static string GetItemIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
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

        /// <summary>
        /// Retrieves the icon for the specified vehicle ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle.</param>
        /// <returns>
        /// The icon associated with the specified vehicle ID.
        /// </returns>
        public static string GetVehicleIcon(ushort id)
        {
            string voidName = "GetIcon";
            try
            {
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

        /// <summary>
        /// Retrieves the items list in JSON format.
        /// </summary>
        /// <returns>
        /// A JSON string containing information about the items.
        /// </returns>
        public static string GetItemsInJson()
        {
            return JArray.FromObject(TShop.Database.GetItemsAsync()).ToString(Formatting.None);
        }

        /// <summary>
        /// Retrieves the vehicles list in JSON format.
        /// </summary>
        /// <returns>
        /// A JSON string containing information about the vehicles.
        /// </returns>
        public static string GetVehiclesInJson()
        {
            return JArray.FromObject(TShop.Database.GetVehiclesAsync()).ToString(Formatting.None);
        }
    }
}
