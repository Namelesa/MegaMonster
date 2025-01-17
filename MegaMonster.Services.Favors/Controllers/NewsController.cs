using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Data;
using MegaMonster.Services.Favors.Dto_s;
using MegaMonster.Services.Favors.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/Favors")]
public class NewsController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("news")]
    public async Task<IActionResult> GetNews()
    {
        var news = await db.News.ToArrayAsync();
        return Ok(news);
    }
    
    // Post Requests //
    [HttpPost("news/add")]
    public async Task<IActionResult> AddNews([Required, FromBody] NewsDto newsDto)
    {
        try
        {
            var news = new News(newsDto.Type, newsDto.Name, newsDto.Description, newsDto.Image, newsDto.Link);
            db.News.Add(news);
            await db.SaveChangesAsync();
            return Ok($"Successfully added news with name = {news.Name}");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Put Requests //
    [HttpPut("news/edit/id/{id}")]
    public async Task<IActionResult> EditNews(int id, [Required, FromBody] NewsDto newsDto)
    {
        try
        {
            var currentNews = await db.News.FindAsync(id);
            if (currentNews != null)
            {
                currentNews.Type = newsDto.Type;
                currentNews.Name = newsDto.Name;
                currentNews.Description = newsDto.Description;
                currentNews.Link = newsDto.Link;
                currentNews.Image = newsDto.Image;
                db.News.Update(currentNews);
                await db.SaveChangesAsync();
                return Ok($"Successfully edit news");
            }
            return NotFound($"Not found news with id = {id}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest("News with this param already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Delete Requests // 
    [HttpDelete("news/delete/id/{id}")]
    public async Task<IActionResult> DeleteNews(int id)
    {
        var currentNews = await db.News.FindAsync(id);
        if (currentNews != null)
        {
            db.News.Remove(currentNews);
            await db.SaveChangesAsync();
            return Ok($"Successfully delete news with name {currentNews.Name}");
        }
        return NotFound("Not found news");
    }
}