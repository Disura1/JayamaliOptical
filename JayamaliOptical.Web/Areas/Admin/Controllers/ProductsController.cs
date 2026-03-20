using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
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

            // Debug: Log values
            System.Diagnostics.Debug.WriteLine($"Creating product - RequiresPrescription: {product.RequiresPrescription}, CategoryId: {product.CategoryId}");

            // Handle photo uploads
            if (ImageFile1 != null)
            {
                product.ImageUrl1 = await UploadFileAsync(ImageFile1);
            }

            if (ImageFile2 != null)
            {
                product.ImageUrl2 = await UploadFileAsync(ImageFile2);
            }

            if (ModelState.IsValid)
            {
                // For Spectacle (5) and Sunglasses (6), ProductType is required
                if ((product.CategoryId == 5 || product.CategoryId == 6) && string.IsNullOrEmpty(product.ProductType))
                {
                    ModelState.AddModelError("ProductType", "Product Type is required for Spectacles and Sunglasses");
                    return View(product);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
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

            // Debug: Log the ProductType value
            System.Diagnostics.Debug.WriteLine($"Editing product - Type: {product.ProductType}, CategoryId: {product.CategoryId}");

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

            // For Spectacle (5) and Sunglasses (6), ProductType is required
            if ((product.CategoryId == 5 || product.CategoryId == 6) && string.IsNullOrEmpty(product.ProductType))
            {
                ModelState.AddModelError("ProductType", "Product Type is required for Spectacles and Sunglasses");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update existing product properties - INCLUDE ProductType
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ProductType = product.ProductType;  // THIS IS IMPORTANT!
                    existingProduct.Brand = product.Brand;
                    existingProduct.FrameType = product.FrameType;
                    existingProduct.LensPower = product.LensPower;
                    existingProduct.IOLType = product.IOLType;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.IsActive = product.IsActive;

                    // ✅ ADD THIS LINE: Update RequiresPrescription
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
                return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}