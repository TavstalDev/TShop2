using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Compability
{
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