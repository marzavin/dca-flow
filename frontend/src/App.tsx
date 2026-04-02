import "./App.less";
import AssetsTable from "./components/AssetsTable";
import ChartTabs from "./components/ChartTabs";
import MetricCard from "./components/MetricCard";
import data from "./data.json";

function App() {
  return (
    <>
      <div className="dashboard">
        <header className="header">
          <h1>Portfolio: {data.portfolioName}</h1>
        </header>
        <section className="metrics">
          <MetricCard title="Total Invested" value={data.totalInvested} type="money" />
          <MetricCard title="Portfolio Value" value={data.portfolioValue} type="money" level={data.totalInvested} />
          <MetricCard
            title="ROI"
            value={((data.portfolioValue - data.totalInvested) / data.totalInvested) * 100}
            type="percent"
            level={0}
          />
          <MetricCard title="Assets" value={data.assets.length} type="text" />
        </section>
        <ChartTabs />
        <section className="table-block">
          <div className="block-header">Assets</div>
          <AssetsTable data={data.assets} />
        </section>
      </div>
    </>
  );
}

export default App;
