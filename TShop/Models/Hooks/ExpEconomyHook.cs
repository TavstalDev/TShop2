using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Player;
using Steamworks;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Hooks;

namespace Tavstal.TShop.Models.Hooks
{
    public class ExpEconomyHook : Hook, IEconomyProvider
    {
        public ExpEconomyHook() : base(TShop.Instance, "thook_expconomy", true) { }

        public override void OnLoad()
        {
            try
            {
                TShop.Logger.Log("Loading ExpEconomy hook...");

                TShop.Logger.Exception("Currency Name >> " + GetCurrencyName());
                TShop.Logger.Exception("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance"));
                TShop.Logger.Log("ExpEconomy hook loaded.");
            }
            catch (Exception e)
            {
                TShop.Logger.Error("Failed to load ExpEconomy hook");
                TShop.Logger.Error(e.ToString());
            }
        }

        public override void OnUnload() { }

        public override bool CanBeLoaded()
        {
            return TShop.Instance.Config.ExpMode;
        }

        #region IPluginProvider Methods
        public T GetConfigValue<T>(string variableName)
        {
            try
            {
                return (T)Convert.ChangeType(TShop.Instance.Config.GetType().GetField(variableName).GetValue(TShop.Instance.Config), typeof(T));
            }
            catch
            {
                try
                {
                    // ReSharper disable PossibleNullReferenceException
                    return (T)Convert.ChangeType(TShop.Instance.Config.GetType().GetProperty(variableName).GetValue(TShop.Instance.Config), typeof(T));
                    // ReSharper restore PossibleNullReferenceException
                }
                catch
                {
                    TShop.Logger.Error($"Failed to get '{variableName}' variable!");
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
                TShop.Logger.Error($"Failed to get config jobj.");
                return null;
            }
        }

        public string Localize(string translationKey, params object[] placeholder)
        {
            return Localize(false, translationKey, placeholder);
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            return TShop.Instance.Localize(addPrefix, translationKey, placeholder);
        }
        #endregion

        #region Economy Methods
#pragma warning disable IDE0060 // Remove unused parameter
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

        public async Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = Task.Run(() => Withdraw(player, amount, method));
            return await task;
        }

        public async Task<decimal> DepositAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = Task.Run(() => Deposit(player, amount, method));
            return await task;
        }

        public async Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<decimal> task = Task.Run(() => GetBalance(UnturnedPlayer.FromCSteamID(player), method));
            return await task;
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            Task<bool> task = Task.Run(() => Has(UnturnedPlayer.FromCSteamID(player), amount, method));
            return await task;
        }


        public string GetCurrencyName()
        {
            string value = "Exp";
            try
            {
                value = GetConfigValue<string>("MoneyName");
            }
            catch { /* ignore */ }
            return value;
        }
#pragma warning restore IDE0060 // Remove unused parameter
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
