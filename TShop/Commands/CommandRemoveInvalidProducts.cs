using System.Collections.Generic;
using System.Threading.Tasks;
using Rocket.API;
using SDG.Unturned;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandRemoveInvalidProducts : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "RemoveInvalidProducts";
        public override string Help => "Removes all invalid products from the database.";
        public override string Syntax => "";
        public override List<string> Aliases => new List<string> { "rminvalidproducts", "rminvalidprods", "deleteinvalidproducts", "delinvalidprods" };
        public override List<string> Permissions => new List<string> { "tshop.commands.removeinvalidproducts" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            if (TShop.IsCleanupInProgress)
            {
                Plugin.SendCommandReply(caller, "error_command_removeinvalidproducts_cleanup_in_progress");
                return true;
            }
                
            TShop.IsCleanupInProgress = true;
            Plugin.SendCommandReply(caller, "command_removeinvalidproducts_cleanup_started");
            var logger = Plugin.GetLogger();
            logger.Debug("Removing invalid products from the database...");
            List<Product> products = await TShop.DatabaseManager.GetProductsAsync();
            List<Product> productsToRemove = products
                .FindAll(x => Assets.find(x.IsVehicle ? EAssetType.VEHICLE : EAssetType.ITEM, x.UnturnedId) == null);

            foreach (var product in productsToRemove)
            {
                logger.Debug($"Removing invalid product: {product.DisplayName} ({product.UnturnedId})");
                await TShop.DatabaseManager.RemoveProductAsync(product.UnturnedId, product.IsVehicle);
            }
            
            logger.Debug($"There were {productsToRemove.Count} invalid products out from {products.Count}.");
            logger.Debug("Invalid products removed from the database. Cleanup finished.");
            Plugin.SendCommandReply(caller, "command_removeinvalidproducts_cleanup_finished", productsToRemove.Count, products.Count);
            TShop.IsCleanupInProgress = false;
            return true;
        }
    }
}