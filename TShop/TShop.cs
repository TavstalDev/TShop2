using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Managers;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Hooks;
using Tavstal.TShop.Utils.Handlers;
using Tavstal.TShop.Utils.Managers;
using Math = System.Math;

namespace Tavstal.TShop
{
    /// <summary>
    /// Represents the TShop plugin with the specified configuration.
    /// </summary>
    /// <typeparam>The type of configuration used by TShop.</typeparam>
    // ReSharper disable once InconsistentNaming
    public class TShop : PluginBase<ShopConfiguration>
    {
        public static TShop Instance { get; private set; }
        public static DatabaseManager DatabaseManager { get; private set; }
        public static IEconomyProvider EconomyProvider { get; private set; }
        public static bool IsConnectionAuthFailed { get; set; }
        public static bool IsCleanupInProgress { get; set; }
        private bool _isLateInited;

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public override void OnLoad()
        {
            Instance = this;
            
            Logger.Log("████████╗░██████╗██╗░░██╗░█████╗░██████╗░", ConsoleColor.Cyan, prefix: null);
            Logger.Log("╚══██╔══╝██╔════╝██║░░██║██╔══██╗██╔══██╗", ConsoleColor.Cyan, prefix: null);
            Logger.Log("░░░██║░░░╚█████╗░███████║██║░░██║██████╔╝", ConsoleColor.Cyan, prefix: null);
            Logger.Log("░░░██║░░░░╚═══██╗██╔══██║██║░░██║██╔═══╝░", ConsoleColor.Cyan, prefix: null);
            Logger.Log("░░░██║░░░██████╔╝██║░░██║╚█████╔╝██║░░░░░", ConsoleColor.Cyan, prefix: null);
            Logger.Log("░░░╚═╝░░░╚═════╝░╚═╝░░╚═╝░╚════╝░╚═╝░░░░░", ConsoleColor.Cyan, prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("#       Thanks for using this plugin!   #", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("# Developed By: Tavstal", prefix: null);
            Logger.Log("# Discord:      @Tavstal", prefix: null);
            Logger.Log("# Website:      https://redstoneplugins.com", prefix: null);
            Logger.Log("# My GitHub:    https://tavstaldev.github.io", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log($"# Plugin Version:    {Version}", prefix: null);
            Logger.Log($"# Build Date:        {BuildDate}", prefix: null);
            Logger.Log($"# TLibrary Version:  {LibraryVersion}", prefix: null);
            Logger.Log("#########################################", prefix: null);
            Logger.Log("# Found an issue or have a suggestion?", prefix: null);
            Logger.Log("# Report it here: https://github.com/TavstalDev/TShop2/issues", prefix: null); 
            Logger.Log("#########################################", prefix: null);
            try
            {
                DatabaseManager = new DatabaseManager(Config);

                UnturnedEventHandler.Attach();
               
                if (!Level.isLoaded || Level.isLoading)
                    Level.onPostLevelLoaded += Event_OnPluginsLoaded;
                else
                    Event_OnPluginsLoaded(0);

                if (IsConnectionAuthFailed)
                    return;

                Logger.Log($"# {GetPluginName()} has been loaded.");
                Logger.Log("# Starting late initialization...");
            }
            catch (Exception ex)
            {
                Logger.Exception($"# Failed to load {GetPluginName()}...");
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Called when the plugin is unloaded.
        /// </summary>
        public override void OnUnLoad()
        {
            UnturnedEventHandler.Detach();
            Level.onPostLevelLoaded -= Event_OnPluginsLoaded;
            _isLateInited = false;
            foreach (SteamPlayer steamPlayer in Provider.clients)
            {
                UIManager.Hide(UnturnedPlayer.FromSteamPlayer(steamPlayer));
                EffectManager.askEffectClearByID(Config.EffectID, steamPlayer.transportConnection);
            }

            if (Config.EnableDiscounts)
                CancelInvoke(nameof(CheckDiscount));

            Logger.Log($"# {GetPluginName()} has been successfully unloaded.");
        }

        /// <summary>
        /// Event handler for when all plugins are loaded.
        /// </summary>
        private void Event_OnPluginsLoaded(int i)
        {
            if (IsConnectionAuthFailed)
            {
                Logger.Warning($"# Unloading {GetPluginName()} due to database authentication error.");
                this.UnloadPlugin();
                return;
            }

            Logger.LateInit();
            Logger.Warning("# Searching for economy plugin...");
            HookManager = new HookManager(this);
            HookManager.LoadAll(Assembly, true);

            if (Config.ExpMode)
            {
                if (!HookManager.IsHookLoadable<ExpEconomyHook>())
                {
                    Logger.Error("# Failed to load economy hook. Unloading TShop...");
                    this.UnloadPlugin();
                    return;
                }
                EconomyProvider = HookManager.GetHook<ExpEconomyHook>();
            }
            else
            {
                if (HookManager.IsHookLoadable<TEconomyHook>())
                    EconomyProvider = HookManager.GetHook<TEconomyHook>();
                else
                {
                    if (!HookManager.IsHookLoadable<UconomyHook>())
                    {
                        Logger.Error("# Failed to load economy hook. Unloading TShop...");
                        this.UnloadPlugin();
                        return;
                    }
                    EconomyProvider = HookManager.GetHook<UconomyHook>();
                }
            }

            if (Config.EnableDiscounts)
                InvokeRepeating(nameof(CheckDiscount), 1f, Config.DiscountInterval);
            _isLateInited = true;
        }

        /// <summary>
        /// Asynchronously checks for any available discounts and updates them.
        /// </summary>
        private void CheckDiscount()
        {
            try
            {
                if (IsConnectionAuthFailed || !_isLateInited)
                    return;
                
                Task.Run(async () =>
                {
                    List<Product> products = await DatabaseManager.GetProductsAsync();
                    // Remove the current discounts
                    foreach (Product item in products.FindAll(x => x.IsDiscounted))
                        await DatabaseManager.UpdateProductAsync(item.UnturnedId, item.IsVehicle, false, 0);

                    // Shuffle the product list
                    if (products.Count > 2)
                        products.Shuffle();

                    // Items
                    List<Product> items = products.FindAll(x => !x.IsVehicle);
                    for (int i = 0; i < Config.ItemCountToDiscount; i++)
                    {
                        if (items.IsValidIndex(i))
                            await DatabaseManager.UpdateProductAsync(items[i].UnturnedId, false, true, Math.Round((decimal)MathHelper.Next(Config.MinDiscount, Config.MaxDiscount), 2));
                    }

                    // Vehicles
                    List<Product> vehs = products.FindAll(x => x.IsVehicle);
                    for (int i = 0; i < Config.VehicleCountToDiscount; i++)
                    {
                        if (vehs.IsValidIndex(i))
                            await DatabaseManager.UpdateProductAsync(vehs[i].UnturnedId, true, true, Math.Round((decimal)MathHelper.Next(Config.MinDiscount, Config.MaxDiscount), 2));
                    }
                });
            }
            catch
            {
                // Not logging because this error has a 99% chance is caused by load error
            }
        }

        /// <summary>
        /// Gets the language packs dictionary for the plugin.
        /// </summary>
        public override Dictionary<string, string> LanguagePacks => new Dictionary<string, string>();

        /// <summary>
        /// Gets the default localization dictionary for the plugin.
        /// </summary>
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
                { "error_command_vehicleshop_color_args", "&aUsage: /vshop color [vehicle name | id] [vehicleColor]" },
                { "error_command_page", "&6That page doesn't exist." },
                { "error_balance_not_enough", "&6You don't have enough money. You need {0} to be able to buy." },
                { "error_item_already_added", "&6The {0} (ID: {1}) is already added to the shop." },
                { "error_item_not_added", "&6This item isn't added to the shop. ({0})" },
                { "error_item_not_found", "&6You have to provide a valid item id or name. ({0})" },
                { "error_item_not_enough", "&6You don't have enough items to sell." },
                { "error_shop_empty", "&6The shop is empty." },
                { "error_item_buy_error", "&6You can't buy this item." },
                { "error_item_sell_error", "&6You can't sell this item" },
                { "error_item_added", "&6Failed to add {0} to the item store." },
                { "error_item_removed", "&6Failed to remove {0} from the item store." },
                { "error_item_updated", "&6Failed to update {0} in the item store." },
                { "error_no_permission", "&6You don't have enough permission to buy or sell that product." },
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
                { "error_vehicle_color_not_hex", "&6Failed to parse the {0} value to hex color." },
                { "error_migrate_console", "&6Failed to migrate, please check the console." },
                { "error_command_removeinvalidproducts_cleanup_in_progress", "&6Cleanup is already in progress." },
                { "success_command_page", "&6Next page: /shoplist {0} {1}." },
                { "success_command_page_end", "&aYou have reached the end of the {0} shop list." },
                { "success_command_page_info", "- {0} (ID: {1}, buy price: {2}, sell price: {3} and Permission: {4})" },
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
                { "success_migrate", "&aYou have successfully migrated the data of ZaupShop. Please check the console for detailed information." },
                { "success_pruchase", "&aYou have successfully bought {0} item(s)." },
                { "success_sell", "&aYou have successfully sold {0} item(s)." },
                { "command_removeinvalidproducts_cleanup_started", "&aStarted removing invalid products from the database... Please be patient." },
                { "command_removeinvalidproducts_cleanup_finished", "&aCleanup finished. Removed {0} invalid products out of {1} products." },
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
                { "ui_success_vehicle_buy", "You have successfully bought a {0} for {2}{1}" },
                { "ui_text_market", "MARKET" },
                { "ui_text_items", "Items" },
                { "ui_text_vehicles", "Vehicles" },
                { "ui_text_basket", "Basket" },
                { "ui_text_account", "ACCOUNT" },
                { "ui_text_logout", "Logout" },
                { "ui_text_products", "Products" },
                { "ui_text_no_products", "No prudcts have been found." },
                { "ui_text_add_to_basket", "Add to basket" },
                { "ui_text_my_basket", "My Basket" },
                { "ui_text_icon", "Icon" },
                { "ui_text_product_name", "Product Name" },
                { "ui_text_quantity", "Quantity" },
                { "ui_text_price", "Price" },
                { "ui_text_actions", "Actions" },
                { "ui_text_basket_empty", "Your basket is empty." },
                { "ui_text_range", "Range:  1-100" },
                { "ui_text_buy_info", "Buy Information" },
                { "ui_text_sell_info", "Sell Information" },
                { "ui_text_subtotal", "Subtotal:" },
                { "ui_text_discount", "Discount:" },
                { "ui_text_total", "Total:" },
                { "ui_text_buy_disabled", "Purchasing is disabled for this product." },
                { "ui_text_sell_disabled", "Selling is disabled for this product." },
                { "ui_text_complete_order", "Complete Order" },
                { "ui_text_all", "All" },
                { "ui_product_search", "Search product by name" },
                { "ui_sort_featured", "Featured" },
                { "ui_sort_az", "Name: A to Z" },
                { "ui_sort_za", "Name: Z to A" },
                { "ui_sort_price_ascending", "Price ascending" },
                { "ui_sort_price_descending", "Price descending" },
                { "ui_sort_selected", "<color=#8CABC0>{0}</color>" },
                { "ui_sort_unselected", "<color=#486C84>{0}</color>" }
            };
    }
}
