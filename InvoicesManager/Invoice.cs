using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicesManager
{
    public class Invoice
    {
        public Invoice(int id, string clientName, string invoiceNumber, DateTime date, decimal amount, string details, string barcodeValue = null)
        {
            Id = id;
            ClientName = clientName;
            InvoiceNumber = invoiceNumber;
            Date = date;
            Amount = amount;
            Details = details;
            BarcodeValue = barcodeValue;
        }

        public int Id { get; set; }
        public string ClientName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Details { get; set; }
        public string BarcodeValue { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
