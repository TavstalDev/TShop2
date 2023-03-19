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
#endregion

namespace Tavstal.TShop.Compability.Hooks
{
    public class UconomyHook : Hook, IEconomyProvider
    {
        public string GetCurrencyName()
        {
            string value = "Credits";
            try
            {
                value = GetConfigValue("MoneyName").ToString();
            }
            catch { }
            return value;
        }

        private MethodInfo _getBalanceMethod;
        private MethodInfo _increaseBalanceMethod;
        private MethodInfo _getTranslation;
        private object _databaseInstance;
        private object _pluginInstance;
        private object uconomyConfig;

        public UconomyHook() : base("uconomy_tphone") { }

        public override void OnLoad()
        {
            try
            {
                Logger.Log("Loading Uconomy hook...");

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

                Logger.LogException("Currency Name >> " + GetCurrencyName());
                Logger.LogException("Initial Balance >> " + GetConfigValue("InitialBalance").ToString());
                Logger.Log("Uconomy hook loaded.");
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Uconomy hook");
                Logger.LogError(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("uconomy"));
        }
        
        public object GetConfigValue(string VariableName)
        {
            object local = new object();
            try
            {
                local = uconomyConfig.GetType().GetField(VariableName).GetValue(uconomyConfig).ToString();
            }
            catch
            {
                local = null;
                Logger.LogError($"Failed to get '{VariableName}' variable!");
            }
            return local;
        }

        public decimal Withdraw(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.CSteamID.m_SteamID.ToString(), -amount
            });
        }

        public decimal Deposit(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.CSteamID.m_SteamID.ToString(), amount
            });
        }

        public decimal GetBalance(UnturnedPlayer player, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.CSteamID.m_SteamID.ToString()
            });
        }

        public bool Has(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (GetBalance(player) - amount) >= 0;
        }

        public void AddTransaction(UnturnedPlayer player, Transaction transaction)
        {

        }

        public decimal Withdraw(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.m_SteamID.ToString(), -amount
            });
        }

        public decimal Deposit(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_increaseBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.m_SteamID.ToString(), amount
            });
        }

        public decimal GetBalance(CSteamID player, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (decimal)_getBalanceMethod.Invoke(_databaseInstance, new object[] {
                player.m_SteamID.ToString()
            });
        }

        public bool Has(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK)
        {
            return (GetBalance(player) - amount) >= 0;
        }

        public string Translate(string translationKey, params object[] placeholder)
        {
            return ((string)_getTranslation.Invoke(_pluginInstance, new object[] { translationKey, placeholder })).Replace("((", "<").Replace("))", ">");
        }

        public void AddTransaction(CSteamID player, Transaction transaction)
        {

        }
    }
}
