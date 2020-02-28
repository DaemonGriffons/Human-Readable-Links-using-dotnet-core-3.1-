using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HumanReadableLinks.Models;
using HumanReadableLinks.Data;
using Microsoft.EntityFrameworkCore;
using HumanReadableLinks.Services;
using Microsoft.Extensions.Options;

namespace HumanReadableLinks.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ISlugifier _slugifier;
        private readonly IFileManager _fileManager;
        private readonly PhotoSettings _photoSettings;

        public HomeController(ApplicationDbContext context, ISlugifier slugifier, IFileManager fileManager, IOptionsSnapshot<PhotoSettings> options)
        {
            _context = context;
            _slugifier = slugifier;
            _fileManager = fileManager;
            _photoSettings = options.Value;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        [Route("/post/{slug}/")]
        [HttpGet]
        public IActionResult Redirects(string slug)
        {
            return LocalRedirectPermanent($"/product/{slug}/");
        }

        [HttpGet]
        [Route("/product/{slug}/")]
        public async Task<IActionResult> Post(string slug)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Slug.Equals(slug));

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpGet]
        public IActionResult CreateProduct() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImageFile == null)
                {
                    ModelState.AddModelError(string.Empty, "You Have to Upload An Image!");
                    return View(model);
                }
                if(model.ImageFile.Length>= _photoSettings.MaxBytes)
                {
                    ModelState.AddModelError(string.Empty, "Image File size Exceeded");
                    return View(model);
                }
                if (!_photoSettings.IsSupported(model.ImageFile.FileName))
                {
                    ModelState.AddModelError(string.Empty, "Invalid File Type");
                    return View(model);
                }
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Slug = _slugifier.CreateSlug(model.Name),
                    Image = _fileManager.SaveImage(model.ImageFile)
                };
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        [HttpGet]
        [Route("/edit/{slug}")]
        public async Task<IActionResult> Edit(string slug)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Slug.Equals(slug));

            if (product == null)
            {
                return NotFound();
            }
            var model = new ProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Slug = product.Slug,
                Image = product.Image
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("/edit/{slug}")]
        public async Task<IActionResult> Edit(string slug, ProductVM model)
        {
            if (ModelState.IsValid)
            {
                
                var product = new Product
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Slug = _slugifier.CreateSlug(model.Name)
                };

                if (model.ImageFile == null)
                {
                    product.Image = model.Image;
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(model.Image))
                    {
                        _fileManager.RemoveImage(model.Image);
                    }

                    product.Image = _fileManager.SaveImage(model.ImageFile);
                }
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        [HttpPost, ValidateAntiForgeryToken]
        [Route("/delete/{slug}")]
        public async Task<IActionResult> Delete(string slug)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Slug.Equals(slug));
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _fileManager.RemoveImage(product.Image);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
