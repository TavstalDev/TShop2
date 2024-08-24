using SDG.Unturned;
using System;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Database.Attributes;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Model.Interfaces;
using UnityEngine;

namespace Tavstal.TShop.Model.Classes
{
    [Serializable]
    public class Product : IProduct
    {
        [SqlMember(isPrimaryKey: true, isUnsigned: true, shouldAutoIncrement: true)]
        public ushort Id { get; set; }
        [SqlMember(isUnsigned: true)]
        public ushort UnturnedId { get; set; }
        [SqlMember(columnType:"varchar(255)")]
        public string DisplayName { get; set; }
        [SqlMember]
        public bool IsVehicle { get; set; }
        [SqlMember(columnType:"varchar(255)", isNullable: true)]
        public string VehicleColor { get; set; }
        [SqlMember(columnType: "decimal(14, 1)")]
        public decimal BuyCost { get; set; }
        [SqlMember(columnType: "decimal(14, 1)")]
        public decimal SellCost { get; set; }
        [SqlMember]
        public bool HasPermission { get; set; }
        [SqlMember(columnType: "varchar(255)", isNullable: true)]
        public string Permission { get; set; }
        [SqlMember]
        public bool IsDiscounted { get; set; }
        [SqlMember(columnType: "decimal(3, 1)")]
        public decimal DiscountPercent { get; set; }

        public Product() { }

        public Product(ushort id, bool isVehicle, string vehicleColor, decimal buycost, decimal sellcost, string perm)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            VehicleColor = vehicleColor;
            BuyCost = buycost;
            SellCost = sellcost;
            HasPermission = true;
            Permission = perm;
            DisplayName = GetName();
        }

        public Product(ushort id, bool isVehicle, string vehicleColor, decimal buycost, decimal sellcost)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            VehicleColor = vehicleColor;
            BuyCost = buycost;
            SellCost = sellcost;
            HasPermission = false;
            Permission = "TShop.item.";
            DisplayName = GetName();
        }

        public Product(ushort id, bool isVehicle, string vehicleColor, decimal buycost, decimal sellcost, bool hasperm, string perm, bool isdiscount, decimal discount)
        {
            UnturnedId = id;
            IsVehicle = isVehicle;
            VehicleColor = vehicleColor;
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
            try
            {
                if (IsVehicle)
                {
                    VehicleAsset asset = UAssetHelper.FindVehicleAsset(UnturnedId) ?? throw new NullReferenceException("Failed to get the unturned asset.");
                    return asset.vehicleName;
                }
                else
                {
                    Asset asset = UAssetHelper.FindItemAsset(UnturnedId) ?? throw new NullReferenceException("Failed to get the unturned asset.");
                    if (asset is ItemAsset itemAsset)
                        return itemAsset.itemName;
                    throw new Exception("The asset is not an item asset. Please review your database.");
                }
            }
            catch (Exception ex)
            {
                string type = IsVehicle ? "vehicle" : "item";
                TShop.Logger.LogWarning($"Failed to get the asset name for '{type}' with {UnturnedId} id. Exception:");
                TShop.Logger.LogException(ex);
                return "unknown_name";
            }
        }

        public Color32 GetVehicleColor()
        {
            if (VehicleColor.IsNullOrEmpty())
                return default;
            
            if (ColorUtility.TryParseHtmlString(VehicleColor, out var newCol))
                return newCol;
            TShop.Logger.LogError("Failed to parse the product's vehicle color. Please fix its database value to html HEX color.");
            return default;
        }

        public decimal GetBuyCost(int amount = 1)
        {
            if (IsDiscounted)
                return (BuyCost - BuyCost * (DiscountPercent / 100)) * amount;
            return BuyCost * amount;
        }

        public decimal GetSellCost(int amount = 1)
        {
            if (IsDiscounted)
                return (SellCost - SellCost * (DiscountPercent / 100)) * amount;
            return  SellCost * amount;
        }

        public decimal GetSellCostByQuality(byte quality = 100)
        {
            decimal cost = TShop.Instance.Config.UseQuality ? (SellCost / 100 * quality) : SellCost;

            if (IsDiscounted)
                return (cost - cost * (DiscountPercent / 100));
            return cost;
        }

        public string GetBuyCostFormatted(int amount = 1)
        {
            if (IsDiscounted)
                return TShop.Instance.Localize(true, "ui_discount", (BuyCost * amount).ToString("0.00"), ((BuyCost - BuyCost * (DiscountPercent / 100)) * amount).ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
            return TShop.EconomyProvider.GetCurrencyName() + (BuyCost * amount).ToString("0.00");
        }

        public string GetSellCostFormatted(int amount = 1)
        {
            if (IsDiscounted)
                return TShop.Instance.Localize(true, "ui_discount", (SellCost * amount).ToString("0.00"), ((SellCost - SellCost * (DiscountPercent / 100)) * amount).ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
            return TShop.EconomyProvider.GetCurrencyName() + (SellCost * amount).ToString("0.00");
        }
    }
   
}