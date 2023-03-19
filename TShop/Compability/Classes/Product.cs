using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
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
}