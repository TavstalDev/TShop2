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
    public static class ShopHelper
    {
        public static async Task<bool> BuyItemAsync(UnturnedPlayer buyer, ushort itemId, decimal totalCost, byte amount = 1, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID buyerSteamId = buyer.CSteamID;

                await TShop.EconomyProvider.WithdrawAsync(buyerSteamId, totalCost);
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    var item = new Item(itemId, true);
                    for (int i = 0; i < amount; i++)
                    {
                        if (!buyer.Inventory.tryAddItem(item, false))
                            ItemManager.dropItem(item, buyer.Position, true, true, false);
                    }
                });


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
                TShop.Logger.LogException("Failed to buy item:");
                TShop.Logger.LogError(ex);
                return false;
            }
        }
        
        public static async Task<bool> SellItemAsync(UnturnedPlayer seller, ushort itemId, decimal totalCost, byte amount = 1, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID sellerSteamId = seller.CSteamID;
                
                List<InventorySearch> search = seller.Inventory.search(itemId, true, true);
                if (search.Count < amount)
                {
                    TShop.Instance.SendCommandReply(seller.SteamPlayer(), "error_item_not_enough");
                    return false;
                }
                
                await TShop.EconomyProvider.DepositAsync(sellerSteamId, totalCost);
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    seller.Player.equipment.dequip();
                    for (int i = 0; i < amount; i++)
                    {
                        seller.Inventory.removeItem(search[i].page,
                            seller.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                    }
                });

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
                TShop.Logger.LogException("Failed to sell item:");
                TShop.Logger.LogError(ex);
                return false;
            }
        }
        
        public static async Task<bool> BuyVehicleAsync(UnturnedPlayer buyer, ushort vehicleId, Color32? vehicleColor, decimal totalCost, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID buyerSteamId = buyer.CSteamID;
                
                await TShop.EconomyProvider.WithdrawAsync(buyerSteamId , totalCost);
                await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                {
                    InteractableVehicle vehicle = UnturnedHelper.SpawnOwnedVehicle(vehicleId, buyer);
                    if (vehicleColor.HasValue)
                        vehicle.ServerSetPaintColor(vehicleColor.Value);
                });

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
                TShop.Logger.LogException("Failed to buy vehicle:");
                TShop.Logger.LogError(ex);
                return false;
            }
        }
        
        public static async Task<bool> SellVehicleAsync(UnturnedPlayer seller, InteractableVehicle vehicle, decimal totalCost, EPaymentMethod paymentMethod = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                CSteamID sellerSteamId = seller.CSteamID;
                
                await MainThreadDispatcher.RunOnMainThreadAsync(() => VehicleManager.askVehicleDestroy(vehicle));
                await TShop.EconomyProvider.DepositAsync(sellerSteamId, totalCost);
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
                TShop.Logger.LogException("Failed to sell vehicle:");
                TShop.Logger.LogError(ex);
                return false;
            }
        }
    }
}