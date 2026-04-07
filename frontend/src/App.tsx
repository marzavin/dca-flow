import { useEffect, useState } from 'react';
import './App.less';
import AssetsTable from './components/AssetsTable';
import ChartTabs from './components/ChartTabs';
import MetricCard from './components/MetricCard';
import { useData } from './services/useData';
import PortfolioModel from './types/PortfolioModel';

function App() {
  const dataProvider = useData();
  const [portfolio, setPortfolio] = useState<PortfolioModel>({
    id: 0,
    name: 'New portfolio',
    allocation: [],
    assets: [],
    transactions: [],
    holdingsValue: 0,
    totalInvested: 0,
    totalReturn: 0,
    totalInvestedTimeline: [],
    holdingsValueTimeline: []
  });

  useEffect(() => {
    dataProvider.getPortfolio(1).then((result) => {
      setPortfolio(result);
    });
  }, [dataProvider]);

  return (
    <>
      <div className="dashboard">
        <header className="header">
          <h1>Portfolio: {portfolio?.name}</h1>
        </header>
        <section className="metrics">
          <MetricCard title="Total Invested" value={portfolio.totalInvested} type="money" />
          <MetricCard
            title="Holdings Value"
            value={portfolio.holdingsValue}
            type="money"
            level={portfolio.totalInvested}
          />
          <MetricCard title="ROI" value={portfolio.totalReturn * 100} type="percent" level={0} />
          <MetricCard title="Assets" value={portfolio.assets.length} type="text" />
        </section>
        <ChartTabs data={portfolio} />
        <section className="table-block">
          <div className="block-header">Assets</div>
          <AssetsTable data={portfolio.assets} />
        </section>
        <section className="transactions-block">
          <div className="block-header">Transactions</div>
          <form className="transaction-form">
            <input placeholder="Ticker" />
            <input type="number" placeholder="Amount $" />
            <input type="number" placeholder="Quantity" />
            <select>
              <option value="1">Buy</option>
              <option value="2">Sell</option>
              <option value="3">Transfer In</option>
              <option value="4">Transfer Out</option>
            </select>
            <input type="datetime-local" />
            <button type="submit">Add</button>
          </form>
          <table className="transactions-table">
            <thead>
              <tr>
                <th>Ticker</th>
                <th>Amount</th>
                <th>Quantity</th>
                <th>Type</th>
                <th>Date</th>
              </tr>
            </thead>
            <tbody>
              {portfolio.transactions.map((item) => {
                return (
                  <tr key={item.id}>
                    <td>{item.ticker}</td>
                    <td>{item.cost}</td>
                    <td>{item.amount}</td>
                    <td className="buy">{item.type}</td>
                    <td>{new Date(item.timestamp).toDateString()}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </section>
      </div>
    </>
  );
}

export default App;
