import { useContext } from 'react';

import { DataContext } from './DataContext';

export const useData = () => {
  const context = useContext(DataContext);

  if (!context) {
    throw new Error('DataContext was not provided. Make sure your component is a child of the DataProvider.');
  }

  return context;
};
