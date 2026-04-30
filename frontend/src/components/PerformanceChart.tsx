import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';

import { MONTHS } from '../helpers/DateFormatter';
import { formatMoneyAxis } from '../helpers/MoneyFormatter';
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

  const pad = (value: number): string => {
    return value.toString().padStart(2, '0');
  };

  const formatXAxisTick = (value: string): string => {
    const date = new Date(value);

    return `${pad(date.getDate())} ${MONTHS[date.getMonth()]}`;
  };

  const formatYAxisTick = (value: number): string => {
    return formatMoneyAxis(value);
  };

  return (
    <div className="chart-container">
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={merged}>
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
            name="Holdings Value"
            stroke="#22c55e"
            strokeWidth={2}
            dot={false}
            activeDot={{ r: 5 }}
            isAnimationActive={false}
          />
          <Line
            type="monotone"
            dataKey="totalInvested"
            name="Total Invested"
            stroke="#ef4444"
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

export default PerformanceChart;
