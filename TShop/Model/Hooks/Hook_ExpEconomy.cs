using Newtonsoft.Json.Linq;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;

namespace Tavstal.TShop.Compability.Hooks
{
    public class ExpEconomyHook : Hook, IEconomyProvider
    {
        public string GetCurrencyName()
        {
            string value = "Exp";
            try
            {
                value = GetConfigValue<string>("MoneyName").ToString();
            }
            catch { }
            return value;
        }

        public ExpEconomyHook() : base("expeconomy_tshop", true) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading ExpEconomy hook...");

                TShop.Logger.LogException("Currency Name >> " + GetCurrencyName());
                TShop.Logger.LogException("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance").ToString());
                TShop.Logger.Log("ExpEconomy hook loaded.");
            }
            catch (Exception e)
            {
                TShop.Logger.LogError("Failed to load ExpEconomy hook");
                TShop.Logger.LogError(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return TShop.Instance.Config.ExpMode;
        }

        public T GetConfigValue<T>(string VariableName)
        {
            try
            {
                return (T)Convert.ChangeType(TShop.Instance.Config.GetType().GetField(VariableName).GetValue(TShop.Instance.Config), typeof(T));
            }
            catch
            {
                try
                {
                    return (T)Convert.ChangeType(TShop.Instance.Config.GetType().GetProperty(VariableName).GetValue(TShop.Instance.Config), typeof(T));
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
                return JObject.FromObject(TShop.Instance.Config);
            }
            catch
            {
                TShop.Logger.LogError($"Failed to get config jobj.");
                return null;
            }
        }

        public bool HasBuiltInTransactionSystem() { return false; }
        public bool HasBuiltInBankCardSystem() { return false; }
        public decimal Withdraw(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return player.Experience -= (uint)amount;
        }

        public decimal Deposit(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return player.Experience += (uint)amount;
        }

        public decimal GetBalance(UnturnedPlayer player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return player.Experience;
        }

        public bool Has(UnturnedPlayer player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return (GetBalance(player) - amount) >= 0;
        }

        public void AddTransaction(UnturnedPlayer player, Transaction transaction)
        {
            
        }

        public decimal Withdraw(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Withdraw(UnturnedPlayer.FromCSteamID(player), amount, method);
        }

        public decimal Deposit(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Deposit(UnturnedPlayer.FromCSteamID(player), amount, method);
        }

        public decimal GetBalance(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return GetBalance(UnturnedPlayer.FromCSteamID(player), method);
        }

        public bool Has(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            return Has(UnturnedPlayer.FromCSteamID(player), amount, method);
        }

        public void AddTransaction(CSteamID player, Transaction transaction)
        {
            AddTransaction(UnturnedPlayer.FromCSteamID(player), transaction);
        }

        public List<Transaction> GetTransactions(UnturnedPlayer player)
        {
            return null;
        }

        public void AddPlayerCard(CSteamID steamID, BankCard newCard)
        {

        }

        public void UpdatePlayerCard(CSteamID steamID, string id, BankCardDetails newData)
        {

        }

        public void RemovePlayerCard(CSteamID steamID, int index, bool isReversed = false)
        {

        }

        public List<BankCard> GetPlayerCards(CSteamID steamID)
        {
            return null;
        }

        public BankCard GetPlayerCard(CSteamID steamID, int index)
        {
            return null;
        }

        public BankCard GetPlayerCard(CSteamID steamID, string id)
        {
            return null;
        }

        public BankCard GetCard(string id)
        {
            return null;
        }

        public string Localize(string translationKey, params object[] placeholder)
        {
            return Localize(false, translationKey, placeholder);
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            return TShop.Instance.Localize(addPrefix, translationKey, placeholder);
        }
    }
}
