using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Core;
using Rocket.Unturned.Player;
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
    public class TEconomyHook : Hook, IEconomyProvider
    {
        public string GetCurrencyName()
        {
            string value = "Credits";
            try
            {
                value = GetConfigValue("MoneyNameFull").ToString();
            }
            catch { }
            return value;
        }

        private MethodInfo _getBalanceMethod {  get; set; }
        private MethodInfo _getBalanceByCurrencyMethod { get; set; }
        private MethodInfo _increaseBalanceMethod { get; set; }
        private MethodInfo _increaseBalanceByCurrencyMethod { get; set; }
        private MethodInfo _addTransactionMethod { get; set; }
        private MethodInfo _getPlayerTransactionMethod { get; set; }
        //private MethodInfo _getBankCard { get; set; }
        private MethodInfo _addBankCard { get; set; }
        private MethodInfo _updateBankCard { get; set; }
        private MethodInfo _removeBankCard { get; set; }
        private MethodInfo _generateCardId { get; set; }
        private MethodInfo _getTranslation { get; set; }
        private object _databaseInstance { get; set; }
        private object _pluginInstance { get; set; }
        private object teconomyConfig { get; set; }
        private Type helperType { get; set; }

        public TEconomyHook() : base("teconomy_tshop", false) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading TEconomy hook...");

                var teconomyPlugin = R.Plugins.GetPlugins().FirstOrDefault(c => c.Name.EqualsIgnoreCase("teconomy"));
                var teconomyType = teconomyPlugin.GetType().Assembly.GetType("Tavstal.TEconomy.TEconomy");

                _pluginInstance =
                    teconomyType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(teconomyPlugin);

                var uconomyConfigInst = teconomyType.GetProperty("Configuration").GetValue(teconomyPlugin);
                teconomyConfig = uconomyConfigInst.GetType().GetProperty("Instance").GetValue(uconomyConfigInst);
                _databaseInstance = _pluginInstance.GetType().GetProperty("Database").GetValue(_pluginInstance);
                Type databaseType = _databaseInstance.GetType();


                _getBalanceMethod = databaseType.GetMethod(
                    "GetBalance", new[] { typeof(ulong) });

                _getBalanceByCurrencyMethod = databaseType.GetMethod(
                    "GetBalance", new[] { typeof(ulong), typeof(EPaymentMethod) });

                _increaseBalanceMethod = databaseType.GetMethod(
                    "IncreaseBalance", new[] { typeof(ulong), typeof(decimal) });

                _increaseBalanceByCurrencyMethod = databaseType.GetMethod(
                    "IncreaseBalance", new[] { typeof(ulong), typeof(decimal), typeof(EPaymentMethod) });

                _addTransactionMethod = databaseType.GetMethod(
                    "AddTransaction", new[] { typeof(ETransaction), typeof(EPaymentMethod), typeof(string), typeof(ulong), typeof(ulong), typeof(decimal), typeof(DateTime) });

                _addBankCard = databaseType.GetMethod(
                    "AddBankCard", new[] { typeof(string), typeof(string), typeof(string), typeof(ulong), typeof(decimal), typeof(decimal), typeof(DateTime) } );

                _generateCardId = ;

                _removeBankCard = ;

                _updateBankCard = ;

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


        public bool HasBuiltInTransactionSystem() { return true; }

        public bool HasBuiltInBankCardSystem() { return true; }

        public decimal Withdraw(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Withdraw(player.CSteamID, amount, method);
        }

        public decimal Deposit(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Deposit(player.CSteamID, amount, method);
        }

        public decimal GetBalance(UnturnedPlayer player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return GetBalance(player.CSteamID, method);
        }

        public bool Has(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Has(player.CSteamID, amount, method);
        }

        public void AddTransaction(UnturnedPlayer player, Transaction transaction)
        {
            AddTransaction(player.CSteamID, transaction);
        }

        public decimal Withdraw(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            switch (method)
            {
                case EPaymentMethod.BANK_ACCOUNT:
                    {
                        return (decimal)_increaseBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount });
                    }
                case EPaymentMethod.CRYPTO_WALLET:
                    {
                        return (decimal)_increaseCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount });
                    }
                case EPaymentMethod.CASH:
                    {
                        return (decimal)_increaseCashBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount });
                    }
                default:
                    {
                        return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID.ToString(), -amount });
                    }
            }
        }

        public decimal Deposit(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            switch (method)
            {
                case EPaymentMethod.BANK_ACCOUNT:
                    {
                        return (decimal)_increaseBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, amount });
                    }
                case EPaymentMethod.CRYPTO_WALLET:
                    {
                        return (decimal)_increaseCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, amount });
                    }
                case EPaymentMethod.CASH:
                    {
                        return (decimal)_increaseCashBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, amount });
                    }
                default:
                    {
                        return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID.ToString(), amount });
                    }
            }
        }

        public decimal GetBalance(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (decimal)_getBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID, method});
        }

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (GetBalance(player, method) - amount) >= 0;
        }

        public T GetConfigValue<T>(string VariableName)
        {
            try
            {
                return (T)Convert.ChangeType(teconomyConfig.GetType().GetField(VariableName).GetValue(teconomyConfig), typeof(T));
            }
            catch
            {
                try
                {
                    return (T)Convert.ChangeType(teconomyConfig.GetType().GetProperty(VariableName).GetValue(teconomyConfig), typeof(T));
                }
                catch
                {
                    TShop.Logger.LogError($"Failed to get '{VariableName}' variable!");
                    return default;
                }
            }
        }

        public object GetConfigValue(string VariableName)
        {
            object local = new object();
            try
            {
                if (teconomyConfig.GetType().GetFields().FirstOrDefault(x => x.Name.EqualsIgnoreCase(VariableName)) != null)
                    local = teconomyConfig.GetType().GetField(VariableName).GetValue(teconomyConfig).ToString();
                else
                    local = teconomyConfig.GetType().GetProperty(VariableName).GetValue(teconomyConfig).ToString();
            }
            catch
            {
                local = null;
                TShop.Logger.LogError($"Failed to get '{VariableName}' variable!");
            }
            return local;
        }

        public JObject GetConfig()
        {
            try
            {
                return JObject.FromObject(teconomyConfig.GetType());
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

        public void AddTransaction(CSteamID player, ITransaction transaction)
        {
            _addTransactionMethod.Invoke(_databaseInstance, new object[] { transaction.Type, transaction.PaymentMethod, transaction.StoreName, transaction.PayerId, transaction.PayeeId, transaction.Amount, transaction.Date });
        }

        public List<ITransaction> GetTransactions(CSteamID player)
        {
            try
            {
                var result = _getPlayerTransactionMethod.Invoke(_databaseInstance, new object[] { player.CSteamID });

                if (result == null)
                    return new List<ITransaction>();

                return JsonConvert.DeserializeObject<List<Transaction>>(JObject.FromObject(result)["Transactions"].ToString(Formatting.None));
            }
            catch (Exception ex)
            {
                TShop.Logger.LogError("Error in GetTransactions(): " + ex);
                return new List<ITransaction>();
            }
        }

        public void AddBankCard(CSteamID steamID, IBankCard newCard)
        {
            newCard.Id = Convert.ToString(_generateCardId.Invoke(helperType, null));
            _addBankCard.Invoke(_databaseInstance, new object[] { steamID, JObject.FromObject(newCard).ToString(Formatting.None) });
        }

        public void UpdateBankCard(CSteamID steamID, string id, IBankCard newData)
        {
            _updateBankCard.Invoke(_databaseInstance, new object[] { steamID, id, JObject.FromObject(newData).ToString(Formatting.None) });
        }

        public void RemoveBankCard(CSteamID steamID, int index, bool isReversed = false)
        {
            _removeBankCard.Invoke(_databaseInstance, new object[] { steamID, index, isReversed });
        }

        List<IBankCard> IEconomyProvider.GetPlayerCards(CSteamID steamID)
        {
            JObject account = GetPlayerAccount(steamID);
            return JsonConvert.DeserializeObject<List<IBankCard>>(account["bankCards"].ToString(Formatting.None));
        }

        IBankCard IEconomyProvider.GetPlayerCard(CSteamID steamID, int index)
        {
           
        }

        IBankCard IEconomyProvider.GetPlayerCard(CSteamID steamID, string id)
        {
            
        }
    }
}
