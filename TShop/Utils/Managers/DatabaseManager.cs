using MySql.Data.MySqlClient;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Database;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Managers;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Model.Classes;
using Tavstal.TShop.Model.Enums;
using Tavstal.TShop.Model.Interfaces;

namespace Tavstal.TShop
{
    public class DatabaseManager : DatabaseManagerBase
    {
        private static TShopConfiguration pluginConfig => TShop.Instance.Config;

        public DatabaseManager(IConfigurationBase configuration) : base(TShop.Instance, configuration)
        {
        }

        protected override async void CheckSchema()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    if (!await connection.OpenSafe())
                        TShop.IsConnectionAuthFailed = true;
                    if (connection.State != System.Data.ConnectionState.Open)
                        throw new Exception("# Failed to connect to the database. Please check the plugin's config file.");

                    //Item Shop
                    if (await connection.DoesTableExistAsync<Product>(pluginConfig.Database.DatabaseTable_Products))
                        await connection.CheckTableAsync<Product>(pluginConfig.Database.DatabaseTable_Products);
                    else
                        await connection.CreateTableAsync<Product>(pluginConfig.Database.DatabaseTable_Products);

                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException("Error in checkSchema:");
                TShop.Logger.LogError(ex);
            }
        }

        public async Task<bool> AddProduct(ushort id, bool isVehicle, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.AddTableRowAsync(tableName: pluginConfig.Database.DatabaseTable_Products, new Product(id, isVehicle, buycost, sellcost, enableperm, permission, false, 0));
        }

        public async Task<bool> RemoveProduct(ushort id, bool isVehicle)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.RemoveTableRowAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", null);
        }

        public async Task<bool> UpdateProduct(ushort id, bool isVehicle, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.UpdateTableRowAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", new List<SqlParameter>
            {
                SqlParameter.Get<Product>(x => x.BuyCost, buycost),
                SqlParameter.Get<Product>(x => x.SellCost, sellcost),
                SqlParameter.Get<Product>(x => x.HasPermission, enablepermission),
                SqlParameter.Get<Product>(x => x.Permission, permission)
            });
        }
        public async Task<bool> UpdateProduct(ushort id, bool isVehicle, bool isdiscounted, decimal percent)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.UpdateTableRowAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", new List<SqlParameter>
            {
                SqlParameter.Get<Product>(x => x.IsDiscounted, isdiscounted),
                SqlParameter.Get<Product>(x => x.DiscountPercent, percent)
            });
        }
        public async Task<List<Product>> GetProducts()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, whereClause: string.Empty, null);
        }

        public async Task<List<Product>> GetItems()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, whereClause: $"IsVehicle='{false}'", null);
        }

        public async Task<List<Product>> GetItems(EItemFilter? filter)
        {
            List<Product> items = await GetItems();
            if (filter == null)
                return items;
            List<Product> local = new List<Product>();
            foreach (var item in items)
            {
                ItemAsset asset = UAssetHelper.FindItemAsset(item.UnturnedId);
                if (asset == null)
                    continue;

                switch (asset.type)
                {
                    case EItemType.HAT:
                    case EItemType.BACKPACK:
                    case EItemType.GLASSES:
                    case EItemType.MASK:
                    case EItemType.PANTS:
                    case EItemType.SHIRT:
                    case EItemType.VEST:
                        {
                            if (filter == EItemFilter.Clothing)
                                local.Add(item);

                            break;
                        }
                    case EItemType.FOOD:
                    case EItemType.WATER:
                        {
                            if (filter == EItemFilter.Food)
                                local.Add(item);
                            break;
                        }
                    case EItemType.MEDICAL:
                        {
                            if (filter == EItemFilter.Medical)
                                local.Add(item);
                            break;
                        }
                    case EItemType.TOOL:
                    case EItemType.COMPASS:
                    case EItemType.FISHER:
                    case EItemType.KEY:
                    case EItemType.OPTIC:
                    case EItemType.MAP:
                        {
                            if (filter == EItemFilter.Tools)
                                local.Add(item);
                            break;
                        }
                    case EItemType.BARRICADE:
                    case EItemType.BOX:
                    case EItemType.CLOUD:
                    case EItemType.FARM:
                    case EItemType.STORAGE:
                    case EItemType.LIBRARY:
                    case EItemType.GROWER:
                        {
                            if (filter == EItemFilter.Barricades)
                                local.Add(item);
                            break;
                        }
                    case EItemType.STRUCTURE:
                        {
                            if (filter == EItemFilter.Structures)
                                local.Add(item);
                            break;
                        }
                    case EItemType.SENTRY:
                    case EItemType.GENERATOR:
                    case EItemType.BEACON:
                        {
                            if (filter == EItemFilter.Electronic)
                                local.Add(item);
                            break;
                        }
                    case EItemType.VEHICLE_REPAIR_TOOL:
                    case EItemType.TIRE:
                        {
                            if (filter == EItemFilter.Vehicles)
                                local.Add(item);
                            break;
                        }
                    case EItemType.FUEL:
                    case EItemType.REFILL:
                    case EItemType.OIL_PUMP:
                        {
                            if (filter == EItemFilter.Fuel)
                                local.Add(item);
                            break;
                        }
                    case EItemType.MELEE:
                        {
                            if (filter == EItemFilter.Melees)
                                local.Add(item);
                            break;
                        }
                    case EItemType.GUN:
                    case EItemType.THROWABLE:
                        {
                            if (filter == EItemFilter.Guns)
                                local.Add(item);
                            break;
                        }
                    case EItemType.BARREL:
                    case EItemType.GRIP:
                    case EItemType.MAGAZINE:
                    case EItemType.SIGHT:
                    case EItemType.TACTICAL:
                        {
                            if (filter == EItemFilter.Attachments)
                                local.Add(item);
                            break;
                        }
                    case EItemType.ARREST_END:
                    case EItemType.ARREST_START:
                    case EItemType.CHARGE:
                    case EItemType.DETONATOR:
                    case EItemType.FILTER:
                    case EItemType.SUPPLY:
                    case EItemType.TANK:
                    case EItemType.TRAP:
                        {
                            if (filter == EItemFilter.Misc)
                                local.Add(item);
                            break;
                        }
                }
            }
            return local;
        }

        public async Task<List<Product>> GetVehicles()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, whereClause: $"IsVehicle='{true}'", null);
        }

        public async Task<List<Product>> GetVehicles(EEngine? engine)
        {
            List<Product> vehicles = await GetVehicles();
            if (engine == null)
                return vehicles;
            List<Product> local = new List<Product>();
            foreach (var vehicle in vehicles)
            {
                VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicle.UnturnedId);
                if (asset == null)
                    continue;

                if (asset.engine == engine)
                    local.Add(vehicle);
            }
            return local;
        }
        public async Task<Product> FindItem(ushort id)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, whereClause: $"UnturnedId='{id}' AND IsVehicle='{false}'", null);
        }

        public async Task<Product> FindVehicle(ushort id)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowAsync<Product>(tableName: pluginConfig.Database.DatabaseTable_Products, whereClause: $"UnturnedId='{id}' AND IsVehicle='{true}'", null);
        }

        #region Zaup
        public MySqlConnection CreateZaupConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (pluginConfig.Database.Port == 0)
                {
                    pluginConfig.Database.Port = 3306;
                    pluginConfig.SaveConfig();
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    pluginConfig.Database.Host,
                    pluginConfig.Database.DatabaseName,
                    pluginConfig.Database.UserName,
                    pluginConfig.Database.UserPassword,
                    pluginConfig.Database.Port
                }));
            }
            catch (Exception exception)
            {
                TShop.Logger.LogException(exception);
            }
            return mySqlConnection;
        }

        public async Task<List<ZaupProduct>> GetZaupProducts(string itemTable, string vehicleTable)
        {
            List<ZaupProduct> i = new List<ZaupProduct>();
            try
            {
                MySqlConnection MySQLConnection = CreateConnection();
                MySqlCommand MySQLCommand = MySQLConnection.CreateCommand();
                await MySQLConnection.OpenSafe();

                // Get Items
                MySQLCommand.CommandText = "SELECT * FROM " + itemTable;
                var Reader = await MySQLCommand.ExecuteReaderAsync();
                while (await Reader.ReadAsync())
                    i.Add(new ZaupProduct(Reader.GetUInt16("id"), false, Reader.GetDecimal("cost"), Reader.GetDecimal("buyback")));
                Reader.Close();

                // Get Vehicles
                MySQLCommand.CommandText = "SELECT * FROM " + vehicleTable;
                Reader = await MySQLCommand.ExecuteReaderAsync();
                while (await Reader.ReadAsync())
                    i.Add(new ZaupProduct(Reader.GetUInt16("id"), true, Reader.GetDecimal("cost"), 0));
                await MySQLConnection.CloseAsync();
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException(ex);
            }
            return i;
        }
        #endregion
    }
}
