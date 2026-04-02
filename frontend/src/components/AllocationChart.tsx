import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from "recharts";

const COLORS = ["#3b82f6", "#22c55e", "#eab308", "#ef4444", "#a855f7", "#14b8a6"];

import KeyValueModel from "../types/KeyValueModel";

interface Props {
  data: KeyValueModel<string, number>[];
}

function AllocationChart({ data }: Props) {
  const total = data.reduce((sum, item) => sum + item.value, 0);

  return (
    <div className="chart-container pie-wrapper">
      <div className="pie-chart">
        <ResponsiveContainer width="100%" height={260}>
          <PieChart>
            <Pie data={data} dataKey="value" nameKey="key" outerRadius={100} isAnimationActive={false}>
              {data.map((_, index) => (
                <Cell key={index} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip />
          </PieChart>
        </ResponsiveContainer>
      </div>
      <div className="pie-legend">
        {data.map((item, index) => {
          const percent = ((item.value / total) * 100).toFixed(2);

          return (
            <div key={item.key} className="legend-item">
              <span className="legend-color" style={{ background: COLORS[index % COLORS.length] }} />
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
