interface AssetModel {
  ticker: string;
  currentMarketPrice: number;
  averageBuyPrice: number;
  totalInvested: number;
  totalHoldings: number;
  totalReturn: number;
  holdingsValue: number;
}

export default AssetModel;
