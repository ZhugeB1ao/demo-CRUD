namespace WebApplication1.Models;

public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }    
        
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category>? Children { get; set; }
          
		public ICollection<Product>? Products { get; set; }		
    }
