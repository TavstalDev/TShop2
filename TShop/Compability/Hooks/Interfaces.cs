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

namespace Tavstal.TShop.Compability
{
    public interface IPluginProvider
    {
        object GetConfigValue(string VariableName);
        string Translate(string translationKey, params object[] placeholder);
    }

    public interface IEconomyProvider : IPluginProvider
    {
        string GetCurrencyName();

        decimal Withdraw(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        decimal Deposit(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        decimal GetBalance(UnturnedPlayer player, EUconomyMethod method = EUconomyMethod.BANK);

        bool Has(UnturnedPlayer player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        void AddTransaction(UnturnedPlayer player, Transaction transaction);

        decimal Withdraw(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        decimal Deposit(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        decimal GetBalance(CSteamID player, EUconomyMethod method = EUconomyMethod.BANK);

        bool Has(CSteamID player, decimal amount, EUconomyMethod method = EUconomyMethod.BANK);

        void AddTransaction(CSteamID player, Transaction transaction);
    }
}
