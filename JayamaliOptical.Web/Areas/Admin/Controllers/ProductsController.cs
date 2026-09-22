using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.CategoryFilter = categoryId;
            ViewBag.CategorySelectList = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", categoryId);
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            return View(await products.ToListAsync());
        }

        // GET: Admin/Products/Create
        public IActionResult Create(int? categoryId)
        {
            ViewBag.CategorySelectList = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", categoryId);
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            ViewBag.PreSelectedCategory = categoryId.HasValue;
            ViewBag.SelectedCategoryId = categoryId;

            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile1, IFormFile? ImageFile2)
        {
            ViewBag.CategorySelectList = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", product.CategoryId);
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            ViewBag.PreSelectedCategory = true;
            ViewBag.SelectedCategoryId = product.CategoryId;

            // Handle photo uploads
            if (ImageFile1 != null)
            {
                if (!FileUploadValidator.IsValidImage(ImageFile1, out var error1))
                {
                    ModelState.AddModelError("ImageFile1", error1!);
                }
                else
                {
                    product.ImageUrl1 = await UploadFileAsync(ImageFile1);
                }
            }

            if (ImageFile2 != null)
            {
                if (!FileUploadValidator.IsValidImage(ImageFile2, out var error2))
                {
                    ModelState.AddModelError("ImageFile2", error2!);
                }
                else
                {
                    product.ImageUrl2 = await UploadFileAsync(ImageFile2);
                }
            }

            if (ModelState.IsValid)
            {
                if (await RequiresProductTypeAsync(product.CategoryId) && string.IsNullOrEmpty(product.ProductType))
                {
                    ModelState.AddModelError("ProductType", "Product Type is required for Spectacles and Sunglasses");
                    return View(product);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),
                    new { categoryId = product.CategoryId });
            }
            return View(product);
        }

        // Spectacles and sunglasses require a frame ProductType (rimless/full-rim/etc.) — looked up by
        // category name rather than a hardcoded id, since seeded category ids aren't guaranteed stable.
        internal async Task<bool> RequiresProductTypeAsync(int categoryId)
        {
            var categoryName = await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            return categoryName == "Spectacle" || categoryName == "Sunglasses";
        }

        // Helper method to upload files
        private async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/products/" + uniqueFileName;
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CategorySelectList = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", product.CategoryId);
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            ViewBag.PreSelectedCategory = true;
            ViewBag.SelectedCategoryId = product.CategoryId;

            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile1, IFormFile? ImageFile2, bool removePhoto1 = false, bool removePhoto2 = false)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            ViewBag.CategorySelectList = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", product.CategoryId);
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            ViewBag.PreSelectedCategory = true;
            ViewBag.SelectedCategoryId = product.CategoryId;

            // Get existing product
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Validate: At least one photo must exist after changes
            bool hasPhoto1 = !string.IsNullOrEmpty(existingProduct.ImageUrl1) && !removePhoto1;
            bool hasPhoto2 = !string.IsNullOrEmpty(existingProduct.ImageUrl2) && !removePhoto2;
            bool uploadingPhoto1 = ImageFile1 != null && ImageFile1.Length > 0;
            bool uploadingPhoto2 = ImageFile2 != null && ImageFile2.Length > 0;

            // Check if Photo 1 will exist after save
            if (!hasPhoto1 && !uploadingPhoto1)
            {
                ModelState.AddModelError("ImageFile1", "At least Photo 1 is required. Please upload a new photo or keep the existing one.");
            }

            if (uploadingPhoto1 && !FileUploadValidator.IsValidImage(ImageFile1!, out var editError1))
            {
                ModelState.AddModelError("ImageFile1", editError1!);
            }

            if (uploadingPhoto2 && !FileUploadValidator.IsValidImage(ImageFile2!, out var editError2))
            {
                ModelState.AddModelError("ImageFile2", editError2!);
            }

            if (await RequiresProductTypeAsync(product.CategoryId) && string.IsNullOrEmpty(product.ProductType))
            {
                ModelState.AddModelError("ProductType", "Product Type is required for Spectacles and Sunglasses");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ProductType = product.ProductType;
                    existingProduct.Brand = product.Brand;
                    existingProduct.FrameType = product.FrameType;
                    existingProduct.LensPower = product.LensPower;
                    existingProduct.IOLType = product.IOLType;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.RequiresPrescription = product.RequiresPrescription;

                    // Handle photo removal
                    if (removePhoto1)
                    {
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl1))
                        {
                            DeleteFile(existingProduct.ImageUrl1);
                        }
                        existingProduct.ImageUrl1 = null;
                    }

                    if (removePhoto2)
                    {
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl2))
                        {
                            DeleteFile(existingProduct.ImageUrl2);
                        }
                        existingProduct.ImageUrl2 = null;
                    }

                    // Handle new photo uploads
                    if (uploadingPhoto1)
                    {
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl1))
                        {
                            DeleteFile(existingProduct.ImageUrl1);
                        }
                        existingProduct.ImageUrl1 = await UploadFileAsync(ImageFile1!);
                    }

                    if (uploadingPhoto2)
                    {
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl2))
                        {
                            DeleteFile(existingProduct.ImageUrl2);
                        }
                        existingProduct.ImageUrl2 = await UploadFileAsync(ImageFile2!);
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index),
                    new { categoryId = existingProduct.CategoryId });
            }

            // If we get here, there was a validation error
            // Reload the existing product to show current state
            var reloadedProduct = await _context.Products.FindAsync(id);
            return View(reloadedProduct);
        }

        // Helper method to delete files
        private void DeleteFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index),
                new { categoryId = product?.CategoryId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // POST: Admin/Products/ToggleFeatured/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsFeatured = !product.IsFeatured;
                await _context.SaveChangesAsync();
                TempData["Success"] = product.IsFeatured
                    ? $"'{product.Name}' is now featured on the home page."
                    : $"'{product.Name}' removed from featured.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}