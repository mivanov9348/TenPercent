import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/layout/Layout';

// Страници на играта
import Home from './pages/Home';
import Agency from './pages/Agency';
import Players from './pages/Players';
import Finance from './pages/Finance';
import Market from './pages/Market';

// Auth Страници
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import CreateAgency from './pages/auth/CreateAgency';

import WorldLayout from './pages/world/WorldLayout';
import Standings from './pages/world/Standings';
import Scorers from './pages/world/Scorers';
import Awards from './pages/world/Awards';
import Inbox from './pages/Inbox';
import RequireAuth from './components/layout/RequireAuth';
import RequireAgency from './components/layout/RequireAgency';

function App() {
  // Проверяваме дали вече сме логнати, за да пренасочим от /login към / директно
  const isLoggedIn = !!localStorage.getItem('userId');

  return (
    <BrowserRouter>
      <Routes>
        {/* Публични страници */}
        <Route path="/login" element={isLoggedIn ? <Navigate to="/" replace /> : <Login />} />
        <Route path="/register" element={isLoggedIn ? <Navigate to="/" replace /> : <Register />} />

        {/* Защитени страници - ПЪРВО ниво (Трябва да си логнат) */}
        <Route element={<RequireAuth />}>
          
          {/* Стъпката за създаване на агенция (изисква само логин) */}
          <Route path="/create-agency" element={<CreateAgency />} />

          {/* Защитени страници - ВТОРО ниво (Трябва да имаш Агенция) */}
          <Route element={<RequireAgency />}>
            <Route path="/" element={<Layout />}>
              <Route index element={<Home />} />
              <Route path="inbox" element={<Inbox />} />
              <Route path="agency" element={<Agency />} />
              <Route path="players" element={<Players />} />
              <Route path="finance" element={<Finance />} />
              <Route path="market" element={<Market />} />
              
              <Route path="world" element={<WorldLayout />}>
                <Route index element={<Navigate to="standings" replace />} />
                <Route path="standings" element={<Standings />} />
                <Route path="scorers" element={<Scorers />} />
                <Route path="awards" element={<Awards />} />
              </Route>
            </Route>
          </Route>
          
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;