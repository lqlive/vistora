import type { RouteObject } from 'react-router-dom';
import DashboardsPage from './DashboardsPage';

export const dashboardsRoutes: RouteObject[] = [
  { path: '/dashboards', element: <DashboardsPage /> },
];
