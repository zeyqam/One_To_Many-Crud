using Fiorello_PB101.Helpers;
using Fiorello_PB101.Helpers.Extensions;
using Fiorello_PB101.Models;
using Fiorello_PB101.Services.Interfaces;
using Fiorello_PB101.ViewModels.Categories;
using Fiorello_PB101.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;

namespace Fiorello_PB101.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;
        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment env)
        {
            _productService = productService;
            _categoryService = categoryService;
            _env = env;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _productService.GetAllPaginateAsync(page, 4);
            var mappeDatas = _productService.GetMappeDatas(products);

            int totalPage = await GetPageAsync(4);


            Paginate<Fiorello_PB101.ViewModels.Products.ProductVM> paginateDatas = new(mappeDatas, totalPage, page);


            return View(paginateDatas);
        }

        private async Task<int> GetPageAsync(int take)
        {
            int productCount = await _productService.GetCountAsync();

            return (int)Math.Ceiling((decimal)productCount / take);
        }
        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }
            var existProduct = await _productService.GetProductByIdAsync((int)id);
            if (existProduct is null)
            {
                return NotFound();
            }

            List<ProductImageVM> images = new();

            foreach (var item in existProduct.ProductImages)
            {
                images.Add(new ProductImageVM
                {
                    Image=item.Name,
                    IsMain=item.IsMain
                });
            }

            ProductDetailVM response = new()
            {
                Name = existProduct.Name,
                Description = existProduct.Description,
                Category = existProduct.Category.Name,
                Price = existProduct.Price,
                Images= images

            };
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag. categories = await _categoryService.GetAllSelectedAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM request)
        {
            ViewBag.categories = await _categoryService.GetAllSelectedAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }
            

            foreach (var item in request.Images)
            {
                if (!item.CheckFileSize(500))
                {
                    ModelState.AddModelError("Images", "Image size must be 500 kb");
                    return View();
                }
                if (!item.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Images", "File type must be fyle type");
                    return View();
                }
            }
            List<ProductImage> images = new();


            foreach (var item in request.Images)
            {
                string fileName = $"{Guid.NewGuid()}-{item.FileName}";
                string path = _env.GenerateFilePath("img", fileName);
                await item.SaveFileLocalAsync(path);
                images.Add(new ProductImage { Name = fileName });
            }

            images.FirstOrDefault().IsMain = true;
            Product product = new Product()
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Price = decimal.Parse(request.Price),
                ProductImages = images

            };

            await _productService.CreateAsync(product);
            return RedirectToAction(nameof(Index));
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }
            var existProduct = await _productService.GetProductByIdAsync((int)id);
            if (existProduct is null)
            {
                return NotFound();
            }
            foreach (var item in existProduct.ProductImages)
            {
                string path = _env.GenerateFilePath("img", item.Name);
                path.DeleteFileFromLocal();
            }
           await _productService.DeleteAsync(existProduct);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }
            var existProduct = await _productService.GetProductByIdAsync((int)id);
            if (existProduct is null)
            {
                return NotFound();
            }
            ViewBag.categories=await _categoryService.GetAllSelectedAsync();
            ProductEditVM response = new()
            {
                Id = existProduct.Id,
                Name = existProduct.Name,
                Description = existProduct.Description,
                CategoryId = existProduct.CategoryId,
                Price = existProduct.Price.ToString(),
                ExistingImages = existProduct.ProductImages.Select(m => new ProductImageVM
                {
                    Id=m.Id,
                    Image = m.Name,
                    IsMain = m.IsMain
                }).ToList()
            };

            return View(response);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (ProductEditVM request)
        {
            ViewBag.categories = await _categoryService.GetAllSelectedAsync();
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var existProduct = await _productService.GetProductByIdAsync(request.Id);
            if (existProduct is null)
            {
                return NotFound();
            }

            if (request.Images != null && request.Images.Count > 0)
            {
                foreach (var item in request.Images)
                {
                    if (!item.CheckFileSize(500))
                    {
                        ModelState.AddModelError("Images", "Image size must be 500 kb");
                        return View(request);
                    }
                    if (!item.CheckFileType("image/"))
                    {
                        ModelState.AddModelError("Images", "File type must be file type");
                        return View(request);
                    }
                }
            }

            existProduct.Name = request.Name;
            existProduct.Description = request.Description;
            existProduct.CategoryId = request.CategoryId;
            existProduct.Price = decimal.Parse(request.Price);

            if (request.Images != null && request.Images.Count > 0)
            {
                

                foreach (var item in request.Images)
                {
                    string fileName = $"{Guid.NewGuid()}-{item.FileName}";
                    string path = _env.GenerateFilePath("img", fileName);
                    await item.SaveFileLocalAsync(path);
                    existProduct.ProductImages.Add(new ProductImage { Name = fileName });
                }

               
            }

            await _productService.UpdateAsync(existProduct);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int id)
        {
            var image = await _productService.GetProductImageByIdAsync(id);
            if (image is null)
            {
                return NotFound();
            }

            await _productService.SetMainImageAsync(image);
            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _productService.GetProductImageByIdAsync(id);
            if (image is null)
            {
                return NotFound();
            }

            string path = _env.GenerateFilePath("img", image.Name);
            path.DeleteFileFromLocal();
            await _productService.DeleteProductImageAsync(image);
            return Ok();
        }

    }
}
