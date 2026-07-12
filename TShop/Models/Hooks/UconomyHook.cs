using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.Core;
using Steamworks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Models.Plugin;

namespace Tavstal.TShop.Models.Hooks
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UconomyHook : Hook, IEconomyProvider
    {
        private readonly TLogger _logger;
        private MethodInfo? _getBalanceMethod;
        private MethodInfo? _increaseBalanceMethod;
        private MethodInfo? _getTranslation;
        //private EventInfo _onPlayerPayMethod;
        //private EventInfo _onBalanceUpdateMethod;
        private object? _databaseInstance;
        private object? _pluginInstance;
        private object? _uconomyConfig;

        public UconomyHook() : base(TShop.Instance, "thook_uconomy", true)
        {
            _logger = new TLogger(TShop.Instance.GetPluginName(), nameof(UconomyHook), TShop.Instance.GetLogLevel());
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        protected override void OnLoad()
        {
            try
            {

                _logger.Info("Loading Uconomy hook...");
                IRocketPlugin plugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("uconomy"));
                if (plugin == null)
                    throw new Exception("Could not find plugin.");

                Type pluginType = plugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
                if (pluginType == null)
                    throw new Exception("Could not get plugin type.");
                
                _pluginInstance = pluginType.GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(plugin);
                if (_pluginInstance == null)
                    throw new Exception("Could not get plugin instance.");
                Type pluginInstanceType = _pluginInstance.GetType();
                
                object? uconomyConfigInst = pluginType.GetProperty("Configuration")?.GetValue(plugin);
                if (uconomyConfigInst == null)
                    throw new Exception("Could not get plugin configuration field.");

                _uconomyConfig = uconomyConfigInst.GetType().GetProperty("Instance")?.GetValue(uconomyConfigInst);
                if (_uconomyConfig == null)
                    throw new Exception("Could not get plugin configuration instance.");
                
                _databaseInstance = pluginInstanceType.GetField("Database").GetValue(_pluginInstance);
                if (_databaseInstance == null)
                    throw new Exception("Failed to get the plugin database instance.");

                _getBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetBalance", new[] { typeof(string) });

                _increaseBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreaseBalance", new[] { typeof(string), typeof(decimal) });

                bool hasLocalize = pluginInstanceType.GetMethods().Any(x => x.Name == "Localize");
                _getTranslation = pluginInstanceType.GetMethod(hasLocalize ? "Localize" : "Translate", new[] { typeof(string), typeof(object[]) });
                
                #region Create Event Delegates
                /* Added because it might be needed in the future
                var parentPlugin = TShop.Instance;
                var parentPluginType = parentPlugin.GetType().Assembly.GetType("Tavstal.TShop.TShop");
                var parentPluginInstance = parentPluginType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(parentPlugin);

                try
                {
                    _onPlayerPayMethod = _pluginInstance.GetType().GetEvent("OnPlayerPay"); // Event in Uconomy
                    // Event handling method in this plugin
                    Delegate handler = Delegate.CreateDelegate(_onPlayerPayMethod.EventHandlerType, parentPlugin, parentPluginInstance.GetType().GetMethod("Event_Uconomy_OnPlayerPay"), true);
                    _onPlayerPayMethod.AddEventHandler(_pluginInstance, handler);

                }
                catch (Exception ex)
                {
                    _logger.Error("Uconomy hook onPlayerPay delegate error:");
                    _logger.Error(ex.ToString());
                }

                try
                {
                    _onBalanceUpdateMethod = _pluginInstance.GetType().GetEvent("OnBalanceUpdate"); // Event in Uconomy
                    // Event handling method in this plugin
                    Delegate handler = Delegate.CreateDelegate(_onBalanceUpdateMethod.EventHandlerType, parentPlugin, parentPluginInstance.GetType().GetMethod("Event_Uconomy_OnPlayerBalanceUpdate"), true);
                    _onBalanceUpdateMethod.AddEventHandler(_pluginInstance, handler);
                }
                catch (Exception ex)
                {
                    _logger.Error("Uconomy hook onBalanceUpdate delegate error:");
                    _logger.Error(ex.ToString());
                }*/
                #endregion

                _logger.Info("Currency Name >> " + GetCurrencyName());
                _logger.Info("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance"));
                _logger.Info("Uconomy hook loaded.");
            }
            catch (Exception e)
            {
                _logger.Error("Failed to load Uconomy hook");
                _logger.Error(e.ToString());
            }
        }

        protected override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("uconomy"));
        }

        #region IPluginProvider Methods
        public T GetConfigValue<T>(string variableName)
        {
            try
            {
                return (T)Convert.ChangeType(_uconomyConfig?.GetType().GetField(variableName).GetValue(_uconomyConfig), typeof(T));
            }
            catch
            {
                try
                {
                    // ReSharper disable PossibleNullReferenceException
                    return (T)Convert.ChangeType(_uconomyConfig?.GetType().GetProperty(variableName).GetValue(_uconomyConfig), typeof(T));
                    // ReSharper restore PossibleNullReferenceException
                }
                catch
                {
                    _logger.Error($"Failed to get '{variableName}' variable!");
                    return default!;
                }
            }
        }

        public JObject GetConfig()
        {
            try
            {
                return JObject.FromObject(_uconomyConfig?.GetType()!);
            }
            catch
            {
                _logger.Error($"Failed to get config jobj.");
                return null!;
            }
        }

        public string Localize(string translationKey, params object[] placeholder)
        {
            return Localize(false, translationKey, placeholder);
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            return (string)_getTranslation?.Invoke(_pluginInstance, new object[] { translationKey, placeholder })!;
        }
        #endregion

        #region Economy Methods

        public Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_increaseBalanceMethod == null)
                {
                    _logger.Error("IncreaseBalance method is null. Cannot withdraw.");
                    return Task.FromResult(-1m);
                }
                
                decimal result = (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                    player.ToString(), -amount
                });
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to withdraw {amount} from player {player}.", ex);
                return Task.FromResult(-1m);
            }
        }

        public Task<decimal> DepositAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_increaseBalanceMethod == null)
                {
                    _logger.Error("IncreaseBalance method is null. Cannot deposit.");
                    return Task.FromResult(-1m);
                }
                
                decimal result = (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[]
                {
                    player.ToString(), amount
                });
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to deposit {amount} to player {player}.", ex);
                return Task.FromResult(-1m);
            }
        }

        public Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_getBalanceMethod == null)
                {
                    _logger.Error("GetBalance method is null. Cannot get balance.");
                    return Task.FromResult(-1m);
                }
                
                decimal result = (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                    player.ToString()
                });
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get balance for player {player}.", ex);
                return Task.FromResult(-1m);
            }
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0)
                return await GetBalanceAsync(player) - amount >= 0;
            return await GetBalanceAsync(player) - Math.Abs(amount) >= 0;
        }

        public string GetCurrencyName()
        {
            string value = "Credits";
            try
            {
                value = GetConfigValue<string>("MoneyName");
            }
            catch { /* ignore */ }
            return value;
        }
        #endregion

        #region TEconomy Methods

        public bool HasTransactionSystem() => false;

        public bool HasBankCardSystem() => false;
        
        public Task AddTransactionAsync(CSteamID player, ITransaction transaction) =>
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");
        
        public Task<List<ITransaction>> GetTransactionsAsync(CSteamID player) =>
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");

        public Task AddBankCardAsync(CSteamID steamID, IBankCard newCard) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task UpdateBankCardAsync(string cardId, decimal limitUsed, bool isActive) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task RemoveBankCardAsync(string cardId) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task<List<IBankCard>> GetBankCardsByPlayerAsync(CSteamID steamID) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task<IBankCard> GetBankCardByIdAsync(string cardId) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        #endregion
    }
}
