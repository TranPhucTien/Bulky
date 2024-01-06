using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET
    public IActionResult Index()
    {
        var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

        return View(objProductList);
    }

    public IActionResult Upsert(int? id)
    {
        IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll()
            .Select(o => new SelectListItem
            {
                Text = o.Name,
                Value = o.Id.ToString()
            });

        var productVM = new ProductVM
        {
            CategoryList = categoryList,
            Product = new Product()
        };

        // Create
        if (id is null || id == 0)
        {
            return View(productVM);
        }

        // Update
        productVM.Product = _unitOfWork.Product.GetFirstOrDefault(o => o.Id == id);
        return View(productVM);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\products");

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = @"\images\products\" + fileName;
            }

            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }

            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            productVM.CategoryList = _unitOfWork.Category.GetAll()
                .Select(o => new SelectListItem
                {
                    Text = o.Name,
                    Value = o.Id.ToString()
                });

            return View(productVM);
        }
    }

    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json(new { data = objProductList });
    }
    
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        string wwwRootPath = _webHostEnvironment.WebRootPath;

        var productToBeDeleted = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        var oldImagePath = Path.Combine(wwwRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));

        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }
        
        _unitOfWork.Product.Remove(productToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "Delete successful" });
    }

    #endregion
}