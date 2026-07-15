using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.Unturned;
using Tavstal.TLibrary.Helpers;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Components;
using Tavstal.TShop.Utils.Helpers;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Handlers.Buttons
{
    public static class BasketButtonHandler
    {
        public static bool Handle(UnturnedPlayer player, ITransportConnection transportConnection, ShopComponent component, string button)
        {
            switch (button)
            {
                case "bt_tshop_basket#page#prev":
                {
                    if (component.PageBasket > 1)
                        component.PageBasket--;
                    UIManager.UpdateBasketPage(player);
                    break;
                }
                case "bt_tshop_basket#page#next":
                {
                    component.PageBasket++;
                    UIManager.UpdateBasketPage(player);
                    break;
                }
                case "bt_tshop_basket#page#1":
                case "bt_tshop_basket#page#2":
                case "bt_tshop_basket#page#3":
                case "bt_tshop_basket#page#4":
                case "bt_tshop_basket#page#5":
                {
                    int btIndex = int.Parse(button.Replace("bt_tshop_basket#page#", "")) - 1;
                    int page = component.PageIndexes[2][btIndex];

                    if (page == -1)
                        break;
                    component.PageBasket = page;
                    UIManager.UpdateBasketPage(player);
                    break;
                }
                case "bt_tshop_basket#product#1#delete":
                case "bt_tshop_basket#product#2#delete":
                case "bt_tshop_basket#product#3#delete":
                case "bt_tshop_basket#product#4#delete":
                case "bt_tshop_basket#product#5#delete":
                case "bt_tshop_basket#product#6#delete":
                case "bt_tshop_basket#product#7#delete":
                case "bt_tshop_basket#product#8#delete":
                case "bt_tshop_basket#product#9#delete":
                case "bt_tshop_basket#product#10#delete":
                case "bt_tshop_basket#product#11#delete":
                case "bt_tshop_basket#product#12#delete":
                {
                    int buttonIndex =
                        Convert.ToInt32(button.Replace("bt_tshop_basket#product#", "")
                            .Replace("#delete", "")) -
                        1;

                    int elementIndex = (component.PageBasket - 1) * 12 + buttonIndex;
                    if (component.Basket.Count - 1 < elementIndex)
                        break;

                    var key = component.Basket.Keys.ElementAt(elementIndex);
                    component.Basket.TryRemove(key, out _);

                    UIManager.UpdateBasketPage(player);
                    break;
                }
                case "bt_tshop_basket#product#1#amount#up":
                case "bt_tshop_basket#product#2#amount#up":
                case "bt_tshop_basket#product#3#amount#up":
                case "bt_tshop_basket#product#4#amount#up":
                case "bt_tshop_basket#product#5#amount#up":
                case "bt_tshop_basket#product#6#amount#up":
                case "bt_tshop_basket#product#7#amount#up":
                case "bt_tshop_basket#product#8#amount#up":
                case "bt_tshop_basket#product#9#amount#up":
                case "bt_tshop_basket#product#10#amount#up":
                case "bt_tshop_basket#product#11#amount#up":
                case "bt_tshop_basket#product#12#amount#up":
                {
                    int buttonIndex =
                        Convert.ToInt32(button.Replace("bt_tshop_basket#product#", "")
                            .Replace("#amount#up", "")) - 1;

                    int elementIndex = (component.PageBasket - 1) * 12 + buttonIndex;
                    if (component.Basket.Count - 1 < elementIndex)
                        break;

                    var key = component.Basket.Keys.ElementAt(elementIndex);
                    if (key.IsVehicle)
                    {
                        component.Basket[key] = 1;
                        component.AddNotifyToQueue(
                            TShop.Instance.Localize("ui_basket_vehicle_quantity_change_prevent"));
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID,
                            player.SteamPlayer().transportConnection, true, button, "1");
                    }
                    else
                    {
                        int amount = component.Basket[key];
                        if (amount >= 100)
                            component.Basket[key] = 1;
                        else
                            component.Basket[key] += 1;
                    }

                    EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID,
                        player.SteamPlayer().transportConnection, true,
                        $"tb_tshop_basket#product#{buttonIndex + 1}#amount", component.Basket[key].ToString());
                    UIManager.UpdateBasketPayment(player);
                    break;
                }
                case "bt_tshop_basket#product#1#amount#down":
                case "bt_tshop_basket#product#2#amount#down":
                case "bt_tshop_basket#product#3#amount#down":
                case "bt_tshop_basket#product#4#amount#down":
                case "bt_tshop_basket#product#5#amount#down":
                case "bt_tshop_basket#product#6#amount#down":
                case "bt_tshop_basket#product#7#amount#down":
                case "bt_tshop_basket#product#8#amount#down":
                case "bt_tshop_basket#product#9#amount#down":
                case "bt_tshop_basket#product#10#amount#down":
                case "bt_tshop_basket#product#11#amount#down":
                case "bt_tshop_basket#product#12#amount#down":
                {
                    int buttonIndex =
                        Convert.ToInt32(button.Replace("bt_tshop_basket#product#", "")
                            .Replace("#amount#down", "")) -
                        1;

                    int elementIndex = (component.PageBasket - 1) * 12 + buttonIndex;
                    if (component.Basket.Count - 1 < elementIndex)
                        break;

                    var key = component.Basket.Keys.ElementAt(elementIndex);
                    if (key.IsVehicle)
                    {
                        component.Basket[key] = 1;
                        component.AddNotifyToQueue(
                            TShop.Instance.Localize("ui_basket_vehicle_quantity_change_prevent"));
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, transportConnection, true, button, "1");
                    }
                    else
                    {
                        int amount = component.Basket[key];
                        if (amount <= 1)
                            component.Basket[key] = 100;
                        else
                            component.Basket[key] -= 1;
                    }

                    EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID,
                        player.SteamPlayer().transportConnection, true,
                        $"tb_tshop_basket#product#{buttonIndex + 1}#amount", component.Basket[key].ToString());
                    UIManager.UpdateBasketPayment(player);
                    break;
                }
                case "bt_tshop_basket#buy":
                {
                    if (component.Basket.Count == 0)
                        break;
                    
                    Task.Run(async () =>
                    {
                        await LockHelper.WaitForLockAsync(player, ELockKind.UI);
                        if (component.Basket.Count == 0)
                            return;
                        
                        try
                        {
                            decimal totalCost = component.Basket.Keys.Where(x => x.BuyCost > 0).Sum(x => x.GetBuyCost(component.Basket[x]));
                            if (totalCost > 0)
                            {
                                if (await TShop.EconomyProvider.GetBalanceAsync(player.CSteamID) < totalCost)
                                {
                                    component.AddNotifyToQueue(TShop.Instance.Localize(
                                        "ui_error_balance_not_enough",
                                        totalCost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName()));
                                    return;
                                }

                                await TShop.EconomyProvider.WithdrawAsync(player.CSteamID, totalCost,
                                    component.PaymentMethod);
                                if (TShop.EconomyProvider.HasTransactionSystem())
                                {
                                    await TShop.EconomyProvider.AddTransactionAsync(
                                        player.CSteamID,
                                        new Transaction(
                                            Guid.NewGuid().ToString(),
                                            ETransaction.PURCHASE,
                                            component.PaymentMethod,
                                            TShop.Instance.Localize(true, "ui_shop_name"),
                                            player.CSteamID.m_SteamID,
                                            0,
                                            totalCost,
                                            DateTime.Now
                                        )
                                    );
                                }
                            }

                            await MainThreadDispatcher.RunAsync(() =>
                            {
                                foreach (var prod in component.Basket)
                                {
                                    if (prod.Key.IsVehicle)
                                    {
                                        VehicleAsset? vehicleAsset = UAssetHelper.FindVehicleAsset(prod.Key.UnturnedId);
                                        if (vehicleAsset == null)
                                        {
                                            TShop.Logger.Error(
                                                $"Vehicle asset with ID {prod.Key.UnturnedId} not found.");
                                            continue;
                                        }

                                        InteractableVehicle vehicle =
                                            UnturnedHelper.SpawnOwnedVehicle(vehicleAsset.id, player);
                                        var color = prod.Key.GetVehicleColor();
                                        if (vehicle && color != null)
                                            vehicle.ServerSetPaintColor(color.Value);

                                        continue;
                                    }


                                    ItemAsset? asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);
                                    if (asset == null)
                                    {
                                        component.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_found",
                                            prod.Key.UnturnedId));
                                        continue;
                                    }

                                    for (int i = 0; i < prod.Value; i++)
                                    {
                                        var item = new Item(asset.id, true);
                                        if (!player.Inventory.tryAddItem(item, false))
                                            ItemManager.dropItem(item, player.Position, true, true, false);
                                    }
                                }

                                component.Basket.Clear();
                                UIManager.UpdateBasketPage(player);
                                component.AddNotifyToQueue(TShop.Instance.Localize("ui_success_transaction"));
                            });
                        }
                        finally
                        {
                            LockHelper.ReleaseLock(player, ELockKind.UI);
                        }
                    });
                    break;
                }
                case "bt_tshop_basket#sell":
                {
                    if (component.Basket.Count == 0)
                        break;

                    BackgroundThreadDispatcher.Run(async () =>
                    {
                        try
                        {
                            await LockHelper.WaitForLockAsync(player, ELockKind.UI);
                            if (component.Basket.Count == 0)
                                return;

                            decimal totalCost = 0;
                            await MainThreadDispatcher.RunAsync(() =>
                            {
                                foreach (var prod in component.Basket)
                                {
                                    if (prod.Key.SellCost <= 0)
                                        continue;

                                    if (prod.Key.IsVehicle)
                                    {
                                        InteractableVehicle vehicle = player.CurrentVehicle;
                                        if (vehicle == null)
                                        {
                                            component.AddNotifyToQueue(
                                                TShop.Instance.Localize("ui_error_vehicle_sell_null"));
                                            continue;
                                        }

                                        if (vehicle.lockedOwner != player.CSteamID || !vehicle.isLocked ||
                                            vehicle.isDead)
                                        {
                                            component.AddNotifyToQueue(
                                                TShop.Instance.Localize("ui_error_vehicle_sell_owner"));
                                            continue;
                                        }

                                        if (vehicle.id != prod.Key.UnturnedId)
                                            continue;
                                        
                                        VehicleManager.askVehicleDestroy(vehicle);

                                        totalCost += TShop.Instance.Config.UseQuality
                                            ? prod.Key.GetSellCostByQuality(
                                                (byte)(vehicle.health / vehicle.asset.healthMax *
                                                       100))
                                            : prod.Key.GetSellCost();
                                        continue;
                                    }

                                    ItemAsset? asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);

                                    if (asset == null)
                                    {
                                        TShop.Logger.Error($"Item asset with ID {prod.Key.UnturnedId} not found.");
                                        continue;
                                    }

                                    List<InventorySearch> search =
                                        player.Inventory.Search(prod.Key.UnturnedId, true);
                                    if (search.Count < prod.Value)
                                    {
                                        component.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_enough"));
                                        continue;
                                    }

                                    decimal? cost = ShopHelper.RemoveAndGetCost(player, prod.Key, prod.Value);
                                    if (cost == null)
                                    {
                                        component.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_sell_error"));
                                        continue;
                                    }

                                    totalCost += cost.Value;
                                }
                            });

                            // Deposit the earnings into the seller's account
                            await TShop.EconomyProvider.DepositAsync(player.CSteamID, totalCost);

                            // Add a transaction record if the transaction system is enabled
                            if (TShop.EconomyProvider.HasTransactionSystem())
                            {
                                await TShop.EconomyProvider.AddTransactionAsync(
                                    player.CSteamID,
                                    new Transaction(
                                        Guid.NewGuid().ToString(),
                                        ETransaction.SALE,
                                        component.PaymentMethod,
                                        TShop.Instance.Localize(true, "ui_shop_name"),
                                        0,
                                        player.CSteamID.m_SteamID,
                                        totalCost,
                                        DateTime.Now
                                    )
                                );
                            }

                            await MainThreadDispatcher.RunAsync(() =>
                            {
                                component.Basket.Clear();
                                UIManager.UpdateBasketPage(player);
                                component.AddNotifyToQueue(TShop.Instance.Localize("ui_success_transaction"));
                            });
                        }
                        finally
                        {
                            LockHelper.ReleaseLock(player, ELockKind.UI);
                        }
                    });
                    break;
                }
                default:
                    return false;
            }

            return true;
        }
    }
}