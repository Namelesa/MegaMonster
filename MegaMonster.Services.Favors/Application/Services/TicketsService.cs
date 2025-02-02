using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Application.Services;

public class TicketsService(ITicketRepository ticketRepository, ITicketConfigurationRepository ticketConfigurationRepository)
{
    public async Task<IEnumerable<Ticket>> GetAllTicketsByStatus(string status) => await ticketRepository.GetTicketByStatus(status);
    public async Task<IEnumerable<TicketConfiguration>> GetAllTicketConfigurations() => await ticketConfigurationRepository.GetAll();

    public async Task<TicketConfiguration> GetConfigurationForUser(string userType)
    {
        // validation
        return await ticketConfigurationRepository.GetConfigurationAsync(userType);
    }

    public async Task<ResultOperation> BuyTicket(Ticket ticket)
    {
        //validation
        var result = await ticketRepository.AddAsync(ticket);
        return result ? ResultOperation.Ok() : ResultOperation.Fail("Can not buy this ticket");
    }
    
    public async Task<ResultOperation> DeleteTicket(int id)
    {
        //validation
        var ticket = await ticketRepository.GetTicketById(id);
        var result = await ticketRepository.DeleteAsync(ticket);
        return result ? ResultOperation.Ok() : ResultOperation.Fail("Can not delete this ticket");
    }
    
    public async Task<ResultOperation> DeleteTicketConfiguration(string userType)
    {
        //validation
        var ticketConfiguration = await ticketConfigurationRepository.GetConfigurationAsync(userType);
        var result = await ticketConfigurationRepository.DeleteAsync(ticketConfiguration);
        return result ? ResultOperation.Ok() : ResultOperation.Fail("Can not delete this configuration");
    }
    
    public async Task<ResultOperation> AddConfiguration(TicketConfiguration ticketConfiguration)
    {
        if (string.IsNullOrWhiteSpace(ticketConfiguration.UserType)) 
            return ResultOperation.Fail("TicketConfiguration user type cannot be empty.");
        return await ticketConfigurationRepository.AddAsync(ticketConfiguration)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error adding configuration.");
    }
    
    public async Task<ResultOperation> EditConfiguration(string userType, double price, int durationInHours, string newUserType)
    {
        var currentConfig = await ticketConfigurationRepository.GetConfigurationAsync(userType);
        
        if (string.IsNullOrWhiteSpace(userType)) 
            return ResultOperation.Fail("TicketConfiguration user type cannot be empty.");
        
        currentConfig.UserType = newUserType;
        currentConfig.Price = price;
        currentConfig.DurationInHours = durationInHours;

        return await ticketConfigurationRepository.EditAsync(currentConfig)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error editing configuration.");
    }
    
}