import { createContext } from 'react';
import PortfolioModel from '../types/PortfolioModel';

export interface IDataProvider {
  getPortfolio(id: number): Promise<PortfolioModel>;
}

export const DataContext = createContext<IDataProvider | undefined>(undefined);
