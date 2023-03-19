using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
    [Serializable]
    public class DatabaseData
    {
        public string DatabaseAddress { get; set; }
        public int DatabasePort { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseTable_Items { get; set; }
        public string DatabaseTable_Vehicles { get; set; }

        public DatabaseData(string address, int port, string user, string password, string database, string table_items, string table_vehicles)
        {
            DatabaseAddress = address;
            DatabasePort = port;
            DatabaseUser = user;
            DatabasePassword = password;
            DatabaseName = database;
            DatabaseTable_Items = table_items;
            DatabaseTable_Vehicles = table_vehicles;
        }

        public DatabaseData() { }
    }
}