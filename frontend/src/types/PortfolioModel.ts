import AssetModel from './AssetModel';
import FractionModel from './FractionModel';
import KeyValueModel from './KeyValueModel';

interface PortfolioModel {
  id: number;
  name: string;
  totalInvested: number;
  holdingsValue: number;
  totalReturn: number;
  assets: AssetModel[];
  allocation: FractionModel[];
  investments: KeyValueModel<Date, number>[];
}

export default PortfolioModel;
