import { useEffect, useState } from 'react';
import './App.less';
import AssetsTable from './components/AssetsTable';
import ChartTabs from './components/ChartTabs';
import MetricCard from './components/MetricCard';
import { useData } from './services/useData';
import PortfolioModel from './types/PortfolioModel';
import TransactionsTable from './components/TransactionsTable';
import CoinModel from './types/CoinModel';
import KeyValueModel from './types/KeyValueModel';
import TransactionModel from './types/TransactionModel';

function App() {
  const dataProvider = useData();
  const [supportedCoins, setSupportedCoins] = useState<CoinModel[]>([]);
  const [supportedTransactionTypes, setSupportedTransactionTypes] = useState<KeyValueModel<number, string>[]>([]);
  const [newTransaction, setNewTransaction] = useState({
    ticker: 'SOL',
    amount: 0,
    cost: 0,
    type: 1,
    timestamp: ''
  });
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
      if (result !== null) {
        setPortfolio(result);
      }
    });
    dataProvider.getSupportedCoins().then((result) => {
      setSupportedCoins(result);
    });
    dataProvider.getSupportedTransactionTypes().then((result) => {
      setSupportedTransactionTypes(result);
    });
  }, [dataProvider]);

  const handleChange = (field: string, value: any) => {
    setNewTransaction((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleAddTransaction = async (e: React.SubmitEvent) => {
    e.preventDefault();

    const payload: TransactionModel = {
      id: 0,
      ticker: newTransaction.ticker,
      amount: newTransaction.amount,
      cost: newTransaction.cost,
      type: newTransaction.type,
      timestamp: new Date(newTransaction.timestamp),
      portfolioId: portfolio.id
    };

    dataProvider.addTransaction(payload).then(function () {
      setNewTransaction({
        ticker: '',
        amount: 0,
        cost: 0,
        type: 1,
        timestamp: ''
      });
    });
  };

  return (
    <>
      <div className="dashboard">
        <header className="header">
          <h1>Portfolio: {portfolio?.name}</h1>
        </header>
        <section className="metrics-block block">
          <MetricCard title="Total Invested" value={portfolio.totalInvested} type="money" />
          <MetricCard
            title="Holdings Value"
            value={portfolio.holdingsValue}
            type="money"
            level={portfolio.totalInvested}
          />
          <MetricCard title="Profit" value={portfolio.holdingsValue - portfolio.totalInvested} type="money" level={0} />
          <MetricCard title="ROI" value={portfolio.totalReturn * 100} type="percent" level={0} />
          <MetricCard title="Assets" value={portfolio.assets.length} type="text" />
        </section>
        <section className="charts-block block panel">
          <ChartTabs data={portfolio} />
        </section>
        <section className="assets-block block panel">
          <div className="block-header">Assets</div>
          <div className="assets-table table">
            <AssetsTable data={portfolio.assets} />
          </div>
        </section>
        <section className="transactions-block block panel">
          <div className="block-header">Transactions</div>
          <form className="transaction-form" onSubmit={handleAddTransaction}>
            <div className="form-grid">
              <div className="field">
                <label htmlFor="ticker">Ticker</label>
                <select
                  id="ticker"
                  name="ticker"
                  value={newTransaction.ticker}
                  onChange={(e) => handleChange(e.target.name, e.target.value)}
                >
                  {supportedCoins.map((coin) => {
                    return (
                      <option key={coin.ticker} value={coin.ticker}>
                        {coin.name}
                      </option>
                    );
                  })}
                </select>
              </div>
              <div className="field">
                <label htmlFor="cost">Cost</label>
                <input
                  id="cost"
                  name="cost"
                  type="number"
                  value={newTransaction.cost}
                  onChange={(e) => handleChange(e.target.name, Number(e.target.value))}
                />
              </div>
              <div className="field">
                <label htmlFor="amount">Amount</label>
                <input
                  id="amount"
                  name="amount"
                  type="number"
                  value={newTransaction.amount}
                  onChange={(e) => handleChange(e.target.name, Number(e.target.value))}
                />
              </div>
              <div className="field">
                <label htmlFor="type">Type</label>
                <select
                  id="type"
                  name="type"
                  value={newTransaction.type}
                  onChange={(e) => handleChange(e.target.name, Number(e.target.value))}
                >
                  {supportedTransactionTypes.map((type) => {
                    return (
                      <option key={type.key} value={type.key}>
                        {type.value}
                      </option>
                    );
                  })}
                </select>
              </div>
              <div className="field">
                <label htmlFor="timestamp">Type</label>
                <input
                  id="timestamp"
                  name="timestamp"
                  type="datetime-local"
                  value={newTransaction.timestamp}
                  onChange={(e) => handleChange(e.target.name, e.target.value)}
                />
              </div>
              <div className="form-actions">
                <button type="submit">Add</button>
              </div>
            </div>
          </form>
          <div className="transactions-table table">
            <TransactionsTable data={portfolio.transactions} />
          </div>
        </section>
      </div>
    </>
  );
}

export default App;
