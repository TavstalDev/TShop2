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

        private MethodInfo _getBalanceMethod;
        private MethodInfo _getCashBalanceMethod;
        private MethodInfo _getBankBalanceMethod;
        private MethodInfo _getCryptoBalanceMethod;
        private MethodInfo _increaseBalanceMethod;
        private MethodInfo _increaseCashBalanceMethod;
        private MethodInfo _increaseCryptoBalanceMethod;
        private MethodInfo _increaseBankBalanceMethod;
        private MethodInfo _addTransactionMethod;
        private MethodInfo _getPlayerTransactionMethod;
        private MethodInfo _getBankCard;
        private MethodInfo _addBankCard;
        private MethodInfo _updateBankCard;
        private MethodInfo _removeBankCard;
        private MethodInfo _generateCardId;
        private MethodInfo _getTranslation;
        private object _databaseInstance;
        private object _pluginInstance;
        private object teconomyConfig;
        private Type helperType;

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

                _getBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetBalance", new[] { typeof(string) });

                _getCashBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetPlayerCash", new[] { typeof(CSteamID) });

                _getBankBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetPlayerBank", new[] { typeof(CSteamID) });

                _getCryptoBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "GetPlayerCrypto", new[] { typeof(CSteamID) });

                _increaseBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreaseBalance", new[] { typeof(string), typeof(decimal) });

                _increaseBankBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreasePlayerBank", new[] { typeof(CSteamID), typeof(decimal) });

                _increaseCashBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreasePlayerCash", new[] { typeof(CSteamID), typeof(decimal) });

                _increaseCryptoBalanceMethod = _databaseInstance.GetType().GetMethod(
                    "IncreasePlayerCrypto", new[] { typeof(CSteamID), typeof(decimal) });

                _addTransactionMethod = _databaseInstance.GetType().GetMethod(
                    "AddPlayerTransaction", new[] { typeof(CSteamID), typeof(string) });

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
            switch (method)
            {
                case EPaymentMethod.BANK_ACCOUNT:
                    {
                        return (decimal)_getBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player});
                    }
                case EPaymentMethod.CRYPTO_WALLET:
                    {
                        return (decimal)_getCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player});
                    }
                case EPaymentMethod.CASH:
                    {
                        return (decimal)_getCashBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player});
                    }
                default:
                    {
                        return (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player.m_SteamID.ToString()});
                    }
            }
        }

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (GetBalance(player, method) - amount) >= 0;
        }

        public void AddTransaction(CSteamID player, Transaction transaction)
        {
            _addTransactionMethod.Invoke(_databaseInstance, new object[] { player, JObject.FromObject(transaction).ToString(Formatting.None) });
        }

        public List<Transaction> GetTransactions(UnturnedPlayer player)
        {
            try
            {
                var result = _getPlayerTransactionMethod.Invoke(_databaseInstance, new object[] { player.CSteamID });

                if (result == null)
                    return new List<Transaction>();

                return JsonConvert.DeserializeObject<List<Transaction>>(JObject.FromObject(result)["Transactions"].ToString(Formatting.None));
            }
            catch (Exception ex)
            {
                TShop.Logger.LogError("Error in GetTransactions(): " + ex);
                return new List<Transaction>();
            }
        }

        public void AddPlayerCard(CSteamID steamID, BankCard newCard)
        {
            newCard.Id = Convert.ToString(_generateCardId.Invoke(helperType, null));
            _addBankCard.Invoke(_databaseInstance, new object[] { steamID, JObject.FromObject(newCard).ToString(Formatting.None) });
        }

        public void UpdatePlayerCard(CSteamID steamID, string id, BankCardDetails newData)
        {
            _updateBankCard.Invoke(_databaseInstance, new object[] { steamID, id, JObject.FromObject(newData).ToString(Formatting.None) });
        }

        public void RemovePlayerCard(CSteamID steamID, int index, bool isReversed = false)
        {
            _removeBankCard.Invoke(_databaseInstance, new object[] { steamID, index, isReversed });
        }

        public List<BankCard> GetPlayerCards(CSteamID steamID)
        {
            JObject account = GetPlayerAccount(steamID);
            return JsonConvert.DeserializeObject<List<BankCard>>(account["bankCards"].ToString(Formatting.None));
        }

        public BankCard GetPlayerCard(CSteamID steamID, int index)
        {
            var cards = GetPlayerCards(steamID);
            if (cards.IsValidIndex(index))
                return cards[index];
            else
                return null;
        }

        public BankCard GetPlayerCard(CSteamID steamID, string id)
        {
            return GetPlayerCards(steamID).Find(x => x.Id == id);
        }

        private JObject GetPlayerAccount(CSteamID steamID)
        {
            var result = _getPlayerTransactionMethod.Invoke(_databaseInstance, new object[] { steamID });
            return JObject.FromObject(result);
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
    }
}
