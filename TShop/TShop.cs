using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Managers;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TShop.Handlers;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Hooks;
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
        public static TShop Instance { get; private set; } = null!;
        public static DatabaseManager DatabaseManager { get; private set; } = null!;
        public static IEconomyProvider EconomyProvider { get; private set; } = null!;
        public static bool IsConnectionAuthFailed { get; set; }
        public static bool IsCleanupInProgress { get; set; }
        private bool _isLateInited;

        public override void OnPreLoad()
        {
            Instance = this;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("████████╗░██████╗██╗░░██╗░█████╗░██████╗░");
            sb.AppendLine("╚══██╔══╝██╔════╝██║░░██║██╔══██╗██╔══██╗");
            sb.AppendLine("░░░██║░░░╚█████╗░███████║██║░░██║██████╔╝");
            sb.AppendLine("░░░██║░░░░╚═══██╗██╔══██║██║░░██║██╔═══╝░");
            sb.AppendLine("░░░██║░░░██████╔╝██║░░██║╚█████╔╝██║░░░░░");
            sb.AppendLine("░░░╚═╝░░░╚═════╝░╚═╝░░╚═╝░╚════╝░╚═╝░░░░░");
            sb.AppendLine();
            sb.AppendLine("[ About ]");
            sb.AppendLine(" ▸ Developer : Tavstal");
            sb.AppendLine(" ▸ Discord   : @Tavstal");
            sb.AppendLine(" ▸ Website   : https://redstoneplugins.com");
            sb.AppendLine(" ▸ GitHub    : https://github.com/TavstalDev");
            sb.AppendLine();
            sb.AppendLine("[ Build ]");
            sb.AppendLine($" ▸ Version   : {Version}");
            sb.AppendLine($" ▸ Build Date: {BuildDate} UTC");
            sb.AppendLine($" ▸ TLibrary  : {LibraryVersion}");
            sb.AppendLine();
            sb.AppendLine("[ Support ]");
            sb.AppendLine(" ▸ Report issues or request features:");
            sb.AppendLine(" ▸ https://github.com/TavstalDev/TShop2/issues");
            sb.AppendLine();
            sb.AppendLine("────────────────────────────────────────────────────────");
            Logger.Log(ELogLevel.COMMAND, sb.ToString(), includePrefixes: false, color:  ConsoleColor.Cyan);
        }

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public override void OnLoad()
        {
            try
            {
                DatabaseManager = new DatabaseManager();
                UnturnedEventHandler.Attach();
               
                if (!Level.isLoaded || Level.isLoading)
                    Level.onPostLevelLoaded += Event_OnPluginsLoaded;
                else
                    Event_OnPluginsLoaded(0);

                if (IsConnectionAuthFailed)
                    return;

                Logger.Info($"# {GetPluginName()} has been loaded.");
                Logger.Info("# Starting late initialization...");
            }
            catch (Exception ex)
            {
                Logger.Error($"# Failed to load {GetPluginName()}...", ex);
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

            Logger.Info($"# {GetPluginName()} has been successfully unloaded.");
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

            Logger.Info($"[▶] INITIATING: {GetPluginName()}...");
            Logger.Info("# Searching for economy plugin...");
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
                EconomyProvider = HookManager.GetHook<ExpEconomyHook>()!;
            }
            else
            {
                if (HookManager.IsHookLoadable<TEconomyHook>())
                    EconomyProvider = HookManager.GetHook<TEconomyHook>()!;
                else
                {
                    if (!HookManager.IsHookLoadable<UconomyHook>())
                    {
                        Logger.Error("# Failed to load economy hook. Unloading TShop...");
                        this.UnloadPlugin();
                        return;
                    }
                    EconomyProvider = HookManager.GetHook<UconomyHook>()!;
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
        public override Dictionary<string, string> DefaultLocalization => ShopDefaultLocalizations.Localizations;
    }
}
