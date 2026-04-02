import { useState } from "react";
import AllocationChart from "./allocationChart";
import data from "../data.json";
import PerformanceChart from "./PerformanceChart";

type Tab = "allocation" | "performance";

const tabs = [
  { id: "allocation", label: "Allocation" },
  { id: "performance", label: "Performance" }
];

function ChartTabs() {
  const [activeTab, setActiveTab] = useState<Tab>("allocation");

  return (
    <div className="chart-block">
      <div className="chart-tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            className={activeTab === tab.id ? "active" : ""}
            onClick={() => setActiveTab(tab.id as Tab)}
          >
            {tab.label}
          </button>
        ))}
      </div>
      {activeTab === "allocation" && <AllocationChart data={data.assets} />}
      {activeTab === "performance" && <PerformanceChart data={data.performance} />}
    </div>
  );
}

export default ChartTabs;
