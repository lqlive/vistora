import { Navigate, Outlet } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import Layout from './shared/layout/Layout';
import { dashboardsRoutes } from './features/dashboards/routes';
import { chartsRoutes } from './features/charts/routes';
import { datasetsRoutes } from './features/datasets/routes';
import { datasourceRoutes } from './features/datasources/routes';
import { sqlEditorRoutes } from './features/sql-editor/routes';

export const routes: RouteObject[] = [
  {
    element: (
      <Layout>
        <Outlet />
      </Layout>
    ),
    children: [
      { path: '/', element: <Navigate to="/dashboards" replace /> },
      ...dashboardsRoutes,
      ...chartsRoutes,
      ...datasetsRoutes,
      ...datasourceRoutes,
      ...sqlEditorRoutes,
    ],
  },
];
