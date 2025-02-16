using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Application.Services;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.WebApi.Dto_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Favors.WebApi.Controllers;

[ApiController]
[Route("api/Favors")]
public class NewsController(NewsService newsService) : ControllerBase
{
    // Get Requests //
    [Authorize]
    [HttpGet("news")]
    public async Task<IActionResult> GetNews()
    {
        var news = await newsService.GetAllNews();
        return Ok(news);
    }
    
    // Post Requests //
    [Authorize(Roles = "Admin")]
    [HttpPost("news/add")]
    public async Task<IActionResult> AddNews([Required, FromBody] NewsDto newsDto)
    {
        var news = new News(newsDto.Type, newsDto.Name, newsDto.Description, newsDto.Image, newsDto.Link);
        var result = await newsService.AddNews(news);
        return result.Success ? Ok($"Successfully added news with name = {news.Name}") : BadRequest(result.Message);
    }
    
    // Put Requests //
    [Authorize(Roles = "Admin")]
    [HttpPut("news/edit/id/{id}")]
    public async Task<IActionResult> EditNews(int id, [Required, FromBody] NewsDto newsDto)
    {
        var result = await newsService.EditNews(id, newsDto.Type, newsDto.Name, newsDto.Description, newsDto.Image, newsDto.Link);
        return result.Success ? Ok("Edit news") : BadRequest(result.Message);
    }
    
    // Delete Requests // 
    [Authorize(Roles = "Admin")]
    [HttpDelete("news/delete/id/{id}")]
    public async Task<IActionResult> DeleteNews(int id)
    {
        var result = await newsService.DeleteNews(id);
        return result.Success ? Ok($"Successfully delete news with id {id}") : BadRequest(result.Message);
    }
    
}