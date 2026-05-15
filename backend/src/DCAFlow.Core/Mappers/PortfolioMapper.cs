using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;

namespace DCAFlow.Core.Mappers;

public static class PortfolioMapper
{
    public static PortfolioModel Map(PortfolioDocument document)
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
