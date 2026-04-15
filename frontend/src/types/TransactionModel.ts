import TransactionType from './TransactionType';

interface TransactionModel {
  id: number;
  ticker: string;
  timestamp: Date;
  type: TransactionType;
  cost: number;
  amount: number;
  portfolioId: number;
}

export default TransactionModel;
