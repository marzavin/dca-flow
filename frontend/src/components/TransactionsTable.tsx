import TransactionModel from '../types/TransactionModel';
import { formatDateTime } from '../helpers/DateFormatter';
import { formatMoneyFull } from '../helpers/MoneyFormatter';
import TransactionType from '../types/TransactionType';

interface Props {
  data: TransactionModel[];
}

function TransactionsTable({ data }: Props) {
  const TypeLabels: Record<TransactionType, string> = {
    [TransactionType.Buy]: 'Buy',
    [TransactionType.Sell]: 'Sell',
    [TransactionType.TransferIn]: 'Deposit',
    [TransactionType.TransferOut]: 'Withdraw'
  };

  const TypeColorClass: Record<TransactionType, string> = {
    [TransactionType.Buy]: 'positive',
    [TransactionType.Sell]: 'negative',
    [TransactionType.TransferIn]: 'positive',
    [TransactionType.TransferOut]: 'negative'
  };

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Ticker</th>
            <th>Amount</th>
            <th>Quantity</th>
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
                <td className={TypeColorClass[item.type]}>{TypeLabels[item.type]}</td>
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
