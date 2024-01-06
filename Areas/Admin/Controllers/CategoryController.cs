using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET
    public IActionResult Index()
    {
        var objCategoryList = _unitOfWork.Category.GetAll().ToList();
        return View(objCategoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
            ModelState.AddModelError("name", "The display order cannot exactly match the Name");
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Add(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0) return NotFound();

        var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);

        if (categoryFromDb is null) return NotFound();

        return View(categoryFromDb);
    }

    [HttpPost]
    public IActionResult Edit(Category obj)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Update(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category edited successfully";
            return RedirectToAction("Index");
        }

        return View();
    }
    
    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0) return NotFound();

        var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);

        if (categoryFromDb is null) return NotFound();

        return View(categoryFromDb);
    }

    [HttpPost, ActionName("delete")]
    public IActionResult DeletePOST(int? id)
    {
        if (id is null || id == 0) return NotFound();

        var obj = _unitOfWork.Category.GetFirstOrDefault(o => o.Id == id);

        if (obj is null) return NotFound();

        _unitOfWork.Category.Remove(obj);
        _unitOfWork.Save(); 
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");
    }
}