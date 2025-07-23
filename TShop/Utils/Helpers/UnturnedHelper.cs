using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TShop.Models;
using UnityEngine;

namespace Tavstal.TShop.Utils.Helpers
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
                FileServerFolder githubElement = TShop.Instance.Config.ItemFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (githubElement == null)
                    return TShop.Instance.Config.DefaultProductIconUrl;
                string filename = id + ".png";
                return githubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                TShop.Logger.LogException("Error in " + voidName + "(): ");
                TShop.Logger.LogError(e);
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
                FileServerFolder githubElement = TShop.Instance.Config.VehicleFolders.FirstOrDefault(x => x.MinItemID <= id && x.MaxItemID >= id);
                if (githubElement == null)
                    return TShop.Instance.Config.DefaultProductIconUrl;
                string filename = id + ".png";
                return githubElement.FolderLink + filename;
            }
            catch (Exception e)
            {
                TShop.Logger.LogException("Error in " + voidName + "(): ");
                TShop.Logger.LogError(e);
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
            return JArray.FromObject(TShop.DatabaseManager.GetItemsAsync()).ToString(Formatting.None);
        }

        /// <summary>
        /// Retrieves the vehicles list in JSON format.
        /// </summary>
        /// <returns>
        /// A JSON string containing information about the vehicles.
        /// </returns>
        public static string GetVehiclesInJson()
        {
            return JArray.FromObject(TShop.DatabaseManager.GetVehiclesAsync()).ToString(Formatting.None);
        }

        /// <summary>
        /// Retrieves the vehicle spawn modifier from the configuration.
        /// </summary>
        /// <returns>
        /// A Vector3 representing the vehicle spawn modifier.
        /// </returns>
        /// <remarks>
        /// If the vehicle spawn modifier cannot be retrieved from the configuration,
        /// a default value of (0, 5, 0) is returned.
        /// </remarks>
        /// <exception cref="Exception">
        /// Logs an error if there is an issue retrieving the vehicle spawn modifier.
        /// </exception>
        private static Vector3 GetVehicleSpawnModifier()
        {
            try
            {
                return TShop.Instance.Config.VehicleSpawnModifier.GetVector3();
            }
            catch (Exception ex)
            {
                TShop.Logger.LogError("Failed to get vehicle spawn modifier. Using default value...");
                TShop.Logger.LogError(ex);
            }

            return new Vector3(0, 5, 5);
        }

        /// <summary>
        /// Spawns a vehicle owned by the specified player at the player's current position,
        /// rotated to match the direction the player is looking.
        /// </summary>
        /// <param name="id">The ID of the vehicle to spawn.</param>
        /// <param name="owner">The player who will own the spawned vehicle.</param>
        /// <returns>
        /// The spawned <see cref="InteractableVehicle"/> instance.
        /// </returns>
        public static InteractableVehicle SpawnOwnedVehicle(ushort id, UnturnedPlayer owner)
        {
            Quaternion playerRotation = Quaternion.LookRotation(owner.Player.look.aim.forward);
            Vector3 spawnPosition =  owner.Position + (playerRotation * GetVehicleSpawnModifier());
            return VehicleManager.spawnLockedVehicleForPlayerV2(id, spawnPosition, owner.Player.transform.rotation, owner.Player);
        }
    }
}
