import AssetModel from "../types/AssetModel";

interface Props {
  data: AssetModel[];
}

function AssetsTable({ data }: Props) {
  const formatUSD = (amount: number): string => {
    const integerPart = Math.floor(amount);
    const decimalPart = Math.round((amount - integerPart) * 100);
    const withCommas = integerPart.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");

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
            <th>Name</th>
            <th>Price</th>
            <th>24h</th>
            <th>Holdings</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {data.map((item) => {
            return (
              <tr key={item.key}>
                <td>{item.key}</td>
                <td>{formatUSD(item.averagePrice)}</td>
                {item.dailyPerformance > 0 && (
                  <td className="positive">+{(item.dailyPerformance * 100).toFixed(2)}%</td>
                )}
                {item.dailyPerformance < 0 && <td className="negative">{(item.dailyPerformance * 100).toFixed(2)}%</td>}
                {item.dailyPerformance === 0 && <td>0.00%</td>}
                <td>{item.holdings}</td>
                <td>{formatUSD(item.value)}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </>
  );
}

export default AssetsTable;
