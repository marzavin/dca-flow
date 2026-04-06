import { useState } from 'react';
import AllocationChart from './AllocationChart';
import PerformanceChart from './PerformanceChart';
import PortfolioModel from '../types/PortfolioModel';

type Tab = 'allocation' | 'investments';

const tabs = [
  { id: 'allocation', label: 'Allocation' },
  { id: 'investments', label: 'Investments' }
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
      {activeTab === 'investments' && <PerformanceChart data={data.investments} />}
    </div>
  );
}

export default ChartTabs;
