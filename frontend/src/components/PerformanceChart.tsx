import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';

import KeyValueModel from '../types/KeyValueModel';

interface Props {
  totalInvestedTimeline: KeyValueModel<Date, number>[];
  holdingsValueTimeline: KeyValueModel<Date, number>[];
}

function PerformanceChart({ totalInvestedTimeline, holdingsValueTimeline }: Props) {
  const merged = totalInvestedTimeline.map((item, index) => ({
    key: item.key,
    totalInvested: item.value,
    holdingsValue: holdingsValueTimeline[index].value
  }));

  return (
    <div className="chart-container">
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={merged}>
          <CartesianGrid stroke="rgba(255,255,255,0.05)" />
          <XAxis dataKey="key" stroke="#94a3b8" />
          <YAxis stroke="#94a3b8" />
          <Tooltip
            contentStyle={{
              background: '#1e293b',
              border: '1px solid #334155',
              borderRadius: '8px'
            }}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="holdingsValue"
            stroke="#22c55e"
            strokeWidth={1}
            dot={false}
            isAnimationActive={false}
          />
          <Line
            type="monotone"
            dataKey="totalInvested"
            stroke="#ef4444"
            strokeWidth={1}
            dot={false}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}

export default PerformanceChart;
