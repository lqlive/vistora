import type { RouteObject } from 'react-router-dom';
import ChartsPage from './ChartsPage';

export const chartsRoutes: RouteObject[] = [
  { path: '/charts', element: <ChartsPage /> },
];
