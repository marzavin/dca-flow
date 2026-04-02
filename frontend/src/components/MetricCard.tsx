type MetricType = "money" | "percent" | "text";

interface Props {
  title: string;
  value: number;
  level?: number;
  type: MetricType;
}

function MetricCard({ title, value, level, type }: Props) {
  const formatNumber = (num: number) => {
    return Number(num.toFixed(2)).toLocaleString();
  };

  const formatValue = () => {
    const formatted = formatNumber(value);

    if (type === "percent") {
      return `${formatted}%`;
    }

    if (type === "money") {
      return `$${formatted}`;
    }

    return formatted;
  };

  const getValueClass = () => {
    if (level === undefined) {
      return "";
    }

    if (value > level) {
      return "positive";
    }
    if (value < level) {
      return "negative";
    }
    return "";
  };

  return (
    <div className="metric-card">
      <div className="metric-title">{title}</div>
      <div className={`metric-value ${getValueClass()}`}>{formatValue()}</div>
    </div>
  );
}

export default MetricCard;
