import MetricType from './MetricType';
import RangeType from './RangeType';

interface MetricModel {
  name: string;
  value: number;
  rangeType: RangeType;
  metricType: MetricType;
}

export default MetricModel;
