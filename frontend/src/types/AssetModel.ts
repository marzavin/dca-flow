import KeyValueModel from "./KeyValueModel";

interface AssetModel extends KeyValueModel<string, number> {
  averagePrice: number;
  dailyPerformance: number;
  holdings: number;
}

export default AssetModel;
