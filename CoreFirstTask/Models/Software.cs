using Microsoft.Xrm.Sdk;

namespace CoreFirstTask.Models
{
    public class Software
    {
        public Guid? new_softwareinventoryid { get; set; }    
        public string new_name { get; set; }    
        public int? new_category { get; set; }    
        public int? new_displayoption { get; set; }    
        public byte[] new_icon { get; set; }    
    }
    
    public class DragAndDropRequest
    {
        public string SoftwareId { get; set; }
        public int DisplayOption { get; set; }
    }
}
