using Rocket.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandMigrateZaupDB : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "migratezaupdb";
        public override string Help => "Migrates data from the database of the zaupshop plugin.";
        public override string Syntax => "[itemtablename] [vehicletablename]";
        public override List<string> Aliases => new List<string> { "mzdb" };
        public override List<string> Permissions => new List<string> { "tshop.admin.migratezaupdb", "tshop.commands.migratezaupdb" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            if (args.Length != 2)
            {
                TShop.Instance.SendCommandReply(caller, "error_command_migrate_args");
                return true;
            }

            try
            {
                TShop.Logger.LogRichWarning("Started migrating the zaup db...");
                List<ZaupProduct> products = await TShop.DatabaseManager.GetZaupProductsAsync(args[0], args[1]);

                TShop.Logger.LogRichWarning("Migrating items...");
                int successCount = 0;
                List<ZaupProduct> productsToCheck = products.FindAll(x => !x.IsVehicle);
                foreach (ZaupProduct product in productsToCheck)
                {
                    if (UAssetHelper.FindItemAsset(product.UnturnedId) == null)
                    {
                        TShop.Logger.LogRichWarning(
                            $"&6Failed to get &citem &6asset with &e'{product.UnturnedId}' &6id.");
                        continue;
                    }

                    await TShop.DatabaseManager.AddProductAsync(product.UnturnedId, false, null, product.BuyCost,
                        product.SellCost, false, "");
                    successCount++;
                }

                TShop.Logger.LogRich(
                    $"&a{successCount}&6/&2{productsToCheck.Count} &6items have been successfully migrated to TShop's table.");

                TShop.Logger.LogRichWarning("Migrating vehicles...");
                successCount = 0;
                productsToCheck = products.FindAll(x => x.IsVehicle);
                foreach (ZaupProduct product in productsToCheck)
                {
                    if (UAssetHelper.FindVehicleAsset(product.UnturnedId) == null)
                    {
                        TShop.Logger.LogRichWarning(
                            $"&6Failed to get &cvehicle &6asset with &e'{product.UnturnedId}' &6id.");
                        continue;
                    }

                    await TShop.DatabaseManager.AddProductAsync(product.UnturnedId, true, null, product.BuyCost,
                        product.SellCost, false, "");
                    successCount++;
                }

                TShop.Logger.LogRich(
                    $"&a{successCount}&6/&2{productsToCheck.Count} &6vehicles have been successfully migrated to TShop's table.");
                TShop.Logger.LogRich(
                    "&bIf there are any items or vehicles that were not migrated then please check Zaup's database or the workshop mod on the server. The problem is not on TShop's side.");
                TShop.Instance.SendCommandReply(caller, "success_migrate");
            }
            catch (Exception ex)
            {
                TShop.Instance.SendCommandReply(caller, "error_migrate_console");
                TShop.Logger.LogError("Migration error: " + ex);
            }

            return true;
        }
    }
}
