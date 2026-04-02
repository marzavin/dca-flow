import "./App.less";
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
          <MetricCard
            title="Total Invested"
            value={data.totalInvested}
            type="money"
          />
          <MetricCard
            title="Portfolio Value"
            value={data.portfolioValue}
            type="money"
            level={data.totalInvested}
          />
          <MetricCard
            title="ROI"
            value={
              ((data.portfolioValue - data.totalInvested) /
                data.totalInvested) *
              100
            }
            type="percent"
            level={0}
          />
          <MetricCard title="Assets" value={data.assets.length} type="text" />
        </section>

        <section className="chart-block">
          <div className="block-header">Performance</div>
          <div className="chart-placeholder"></div>
        </section>

        <section className="table-block">
          <div className="block-header">Assets</div>

          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Price</th>
                <th>24h</th>
                <th>Holdings</th>
                <th>Value</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>BTC</td>
                <td>$68,000</td>
                <td className="positive">+2.1%</td>
                <td>0.5</td>
                <td>$34,000</td>
              </tr>

              <tr>
                <td>ETH</td>
                <td>$3,200</td>
                <td className="negative">-1.4%</td>
                <td>10</td>
                <td>$32,000</td>
              </tr>
            </tbody>
          </table>
        </section>
      </div>
    </>
  );
}

export default App;
