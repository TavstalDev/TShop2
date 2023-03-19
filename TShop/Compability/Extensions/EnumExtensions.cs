using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Tavstal.TShop.Managers;
using System.Reflection;
using Tavstal.TShop.Helpers;

namespace Tavstal.TShop.Compability
{
    public static class EnumExtensions
    {
        public static ECurrency ToCurrency(this EUconomyMethod type)
        {
            switch (type)
            {
                case EUconomyMethod.CASH:
                    {
                        return ECurrency.CASH;
                    }
                case EUconomyMethod.BANK:
                    {
                        return ECurrency.BANK;
                    }
                case EUconomyMethod.CRYPTO:
                    {
                        return ECurrency.CRYPTO;
                    }
                default:
                    {
                        return ECurrency.CASH;
                    }
            }
        }
    }
}
