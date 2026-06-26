import type { RouteObject } from 'react-router-dom';
import DashboardsPage from './DashboardsPage';
import DashboardEditorPage from './DashboardEditorPage';

export const dashboardsRoutes: RouteObject[] = [
  { path: '/dashboards', element: <DashboardsPage /> },
  { path: '/dashboards/new', element: <DashboardEditorPage /> },
  { path: '/dashboards/:id/edit', element: <DashboardEditorPage /> },
];
