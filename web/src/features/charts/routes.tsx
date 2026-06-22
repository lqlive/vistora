import type { RouteObject } from 'react-router-dom';
import ChartEditorPage from './ChartEditorPage';
import ChartsPage from './ChartsPage';

export const chartsRoutes: RouteObject[] = [
  { path: '/charts', element: <ChartsPage /> },
  { path: '/charts/new', element: <ChartEditorPage /> },
  { path: '/charts/:id/edit', element: <ChartEditorPage /> },
];
