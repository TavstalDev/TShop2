using SDG.Unturned;
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
        [SqlMember(isPrimaryKey: true, isUnsigned: true, shouldAutoIncrement: true)]
        public ushort Id { get; set; }
        [SqlMember(isUnsigned: true)]
        public ushort UnturnedId { get; set; }
        [SqlMember(columnType:"varchar(255)")]
        public string DisplayName { get; set; }
        [SqlMember]
        public bool IsVehicle { get; set; }
        [SqlMember(columnType: "decimal(11, 1)")]
        public decimal BuyCost { get; set; }
        [SqlMember(columnType: "decimal(11, 1)")]
        public decimal SellCost { get; set; }
        [SqlMember]
        public bool HasPermission { get; set; }
        [SqlMember(columnType: "varchar(255)", isNullable: true)]
        public string Permission { get; set; }
        [SqlMember]
        public bool IsDiscounted { get; set; }
        [SqlMember(columnType: "decimal(3, 1)")]
        public decimal DiscountPercent { get; set; }

        public ShopItem() { }

        public ShopItem(ushort id, bool isVehicle, decimal buycost, decimal sellcost, string perm)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            BuyCost = buycost;
            SellCost = sellcost;
            HasPermission = true;
            Permission = perm;
            DisplayName = GetName();
        }

        public ShopItem(ushort id, bool isVehicle, decimal buycost, decimal sellcost)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            BuyCost = buycost;
            SellCost = sellcost;
            HasPermission = false;
            Permission = "TShop.item.";
            DisplayName = GetName();
        }

        public ShopItem(ushort id, bool isVehicle, decimal buycost, decimal sellcost, bool hasperm, string perm, bool isdiscount, decimal discount)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            BuyCost = buycost;
            SellCost = sellcost;
            HasPermission = hasperm;
            Permission = perm;
            IsDiscounted = isdiscount;
            DiscountPercent = discount;
            DisplayName = GetName();
        }

        public string GetName()
        {
            return (IsVehicle ? 
                (Assets.find(EAssetType.VEHICLE, UnturnedId) as VehicleAsset).vehicleName 
                : 
                (Assets.find(EAssetType.ITEM, UnturnedId) as ItemAsset).itemName).Replace("'", "").Replace("`", "");
        }

        public decimal GetBuyCost(int amount = 1)
        {
            if (IsDiscounted)
                return (BuyCost - BuyCost * (DiscountPercent / 100)) * amount;
            else
                return BuyCost * amount;
        }

        public decimal GetSellCost(int amount = 1)
        {
            if (IsDiscounted)
                return (SellCost - SellCost * (DiscountPercent / 100)) * amount;
            else
                return  SellCost * amount;
        }

        public string GetBuyCostFormatted(int amount = 1)
        {
            if (IsDiscounted)
                return TShop.Instance.Localize(true, "ui_discount", (BuyCost * amount).ToString("0.00"), ((BuyCost - BuyCost * (DiscountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue<string>("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue<string>("MoneySymbol") + (BuyCost * amount).ToString("0.00");
        }

        public string GetSellCostFormatted(int amount = 1)
        {
            if (IsDiscounted)
                return TShop.Instance.Localize(true, "ui_discount", (SellCost * amount).ToString("0.00"), ((SellCost - SellCost * (DiscountPercent / 100)) * amount).ToString("0.00"), TShop.economyProvider.GetConfigValue<string>("MoneySymbol")).ToString();
            else
                return TShop.economyProvider.GetConfigValue<string>("MoneySymbol") + (SellCost * amount).ToString("0.00");
        }
    }
   
}