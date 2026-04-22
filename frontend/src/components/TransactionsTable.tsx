import { formatDateTime } from '../helpers/DateFormatter';
import { formatMoneyFull } from '../helpers/MoneyFormatter';
import TransactionModel from '../types/TransactionModel';
import TransactionType from '../types/TransactionType';

interface Props {
  data: TransactionModel[];
}

function TransactionsTable({ data }: Props) {
  const TransactionTypeMeta = {
    [TransactionType.Buy]: { label: 'Buy', className: 'positive' },
    [TransactionType.Sell]: { label: 'Sell', className: 'negative' },
    [TransactionType.TransferIn]: { label: 'Deposit', className: 'positive' },
    [TransactionType.TransferOut]: { label: 'Withdraw', className: 'negative' }
  };

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Ticker</th>
            <th>Cost</th>
            <th>Amount</th>
            <th>Type</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {data.map((item) => {
            return (
              <tr key={item.id}>
                <td>{item.ticker}</td>
                <td>{formatMoneyFull(item.cost)}</td>
                <td>{item.amount}</td>
                <td className={TransactionTypeMeta[item.type].className}>{TransactionTypeMeta[item.type].label}</td>
                <td>{formatDateTime(new Date(item.timestamp))}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </>
  );
}

export default TransactionsTable;
