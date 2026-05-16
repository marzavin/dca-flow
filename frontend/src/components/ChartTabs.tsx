import { useState } from 'react';

import AllocationChart from './AllocationChart';
import AnnualizedReturnChart from './AnnualizedReturnChart';
import PerformanceChart from './PerformanceChart';
import PortfolioModel from '../types/PortfolioModel';

type Tab = 'allocation' | 'performance' | 'annualizedReturn';

const tabs = [
  { id: 'allocation', label: 'Allocation' },
  { id: 'performance', label: 'Performance' },
  { id: 'annualizedReturn', label: 'Annualized Return' }
];

interface Props {
  data: PortfolioModel;
}

function ChartTabs({ data }: Props) {
  const [activeTab, setActiveTab] = useState<Tab>('allocation');

  return (
    <>
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
      {activeTab === 'allocation' && <AllocationChart allocation={data.allocation} />}
      {activeTab === 'performance' && (
        <PerformanceChart
          totalInvestedTimeline={data.totalInvestedTimeline}
          holdingsValueTimeline={data.holdingsValueTimeline}
        />
      )}
      {activeTab === 'annualizedReturn' && (
        <AnnualizedReturnChart annualizedReturnTimeline={data.annualizedReturnTimeline} />
      )}
    </>
  );
}

export default ChartTabs;
