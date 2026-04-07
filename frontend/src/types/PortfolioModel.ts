import AssetModel from './AssetModel';
import FractionModel from './FractionModel';
import KeyValueModel from './KeyValueModel';
import TransactionModel from './TransactionModel';

interface PortfolioModel {
  id: number;
  name: string;
  totalInvested: number;
  holdingsValue: number;
  totalReturn: number;
  assets: AssetModel[];
  allocation: FractionModel[];
  transactions: TransactionModel[];
  totalInvestedTimeline: KeyValueModel<Date, number>[];
  holdingsValueTimeline: KeyValueModel<Date, number>[];
}

export default PortfolioModel;
