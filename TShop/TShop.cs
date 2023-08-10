using Rocket.API;
using Rocket.Core.Plugins;
using System;
using Rocket.API.Collections;
using UnityEngine;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System.Linq;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.Unturned;
using System.Collections.Generic;
using System.IO;
using Math = System.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Compability.Hooks;
using PlayerGesture =  Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture;
using SDG.Framework.Utilities;
using System.Reflection;
using Tavstal.TShop.Helpers;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;
using Tavstal.TLibrary.Managers;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary;

namespace Tavstal.TShop
{
    public class TShop : PluginBase<TShopConfiguration>
    {
        public new static TShop Instance { get; private set; }
        public static DatabaseManager Database { get; private set; }
        public static IEconomyProvider economyProvider { get; private set; }
        internal DateTime _nextUpdate { get; set; }

        public override void OnLoad()
        {
            Instance = this;
            
            Logger.LogWarning("████████╗░██████╗██╗░░██╗░█████╗░██████╗░");
            Logger.LogWarning("╚══██╔══╝██╔════╝██║░░██║██╔══██╗██╔══██╗");
            Logger.LogWarning("░░░██║░░░╚█████╗░███████║██║░░██║██████╔╝");
            Logger.LogWarning("░░░██║░░░░╚═══██╗██╔══██║██║░░██║██╔═══╝░");
            Logger.LogWarning("░░░██║░░░██████╔╝██║░░██║╚█████╔╝██║░░░░░");
            Logger.LogWarning("░░░╚═╝░░░╚═════╝░╚═╝░░╚═╝░╚════╝░╚═╝░░░░░");
            Logger.Log("#########################################");
            Logger.Log("# Thanks for using my plugin");
            Logger.Log("# Plugin Created By Tavstal");
            Logger.Log("# Discord: Tavstal#6189");
            Logger.Log("# Website: https://redstoneplugins.com");
            Logger.Log("# Discord: https://discord.gg/redstoneplugins");
            Logger.Log("#########################################");
            Logger.Log(string.Format("# Build Version: {0}", Version));
            Logger.Log(string.Format("# Build Date: {0}", BuildDate));
            Logger.Log("#########################################");
            try
            {
                Database = new DatabaseManager(Config);

                EffectManager.onEffectTextCommitted += Event_OnInputFieldEdit;
                EffectManager.onEffectButtonClicked += Event_OnButtonClick;
                U.Events.OnPlayerConnected += Event_OnPlayerJoin;
               
                if (!Level.isLoaded || Level.isLoading)
                    Level.onPostLevelLoaded += Event_OnPluginsLoaded;
                else
                    Event_OnPluginsLoaded(0);

                if (MySqlExtensions.IsConnectionAuthFailed)
                    return;

                Logger.Log("# TShop has been loaded.");
                Logger.Log("# Starting late initialization...");
            }
            catch (Exception ex)
            {
                Logger.LogException("# Failed to load TShop...");
                Logger.LogError(ex);
            }
        }

        public override void OnUnLoad()
        {
            EffectManager.onEffectTextCommitted -= Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked -= Event_OnButtonClick;
            U.Events.OnPlayerConnected -= Event_OnPlayerJoin;
            Level.onPostLevelLoaded -= Event_OnPluginsLoaded;
            Logger.Log("# TShop has been successfully unloaded.");
        }

        private void Event_OnPluginsLoaded(int i)
        {
            if (MySqlExtensions.IsConnectionAuthFailed)
            {
                Logger.LogWarning($"# Unloading {PluginName} due to database authentication error.");
                UnloadPlugin();
                return;
            }

            Logger.LogLateInit();
            Logger.LogWarning("# Searching for economy plugin...");
            HookManager = new HookManager();
            HookManager.LoadAll(Assembly);

            if (Config.ExpMode)
            {
                if (!HookManager.IsHookLoadable<ExpEconomyHook>())
                {
                    Logger.LogError("# Failed to load economy hook. Unloading TShop...");
                    this?.UnloadPlugin();
                    return;
                }
                economyProvider = HookManager.GetHook<ExpEconomyHook>();
            }
            else
            {
                if (HookManager.IsHookLoadable<TEconomyHook>())
                    economyProvider = HookManager.GetHook<TEconomyHook>();
                else
                {
                    if (!HookManager.IsHookLoadable<UconomyHook>())
                    {
                        Logger.LogError("# Failed to load economy hook. Unloading TShop...");
                        this?.UnloadPlugin();
                        return;
                    }
                    economyProvider = HookManager.GetHook<UconomyHook>();
                }
            }
        }

        private void Event_OnPlayerJoin(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(Config.EffectID, (short)Config.EffectID, player.SteamPlayer().transportConnection, true);
        }

        private void Event_OnInputFieldEdit(Player player, string button, string text)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();

            if (button.ContainsIgnoreCase("inputf_shop_cont_cart_item#") && button.ContainsIgnoreCase("_amount"))
            {
                int index = 6 * comp.cart_page + Convert.ToInt32(button.Replace("inputf_shop_cont_cart_item#", "").Replace("_amount", "")) - 1;

                if (comp.products.IsValidIndex(index))
                {
                    comp.products[index].Amount = MathHelper.Clamp(Convert.ToInt32(text), 1, comp.products[index].isVehicle ? 1 : 100);
                    UIManager.UpdateTotalPay(uPlayer);
                }
            }
        }

        private void Event_OnButtonClick(Player player, string button)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();

            if (button.EqualsIgnoreCase("bt_shop_cart_payment#wallet"))
            {
                comp.PaymentMethod = EPaymentMethod.WALLET;
            }
            else if (button.EqualsIgnoreCase("bt_shop_cart_payment#bank"))
            {
                comp.PaymentMethod = EPaymentMethod.BANK;
            }
            else if (button.EqualsIgnoreCase("bt_shop_cart_payment#crypto"))
            {
                comp.PaymentMethod = EPaymentMethod.CRYPTO;
            }
            else if (button.ContainsIgnoreCase("bt_shop_cont_cart_item#") && button.ContainsIgnoreCase("_remove"))
            {
                int index = 6 * comp.cart_page + Convert.ToInt32(button.Replace("bt_shop_cont_cart_item#", "").Replace("_remove", "")) - 1;

                if (comp.products.IsValidIndex(index))
                {
                    comp.products.RemoveAt(index);
                    UIManager.UpdatePaymentPage(uPlayer);
                    UIManager.UpdateTotalPay(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cart_buy"))
            {
                try
                {
                    if (comp.products.Count > 0)
                    {
                        decimal cost = 0;
                        int successBought = 0;
                        List<ShopItem> items = Database.GetItems();
                        List<ShopItem> vehs = Database.GetVehicles();
                        foreach (var p in comp.products)
                        {
                            if (p.isVehicle)
                                cost += vehs.Find(x => x.Id == p.Id).GetBuyCost(p.Amount);
                            else
                                cost += items.Find(x => x.Id == p.Id).GetBuyCost(p.Amount);
                        }

                        if (economyProvider.GetBalance(uPlayer, comp.PaymentMethod) < cost)
                        {
                            UnturnedHelper.SendChatMessage(uPlayer.SteamPlayer(), Localize(true, "error_balance_not_enough", cost - economyProvider.GetBalance(uPlayer, comp.PaymentMethod)));
                            return;
                        }

                        economyProvider.Withdraw(uPlayer.CSteamID, cost, comp.PaymentMethod);
                        foreach (var p in comp.products)
                        {
                            if (p.isVehicle)
                            {
                                VehicleManager.spawnLockedVehicleForPlayerV2(p.Id, uPlayer.Position + new Vector3(0, 0, 5), player.transform.rotation, player);
                            }
                            else
                            {
                                for (int i = 0; i < p.Amount; i++)
                                    if (!uPlayer.Inventory.tryAddItem(new Item(p.Id, true), false))
                                        ItemManager.dropItem(new Item(p.Id, true), uPlayer.Position, true, true, false);
                            }
                            successBought += p.Amount;
                        }
                        comp.products = new List<Product>();
                        economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.PURCHASE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                        UnturnedHelper.SendChatMessage(uPlayer.SteamPlayer(), Localize(true, "success_pruchase", successBought));
                        UIManager.UpdatePaymentPage(uPlayer);
                        UIManager.UpdateTotalPay(uPlayer);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in Button click():");
                    Logger.LogError(ex);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cart_sell"))
            {
                try
                {
                    if (comp.products.Count > 0)
                    {
                        decimal cost = 0;
                        List<ShopItem> items = Database.GetItems();
                        List<Product> soldProducts = new List<Product>();
                        int successSold = 0;

                        foreach (var p in comp.products)
                        {
                            ShopItem item = items.Find(x => x.Id == p.Id);
                            decimal localCost = 0;
                            if (!p.isVehicle && item.GetSellCost(p.Amount) > 0)
                                localCost = item.GetSellCost(p.Amount);
                            cost += localCost;

                            List<InventorySearch> search = uPlayer.Inventory.search(p.Id, true, true);
                            if (search.Count >= p.Amount)
                            {
                                for (int i = 0; i < p.Amount; i++)
                                {
                                    uPlayer.Inventory.removeItem(search[i].page, uPlayer.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                                }
                                economyProvider.Deposit(uPlayer.CSteamID, cost, comp.PaymentMethod);
                                soldProducts.Add(p);
                                successSold += p.Amount;
                            }
                        }

                        foreach (var p in soldProducts)
                            if (comp.products.Any(x=> x.Id == p.Id && p.isVehicle == x.isVehicle))
                                comp.products.Remove(p);
                        
                        economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.SALE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                        UnturnedHelper.SendChatMessage(uPlayer.SteamPlayer(), Localize(true, "success_sell", successSold));
                        UIManager.UpdatePaymentPage(uPlayer);
                        UIManager.UpdateTotalPay(uPlayer);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in Button click():");
                    Logger.LogError(ex);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_cart_prev"))
            {
                if (comp.cart_page - 1 >= 0)
                {
                    comp.cart_page -= 1;
                    UIManager.UpdatePaymentPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_cart_next"))
            {
                if (comp.cart_page + 1 <= comp.products.Count / 6)
                {
                    comp.cart_page += 1;
                    UIManager.UpdatePaymentPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_vehicles_prev"))
            {
                if (comp.vehicle_page - 1 >= 0)
                {
                    comp.vehicle_page -= 1;
                    UIManager.UpdateVehiclessPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_vehicles_next"))
            {
                List<ShopItem> products = Database.GetItems().FindAll(x => !comp.products.Any(y => y.Id == x.Id && y.isVehicle));

                if (comp.vehicle_page + 1 <= products.Count / 8)
                {
                    comp.vehicle_page += 1;
                    UIManager.UpdateVehiclessPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_items_prev"))
            {
                if (comp.item_page - 1 >= 0)
                {
                    comp.item_page -= 1;
                    UIManager.UpdateItemsPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_cont_items_next"))
            {
                List<ShopItem> products = Database.GetItems().FindAll(x => !comp.products.Any(y => y.Id == x.Id && !y.isVehicle));

                if (comp.item_page + 1 <= products.Count / 8)
                {
                    comp.item_page += 1;
                    UIManager.UpdateItemsPage(uPlayer);
                }
            }
            else if (button.EqualsIgnoreCase("bt_shop_menu#logout"))
            {
                player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, uPlayer.SteamPlayer().transportConnection, true, "Panel_Shop", false);
            }
            else if (button.EqualsIgnoreCase("bt_shop_menu#items"))
            {
                UIManager.UpdateItemsPage(uPlayer);
            }
            else if (button.EqualsIgnoreCase("bt_shop_menu#vehicles"))
            {
                UIManager.UpdateVehiclessPage(uPlayer);
            }
            else if (button.EqualsIgnoreCase("bt_shop_menu#cart"))
            {
                UIManager.UpdateTotalPay(uPlayer);
                UIManager.UpdatePaymentPage(uPlayer);
            }
            else if (button.ContainsIgnoreCase("bt_shop_item#") && button.ContainsIgnoreCase("_addcart"))
            {
                int index = 8 * comp.cart_page + Convert.ToInt32(button.Replace("bt_shop_item#", "").Replace("_addcart", "")) - 1;
                List<ShopItem> items = Database.GetItems().FindAll(x => !comp.products.Any(y => y.Id == x.Id && !y.isVehicle));

                if (items.IsValidIndex(index))
                {
                    comp.products.Add(new Product(items[index].Id, 1, false));
                    UIManager.UpdateItemsPage(uPlayer);
                }
            }
            else if (button.ContainsIgnoreCase("bt_shop_vehicles#") && button.ContainsIgnoreCase("_addcart"))
            {
                int index = 8 * comp.vehicle_page + Convert.ToInt32(button.Replace("bt_shop_vehicles#", "").Replace("_addcart", "")) - 1;
                List<ShopItem> items = Database.GetVehicles().FindAll(x => !comp.products.Any(y => y.Id == x.Id && y.isVehicle));

                if (items.IsValidIndex(index))
                {
                    comp.products.Add(new Product(items[index].Id, 1, true));
                    UIManager.UpdateVehiclessPage(uPlayer);
                }
            }
        }

        private void Update()
        {
            if (MySqlExtensions.IsConnectionAuthFailed)
                return;

            if (_nextUpdate > DateTime.Now || !Config.EnableDiscounts)
                return;

            List<ShopItem> items = Database.GetItems();
            List<ShopItem> vehs = Database.GetVehicles();

            foreach (ShopItem item in items.FindAll(x => x.isDiscounted))
                Database.UpdateItem(item.Id, false, 0);

            foreach (ShopItem item in vehs.FindAll(x => x.isDiscounted))
                Database.UpdateItem(item.Id, false, 0);

            if (items.Count > 2)
                items.Shuffle();
            if (items.Count > 2)
                vehs.Shuffle();

            for (int i = 0; i < Config.ItemCountToDiscount; i++)
            {
                if (items.IsValidIndex(i))
                    Database.UpdateItem(items[i].Id, true, Math.Round((decimal)MathHelper.Next(Config.minDiscount, Config.maxDiscount), 2));
            }

            for (int i = 0; i < Config.VehicleCountToDiscount; i++)
            {
                if (vehs.IsValidIndex(i))
                    Database.UpdateVehicle(vehs[i].Id, true, Math.Round((decimal)MathHelper.Next(Config.minDiscount, Config.maxDiscount), 2));
            }

            _nextUpdate = DateTime.Now.AddSeconds(Config.DiscountInterval);
        }

        public override Dictionary<string, string> DefaultLocalization =>
            new Dictionary<string, string>
            {
                { "prefix", "&e[TShop] " },
                { "error_command_buyitem_args", "&aUsage: /buy [Item Id | Name] <Amount>" },
                { "error_command_costitem_args", "&aUsage: /cost [Item Id | Name]" },
                { "error_command_sellitem_args", "&aUsage: /sell [Item Id | Name] <Amount>" },
                { "error_command_buyvehicle_args", "&aUsage: /buyv [Vehicle Id | Name]" },
                { "error_command_costvehicle_args", "&aUsage: /costv [Vehicle Id | Name]" },
                { "error_command_sellvehicle_args", "&aUsage: /sellv [Current vehicle]" },
                { "error_command_migrate_args", "&aUsage: /mzdb [itemtablename] [vehicletablename]" },
                { "error_usage_list", "&aUsage: /shoplist <Page>" },
                { "error_command_itemshop_args", "&6Usage: /itemshop add [item name | id] [buycost] [sellcost] <permission> | remove  [item name | id] | update [item name | id] [buycost] [sellcost] <permission>" },
                { "error_command_itemshop_add_args", "&6Usage: /itemshop add [item name | id] [buycost] [sellcost] <permission>" },
                { "error_command_itemshop_update_args", "&6Usage: /itemshop update [item name | id] [buycost] [sellcost] <permission>" },
                { "error_command_vehicleshop_args", "&6Usage: /vshop add [vehicle name | id] [buycost] [sellcost] <permission> | remove  [vehicle name | id] | update [vehicle name | id] [buycost] [sellcost] <permission>" },
                { "error_command_vehicleshop_add_args", "&6Usage: /vshop add [vehicle name | id] [buycost] [sellcost] <permission>" },
                { "error_command_vehicleshop_update_args", "&6Usage: /vshop update [vehicle name | id] [buycost] [sellcost] <permission>" },
                { "success_command_page", "((color=orange))Next page: /shoplist {0} {1}." },
                { "error_command_page", "&6That page doesn't exist." },
                { "success_command_page_end", "&aYou have reached the end of the {0} shop list." },
                { "success_command_page_info", "- {0} (ID: {1}, buy price: {2}, sell price: {3} and Permission: {4})" },
                { "error_balance_not_enough", "&6You don't have enough money. You need {0} to be able to buy." },
                { "error_item_already_added", "&6The {0} (ID: {1}) is already added to the shop." },
                { "error_item_not_added", "&6This item isn't added to the shop. ({0})" },
                { "error_item_not_found", "&aYou have to add a valid item id or name. ({0})" },
                { "error_item_not_enough", "&6You don't have enough items to sell." },
                { "error_shop_empty", "&6The shop is empty" },
                { "error_item_buy_error", "&6You can't buy this item" },
                { "error_item_sell_error", "&6You can't sell this item" },
                { "error_item_added", "&6Failed to add {0} to the item store." },
                { "error_item_removed", "&6Failed to remove {0} from the item store." },
                { "error_item_updated", "&6Failed to update {0} in the item store." },
                { "error_no_permission", "&6You don't have enough permission to buy or sell that item." },
                { "error_vehicle_not_exists", "&6This vehicle does not exists." },
                { "error_vehicle_not_added", "&6This vehicle isn't added to the shop." },
                { "error_vehicle_buy_error", "&6You can't buy this vehicle." },
                { "error_vehicle_sell_error", "&6You can't sell this vehicle." },
                { "error_vehicle_sell_null", "&6You have to get in a vehicle before trying to sell one." },
                { "error_vehicle_sell_owner", "&6You are not the owner of this vehicle." },
                { "error_vehicle_already_added", "&6This vehicle has been already added to the vehicle shop." },
                { "error_vehicle_added", "&6Failed to add {0} to the vehicle store." },
                { "error_vehicle_removed", "&6Failed to remove {0} from the vehicle store." },
                { "error_vehicle_updated", "&6Failed to update {0} in the vehicle store." },
                { "error_migrate_console", "&6Failed to migrate, please check the console." },
                { "success_item_buy", "&aYou have successfully bought {1}x {0} for {3}{2}" },
                { "success_item_sell", "&aYou have successfully sold {1}x {0} for {3}{2}." },
                { "success_item_cost", "&a- {0}'s buycost: {3}{1} sellcost: {3}{2}" },
                { "success_vehicle_buy", "&aYou have successfully bought a {0} for {2}{1}" },
                { "success_vehicle_sell", "&aYou have successfully sold your {0} for {2}{1}." },
                { "success_vehicle_cost", "&a- {0}'s buycost: {3}{1} sellcost: {3}{2}" },
                { "success_item_removed", "&aYou have successfully removed {0} from the item store." },
                { "success_item_added", "&aYou have successfully added {0} to the item store." },
                { "success_item_updated", "&aYou have successfully updated {0} in the item store." },
                { "success_vehicle_added", "&aYou have successfully added {0} to the vehicle store." },
                { "success_vehicle_removed", "&aYou have successfully removed {0} from the vehicle store." },
                { "success_vehicle_update", "&aYou have successfully updated {0} in the vehicle store." },
                { "success_migrate", "&aYou have successfully migrated the data of ZaupShop." },
                { "success_pruchase", "&aYou have successfully bought {0} item(s)." },
                { "success_sell", "&aYou have successfully sold {0} item(s)." },
                { "ui_total_buy", "BUY SUBTOTAL: {1}{0}" },
                { "ui_total_sell", "SELL SUBTOTAL: {1}{0}" },
                { "ui_discount", "<color=red><size=8><i>{2}{0}</i></size></color> {2}{1}" },
                { "ui_shopname", "TShop" }
            };
    }
}
