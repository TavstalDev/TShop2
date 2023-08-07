using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility.Database;

namespace Tavstal.TShop.Compability
{
    [Serializable]
    public class ShopItem
    {
        [SqlMember(isPrimaryKey: true, shouldAutoIncrement: true)]
        public ushort Id { get; set; }
        [SqlMember]
        public decimal BuyCost { get; set; }
        [SqlMember]
        public decimal SellCost { get; set; }
        [SqlMember]
        public bool hasPermission { get; set; }
        [SqlMember(isNullable: true)]
        public string Permission { get; set; }
        [SqlMember]
        public bool isDiscounted { get; set; }
        [SqlMember(columnType: "decimal(3, 1)")]
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
                return TShop.Instance.Localize(true, "ui_discount", (BuyCost * amount).ToString("0.00"), ((BuyCost - BuyCost * (discountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue("MoneySymbol") + (BuyCost * amount).ToString("0.00");
        }

        public string GetSellCostFormatted(int amount = 1)
        {
            if (isDiscounted)
                return TShop.Instance.Localize(true, "ui_discount", (SellCost * amount).ToString("0.00"), ((SellCost - SellCost * (discountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue("MoneySymbol") + (SellCost * amount).ToString("0.00");
        }
    }
   
}