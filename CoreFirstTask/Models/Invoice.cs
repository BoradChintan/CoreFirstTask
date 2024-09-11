using Microsoft.Xrm.Sdk;

namespace CoreFirstTask.Models
{
    public class Invoice
    {
        public Guid invoiceid { get; set; }
        public Guid? accountid { get; set; }
        public Guid? customerid { get; set; }
        public string accountidname { get; set; } 
        public string customeridname { get; set; } 
        public string invoicenumber { get; set; } 
        public string name { get; set; } 
        public DateTime createdon { get; set; } 
    }
}
