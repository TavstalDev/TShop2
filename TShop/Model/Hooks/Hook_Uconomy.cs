using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.Core;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Extensions;

namespace Tavstal.TShop.Model.Hooks
{
    public class UconomyHook : Hook, IEconomyProvider
    {
        private MethodInfo _getBalanceMethod;
        private MethodInfo _increaseBalanceMethod;
        private MethodInfo _getTranslation;
        //private EventInfo _onPlayerPayMethod;
        //private EventInfo _onBalanceUpdateMethod;
        private object _databaseInstance;
        private object _pluginInstance;
        private object _uconomyConfig;


        public UconomyHook() : base(TShop.Instance, "thook_uconomy", true) { }

        public override void OnLoad()
        {
            try
            {
                // ReSharper disable PossibleNullReferenceException
                TShop.Logger.Log("Loading Uconomy hook...");

                TShop.Logger.LogDebug("UconomyHook #1: Searching for IRocketPlugin");
                IRocketPlugin uconomyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("uconomy"));
                
                TShop.Logger.LogDebug($"UconomyHook #2: Searching for plugin type. IRocketPlugin valid?: {uconomyPlugin != null}");
                
                Type uconomyType = uconomyPlugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
                

                TShop.Logger.LogDebug($"UconomyHook #3: Searching for plugin instance. Plugin type valid?: {uconomyType != null}");
                _pluginInstance = uconomyType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(uconomyPlugin);


                TShop.Logger.LogDebug($"UconomyHook #4: Searching for plugin instance type. Plugin instance valid?: {_pluginInstance != null}");
                Type pluginInstanceType = _pluginInstance.GetType();

                TShop.Logger.LogDebug($"UconomyHook #5: Searching for plugin configuration. Plugin instance type valid?: {pluginInstanceType != null}");
                object uconomyConfigInst = uconomyType.GetProperty("Configuration").GetValue(uconomyPlugin);

                _uconomyConfig = uconomyConfigInst.GetType().GetProperty("Instance").GetValue(uconomyConfigInst);

                TShop.Logger.LogDebug($"UconomyHook #6: Searching for plugin database. Plugin config valid?: {_uconomyConfig != null}");
                _databaseInstance = pluginInstanceType.GetField("Database").GetValue(_pluginInstance);

                TShop.Logger.LogDebug($"UconomyHook #7: Getting database methods. Database instance valid?: {_databaseInstance != null}");
                _getBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetBalance", new[] { typeof(string) });

                _increaseBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreaseBalance", new[] { typeof(string), typeof(decimal) });
                TShop.Logger.LogDebug($"UconomyHook #8: Getting translation method");
                if (pluginInstanceType.GetMethods().Any(x => x.Name == "Localize"))
                    _getTranslation = pluginInstanceType.GetMethod("Localize", new[] { typeof(string), typeof(object[]) });
                else
                    _getTranslation = pluginInstanceType.GetMethod("Translate", new[] { typeof(string), typeof(object[]) });
                TShop.Logger.LogDebug($"UconomyHook #9: Searching for events");
                // ReSharper restore PossibleNullReferenceException
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
                    TShop.Logger.LogError("Uconomy hook onPlayerPay delegate error:");
                    TShop.Logger.LogError(ex.ToString());
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
                    TShop.Logger.LogError("Uconomy hook onBalanceUpdate delegate error:");
                    TShop.Logger.LogError(ex.ToString());
                }*/
                #endregion

                TShop.Logger.LogException("Currency Name >> " + GetCurrencyName());
                TShop.Logger.LogException("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance"));
                TShop.Logger.Log("Uconomy hook loaded.");
            }
            catch (Exception e)
            {
                TShop.Logger.LogError("Failed to load Uconomy hook");
                TShop.Logger.LogError(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("uconomy"));
        }

        #region IPluginProvider Methods
        public T GetConfigValue<T>(string variableName)
        {
            try
            {
                return (T)Convert.ChangeType(_uconomyConfig.GetType().GetField(variableName).GetValue(_uconomyConfig), typeof(T));
            }
            catch
            {
                try
                {
                    // ReSharper disable PossibleNullReferenceException
                    return (T)Convert.ChangeType(_uconomyConfig.GetType().GetProperty(variableName).GetValue(_uconomyConfig), typeof(T));
                    // ReSharper restore PossibleNullReferenceException
                }
                catch
                {
                    TShop.Logger.LogError($"Failed to get '{variableName}' variable!");
                    return default;
                }
            }
        }

        public JObject GetConfig()
        {
            try
            {
                return JObject.FromObject(_uconomyConfig.GetType());
            }
            catch
            {
                TShop.Logger.LogError($"Failed to get config jobj.");
                return null;
            }
        }

        public string Localize(string translationKey, params object[] placeholder)
        {
            return Localize(false, translationKey, placeholder);
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            return ((string)_getTranslation.Invoke(_pluginInstance, new object[] { translationKey, placeholder }));
        }
        #endregion

        #region Economy Methods
        public decimal Withdraw(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.ToString(), -amount
            });
        }

        public decimal Deposit(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.ToString(), amount
            });
        }

        public decimal GetBalance(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.ToString()
            });
        }

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0)
                return (GetBalance(player) - amount) >= 0;
            else
                return (GetBalance(player) - Math.Abs(amount)) >= 0;
        }

        public async Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            await Task.Run(() =>
            {
                try
                {
                    decimal result = (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                        player.ToString(), -amount
                    });
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            return await taskCompletionSource.Task;
        }

        public async Task<decimal> DepositAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            await Task.Run(() =>
            {
                try
                {
                    decimal result = (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                        player.ToString(), amount
                    });
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            return await taskCompletionSource.Task;
        }

        public async Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            await Task.Run(() =>
            {
                try
                {
                    decimal result = (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                        player.ToString()
                    });
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            return await taskCompletionSource.Task;
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0)
                return (await GetBalanceAsync(player) - amount) >= 0;
            else
                return (await GetBalanceAsync(player) - Math.Abs(amount)) >= 0;
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
        public bool HasTransactionSystem()
        {
            return false;
        }

        public bool HasBankCardSystem()
        {
            return false;
        }
        public void AddTransaction(CSteamID player, ITransaction transaction)
        {
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");
        }

        public List<ITransaction> GetTransactions(CSteamID player)
        {
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");
        }

        public void AddBankCard(CSteamID steamID, IBankCard newCard)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public void UpdateBankCard(string cardId, decimal limitUsed, bool isActive)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public void RemoveBankCard(string cardId)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public List<IBankCard> GetBankCardsByPlayer(CSteamID steamID)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public IBankCard GetBankCardById(string cardId)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task AddTransactionAsync(CSteamID player, ITransaction transaction)
        {
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");
        }

        public async Task<List<ITransaction>> GetTransactionsAsync(CSteamID player)
        {
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");
        }

        public async Task AddBankCardAsync(CSteamID steamID, IBankCard newCard)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public async Task UpdateBankCardAsync(string cardId, decimal limitUsed, bool isActive)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public async Task RemoveBankCardAsync(string cardId)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public async Task<List<IBankCard>> GetBankCardsByPlayerAsync(CSteamID steamID)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }

        public async Task<IBankCard> GetBankCardByIdAsync(string cardId)
        {
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
    }
}
