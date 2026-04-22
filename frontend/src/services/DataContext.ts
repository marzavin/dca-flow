import { createContext } from 'react';

import CoinModel from '../types/CoinModel';
import KeyValueModel from '../types/KeyValueModel';
import PortfolioModel from '../types/PortfolioModel';
import TransactionModel from '../types/TransactionModel';

export interface IDataProvider {
  getPortfolio(id: number): Promise<PortfolioModel | null>;
  getSupportedCoins(): Promise<CoinModel[]>;
  getSupportedTransactionTypes(): Promise<KeyValueModel<number, string>[]>;
  addTransaction(model: TransactionModel): Promise<void>;
}

export const DataContext = createContext<IDataProvider | undefined>(undefined);
