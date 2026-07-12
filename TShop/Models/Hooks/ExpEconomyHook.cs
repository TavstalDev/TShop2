using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Rocket.Unturned.Player;
using Steamworks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Models.Plugin;

namespace Tavstal.TShop.Models.Hooks
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ExpEconomyHook : Hook, IEconomyProvider
    {
        private readonly TLogger _logger;

        public ExpEconomyHook() : base(TShop.Instance, "thook_expconomy", true)
        {
            _logger = new TLogger(TShop.Instance.GetPluginName(), nameof(ExpEconomyHook), TShop.Instance.GetLogLevel());
        }

        protected override void OnLoad()
        {
            try
            {
                _logger.Info("Loading ExpEconomy hook...");

                _logger.Info("Currency Name >> " + GetCurrencyName());
                _logger.Info("Initial Balance >> " + GetConfigValue<decimal>("InitialBalance"));
                _logger.Info("ExpEconomy hook loaded.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load ExpEconomy hook", ex);
            }
        }

        protected override void OnUnload() { }

        public override bool CanBeLoaded() =>
            TShop.Instance.Config.ExpMode;

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
                    _logger.Error($"Failed to get '{variableName}' variable!");
                    return default!;
                }
            }
        }

        public JObject? GetConfig()
        {
            try
            {
                return JObject.FromObject(TShop.Instance.Config);
            }
            catch
            {
                _logger.Error($"Failed to get config jobj.");
                return null;
            }
        }

        public string Localize(string translationKey, params object[] placeholder) =>
            Localize(false, translationKey, placeholder);

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder) =>
            TShop.Instance.Localize(addPrefix, translationKey, placeholder);
        #endregion

        #region Economy Methods

        public Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromCSteamID(player);
            if (uPlayer == null)
                return Task.FromResult(-1m);
            return Task.FromResult((decimal)(uPlayer.Experience -= (uint)amount));
        }

        public Task<decimal> DepositAsync(CSteamID player, decimal amount,
            EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromCSteamID(player);
            if (uPlayer == null)
                return Task.FromResult(-1m);
            return Task.FromResult((decimal)(uPlayer.Experience += (uint)amount));
        }

        public Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromCSteamID(player);
            if (uPlayer == null)
                return Task.FromResult(-1m);
            return Task.FromResult((decimal)uPlayer.Experience);
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            var result = await GetBalanceAsync(player, method);
            return result >= amount;
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

        #endregion

        #region TEconomy Methods

        public bool HasTransactionSystem() => false;

        public bool HasBankCardSystem() => false;
        
        public Task AddTransactionAsync(CSteamID player, ITransaction transaction) =>
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");

        public Task<List<ITransaction>> GetTransactionsAsync(CSteamID player) =>
            throw new NotImplementedException($"Transaction system is not supported by the current economy plugin.");

        public Task AddBankCardAsync(CSteamID steamID, IBankCard newCard) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task UpdateBankCardAsync(string cardId, decimal limitUsed, bool isActive) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task RemoveBankCardAsync(string cardId) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task<List<IBankCard>> GetBankCardsByPlayerAsync(CSteamID steamID) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");

        public Task<IBankCard> GetBankCardByIdAsync(string cardId) =>
            throw new NotImplementedException($"Bank card system is not supported by the current economy plugin.");
        
        #endregion
    }
}
