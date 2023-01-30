using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
    public enum ECodeHotkey
    {

    }

    public enum EPaymentMethod
    {
        wallet,
        bank,
        crypto
    }

    public enum ECurrency
    {
        xp,
        cash,
        bank,
        bankcard,
        crypto
    }

    public enum ETransaction
    {
        DEPOSIT,
        WITHDRAW,
        REFUND,
        SALE,
        PURCHASE,
        PAYMENT
    }
}
