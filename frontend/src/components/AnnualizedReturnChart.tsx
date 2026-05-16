import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';

import { MONTHS } from '../helpers/DateFormatter';
import KeyValueModel from '../types/KeyValueModel';

interface Props {
  annualizedReturnTimeline: KeyValueModel<Date, number>[];
}

function AnnualizedReturnChart({ annualizedReturnTimeline }: Props) {
  const pad = (value: number): string => {
    return value.toString().padStart(2, '0');
  };

  const formatXAxisTick = (value: string): string => {
    const date = new Date(value);

    return `${pad(date.getDate())} ${MONTHS[date.getMonth()]}`;
  };

  const formatYAxisTick = (value: number): string => {
    return `${(value * 100).toFixed(2)}%`;
  };

  return (
    <div className="chart-container">
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={annualizedReturnTimeline}>
          <CartesianGrid stroke="rgba(148, 163, 184, 0.08)" vertical={false} />
          <XAxis
            dataKey="key"
            stroke="#94a3b8"
            tickFormatter={formatXAxisTick}
            tick={{ fill: '#94a3b8', fontSize: 12 }}
            axisLine={false}
            tickLine={false}
            minTickGap={30}
          />
          <YAxis
            stroke="#94a3b8"
            tickFormatter={formatYAxisTick}
            tick={{ fill: '#94a3b8', fontSize: 12 }}
            axisLine={false}
            tickLine={false}
          />
          <Tooltip
            formatter={(value) => formatYAxisTick(Number(value))}
            contentStyle={{
              background: '#1e293b',
              border: '1px solid #334155',
              borderRadius: '8px'
            }}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="value"
            name="Annualized Return"
            stroke="#3b82f6"
            strokeWidth={2}
            dot={false}
            activeDot={{ r: 5 }}
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}

export default AnnualizedReturnChart;
