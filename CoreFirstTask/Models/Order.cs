using Microsoft.Xrm.Sdk;

namespace CoreFirstTask.Models
{
    //salesorder
     //ColumnSet = new ColumnSet("salesorderid", "customerid", "customeridname", "name", "statecode", "statecodename"),
    public class Order
    {
        public Guid salesorderid { get; set; } 

        public string name { get; set; } 
        public string customername { get; set; } 

        public decimal totalamount { get; set; }
         
        public int statecode { get; set; }
    }
}
