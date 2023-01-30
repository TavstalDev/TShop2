#region References
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Managers;
using Tavstal.TShop.Compability;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using Rocket.Core.Permissions;
using Rocket.API.Serialisation;
using Rocket.Core.Commands;
using System.Text.RegularExpressions;
using Rocket.Unturned.Events;
using System.IO;
using UnityEngine.Networking;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using Newtonsoft.Json.Linq;
#endregion

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
        private MethodInfo _getTranslation;
        private object _databaseInstance;
        private object _pluginInstance;
        private object teconomyConfig;

        public TEconomyHook() : base("teconomy_tphone") { }

        public override void OnLoad()
        {
            try
            {
                Logger.Log("Loading TEconomy hook...");

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

                _getTranslation = _pluginInstance.GetType().GetMethod("Translate", new[] { typeof(string), typeof(object[]) });

                //var ucPlugin = _pluginInstance.GetType().GetProperty("uconomyPlugin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_pluginInstance);
                //var ucType = ucPlugin.GetType().Assembly.GetType("fr34kyn01535.Uconomy.Uconomy");
                //var ucInstance = ucType.GetField("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(ucPlugin);

                //Logger.LogException("Currency Name >> " + GetCurrencyName());
                //Logger.LogException("Initial Balance >> " + GetConfigValue("InitialBalance").ToString());
                Logger.Log("TEconomy hook loaded.");
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load TEconomy hook");
                Logger.LogError(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("teconomy"));
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
                Logger.LogError($"Failed to get '{VariableName}' variable!");
            }
            return local;
        }

        public decimal Withdraw(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            return Withdraw(player.CSteamID, amount, method);
        }

        public decimal Deposit(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            return Deposit(player.CSteamID, amount, method);
        }

        public decimal GetBalance(UnturnedPlayer player, EPaymentMethod method = EPaymentMethod.bank)
        {
            return GetBalance(player.CSteamID, method);
        }

        public bool Has(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            return (GetBalance(player) - amount) >= 0;
        }

        public void AddTransaction(UnturnedPlayer player, Transaction transaction)
        {
            AddTransaction(player.CSteamID, transaction);
        }

        public decimal Withdraw(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            switch (method)
            {
                case EPaymentMethod.bank:
                    {
                        return (decimal)_increaseBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount });
                    }
                case EPaymentMethod.crypto:
                    {
                        return (decimal)_increaseCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, -amount });
                    }
                case EPaymentMethod.wallet:
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

        public decimal Deposit(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            switch (method)
            {
                case EPaymentMethod.bank:
                    {
                        return (decimal)_increaseBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, amount });
                    }
                case EPaymentMethod.crypto:
                    {
                        return (decimal)_increaseCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player, amount });
                    }
                case EPaymentMethod.wallet:
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

        public decimal GetBalance(CSteamID player, EPaymentMethod method = EPaymentMethod.bank)
        {
            switch (method)
            {
                case EPaymentMethod.bank:
                    {
                        return (decimal)_getBankBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player});
                    }
                case EPaymentMethod.crypto:
                    {
                        return (decimal)_getCryptoBalanceMethod.Invoke(_databaseInstance, new object[] {
                            player});
                    }
                case EPaymentMethod.wallet:
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

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.bank)
        {
            return (GetBalance(player, method) - amount) >= 0;
        }

        public void AddTransaction(CSteamID player, Transaction transaction)
        {
            _addTransactionMethod.Invoke(_databaseInstance, new object[] { player, JObject.FromObject(transaction).ToString(Formatting.None) });
        }

        public string Translate(string translationKey, params object[] placeholder)
        {
            return ((string)_getTranslation.Invoke(_pluginInstance, new object[] { translationKey, placeholder })).Replace("((", "<").Replace("))", ">");
        }
    }
}
