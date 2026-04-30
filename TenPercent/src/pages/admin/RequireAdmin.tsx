import { Navigate } from 'react-router-dom';

export default function RequireAdmin({ children }: { children: React.ReactNode }) {
  const role = localStorage.getItem('role');
  
  if (role !== 'Admin') {
    return <Navigate to="/" replace />; // Ако не си админ, те гони обратно в играта
  }

  return <>{children}</>;
}