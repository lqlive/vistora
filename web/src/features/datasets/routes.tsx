import type { RouteObject } from 'react-router-dom';
import DatasetsPage from './DatasetsPage';

export const datasetsRoutes: RouteObject[] = [
  { path: '/datasets', element: <DatasetsPage /> },
];
