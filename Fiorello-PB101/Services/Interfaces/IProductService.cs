﻿using Fiorello_PB101.Models;
using Fiorello_PB101.ViewModels.Products;

namespace Fiorello_PB101.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int? id);
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        IEnumerable<ProductVM> GetMappeDatas(IEnumerable<Product> products);
        Task<IEnumerable<Product>> GetAllPaginateAsync(int page,int take);
        Task<int> GetCountAsync();
        Task CreateAsync(Product product);
        Task DeleteAsync(Product product);
        Task UpdateAsync(Product product);  
        Task SetMainImageAsync(ProductImage image);
        Task<ProductImage> GetProductImageByIdAsync(int id);
        Task DeleteProductImageAsync(ProductImage image);
    }
}
