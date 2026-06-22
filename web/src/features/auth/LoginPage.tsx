import React, { useEffect, useState } from 'react';
import { Link, Navigate, useSearchParams } from 'react-router-dom';
import {
  ChartBarIcon,
  CheckCircleIcon,
  RocketLaunchIcon,
} from '@heroicons/react/24/outline';
import { useAuth } from './AuthContext';

const GitHubIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg viewBox="0 0 16 16" className={className} aria-hidden="true">
    <path
      fill="currentColor"
      d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.01 8.01 0 0 0 16 8c0-4.42-3.58-8-8-8Z"
    />
  </svg>
);

const LoginPage: React.FC = () => {
  const { user, loading, login } = useAuth();
  const [searchParams] = useSearchParams();
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState<string>('');

  const redirectUri = searchParams.get('redirectUri') || searchParams.get('returnUrl') || '/';
  const isSignedOut = searchParams.get('signedOut') === 'true';

  useEffect(() => {
    if (isSignedOut) {
      setMessage('You have been signed out.');
    }
  }, [isSignedOut]);

  if (!loading && user) {
    return <Navigate to={redirectUri} replace />;
  }

  const handleGitHubLogin = () => {
    setSubmitting(true);
    login(`${window.location.origin}${redirectUri}`);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-blue-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8 relative overflow-hidden">
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute top-20 left-10 w-20 h-20 bg-blue-200/20 rounded-full animate-pulse" />
        <div className="absolute top-40 right-20 w-16 h-16 bg-indigo-200/15 rounded-full animate-pulse" />
        <div className="absolute bottom-40 left-20 w-12 h-12 bg-purple-200/20 rounded-full animate-pulse" />
        <div className="absolute bottom-20 right-40 w-18 h-18 bg-cyan-200/15 rounded-full animate-pulse" />

        <div className="absolute inset-0 opacity-5">
          <div className="grid grid-cols-12 h-full">
            {Array.from({ length: 12 }).map((_, index) => (
              <div key={index} className="border-r border-gray-300" />
            ))}
          </div>
          <div className="absolute inset-0">
            <div className="grid grid-rows-12 h-full">
              {Array.from({ length: 12 }).map((_, index) => (
                <div key={index} className="border-b border-gray-300" />
              ))}
            </div>
          </div>
        </div>

        <svg className="absolute inset-0 w-full h-full opacity-10" preserveAspectRatio="none">
          <defs>
            <linearGradient id="loginLineGradient" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stopColor="#111827" />
              <stop offset="50%" stopColor="#2563EB" />
              <stop offset="100%" stopColor="#7C3AED" />
            </linearGradient>
          </defs>
          <path
            d="M 0,200 Q 200,100 400,200 T 800,200"
            stroke="url(#loginLineGradient)"
            strokeWidth="2"
            fill="none"
            className="animate-pulse"
          />
          <path
            d="M 0,420 Q 300,300 600,420 T 1200,420"
            stroke="url(#loginLineGradient)"
            strokeWidth="2"
            fill="none"
            className="animate-pulse"
          />
          <circle cx="200" cy="200" r="4" fill="#2563EB" className="animate-ping" />
          <circle cx="600" cy="420" r="4" fill="#7C3AED" className="animate-ping" />
        </svg>
      </div>

      <div className="sm:mx-auto sm:w-full sm:max-w-md relative z-10">
        <Link to="/" className="flex justify-center items-center mb-6">
          <div className="relative">
            <RocketLaunchIcon className="h-10 w-10 text-gray-900 mr-3 drop-shadow-sm" />
            <div className="absolute -inset-1 bg-gradient-to-r from-gray-900 to-blue-600 rounded-full opacity-20 blur animate-pulse" />
          </div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 via-blue-900 to-purple-900 bg-clip-text text-transparent">
            Nexova
          </h1>
        </Link>

        <h2 className="text-center text-2xl font-medium text-gray-900 mb-2">
          Sign in to your workspace
        </h2>
        <p className="text-center text-sm text-gray-600 mb-8">
          Connect your data sources and explore queries securely.
        </p>
      </div>

      <div className="sm:mx-auto sm:w-full sm:max-w-md relative z-10">
        <div className="bg-white/85 backdrop-blur-sm py-8 px-4 shadow-xl rounded-xl sm:px-10 border border-white/30">
          {message && (
            <div className="mb-6 p-3 bg-green-50 border border-green-200 rounded-md">
              <div className="flex items-center">
                <CheckCircleIcon className="h-4 w-4 text-green-500 mr-2" />
                <span className="text-sm text-green-800">{message}</span>
              </div>
            </div>
          )}

          <button
            onClick={handleGitHubLogin}
            disabled={loading || submitting}
            className="w-full flex items-center justify-center px-4 py-2.5 border border-gray-900 rounded-md text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed bg-gray-900 hover:bg-gray-800 text-white"
          >
            {submitting ? (
              <div className="flex items-center">
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                Connecting...
              </div>
            ) : (
              <div className="flex items-center">
                <GitHubIcon className="h-4 w-4 mr-2" />
                Continue with GitHub
              </div>
            )}
          </button>

          <div className="mt-6 rounded-lg border border-gray-100 bg-gray-50 px-4 py-3">
            <div className="flex items-start gap-3">
              <div className="mt-0.5 h-8 w-8 rounded-md bg-white border border-gray-200 flex items-center justify-center">
                <ChartBarIcon className="h-4 w-4 text-gray-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-900">One account for your workspace</p>
                <p className="mt-1 text-xs text-gray-500">
                  Your data sources, query documents, and workspace connections are tied to
                  your signed-in user.
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-8 text-center relative z-10">
          <p className="text-xs text-gray-500">
            By signing in, you agree to Nexova's workspace access policies.
          </p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
