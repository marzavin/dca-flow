import { formatMoneyFull } from '../helpers/MoneyFormatter';
import MetricModel from '../types/MetricModel';
import MetricType from '../types/MetricType';
import RangeType from '../types/RangeType';

interface Props {
  data: MetricModel;
}

function MetricCard({ data }: Props) {
  const formatNumber = (num: number) => {
    return Number(num.toFixed(2)).toLocaleString();
  };

  const formatValue = () => {
    const formatted = formatNumber(data.value);

    if (data.metricType === MetricType.Percent) {
      return `${formatted}%`;
    }

    if (data.metricType === MetricType.Money) {
      return formatMoneyFull(data.value);
    }

    return formatted;
  };

  const getValueClass = () => {
    if (data.rangeType === RangeType.Positive) {
      return 'positive';
    } else if (data.rangeType === RangeType.Negative) {
      return 'negative';
    }

    return '';
  };

  return (
    <div className="metric-card panel">
      <div className="metric-title">{data.name}</div>
      <div className={`metric-value ${getValueClass()}`}>{formatValue()}</div>
    </div>
  );
}

export default MetricCard;
