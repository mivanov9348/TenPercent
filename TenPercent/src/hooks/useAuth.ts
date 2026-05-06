import { useNavigate } from 'react-router-dom';

export function useAuth() {
  const navigate = useNavigate();
  const userId = localStorage.getItem('userId');

  // Удобна функция, която хем ти дава ID-то, хем те разлогва, ако го няма
  const getUserIdOrRedirect = () => {
    if (!userId) {
      navigate('/login');
      return null;
    }
    return userId;
  };

  return { 
    userId, 
    isAuthenticated: !!userId,
    getUserIdOrRedirect 
  };
}