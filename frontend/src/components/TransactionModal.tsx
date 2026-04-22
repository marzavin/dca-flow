import { useState, useEffect } from 'react';

import { useData } from '../services/useData';
import CoinModel from '../types/CoinModel';
import KeyValueModel from '../types/KeyValueModel';
import TransactionFormModel from '../types/TransactionFormModel';
import TransactionType from '../types/TransactionType';

type Props = {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: TransactionFormModel) => void;
};

function TransactionModal({ isOpen, onClose, onSubmit }: Props) {
  const dataProvider = useData();

  const [supportedCoins, setSupportedCoins] = useState<CoinModel[]>([]);
  const [supportedTransactionTypes, setSupportedTransactionTypes] = useState<KeyValueModel<number, string>[]>([]);

  useEffect(() => {
    dataProvider.getSupportedCoins().then((result) => {
      setSupportedCoins(result);
    });
  }, [dataProvider]);

  useEffect(() => {
    dataProvider.getSupportedTransactionTypes().then((result) => {
      setSupportedTransactionTypes(result);
    });
  }, [dataProvider]);

  const [form, setForm] = useState<TransactionFormModel>({
    ticker: 'BTC',
    amount: 0,
    cost: 0,
    type: TransactionType.Buy,
    timestamp: ''
  });

  const handleChange = <K extends keyof TransactionFormModel>(field: K, value: TransactionFormModel[K]) => {
    setForm((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = (e: React.SubmitEvent) => {
    e.preventDefault();

    const payload = {
      ...form,
      timestamp: new Date(form.timestamp).toISOString()
    };

    onSubmit(payload);
    onClose();

    setForm({
      ticker: 'BTC',
      amount: 0,
      cost: 0,
      type: TransactionType.Buy,
      timestamp: ''
    });
  };

  useEffect(() => {
    const onEsc = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    window.addEventListener('keydown', onEsc);
    return () => window.removeEventListener('keydown', onEsc);
  }, [onClose]);

  if (!isOpen) {
    return null;
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Transaction</h3>
          <button onClick={onClose}>✕</button>
        </div>
        <form className="modal-form" onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="ticker">Ticker</label>
            <select
              id="ticker"
              name="ticker"
              value={form.ticker}
              onChange={(e) => handleChange('ticker', e.target.value)}
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
              value={form.cost}
              onChange={(e) => handleChange('cost', Number(e.target.value))}
            />
          </div>
          <div className="field">
            <label htmlFor="amount">Amount</label>
            <input
              id="amount"
              name="amount"
              type="number"
              value={form.amount}
              onChange={(e) => handleChange('amount', Number(e.target.value))}
            />
          </div>
          <div className="field">
            <label htmlFor="type">Type</label>
            <select
              id="type"
              name="type"
              value={form.type}
              onChange={(e) => handleChange('type', Number(e.target.value))}
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
              value={form.timestamp}
              onChange={(e) => handleChange('timestamp', e.target.value)}
            />
          </div>
          <button type="submit" className="submit-btn">
            Save
          </button>
        </form>
      </div>
    </div>
  );
}

export default TransactionModal;
