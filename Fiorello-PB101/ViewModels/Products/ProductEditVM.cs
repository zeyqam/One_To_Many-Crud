namespace Fiorello_PB101.ViewModels.Products
{
    public class ProductEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string Price { get; set; }
        public List<IFormFile> Images { get; set; }
        public List<ProductImageVM> ExistingImages { get; set; }

    }
}
