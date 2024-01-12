using SDG.Unturned;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;
using Tavstal.TLibrary.Managers;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Compability.Hooks;
using Tavstal.TShop.Handlers;
using Math = System.Math;

namespace Tavstal.TShop
{
    public class TShop : PluginBase<TShopConfiguration>
    {
        public new static TShop Instance { get; private set; }
        public static DatabaseManager Database { get; private set; }
        public static IEconomyProvider economyProvider { get; private set; }
        public static bool IsConnectionAuthFailed { get; set; }
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

                UnturnedEventHandler.Attach();
               
                if (!Level.isLoaded || Level.isLoading)
                    Level.onPostLevelLoaded += Event_OnPluginsLoaded;
                else
                    Event_OnPluginsLoaded(0);

                if (IsConnectionAuthFailed)
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
            UnturnedEventHandler.Unattach();
            Level.onPostLevelLoaded -= Event_OnPluginsLoaded;
            Logger.Log("# TShop has been successfully unloaded.");
        }

        private void Event_OnPluginsLoaded(int i)
        {
            if (IsConnectionAuthFailed)
            {
                Logger.LogWarning($"# Unloading {GetPluginName()} due to database authentication error.");
                this?.UnloadPlugin();
                return;
            }

            Logger.LogLateInit();
            Logger.LogWarning("# Searching for economy plugin...");
            HookManager = new HookManager(this);
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

        private void Update()
        {
            try
            {
                if (IsConnectionAuthFailed)
                    return;

                if (_nextUpdate > DateTime.Now || !Config.EnableDiscounts)
                    return;

                List<ShopItem> products = Database.GetProducts();

                foreach (ShopItem item in products.FindAll(x => x.IsDiscounted))
                    Database.UpdateProduct(item.UnturnedId, item.IsVehicle, false, 0);

                if (products.Count > 2)
                    products.Shuffle();

                List<ShopItem> items = products.FindAll(x => !x.IsVehicle);
                List<ShopItem> vehs = products.FindAll(x => x.IsVehicle);

                for (int i = 0; i < Config.ItemCountToDiscount; i++)
                {
                    if (items.IsValidIndex(i))
                        Database.UpdateProduct(items[i].UnturnedId, false, true, Math.Round((decimal)MathHelper.Next(Config.minDiscount, Config.maxDiscount), 2));
                }

                for (int i = 0; i < Config.VehicleCountToDiscount; i++)
                {
                    if (vehs.IsValidIndex(i))
                        Database.UpdateProduct(vehs[i].UnturnedId, true, true, Math.Round((decimal)MathHelper.Next(Config.minDiscount, Config.maxDiscount), 2));
                }

                _nextUpdate = DateTime.Now.AddSeconds(Config.DiscountInterval);
            }
            catch (NullReferenceException nex)
            { 
                // Not logging because this error has a 99% chance is caused by load error
            }
        }

        public override Dictionary<string, string> LanguagePacks => new Dictionary<string, string>();

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
                { "success_command_page", "&6Next page: /shoplist {0} {1}." },
                { "error_command_page", "&6That page doesn't exist." },
                { "success_command_page_end", "&aYou have reached the end of the {0} shop list." },
                { "success_command_page_info", "- {0} (ID: {1}, buy price: {2}, sell price: {3} and Permission: {4})" },
                { "error_balance_not_enough", "&6You don't have enough money. You need {0} to be able to buy." },
                { "error_item_already_added", "&6The {0} (ID: {1}) is already added to the shop." },
                { "error_item_not_added", "&6This item isn't added to the shop. ({0})" },
                { "error_item_not_found", "&aYou have to provide a valid item id or name. ({0})" },
                { "error_item_not_enough", "&6You don't have enough items to sell." },
                { "error_shop_empty", "&6The shop is empty." },
                { "error_item_buy_error", "&6You can't buy this item." },
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
                { "success_item_buy", "&aYou have successfully bought {1}x {0} for {3}{2}." },
                { "success_item_sell", "&aYou have successfully sold {1}x {0} for {3}{2}." },
                { "success_item_cost", "&a- {0}'s buycost: {3}{1} sellcost: {3}{2}." },
                { "success_vehicle_buy", "&aYou have successfully bought a {0} for {2}{1}" },
                { "success_vehicle_sell", "&aYou have successfully sold your {0} for {2}{1}." },
                { "success_vehicle_cost", "&a- {0}'s buycost: {3}{1} sellcost: {3}{2}." },
                { "success_item_removed", "&aYou have successfully removed {0} from the item store." },
                { "success_item_added", "&aYou have successfully added {0} to the item store." },
                { "success_item_updated", "&aYou have successfully updated {0} in the item store." },
                { "success_vehicle_added", "&aYou have successfully added {0} to the vehicle store." },
                { "success_vehicle_removed", "&aYou have successfully removed {0} from the vehicle store." },
                { "success_vehicle_update", "&aYou have successfully updated {0} in the vehicle store." },
                { "success_migrate", "&aYou have successfully migrated the data of ZaupShop." },
                { "success_pruchase", "&aYou have successfully bought {0} item(s)." },
                { "success_sell", "&aYou have successfully sold {0} item(s)." },
                { "ui_product_free", "Free" },
                { "ui_product_notavailable", "N/A" },
                { "ui_product_buycost", "${0}" },
                { "ui_product_discount", "${0}" },
                { "ui_product_sellcost", "${0}" },
                { "ui_total_buy", "BUY SUBTOTAL: {1}{0}" },
                { "ui_total_sell", "SELL SUBTOTAL: {1}{0}" },
                { "ui_discount", "<color=red><size=8><i>{2}{0}</i></size></color> {2}{1}" },
                { "ui_shopname", "TShop" },
                { "ui_basket_contains_product_already", "The basket contains the '{0}' product already." },
                { "ui_basket_product_added", "The '{0}' product has been successfully added to your basket." },
                { "ui_basket_contains_vehicle_already", "You are not allowed to add multiple vehicles to your basket. (Abuse prevention)" },
                { "ui_basket_vehicle_quantity_change_prevent", "You are not allowed to change the quantity of the vehicle. (Abuse prevention)" },
                { "ui_error_vehicle_sell_null", "You have to get in a vehicle before trying to sell one." },
                { "ui_error_vehicle_sell_owner", "You are not the owner of this vehicle." },
                { "ui_error_vehicle_not_found", "Your current vehicle and the one in your basket does not match." },
                { "ui_error_vehicle_not_exists", "This vehicle does not exists." },
                { "ui_error_vehicle_buy_error", "You can't buy this vehicle." },
                { "ui_error_item_not_found", "Failed to get the item asset with '{0}' id." },
                { "ui_error_item_not_enough", "You don't have enough items to sell." },
                { "ui_error_item_sell_error", "You can't sell this item." },
                { "ui_error_item_buy_error", "You can't buy this item." },
                { "ui_error_balance_not_enough", "You don't have enough money. You need {0} to be able to buy." },
                { "ui_success_item_buy", "You have successfully bought {1}x {0} for {3}{2}" },
                { "ui_success_item_sell", "You have successfully sold {1}x {0} for {3}{2}." },
                { "ui_success_vehicle_sell", "You have successfully sold your {0} for {2}{1}." },
                { "ui_success_vehicle_buy", "You have successfully bought a {0} for {2}{1}" }
            };
    }
}
