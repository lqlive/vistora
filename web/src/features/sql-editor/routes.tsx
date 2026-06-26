import type { RouteObject } from 'react-router-dom';
import SqlEditorPage from './SqlEditorPage';

export const sqlEditorRoutes: RouteObject[] = [
  { path: '/sql-editor', element: <SqlEditorPage /> },
  { path: '/sql-editor/:id', element: <SqlEditorPage /> },
];
