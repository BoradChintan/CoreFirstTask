namespace CoreFirstTask.Models
{
    public class CategoryModel
    {
        public string new_category { get; set; } 
        public List<IconGroup> current { get; set; } 
        public List<IconGroup> future { get; set; } 
        public List<IconGroup> other { get; set; } 
    }

    public class IconGroup
    {
        public Guid SoftwareID { get; set; }
        public string Icon { get; set; }
    }
}
