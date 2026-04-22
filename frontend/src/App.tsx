import { useEffect, useState } from 'react';

import './App.less';
import AssetsTable from './components/AssetsTable';
import ChartTabs from './components/ChartTabs';
import MetricCard from './components/MetricCard';
import TransactionModal from './components/TransactionModal';
import TransactionsTable from './components/TransactionsTable';
import { getMetrics } from './helpers/MetricCalculator';
import { useData } from './services/useData';
import PortfolioModel from './types/PortfolioModel';
import TransactionFormModel from './types/TransactionFormModel';
import TransactionModel from './types/TransactionModel';

function App() {
  const dataProvider = useData();

  const [isTransactionModalOpen, setIsTransactionModalOpen] = useState(false);
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
  }, [dataProvider]);

  useEffect(() => {
    const onEsc = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setIsTransactionModalOpen(false);
      }
    };

    window.addEventListener('keydown', onEsc);
    return () => window.removeEventListener('keydown', onEsc);
  }, []);

  const handleAddTransaction = async (data: TransactionFormModel) => {
    const payload: TransactionModel = {
      id: 0,
      ticker: data.ticker,
      amount: data.amount,
      cost: data.cost,
      type: data.type,
      timestamp: new Date(data.timestamp),
      portfolioId: portfolio.id
    };

    dataProvider.addTransaction(payload).then(function () {
      dataProvider.getPortfolio(1).then((result) => {
        if (result !== null) {
          setPortfolio(result);
        }
      });
    });
  };

  const metrics = getMetrics(portfolio);

  return (
    <>
      <div className="dashboard">
        <header className="header">
          <h1>Portfolio: {portfolio?.name}</h1>
        </header>
        <section className="metrics-block block">
          {metrics.map((item) => {
            return <MetricCard key={item.name} data={item} />;
          })}
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
          <button onClick={() => setIsTransactionModalOpen(true)}>+</button>
          <div className="transactions-table table">
            <TransactionsTable data={portfolio.transactions} />
          </div>
        </section>
        <TransactionModal
          isOpen={isTransactionModalOpen}
          onClose={() => setIsTransactionModalOpen(false)}
          onSubmit={handleAddTransaction}
        />
      </div>
    </>
  );
}

export default App;
