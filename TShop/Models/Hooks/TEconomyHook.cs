using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Rocket.Core;
using Steamworks;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Hooks;
using Tavstal.TLibrary.Models.Logging;

namespace Tavstal.TShop.Models.Hooks
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TEconomyHook : Hook, IEconomyProvider
    {
        private readonly TLogger _logger;
        private MethodInfo? _getBalanceByCurrencyMethod;
        private MethodInfo? _increaseBalanceByCurrencyMethod;
        private MethodInfo? _addTransactionMethod;

        private MethodInfo? _getTransactionsMethod;
        private MethodInfo? _addBankCard;
        private MethodInfo? _updateBankCard;
        private MethodInfo? _removeBankCard;
        private MethodInfo? _getBankCard;
        private MethodInfo? _getBankCards;
        private MethodInfo? _getTranslation;
        private object? _databaseInstance;
        private object? _pluginInstance;
        private object? _pluginConfig;

        public TEconomyHook() : base(TShop.Instance, "thook_teconomy", false)
        {
            _logger = new TLogger(TShop.Instance.GetPluginName(), nameof(TEconomyHook), TShop.Instance.GetLogLevel());
        }

        protected override void OnLoad()
        {
            try
            {
                _logger.Info("Loading TEconomy hook...");

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
                _logger.Info("TEconomy hook loaded.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load TEconomy hook", ex);
            }
        }

        protected override void OnUnload() { }

        public override bool CanBeLoaded() =>
            R.Plugins.GetPlugins().Any(c => c.Name.EqualsIgnoreCase("teconomy"));


        #region IPluginProvider
        public T GetConfigValue<T>(string variableName)
        {
            try
            {
                return (T)Convert.ChangeType(_pluginConfig?.GetType().GetField(variableName).GetValue(_pluginConfig), typeof(T));
            }
            catch
            {
                try
                {
                    return (T)Convert.ChangeType(_pluginConfig?.GetType().GetProperty(variableName)?.GetValue(_pluginConfig), typeof(T));
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
                return JObject.FromObject(_pluginConfig!.GetType());
            }
            catch
            {
                _logger.Error($"Failed to get config jobj.");
                return null;
            }
        }

        public string Localize(bool addPrefix, string translationKey, params object[] placeholder)
        {
            try
            {
               return (string)_getTranslation?.Invoke(_pluginInstance, new object[] { addPrefix, translationKey, placeholder })!;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to localize {translationKey}", ex);
                return translationKey;
            }
        }

        public string Localize(string translationKey, params object[] placeholder) =>
            Localize(false, translationKey, placeholder);
        #endregion

        #region Basic Economy Methods
        public async Task<decimal> WithdrawAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_increaseBalanceByCurrencyMethod == null)
                {
                    _logger.Error("Failed to withdraw: _increaseBalanceByCurrencyMethod is null.");
                    return -1;
                }
                
                Task<decimal> task = (Task<decimal>)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance,
                    new object[]
                    {
                        player.m_SteamID, -amount, method
                    });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to withdraw {amount} from player {player.m_SteamID} using method {method}.", ex);
                return -1;
            }
        }

        public async Task<decimal> DepositAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_increaseBalanceByCurrencyMethod == null)
                {
                    _logger.Error("Failed to deposit: _increaseBalanceByCurrencyMethod is null.");
                    return -1;
                }

                Task<decimal> task = (Task<decimal>)_increaseBalanceByCurrencyMethod.Invoke(_databaseInstance,
                    new object[]
                    {
                        player.m_SteamID, +amount, method
                    });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to deposit {amount} to player {player.m_SteamID} using method {method}.", ex);
                return -1;
            }
        }

        public async Task<decimal> GetBalanceAsync(CSteamID player, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            try
            {
                if (_getBalanceByCurrencyMethod == null)
                {
                    _logger.Error("Failed to get balance: _getBalanceByCurrencyMethod is null.");
                    return -1;
                }
                
                Task<decimal> task = (Task<decimal>)_getBalanceByCurrencyMethod.Invoke(_databaseInstance, new object[]
                {
                    player.m_SteamID, method
                });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get balance for player {player.m_SteamID} using method {method}.", ex);
                return -1;
            }
        }

        public async Task<bool> HasAsync(CSteamID player, decimal amount, EPaymentMethod method = EPaymentMethod.BANK_ACCOUNT)
        {
            if (amount >= 0)
                return await GetBalanceAsync(player, method) - amount >= 0;
            return await GetBalanceAsync(player, method) - amount * -1 >= 0;
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

        public bool HasTransactionSystem() => true;

        public bool HasBankCardSystem() => true;

        public async Task AddTransactionAsync(CSteamID player, ITransaction transaction)
        {
            try
            {
                if (_addTransactionMethod == null)
                {
                    _logger.Error("Failed to add transaction: _addTransactionMethod is null.");
                    return;
                }

                Task task = (Task)_addTransactionMethod.Invoke(_databaseInstance,
                    new object[]
                    {
                        transaction.Type, transaction.PaymentMethod, transaction.StoreName, transaction.PayerId,
                        transaction.PayeeId, transaction.Amount, transaction.Date
                    });
                await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to add transaction for player {player.m_SteamID}.", ex);
            }
        }

        public async Task<List<ITransaction>> GetTransactionsAsync(CSteamID player)
        {
            try
            {
                if (_getTransactionsMethod == null)
                {
                    _logger.Error("Failed to get transactions: _getTransactionsMethod is null.");
                    return new List<ITransaction>();
                }
                
                Task<List<ITransaction>> task =
                    (Task<List<ITransaction>>)_getTransactionsMethod.Invoke(_databaseInstance,
                        new object[] { player.m_SteamID });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get transactions for player {player.m_SteamID}.", ex);
                return new List<ITransaction>();
            }
        }

        public async Task AddBankCardAsync(CSteamID steamID, IBankCard newCard)
        {
            try
            {
                if (_addBankCard == null)
                {
                    _logger.Error("Failed to add bank card: _addBankCard is null.");
                    return;
                }

                Task task = (Task)_addBankCard.Invoke(_databaseInstance,
                    new object[]
                    {
                        newCard.Iban, newCard.Cvc, newCard.PinCode, newCard.HolderId, newCard.BalanceUsed,
                        newCard.BalanceLimit, newCard.ExpireDate
                    });
                await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to add bank card for player {steamID.m_SteamID}.", ex);
            }
        }

        public async Task UpdateBankCardAsync(string cardId, decimal limitUsed, bool isActive)
        {
            try
            {
                if (_updateBankCard == null)
                {
                    _logger.Error("Failed to update bank card: _updateBankCard is null.");
                    return;
                }
                
                Task task = (Task)_updateBankCard.Invoke(_databaseInstance,
                    new object[] { cardId, limitUsed, isActive });
                await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to update bank card for player {cardId}.", ex);
            }
        }

        public async Task RemoveBankCardAsync(string cardId)
        {
            try
            {
                if (_removeBankCard == null)
                {
                    _logger.Error("Failed to remove bank card: _removeBankCard is null.");
                    return;
                }

                Task task = (Task)_removeBankCard.Invoke(_databaseInstance, new object[] { cardId });
                await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to remove bank card for player {cardId}.", ex);
            }
        }

        public async Task<List<IBankCard>> GetBankCardsByPlayerAsync(CSteamID steamID)
        {
            try
            {
                if (_getBankCards == null)
                {
                    _logger.Error("Failed to get bank cards: _getBankCards is null.");
                    return new List<IBankCard>();
                }

                Task<List<IBankCard>> task =
                    (Task<List<IBankCard>>)_getBankCards.Invoke(_databaseInstance, new object[] { steamID.m_SteamID });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get bank cards for player {steamID}.", ex);
                return new List<IBankCard>();
            }
        }

        public async Task<IBankCard> GetBankCardByIdAsync(string cardId)
        {
            try 
            {
                if (_getBankCard == null)
                {
                    _logger.Error("Failed to get bank card: _getBankCard is null.");
                    return null!;
                }
                
                Task<IBankCard> task = (Task<IBankCard>)_getBankCard.Invoke(_databaseInstance, new object[] { cardId });
                return await task;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get bank card for player {cardId}.", ex);
                return null!;
            }
        }
        #endregion
    }
}
