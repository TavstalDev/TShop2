using Newtonsoft.Json.Linq;
using Rocket.Core;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Compatibility.Interfaces.Economy;
using Tavstal.TLibrary.Extensions;

namespace Tavstal.TShop.Compability.Hooks
{
    public class UconomyHook : Hook, IEconomyProvider
    {
        private MethodInfo _getBalanceMethod;
        private MethodInfo _increaseBalanceMethod;
        private MethodInfo _getTranslation;
        private object _databaseInstance;
        private object _pluginInstance;
        private object uconomyConfig;

        public UconomyHook() : base("uconomy_tshop", true) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading Uconomy hook...");

                var uconomyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("uconomy"));
                var uconomyType = uconomyPlugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
                _pluginInstance =
                    uconomyType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(uconomyPlugin);

                var uconomyConfigInst = uconomyType.GetProperty("Configuration").GetValue(uconomyPlugin);
                uconomyConfig = uconomyConfigInst.GetType().GetProperty("Instance").GetValue(uconomyConfigInst);

                _databaseInstance = _pluginInstance.GetType().GetField("Database").GetValue(_pluginInstance);

                _getBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetBalance", new[] { typeof(string) });

                _increaseBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreaseBalance", new[] { typeof(string), typeof(decimal) });

                _getTranslation = _pluginInstance.GetType().GetMethod("Translate", new[] { typeof(string), typeof(object[]) });

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
        public bool HasBuiltInTransactionSystem() { return false; }
        public bool HasBuiltInBankCardSystem() { return false; }
        public void AddTransaction(CSteamID player, ITransaction transaction)
        {
            // Not implemented
        }

        public List<ITransaction> GetTransactions(CSteamID player)
        {
            // Not implemented
            return default;
        }

        public void AddBankCard(CSteamID steamID, IBankCard newCard)
        {
            // Not implemented
        }

        public void UpdateBankCard(string cardId, decimal limitUsed, bool isActive)
        {
            // Not implemented
        }

        public void RemoveBankCard(string cardId)
        {
            // Not implemented
        }

        public List<IBankCard> GetBankCardsByPlayer(CSteamID steamID)
        {
            // Not implemented
            return default;
        }

        public IBankCard GetBankCardById(string cardId)
        {
            // Not implemented
            return default;
        }
        #endregion
    }
}
