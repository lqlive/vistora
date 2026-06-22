import { Navigate, Outlet } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import Layout from './shared/layout/Layout';
import { dashboardsRoutes } from './features/dashboards/routes';
import { chartsRoutes } from './features/charts/routes';
import { datasetsRoutes } from './features/datasets/routes';
import { datasourceRoutes } from './features/datasources/routes';
import { sqlEditorRoutes } from './features/sql-editor/routes';
import LoginPage from './features/auth/LoginPage';
import RequireAuth from './features/auth/RequireAuth';

export const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  {
    element: <RequireAuth />,
    children: [
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
    ],
  },
];
