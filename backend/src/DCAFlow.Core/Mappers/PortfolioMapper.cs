using DCAFlow.Contracts.Models;
using DCAFlow.Data.Entities;

namespace DCAFlow.Core.Mappers;

public static class PortfolioMapper
{
    public static PortfolioModel Map(PortfolioEntity document)
    {
        if (document is null)
        {
            return null;
        }

        return new PortfolioModel
        {
            Id = document.Id,
            Name = document.Name,
            HoldingsValue = 0D,
            TotalInvested = 0D,
            TotalReturn = 0D      
        };
    }
}
