import MetricModel from '../types/MetricModel';
import MetricType from '../types/MetricType';
import PortfolioModel from '../types/PortfolioModel';
import RangeType from '../types/RangeType';

const getRangeTypeByLevel = (value: number, level: number): RangeType => {
  if (Math.abs(value - level) < 0.001) {
    return RangeType.Neutral;
  }

  if (value > level) {
    return RangeType.Positive;
  }

  if (value < level) {
    return RangeType.Negative;
  }

  return RangeType.Neutral;
};

export function getMetrics(portfolio: PortfolioModel): MetricModel[] {
  const metrics: MetricModel[] = [];

  if (portfolio.assets.length === 0) {
    return metrics;
  }

  metrics.push({
    name: 'Total Invested',
    value: portfolio.totalInvested,
    metricType: MetricType.Money,
    rangeType: RangeType.Neutral
  });

  metrics.push({
    name: 'Holdings Value',
    value: portfolio.holdingsValue,
    metricType: MetricType.Money,
    rangeType: getRangeTypeByLevel(portfolio.holdingsValue, portfolio.totalInvested)
  });

  metrics.push({
    name: 'Profit',
    value: portfolio.holdingsValue - portfolio.totalInvested,
    metricType: MetricType.Money,
    rangeType: getRangeTypeByLevel(portfolio.holdingsValue, portfolio.totalInvested)
  });

  metrics.push({
    name: 'ROI',
    value: portfolio.totalReturn * 100,
    metricType: MetricType.Percent,
    rangeType: getRangeTypeByLevel(portfolio.totalReturn, 0)
  });

  if (portfolio.annualizedReturn !== null) {
    metrics.push({
      name: 'Annualized Return',
      value: portfolio.annualizedReturn * 100,
      metricType: MetricType.Percent,
      rangeType: getRangeTypeByLevel(portfolio.annualizedReturn, 0)
    });
  }

  metrics.push({
    name: 'Assets',
    value: portfolio.assets.length,
    metricType: MetricType.Text,
    rangeType: RangeType.Neutral
  });

  return metrics;
}
