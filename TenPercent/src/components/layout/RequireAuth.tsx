import { Navigate, Outlet } from 'react-router-dom';

export default function RequireAuth() {
  const userId = localStorage.getItem('userId');
  
  if (!userId) {
    // Ако няма userId, връщаме към логин
    return <Navigate to="/login" replace />;
  }

  return <Outlet />; // Ако всичко е наред, зареждаме съдържанието
}