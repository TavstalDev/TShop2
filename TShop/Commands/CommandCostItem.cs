using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandCostItem : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "cost";
        public override string Help => "Checks the cost of a specific item.";
        public override string Syntax => "[itemID]";
        public override List<string> Aliases => new List<string> { "costitem", "costi" };
        public override List<string> Permissions => new List<string> { "tshop.cost.item", "tshop.commands.cost.item" };
        public override List<ISubcommand>? SubCommands => null;

        protected override async Task<bool> HandleExecuteAsync(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;

            if (args.Length != 1)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_costitem_args");
                return true;
            }

            ushort.TryParse(args[0], out var id);
            var asset = id > 0 ? UAssetHelper.FindItemAsset(id) : UAssetHelper.FindItemAsset(args[0]);

            if (asset == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_found", args[0]);
                return true;
            }

            id = asset.id;

            Product? item = await TShop.DatabaseManager.FindItemAsync(id);
            if (item == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_item_not_added", args[0]);
                return true;
            }

            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_item_cost", asset.itemName,
                item.GetBuyCost(), item.GetSellCost(), TShop.EconomyProvider.GetCurrencyName());

            return true;
        }
    }
}
