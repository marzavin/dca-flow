import { useState } from 'react';
import AllocationChart from './AllocationChart';
import PerformanceChart from './PerformanceChart';
import PortfolioModel from '../types/PortfolioModel';

type Tab = 'allocation' | 'performance';

const tabs = [
  { id: 'allocation', label: 'Allocation' },
  { id: 'performance', label: 'Performance' }
];

interface Props {
  data: PortfolioModel;
}

function ChartTabs({ data }: Props) {
  const [activeTab, setActiveTab] = useState<Tab>('allocation');

  return (
    <div className="chart-block">
      <div className="chart-tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={activeTab === tab.id ? 'active' : ''}
            onClick={() => setActiveTab(tab.id as Tab)}
          >
            {tab.label}
          </button>
        ))}
      </div>
      {activeTab === 'allocation' && <AllocationChart data={data.allocation} />}
      {activeTab === 'performance' && (
        <PerformanceChart
          totalInvestedTimeline={data.totalInvestedTimeline}
          holdingsValueTimeline={data.holdingsValueTimeline}
        />
      )}
    </div>
  );
}

export default ChartTabs;
