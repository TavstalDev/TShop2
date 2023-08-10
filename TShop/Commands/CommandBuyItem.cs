using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop
{
    public class CommandBuyItem : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "buy";
        public string Help => "Buys a specific amount of items.";
        public string Syntax => "[itemID | itemName] <amount>";
        public List<string> Aliases => new List<string> { "buyi", "buyitem" };
        public List<string> Permissions => new List<string> { "tshop.buy.item" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 1 ||args.Length == 2)
            {
                ushort id = 0;
                int amount = 1;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { }

                if (args.Length == 2)
                {
                    try
                    {
                        int.TryParse(args[1], out amount);
                    }
                    catch { }
                }

                ItemAsset asset = null;

                if (id > 0)
                    asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                else
                    asset = UAssetHelper.FindItemAsset(args[0]);

                if (asset == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_item_not_exists", args[0]);
                    return;
                }
                id = asset.id;

                ShopItem item = TShop.Database.FindItem(id);
                if (item == null)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_item_not_added", args[0]);
                    return;
                }

                decimal cost =  item.GetBuyCost(amount);

                if (TShop.economyProvider.GetBalance(callerPlayer) < cost)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_balance_not_enough");
                    return;
                }

                if (cost == 0)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_item_buy_error");
                    return;
                }

                TShop.economyProvider.Withdraw(callerPlayer, cost);
                for (int i = 0; i < amount; i++)
                {
                    if (!callerPlayer.Inventory.tryAddItem(new Item(asset.id, true), false))
                        ItemManager.dropItem(new Item(asset.id, true), callerPlayer.Position, true, true, false);
                }
                TShop.economyProvider.AddTransaction(callerPlayer, new Transaction(ETransaction.PURCHASE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), callerPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "success_item_buy", asset.itemName, amount, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol"));
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(),  "error_command_buyitem_args");
        }
    }
}
