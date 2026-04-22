import { formatMoneyFull } from '../helpers/MoneyFormatter';
import AssetModel from '../types/AssetModel';

interface Props {
  data: AssetModel[];
}

function AssetsTable({ data }: Props) {
  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Ticker</th>
            <th>Market Price</th>
            <th>Buy Price (AVG)</th>
            <th>Total Invested</th>
            <th>ROI</th>
            <th>Holdings</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {data.map((item) => {
            return (
              <tr key={item.ticker}>
                <td>{item.ticker}</td>
                <td>{formatMoneyFull(item.currentMarketPrice)}</td>
                <td>{formatMoneyFull(item.averageBuyPrice)}</td>
                <td>{formatMoneyFull(item.totalInvested)}</td>
                {item.totalReturn > 0 && <td className="positive">+{(item.totalReturn * 100).toFixed(2)}%</td>}
                {item.totalReturn < 0 && <td className="negative">{(item.totalReturn * 100).toFixed(2)}%</td>}
                {item.totalReturn === 0 && <td>0.00%</td>}
                <td>{item.totalHoldings}</td>
                <td>{formatMoneyFull(item.holdingsValue)}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </>
  );
}

export default AssetsTable;
