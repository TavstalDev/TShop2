using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
    public class GithubFolders
    {
        public string FolderName { get; set; }
        public string FolderLink { get; set; }
        public int MinItemID { get; set; }
        public int MaxItemID { get; set; }
    }

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

    [Serializable]
    public class ShopItem
    {
        public ushort Id { get; set; }
        public decimal BuyCost { get; set; }
        public decimal SellCost { get; set; }
        public bool hasPermission { get; set; }
        public string Permission { get; set; }
        public bool isDiscounted { get; set; }
        public decimal discountPercent { get; set; }

        public ShopItem(ushort id, decimal buycost, decimal sellcost, string perm)
        {
            Id = id;
            BuyCost = buycost;
            SellCost = sellcost;
            hasPermission = true;
            Permission = perm;
        }

        public ShopItem(ushort id, decimal buycost, decimal sellcost)
        {
            Id = id;
            BuyCost = buycost;
            SellCost = sellcost;
            hasPermission = false;
            Permission = "TShop.item.";
        }

        public ShopItem(ushort id, decimal buycost, decimal sellcost, bool hasperm, string perm, bool isdiscount, decimal discount)
        {
            Id = id;
            BuyCost = buycost;
            SellCost = sellcost;
            hasPermission = hasperm;
            Permission = perm;
            isDiscounted = isdiscount;
            discountPercent = discount;
        }

        public decimal GetBuyCost(int amount = 1)
        {
            if (isDiscounted)
                return (BuyCost - BuyCost * (discountPercent / 100)) * amount;
            else
                return BuyCost * amount;
        }

        public decimal GetSellCost(int amount = 1)
        {
            if (isDiscounted)
                return (SellCost - SellCost * (discountPercent / 100)) * amount;
            else
                return  SellCost * amount;
        }

        public string GetBuyCostFormatted(int amount = 1)
        {
            if (isDiscounted)
                return TShop.Instance.Translate(true, "ui_discount", (BuyCost * amount).ToString("0.00"), ((BuyCost - BuyCost * (discountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue("MoneySymbol") + (BuyCost * amount).ToString("0.00");
        }

        public string GetSellCostFormatted(int amount = 1)
        {
            if (isDiscounted)
                return TShop.Instance.Translate(true, "ui_discount", (SellCost * amount).ToString("0.00"), ((SellCost - SellCost * (discountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue("MoneySymbol") + (SellCost * amount).ToString("0.00");
        }
    }
    public class Product
    {
        public bool isVehicle { get; set; }
        public ushort Id { get; set; }
        public int Amount { get; set; }

        public Product(ushort id, int amount, bool vehicle)
        {
            Id = id;
            Amount = amount;
            isVehicle = vehicle;
        }
    }

    [Serializable]
    public class Transaction
    {
        public ETransaction TransactionType { get; set; }
        public ECurrency CurrencyType { get; set; }
        public string StoreName { get; set; }
        public ulong PayerID { get; set; }
        public ulong PayeeID { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        public Transaction(ETransaction type, ECurrency currency, string storename, ulong payer, ulong payee, decimal amount, DateTime date) { TransactionType = type; CurrencyType = currency; StoreName = storename; PayerID = payer; PayeeID = payee; Amount = amount; TransactionDate = date; }

        public Transaction() { }
    }
}