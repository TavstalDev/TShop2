using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using Tavstal.TLibrary;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Economy;
using UnityEngine;

namespace Tavstal.TShop.Utils.Helpers
{
    /// <summary>
    /// Provides helper methods for shop-related operations such as buying and selling items and vehicles.
    /// </summary>
    public static class ShopHelper
    {
        /// <summary>
        /// Handles the purchase of an item by a player.
        /// </summary>
        /// <param name="buyer">The player buying the item.</param>
        /// <param name="itemId">The ID of the item to be purchased.</param>
        /// <param name="totalCost">The total cost of the item.</param>
        /// <param name="amount">The quantity of the item to be purchased. Defaults to 1.</param>
        /// <param name="paymentMethod">The payment method used for the purchase. Defaults to BANK_ACCOUNT.</param>
        /// <returns>A task that resolves to true if the purchase was successful, otherwise false.</returns>
        public static async Task<bool> BuyItemAsync(UnturnedPlayer buyer, ushort itemId, decimal totalCost, byte amount = 1, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID buyerSteamId = buyer.CSteamID;

                // Withdraw the cost from the buyer's account
                await TShop.EconomyProvider.WithdrawAsync(buyerSteamId, totalCost);

                // Add the item to the buyer's inventory or drop it if inventory is full
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    var item = new Item(itemId, true);
                    for (int i = 0; i < amount; i++)
                    {
                        if (!buyer.Inventory.tryAddItem(item, false))
                            ItemManager.dropItem(item, buyer.Position, true, true, false);
                    }
                });

                // Add a transaction record if the transaction system is enabled
                if (!TShop.EconomyProvider.HasTransactionSystem())
                    return true;

                await TShop.EconomyProvider.AddTransactionAsync(
                    buyerSteamId,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.PURCHASE,
                        paymentMethod,
                        TShop.Instance.Localize(true, "ui_shopname"),
                        buyerSteamId.m_SteamID,
                        0,
                        totalCost,
                        DateTime.Now
                    )
                );
                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the purchase
                TShop.Logger.Exception("Failed to buy item:");
                TShop.Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Handles the sale of an item by a player.
        /// </summary>
        /// <param name="seller">The player selling the item.</param>
        /// <param name="itemId">The ID of the item to be sold.</param>
        /// <param name="totalCost">The total cost of the item.</param>
        /// <param name="amount">The quantity of the item to be sold. Defaults to 1.</param>
        /// <param name="paymentMethod">The payment method used for the sale. Defaults to BANK_ACCOUNT.</param>
        /// <returns>A task that resolves to true if the sale was successful, otherwise false.</returns>
        public static async Task<bool> SellItemAsync(UnturnedPlayer seller, ushort itemId, decimal totalCost, byte amount = 1, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID sellerSteamId = seller.CSteamID;

                // Search for the item in the seller's inventory
                List<InventorySearch> search = seller.Inventory.search(itemId, true, true);
                if (search.Count < amount)
                {
                    TShop.Instance.SendCommandReply(seller.SteamPlayer(), "error_item_not_enough");
                    return false;
                }

                // Deposit the earnings into the seller's account
                await TShop.EconomyProvider.DepositAsync(sellerSteamId, totalCost);

                // Remove the item from the seller's inventory
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    seller.Player.equipment.dequip();
                    for (int i = 0; i < amount; i++)
                    {
                        seller.Inventory.removeItem(search[i].page,
                            seller.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                    }
                });

                // Add a transaction record if the transaction system is enabled
                if (!TShop.EconomyProvider.HasTransactionSystem())
                    return true;

                await TShop.EconomyProvider.AddTransactionAsync(
                    sellerSteamId,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.SALE,
                        paymentMethod,
                        TShop.Instance.Localize(true, "ui_shopname"),
                        0,
                        sellerSteamId.m_SteamID,
                        totalCost,
                        DateTime.Now
                    )
                );
                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the sale
                TShop.Logger.Exception("Failed to sell item:");
                TShop.Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Handles the purchase of a vehicle by a player.
        /// </summary>
        /// <param name="buyer">The player buying the vehicle.</param>
        /// <param name="vehicleId">The ID of the vehicle to be purchased.</param>
        /// <param name="vehicleColor">The color of the vehicle. Optional.</param>
        /// <param name="totalCost">The total cost of the vehicle.</param>
        /// <param name="paymentMethod">The payment method used for the purchase. Defaults to BANK_ACCOUNT.</param>
        /// <returns>A task that resolves to true if the purchase was successful, otherwise false.</returns>
        public static async Task<bool> BuyVehicleAsync(UnturnedPlayer buyer, ushort vehicleId, Color32? vehicleColor, decimal totalCost, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID buyerSteamId = buyer.CSteamID;

                // Withdraw the cost from the buyer's account
                await TShop.EconomyProvider.WithdrawAsync(buyerSteamId, totalCost);

                // Spawn the vehicle and set its color if specified
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    InteractableVehicle vehicle = UnturnedHelper.SpawnOwnedVehicle(vehicleId, buyer);
                    if (vehicleColor.HasValue)
                        vehicle.ServerSetPaintColor(vehicleColor.Value);
                });

                // Add a transaction record if the transaction system is enabled
                if (!TShop.EconomyProvider.HasTransactionSystem())
                    return true;

                await TShop.EconomyProvider.AddTransactionAsync(
                    buyerSteamId,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.PURCHASE,
                        paymentMethod,
                        TShop.Instance.Localize(true, "ui_shopname"),
                        buyerSteamId.m_SteamID,
                        0,
                        totalCost,
                        DateTime.Now
                    )
                );
                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the purchase
                TShop.Logger.Exception("Failed to buy vehicle:");
                TShop.Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Handles the sale of a vehicle by a player.
        /// </summary>
        /// <param name="seller">The player selling the vehicle.</param>
        /// <param name="vehicle">The vehicle to be sold.</param>
        /// <param name="totalCost">The total cost of the vehicle.</param>
        /// <param name="paymentMethod">The payment method used for the sale. Defaults to BANK_ACCOUNT.</param>
        /// <returns>A task that resolves to true if the sale was successful, otherwise false.</returns>
        public static async Task<bool> SellVehicleAsync(UnturnedPlayer seller, InteractableVehicle vehicle, decimal totalCost, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID sellerSteamId = seller.CSteamID;

                // Destroy the vehicle
                await MainThreadDispatcher.RunOnMainThreadAsync(() => VehicleManager.askVehicleDestroy(vehicle));

                // Deposit the earnings into the seller's account
                await TShop.EconomyProvider.DepositAsync(sellerSteamId, totalCost);

                // Add a transaction record if the transaction system is enabled
                if (!TShop.EconomyProvider.HasTransactionSystem())
                    return true;

                await TShop.EconomyProvider.AddTransactionAsync(
                    sellerSteamId,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.SALE,
                        paymentMethod,
                        TShop.Instance.Localize(true, "ui_shopname"),
                        0,
                        sellerSteamId.m_SteamID,
                        totalCost,
                        DateTime.Now
                    )
                );
                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the sale
                TShop.Logger.Exception("Failed to sell vehicle:");
                TShop.Logger.Error(ex);
                return false;
            }
        }
    }
}