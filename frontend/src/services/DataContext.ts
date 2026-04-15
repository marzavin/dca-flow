import { createContext } from 'react';
import PortfolioModel from '../types/PortfolioModel';
import CoinModel from '../types/CoinModel';
import TransactionModel from '../types/TransactionModel';
import KeyValueModel from '../types/KeyValueModel';

export interface IDataProvider {
  getPortfolio(id: number): Promise<PortfolioModel | null>;
  getSupportedCoins(): Promise<CoinModel[]>;
  getSupportedTransactionTypes(): Promise<KeyValueModel<number, string>[]>;
  addTransaction(model: TransactionModel): Promise<void>;
}

export const DataContext = createContext<IDataProvider | undefined>(undefined);
