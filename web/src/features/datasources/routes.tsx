import type { RouteObject } from 'react-router-dom';
import DatasourcesPage from './DatasourcesPage';
import DatasourceDetailPage from './DatasourceDetailPage';

export const datasourceRoutes: RouteObject[] = [
  { path: '/datasources', element: <DatasourcesPage /> },
  { path: '/datasources/:id', element: <DatasourceDetailPage /> },
];
