import axios from 'axios';
import { DataContext, IDataProvider } from './DataContext';
import PortfolioModel from '../types/PortfolioModel';

const createDataProvider = (): IDataProvider => {
  //const baseUrl = window.location.origin;
  const baseUrl = 'http://localhost:5500';
  const axiosInstance = axios.create();

  return {
    async getPortfolio(id: number): Promise<PortfolioModel> {
      return axiosInstance.get<PortfolioModel>(`${baseUrl}/api/portfolios/${id}`).then(function (response) {
        return response.data;
      });
    }
  };
};

const DataProvider = ({ children }: { children: React.ReactNode }) => {
  const dataProvider = createDataProvider();
  return <DataContext.Provider value={dataProvider}>{children}</DataContext.Provider>;
};

export default DataProvider;
