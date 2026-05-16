using DCAFlow.Contracts.Models;

namespace DCAFlow.Core.Services;

public static class AnnualizedReturnCalculator
{
    public static double CalculateAnnualizedReturn(List<CashFlowModel> transactions, double guess = 0.1, double tolerance = 0.000001, int maxIterations = 100)
    {
        if (transactions == null || transactions.Count < 2)
        {
            throw new ApplicationException("At least two transactions are required.");
        }

        var hasPositive = transactions.Any(cf => cf.Value > 0D);
        var hasNegative = transactions.Any(cf => cf.Value < 0D);

        if (!hasPositive || !hasNegative)
        { 
            throw new ApplicationException("Transactions must contain both positive and negative values.");
        }

        var firstDate = transactions.Min(transaction => transaction.Timestamp);

        var x0 = guess;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var fValue = 0D;
            var fDerivative = 0D;

            foreach (var transaction in transactions)
            {
                var days = (transaction.Timestamp - firstDate).TotalDays;
                var years = days / 365.25;

                var amount = (double)transaction.Value;

                var denominator = Math.Pow(1D + x0, years);

                fValue += amount / denominator;

                fDerivative += -years * amount / Math.Pow(1D + x0, years + 1D);
            }

            var x1 = x0 - fValue / fDerivative;

            if (Math.Abs(x1 - x0) <= tolerance)
            {
                return x1;
            }

            x0 = x1;
        }

        throw new ApplicationException("XIRR calculation did not converge.");
    }
}
