using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.Core;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Compatibility.Interfaces;
using Tavstal.TLibrary.Compatibility.Interfaces.Economy;
using Tavstal.TLibrary.Extensions;

namespace Tavstal.TShop.Compability.Hooks
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
        private object uconomyConfig;


        public UconomyHook() : base(TShop.Instance, "uconomy_tshop", true) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading Uconomy hook...");

                IRocketPlugin uconomyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("uconomy"));
                Type uconomyType = uconomyPlugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
                _pluginInstance =
                    uconomyType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(uconomyPlugin);
                Type pluginInstanceType = _pluginInstance.GetType();

                object uconomyConfigInst = uconomyType.GetProperty("Configuration").GetValue(uconomyPlugin);
                uconomyConfig = uconomyConfigInst.GetType().GetProperty("Instance").GetValue(uconomyConfigInst);

                _databaseInstance = pluginInstanceType.GetField("Database").GetValue(_pluginInstance);

                _getBalanceMethod = pluginInstanceType.GetMethod(
                    "GetBalance", new[] { typeof(string) });

                _increaseBalanceMethod = pluginInstanceType.GetMethod(
                    "IncreaseBalance", new[] { typeof(string), typeof(decimal) });

                if (pluginInstanceType.GetMethods().Any(x => x.Name == "Localize"))
                    _getTranslation = pluginInstanceType.GetMethod("Localize", new[] { typeof(string), typeof(object[]) });
                else
                    _getTranslation = pluginInstanceType.GetMethod("Translate", new[] { typeof(string), typeof(object[]) });

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
                TShop.Logger.LogException("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance").ToString());
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
        public T GetConfigValue<T>(string VariableName)
        {
            try
            {
                return (T)Convert.ChangeType(uconomyConfig.GetType().GetField(VariableName).GetValue(uconomyConfig), typeof(T));
            }
            catch
            {
                try
                {
                    return (T)Convert.ChangeType(uconomyConfig.GetType().GetProperty(VariableName).GetValue(uconomyConfig), typeof(T));
                }
                catch
                {
                    TShop.Logger.LogError($"Failed to get '{VariableName}' variable!");
                    return default;
                }
            }
        }

        public JObject GetConfig()
        {
            try
            {
                return JObject.FromObject(uconomyConfig.GetType());
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
                value = GetConfigValue<string>("MoneyName").ToString();
            }
            catch { }
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
