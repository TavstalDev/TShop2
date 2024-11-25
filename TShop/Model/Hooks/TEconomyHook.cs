using Newtonsoft.Json.Linq;
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
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TEconomyHook : Hook, IEconomyProvider
    {
        private MethodInfo _getBalanceByCurrencyMethod;
        private MethodInfo _increaseBalanceByCurrencyMethod;
        private MethodInfo _addTransactionMethod;

        private MethodInfo _getTransactionsMethod;
        private MethodInfo _addBankCard;
        private MethodInfo _updateBankCard;
        private MethodInfo _removeBankCard;
        private MethodInfo _getBankCard;
        private MethodInfo _getBankCards;
        private MethodInfo _getTranslation;
        private object _databaseInstance;
        private object _pluginInstance;
        private object _pluginConfig;

        public TEconomyHook() : base(TShop.Instance, "thook_teconomy", false) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading TEconomy hook...");

                var plugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("teconomy"));
                if (plugin == null)
                    throw new Exception("Could not find plugin.");
                
                var pluginType = plugin.GetType().Assembly.GetType("Tavstal.TEconomy.TEconomy");
                if (pluginType == null)
                    throw new Exception("Could not get plugin type.");

                _pluginInstance =
                    pluginType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(plugin);
                if (_pluginInstance == null)
                    throw new Exception("Could not get plugin instance.");

                _pluginConfig = _pluginInstance.GetType().GetProperty("Config")?.GetValue(_pluginInstance);
                if (_pluginConfig == null)
                    throw new Exception("Could not get plugin configuration.");
                
                _databaseInstance = _pluginInstance.GetType().GetProperty("DatabaseManager")?.GetValue(_pluginInstance);
                if (_databaseInstance == null)
                    throw new Exception("Failed to get the plugin database instance.");
                Type databaseType = _databaseInstance.GetType();

                _getBalanceByCurrencyMethod = databaseType.GetMethod(
                    "GetBalance", new[] { typeof(ulong), typeof(EPaymentMethod) });

                _increaseBalanceByCurrencyMethod = databaseType.GetMethod(
                    "IncreaseBalance", new[] { typeof(ulong), typeof(decimal), typeof(EPaymentMethod) });

                _addTransactionMethod = databaseType.GetMethod(
                    "AddTransaction", new[] { typeof(ETransaction), typeof(EPaymentMethod), typeof(string), typeof(ulong), typeof(ulong), typeof(decimal), typeof(DateTime) });

                _getTransactionsMethod = databaseType.GetMethod(
                    "GetTransactionsByParticipant", new [] { typeof(ulong) });

                _addBankCard = databaseType.GetMethod(
                    "AddBankCard", new[] { typeof(string), typeof(string), typeof(string), typeof(ulong), typeof(decimal), typeof(decimal), typeof(DateTime) } );

                _removeBankCard = databaseType.GetMethod(
                    "RemoveBankCard", new[] { typeof(string) });

                _updateBankCard = databaseType.GetMethod(
                    "UpdateBankCard", new[] { typeof(string), typeof(decimal), typeof(bool) });

                _getBankCard = databaseType.GetMethod(
                    "FindBankCard", new[] { typeof(string) });

                _getBankCards = databaseType.GetMethod(
                    "GetBankCards", new[] { typeof(ulong) });

                _getTranslation = _pluginInstance.GetType().GetMethod("Localize", new[] { typeof(bool), typeof(string), typeof(object[]) });
                TShop.Logger.Log("TEconomy hook loaded.");
            }
            catch (Exception e)
            {
                TShop.Logger.LogError("Failed to load TEconomy hook");
                TShop.Logger.LogError(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("teconomy"));
        }


        #region IPluginProvider
        public T GetConfigValue<T>(string variableName)
        {
            try
            {
                return (T)Convert.ChangeType(_pluginConfig.GetType().GetField(variableName).GetValue(_pluginConfig), typeof(T));
            }
            catch
            {
                try
                {
                    return (T)Convert.ChangeType(_pluginConfig.GetType().GetProperty(variableName)?.GetValue(_pluginConfig), typeof(T));
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
                return JObject.FromObject(_pluginConfig.GetType());
            }
            catch
            {
                TShop.Logger.LogError($"Failed to get config jobj.");
                return null;
            }
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            return ((string)_getTranslation.Invoke(_pluginInstance, new object[] { addPrefix, translationKey, placeholder }));
        }

        public string Localize(string translationKey, params object[] placeholder)
        {
            return Localize(false, translationKey, placeholder);
        }
        #endregion

        #region Basic Economy Methods
        public decimal Withdraw(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount, method });
        }

        public decimal Deposit(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player, amount, method });
        }

        public decimal GetBalance(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_getBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID, method});
        }

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0) 
                return (GetBalance(player, method) - amount) >= 0;
            else
                return (GetBalance(player, method) - (amount * -1)) >= 0;
        }

        public async Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = (Task<decimal>)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount, method });
            return await task;
        }

        public async Task<decimal> DepositAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = (Task<decimal>)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player, +amount, method });
            return await task;
        }

        public async Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = (Task<decimal>)_getBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID, method});
            return await task;
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0)
                return (await GetBalanceAsync(player, method) - amount) >= 0;
            else
                return (await GetBalanceAsync(player, method) - (amount * -1)) >= 0;
        }

        public string GetCurrencyName()
        {
            string value = "Credits";
            try
            {
                value = GetConfigValue<string>("MoneyNameFull");
            }
            catch { /* ignore */ }
            return value;
        }
        #endregion

        #region TEconomy Methods
        public bool HasTransactionSystem()
        {
            return true;
        }

        public bool HasBankCardSystem()
        {
            return true;
        }
        public void AddTransaction(CSteamID player, ITransaction transaction)
        {
            _addTransactionMethod.Invoke(_databaseInstance, new object[] { transaction.Type, transaction.PaymentMethod, transaction.StoreName, transaction.PayerId, transaction.PayeeId, transaction.Amount, transaction.Date });
        }

        public List<ITransaction> GetTransactions(CSteamID player)
        {
            try
            {
                return (List<ITransaction>)_getTransactionsMethod.Invoke(_databaseInstance, new object[] { player.m_SteamID });
            }
            catch (Exception ex)
            {
                TShop.Logger.LogError("Error in GetTransactions(): " + ex);
                return new List<ITransaction>();
            }
        }

        public void AddBankCard(CSteamID steamID, IBankCard newCard)
        {
            _addBankCard.Invoke(_databaseInstance, new object[] { newCard.Id, newCard.SecurityCode, newCard.PinCode, newCard.HolderId, newCard.BalanceUse, newCard.BalanceLimit, newCard.ExpireDate });
        }

        public void UpdateBankCard(string cardId, decimal limitUsed, bool isActive)
        {
            _updateBankCard.Invoke(_databaseInstance, new object[] { cardId, limitUsed, isActive });
        }

        public void RemoveBankCard(string cardId)
        {
            _removeBankCard.Invoke(_databaseInstance, new object[] { cardId });
        }

        public List<IBankCard> GetBankCardsByPlayer(CSteamID steamID)
        {
            return (List<IBankCard>)_getBankCards.Invoke(_databaseInstance, new object[] { steamID.m_SteamID });
        }

        public IBankCard GetBankCardById(string cardId)
        {
            return (IBankCard)_getBankCard.Invoke(_databaseInstance, new object[] { cardId });
        }

        public async Task AddTransactionAsync(CSteamID player, ITransaction transaction)
        {
            Task task = (Task)_addTransactionMethod.Invoke(_databaseInstance, new object[] { transaction.Type, transaction.PaymentMethod, transaction.StoreName, transaction.PayerId, transaction.PayeeId, transaction.Amount, transaction.Date });
            await task;
        }

        public async Task<List<ITransaction>> GetTransactionsAsync(CSteamID player)
        {
            Task<List<ITransaction>> task = (Task<List<ITransaction>>)_getTransactionsMethod.Invoke(_databaseInstance, new object[] { player.m_SteamID });
            return await task;
        }

        public async Task AddBankCardAsync(CSteamID steamID, IBankCard newCard)
        {
            Task task = (Task)_addBankCard.Invoke(_databaseInstance, new object[] { newCard.Id, newCard.SecurityCode, newCard.PinCode, newCard.HolderId, newCard.BalanceUse, newCard.BalanceLimit, newCard.ExpireDate });
            await task;
        }

        public async Task UpdateBankCardAsync(string cardId, decimal limitUsed, bool isActive)
        {
            Task task = (Task)_updateBankCard.Invoke(_databaseInstance, new object[] { cardId, limitUsed, isActive });
            await task;
        }

        public async Task RemoveBankCardAsync(string cardId)
        {
            Task task = (Task)_removeBankCard.Invoke(_databaseInstance, new object[] { cardId });
            await task;
        }

        public async Task<List<IBankCard>> GetBankCardsByPlayerAsync(CSteamID steamID)
        {
            Task<List<IBankCard>> task = (Task<List<IBankCard>>)_getBankCards.Invoke(_databaseInstance, new object[] { steamID.m_SteamID });
            return await task;
        }

        public async Task<IBankCard> GetBankCardByIdAsync(string cardId)
        {
            Task<IBankCard> task = (Task<IBankCard>)_getBankCard.Invoke(_databaseInstance, new object[] { cardId });
            return await task;
        }
        #endregion
    }
}
