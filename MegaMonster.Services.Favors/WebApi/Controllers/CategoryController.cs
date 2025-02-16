using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Favors.WebApi.Controllers;

[ApiController]
[Route("api/Favors")]
public class CategoryController(CategoryService categoryService) : ControllerBase
{
    // GET Requests
    [Authorize]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await categoryService.GetAllCategories();
        return Ok(categories);
    }

    [Authorize]
    [HttpGet("category/id/{categoryId}")]
    public async Task<IActionResult> GetCategoryById(int categoryId)
    {
        if (categoryId <= 0) return BadRequest("Invalid category ID.");

        var category = await categoryService.GetCategoryById(categoryId);
        return category is not null ? Ok(category) : NotFound($"Category with ID {categoryId} not found.");
    }
    
    [Authorize]
    [HttpGet("category/name/{name}")]
    public async Task<IActionResult> GetCategoryByName(string name)
    {
        var category = await categoryService.GetCategoryByName(name);
        return category is not null ? Ok(category) : NotFound($"Category with name '{name}' not found.");
    }

    // POST Requests
    [Authorize(Roles = "Admin")]
    [HttpPost("category/add")]
    public async Task<IActionResult> AddCategory([Required] string categoryName)
    {
        var result = await categoryService.AddCategory(categoryName);
        return result.Success ? Ok("Category added successfully.") : BadRequest(result.Message);
    }

    // PUT Requests
    [Authorize(Roles = "Admin")]
    [HttpPut("category/edit/name/{currentName}")]
    public async Task<IActionResult> EditCategory(string currentName, [Required] string newName)
    {
        var result = await categoryService.EditCategory(currentName, newName);
        return result.Success ? Ok($"Category '{currentName}' renamed to '{newName}'.") : BadRequest(result.Message);
    }

    // DELETE Requests
    [Authorize(Roles = "Admin")]
    [HttpDelete("category/delete/name/{currentName}")]
    public async Task<IActionResult> DeleteCategory(string currentName)
    {
        var result = await categoryService.DeleteCategory(currentName);
        return result.Success ? Ok("Category deleted successfully.") : BadRequest(result.Message);
    }
}
