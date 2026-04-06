import AssetModel from '../types/AssetModel';

interface Props {
  data: AssetModel[];
}

function AssetsTable({ data }: Props) {
  const formatUSD = (amount: number): string => {
    const integerPart = Math.floor(amount);
    const decimalPart = Math.round((amount - integerPart) * 100);
    const withCommas = integerPart.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');

    if (decimalPart === 0) {
      return `$${withCommas}`;
    }

    const decimalStr = decimalPart < 10 ? `0${decimalPart}` : `${decimalPart}`;

    return `$${withCommas}.${decimalStr}`;
  };

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Ticker</th>
            <th>Average Buy Price</th>
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
                <td>{formatUSD(item.averageBuyPrice)}</td>
                <td>{item.totalInvested}</td>
                {item.totalReturn > 0 && <td className="positive">+{(item.totalReturn * 100).toFixed(2)}%</td>}
                {item.totalReturn < 0 && <td className="negative">{(item.totalReturn * 100).toFixed(2)}%</td>}
                {item.totalReturn === 0 && <td>0.00%</td>}
                <td>{item.totalHoldings}</td>
                <td>{formatUSD(item.totalHoldings * item.averageBuyPrice)}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </>
  );
}

export default AssetsTable;
