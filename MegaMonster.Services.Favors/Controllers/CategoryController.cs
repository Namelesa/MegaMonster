using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Data;
using MegaMonster.Services.Favors.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/Favors")]
public class CategoryController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await db.Categories.ToArrayAsync();
        return Ok(categories);
    }
    
    [HttpGet("category/id/{categoryId}")]
    public async Task<IActionResult> GetCategoryById(int categoryId)
    {
        var category = await db.Categories.FindAsync(categoryId);
        if (category != null)
        {
            return Ok(category);
        }

        return NotFound("Not found category with this id");
    }
    
    [HttpGet("category/name/{name}")]
    public async Task<IActionResult> GetCategoryByName(string name)
    {
        var category = await db.Categories.FirstOrDefaultAsync(u => u.Name == name);
        if (category != null)
        {
            return Ok(category);
        }

        return NotFound("Not found category with this name");
    }
    
    // Post Requests //
    [HttpPost("category/add")]
    public async Task<IActionResult> AddCategory(string categoryName)
    {
        try
        {
            var category = new Category(categoryName);
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Ok($"Successfully added category with name = {category.Name}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Category with name '{categoryName}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Put Requests //
    [HttpPut("category/edit/name/{currentName}")]
    public async Task<IActionResult> EditCategory(string currentName, [Required]string newName)
    {
        try
        {
            var currentCategory = db.Categories.FirstOrDefault(u => u.Name == currentName);
            if (currentCategory != null)
            {
                currentCategory.Name = newName;
                db.Categories.Update(currentCategory);
                await db.SaveChangesAsync();
                return Ok($"Successfully edit category name {currentName} to  {newName}");
            }
            return NotFound($"Not found category with name = {currentName}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Category with name '{newName}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Delete Requests // 
    [HttpDelete("category/delete/name/{currentName}")]
    public async Task<IActionResult> DeleteCategory(string currentName)
    {
        var currentCategory = db.Categories.FirstOrDefault(u => u.Name == currentName);
        if (currentCategory != null)
        {
            db.Categories.Remove(currentCategory);
            await db.SaveChangesAsync();
            return Ok($"Successfully delete category with name {currentName}");
        }
        return NotFound($"Not found category with name = {currentName}");
    }
}