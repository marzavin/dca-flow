import axios from 'axios';
import { toast } from 'react-toastify';
import { DataContext, IDataProvider } from './DataContext';
import PortfolioModel from '../types/PortfolioModel';
import CoinModel from '../types/CoinModel';
import TransactionModel from '../types/TransactionModel';
import KeyValueModel from '../types/KeyValueModel';

const createDataProvider = (): IDataProvider => {
  //const baseUrl = window.location.origin;
  const baseUrl = 'http://localhost:5500';
  const axiosInstance = axios.create();
  const serverErrorText = 'An error occurred during execution your request.';

  return {
    async getPortfolio(id: number): Promise<PortfolioModel | null> {
      return axiosInstance.get<PortfolioModel>(`${baseUrl}/api/portfolios/${id}`).then(
        function (response) {
          return response.data;
        },
        function (error) {
          console.log(error);
          toast.error(serverErrorText);
          return null;
        }
      );
    },
    async getSupportedCoins(): Promise<CoinModel[]> {
      return axiosInstance.get<CoinModel[]>(`${baseUrl}/api/coins`).then(
        function (response) {
          return response.data;
        },
        function (error) {
          console.log(error);
          toast.error(serverErrorText);
          return [];
        }
      );
    },
    async addTransaction(model: TransactionModel): Promise<void> {
      return axiosInstance.post(`${baseUrl}/api/transactions`, model).then(
        function () {
          return;
        },
        function (error) {
          console.log(error);
          toast.error(serverErrorText);
          return;
        }
      );
    },
    async getSupportedTransactionTypes(): Promise<KeyValueModel<number, string>[]> {
      return axiosInstance.get<KeyValueModel<number, string>[]>(`${baseUrl}/api/transactions/types`).then(
        function (response) {
          return response.data;
        },
        function (error) {
          console.log(error);
          toast.error(serverErrorText);
          return [];
        }
      );
    }
  };
};

const DataProvider = ({ children }: { children: React.ReactNode }) => {
  const dataProvider = createDataProvider();
  return <DataContext.Provider value={dataProvider}>{children}</DataContext.Provider>;
};

export default DataProvider;
