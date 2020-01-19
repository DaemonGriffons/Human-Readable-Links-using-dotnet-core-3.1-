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

namespace HumanReadableLinks.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ISlugifier _slugifier;

        public HomeController(ApplicationDbContext context, ISlugifier slugifier)
        {
            _context = context;
            _slugifier = slugifier;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        [Route("/post/{slug}")]
        [HttpGet]
        public IActionResult Redirects(string slug)
        {
            return LocalRedirectPermanent($"/product/{slug}");
        }

        [HttpGet]
        [Route("/product/{slug}")]
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
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Slug = _slugifier.CreateSlug(model.Name)
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
