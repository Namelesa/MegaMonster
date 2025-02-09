using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Infrastructure;
using MegaMonster.Services.Favors.Infrastructure.Redis;

namespace MegaMonster.Services.Favors.Application.Services;

public class TicketsService(
    ITicketRepository ticketRepository, 
    ITicketConfigurationRepository ticketConfigurationRepository, 
    IRedisService redisService)
{
    private const int CacheDurationMinutes = 60;

    public async Task<IEnumerable<Ticket>> GetAllTicketsByStatus(string status)
    {
        var cacheKey = $"Tickets_Status_{status}";
        return await GetOrSetCache(cacheKey, () => ticketRepository.GetTicketByStatus(status));
    }

    public async Task<IEnumerable<TicketConfiguration>> GetAllTicketConfigurations()
    {
        const string cacheKey = "All_Ticket_Configurations";
        return await GetOrSetCache(cacheKey, async () => (await ticketConfigurationRepository.GetAll()).ToArray());
    }

    public async Task<TicketConfiguration> GetConfigurationForUser(string userType)
    {
        var cacheKey = $"TicketConfig_{userType}";
        return await GetOrSetCache(cacheKey, () => ticketConfigurationRepository.GetConfigurationAsync(userType));
    }

    public async Task<ResultOperation> BuyTicket(Ticket ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket.UserName)) 
            return ResultOperation.Fail("UserName cannot be empty.");
        
        return await ProcessTicketChange(() => ticketRepository.AddAsync(ticket), $"Tickets_Status_{ticket.UserType}");
    }
    
    public async Task<ResultOperation> DeleteTicket(int id)
    {
        var ticket = await ticketRepository.GetTicketById(id);

        return await ProcessTicketChange(() => ticketRepository.DeleteAsync(ticket), $"Tickets_Status_{ticket.UserType}");
    }
    
    public async Task<ResultOperation> DeleteTicketConfiguration(string userType)
    {
        if (string.IsNullOrWhiteSpace(userType)) 
            return ResultOperation.Fail("UserType cannot be empty.");
        
        var ticketConfiguration = await ticketConfigurationRepository.GetConfigurationAsync(userType);

        return await ProcessConfigurationChange(() => ticketConfigurationRepository.DeleteAsync(ticketConfiguration), userType);
    }
    
    public async Task<ResultOperation> AddConfiguration(TicketConfiguration ticketConfiguration)
    {
        if (string.IsNullOrWhiteSpace(ticketConfiguration.UserType)) 
            return ResultOperation.Fail("TicketConfiguration user type cannot be empty.");
        
        return await ProcessConfigurationChange(() => ticketConfigurationRepository.AddAsync(ticketConfiguration), ticketConfiguration.UserType);
    }
    
    public async Task<ResultOperation> EditConfiguration(string userType, double price, int durationInHours, string newUserType)
    {
        var currentConfig = await ticketConfigurationRepository.GetConfigurationAsync(userType);

        currentConfig.UserType = newUserType;
        currentConfig.Price = price;
        currentConfig.DurationInHours = durationInHours;

        return await ProcessConfigurationChange(() => ticketConfigurationRepository.EditAsync(currentConfig), userType, newUserType);
    }
    
    private async Task<T> GetOrSetCache<T>(string cacheKey, Func<Task<T>> fetchFunction)
    {
        var cachedData = await redisService.GetAsync<T>(cacheKey);
        if (cachedData != null) return cachedData;

        var data = await fetchFunction();
        if (data != null)
            await redisService.SetAsync(cacheKey, data, TimeSpan.FromMinutes(CacheDurationMinutes));
        
        return data;
    }

    private async Task<ResultOperation> ProcessTicketChange(Func<Task<bool>> operation, string cacheKey)
    {
        return await ProcessChange(operation, new[] { cacheKey });
    }
    
    private async Task<ResultOperation> ProcessConfigurationChange(Func<Task<bool>> operation, string oldKey, string? newKey = null)
    {
        var keysToRemove = new List<string> { $"TicketConfig_{oldKey}", "All_Ticket_Configurations" };
        if (!string.IsNullOrWhiteSpace(newKey))
            keysToRemove.Add($"TicketConfig_{newKey}");
        
        return await ProcessChange(operation, keysToRemove);
    }
    
    private async Task<ResultOperation> ProcessChange(Func<Task<bool>> operation, IEnumerable<string> cacheKeys)
    {
        if (!await operation()) return ResultOperation.Fail("Operation failed.");
        foreach (var key in cacheKeys)
        {
            await redisService.RemoveAsync(key);
        }
        return ResultOperation.Ok();
    }
}
