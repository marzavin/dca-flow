interface TransactionModel {
  id: number;
  ticker: string;
  timestamp: Date;
  type: number;
  cost: number;
  amount: number;
}

export default TransactionModel;
