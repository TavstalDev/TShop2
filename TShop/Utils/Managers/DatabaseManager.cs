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

namespace Tavstal.TShop
{
    /// <summary>
    /// Represents a database manager derived from <see cref="DatabaseManagerBase"/>.
    /// </summary>
    public class DatabaseManager : DatabaseManagerBase
    {
        private static TShopConfiguration _pluginConfig => TShop.Instance.Config;

        public DatabaseManager(IConfigurationBase configuration) : base(TShop.Instance, configuration)
        {
        }

        /// <summary>
        /// Checks the schema of the database.
        /// </summary>
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
                    if (await connection.DoesTableExistAsync<Product>(_pluginConfig.Database.DatabaseTable_Products))
                        await connection.CheckTableAsync<Product>(_pluginConfig.Database.DatabaseTable_Products);
                    else
                        await connection.CreateTableAsync<Product>(_pluginConfig.Database.DatabaseTable_Products);

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

        /// <summary>
        /// Asynchronously adds a product to the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="buycost">The buy cost of the product.</param>
        /// <param name="sellcost">The sell cost of the product.</param>
        /// <param name="enableperm">A boolean value indicating whether the product requires permission to be enabled.</param>
        /// <param name="permission">The permission required to enable the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully added to the database.
        /// </returns>
        public async Task<bool> AddProductAsync(ushort id, bool isVehicle, decimal buycost, decimal sellcost, bool enableperm, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.AddTableRowAsync(tableName: _pluginConfig.Database.DatabaseTable_Products, new Product(id, isVehicle, buycost, sellcost, enableperm, permission, false, 0));
        }

        /// <summary>
        /// Asynchronously removes a product from the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully removed from the database.
        /// </returns>
        public async Task<bool> RemoveProductAsync(ushort id, bool isVehicle)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.RemoveTableRowAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", null);
        }

        /// <summary>
        /// Asynchronously updates a product in the database.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="buycost">The new buy cost of the product.</param>
        /// <param name="sellcost">The new sell cost of the product.</param>
        /// <param name="enablepermission">A boolean value indicating whether the product requires permission to be enabled.</param>
        /// <param name="permission">The new permission required to enable the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully updated in the database.
        /// </returns>
        public async Task<bool> UpdateProductAsync(ushort id, bool isVehicle, decimal buycost, decimal sellcost, bool enablepermission, string permission)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.UpdateTableRowAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", new List<SqlParameter>
            {
                SqlParameter.Get<Product>(x => x.BuyCost, buycost),
                SqlParameter.Get<Product>(x => x.SellCost, sellcost),
                SqlParameter.Get<Product>(x => x.HasPermission, enablepermission),
                SqlParameter.Get<Product>(x => x.Permission, permission)
            });
        }

        /// <summary>
        /// Asynchronously updates a product in the database with discount information.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="isVehicle">A boolean value indicating whether the product is a vehicle.</param>
        /// <param name="isdiscounted">A boolean value indicating whether the product is discounted.</param>
        /// <param name="percent">The discount percentage of the product.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the product was successfully updated in the database.
        /// </returns>
        public async Task<bool> UpdateProductAsync(ushort id, bool isVehicle, bool isdiscounted, decimal percent)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.UpdateTableRowAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, $"UnturnedId='{id}' AND IsVehicle='{isVehicle}'", new List<SqlParameter>
            {
                SqlParameter.Get<Product>(x => x.IsDiscounted, isdiscounted),
                SqlParameter.Get<Product>(x => x.DiscountPercent, percent)
            });
        }

        /// <summary>
        /// Asynchronously retrieves a list of products from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of products retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetProductsAsync()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, whereClause: string.Empty, null);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of items retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetItemsAsync()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, whereClause: $"IsVehicle='{false}'", null);
        }

        /// <summary>
        /// Asynchronously retrieves a list of items from the database based on the specified filter.
        /// </summary>
        /// <param name="filter">An optional filter to apply to the items.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of items retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetItemsAsync(EItemFilter? filter)
        {
            List<Product> items = await GetItemsAsync();
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

        /// <summary>
        /// Asynchronously retrieves a list of vehicles from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of vehicles retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetVehiclesAsync()
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowsAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, whereClause: $"IsVehicle='{true}'", null);
        }

        /// <summary>
        /// Asynchronously retrieves a list of vehicles from the database based on the specified engine type.
        /// </summary>
        /// <param name="engine">An optional engine type to filter the vehicles.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of vehicles retrieved from the database.
        /// </returns>
        public async Task<List<Product>> GetVehiclesAsync(EEngine? engine)
        {
            List<Product> vehicles = await GetVehiclesAsync();
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

        /// <summary>
        /// Asynchronously finds an item in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the item to find.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the item found in the database, if any.
        /// </returns>
        public async Task<Product> FindItemAsync(ushort id)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, whereClause: $"UnturnedId='{id}' AND IsVehicle='{false}'", null);
        }

        /// <summary>
        /// Asynchronously finds a vehicle in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle to find.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains the vehicle found in the database, if any.
        /// </returns>
        public async Task<Product> FindVehicleAsync(ushort id)
        {
            MySqlConnection MySQLConnection = CreateConnection();
            return await MySQLConnection.GetTableRowAsync<Product>(tableName: _pluginConfig.Database.DatabaseTable_Products, whereClause: $"UnturnedId='{id}' AND IsVehicle='{true}'", null);
        }

        #region Zaup
        /// <summary>
        /// Creates a MySQL connection to the Zaup database.
        /// </summary>
        /// <returns>
        /// A MySqlConnection object representing the connection to the Zaup database.
        /// </returns>
        public MySqlConnection CreateZaupConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (_pluginConfig.Database.Port == 0)
                {
                    _pluginConfig.Database.Port = 3306;
                    _pluginConfig.SaveConfig();
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
                    _pluginConfig.Database.Host,
                    _pluginConfig.Database.DatabaseName,
                    _pluginConfig.Database.UserName,
                    _pluginConfig.Database.UserPassword,
                    _pluginConfig.Database.Port
                }));
            }
            catch (Exception exception)
            {
                TShop.Logger.LogException(exception);
            }
            return mySqlConnection;
        }

        /// <summary>
        /// Asynchronously retrieves a list of Zaup products from the specified item and vehicle tables in the Zaup database.
        /// </summary>
        /// <param name="itemTable">The name of the table containing item products.</param>
        /// <param name="vehicleTable">The name of the table containing vehicle products.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a list of Zaup products retrieved from the specified tables.
        /// </returns>
        public async Task<List<ZaupProduct>> GetZaupProductsAsync(string itemTable, string vehicleTable)
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
