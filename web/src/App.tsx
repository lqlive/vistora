import React from 'react';
import { BrowserRouter as Router, useRoutes } from 'react-router-dom';
import { routes } from './routes';

const AppRoutes: React.FC = () => useRoutes(routes);

const App: React.FC = () => (
  <Router>
    <AppRoutes />
  </Router>
);

export default App;
