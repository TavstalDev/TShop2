using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop
{
    public class CommandCostItem : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "cost";
        public string Help => "Checks the cost of a specific item.";
        public string Syntax => "[itemID]";
        public List<string> Aliases => new List<string> { "costitem", "costi" };
        public List<string> Permissions => new List<string> { "tshop.cost.item" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 1)
            {
                ushort id = 0;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { }

                ItemAsset asset = null;

                if (id > 0)
                    asset = UAssetHelper.FindItemAsset(id);
                else
                    asset = UAssetHelper.FindItemAsset(args[0]);

                if (asset == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_item_not_found", args[0]);
                    return;
                }
                id = asset.id;

                Product item = await TShop.Database.FindItemAsync(id);
                if (item == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_item_not_added", args[0]);
                    return;
                }

                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "success_item_cost", asset.itemName, item.GetBuyCost(), item.GetSellCost(), TShop.EconomyProvider.GetConfigValue<string>("MoneySymbol"));
            }
            else
                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_command_costitem_args");
        }
    }
}
