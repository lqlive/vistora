import React from 'react';
import { BrowserRouter as Router, useRoutes } from 'react-router-dom';
import { routes } from './routes';
import { AuthProvider } from './features/auth/AuthContext';

const AppRoutes: React.FC = () => useRoutes(routes);

const App: React.FC = () => (
  <Router>
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  </Router>
);

export default App;
