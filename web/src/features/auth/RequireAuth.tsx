import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

const RequireAuth: React.FC = () => {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="flex items-center gap-3 text-sm text-gray-500">
          <div className="h-4 w-4 rounded-full border-2 border-gray-300 border-t-gray-900 animate-spin" />
          Loading workspace...
        </div>
      </div>
    );
  }

  if (!user) {
    const redirectUri = `${location.pathname}${location.search}`;
    return <Navigate to={`/login?redirectUri=${encodeURIComponent(redirectUri)}`} replace />;
  }

  return <Outlet />;
};

export default RequireAuth;
