import { PieChart, Pie, Tooltip, ResponsiveContainer } from "recharts";

const COLORS = ["#3b82f6", "#22c55e", "#eab308", "#ef4444", "#a855f7", "#14b8a6"];

import KeyValueModel from "../types/KeyValueModel";

interface Props {
  data: KeyValueModel<string, number>[];
}

function AllocationChart({ data }: Props) {
  const total = data.reduce((sum, item) => sum + item.value, 0);
  const chartModel = data.map((item, index) => {
    return { key: item.key, value: item.value, fill: COLORS[index % COLORS.length] };
  });

  return (
    <div className="chart-container pie-wrapper">
      <div className="pie-chart">
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie data={chartModel} dataKey="value" nameKey="key" outerRadius={100} isAnimationActive={false} />
            <Tooltip />
          </PieChart>
        </ResponsiveContainer>
      </div>
      <div className="pie-legend">
        {chartModel.map((item) => {
          const percent = ((item.value / total) * 100).toFixed(2);
          return (
            <div key={item.key} className="legend-item">
              <span className="legend-color" style={{ background: item.fill }} />
              <span className="legend-name">{item.key}</span>
              <span className="legend-value">{percent}%</span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default AllocationChart;
