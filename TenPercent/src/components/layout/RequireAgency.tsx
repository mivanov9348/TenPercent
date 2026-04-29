import { Navigate, Outlet } from 'react-router-dom';

export default function RequireAgency() {
  const hasAgency = localStorage.getItem('hasAgency') === 'true';
  
  if (!hasAgency) {
    return <Navigate to="/create-agency" replace />;
  }

  return <Outlet />;
}