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

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Публични страници (Без Navbar) */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/create-agency" element={<CreateAgency />} />

        {/* Защитени страници (С Navbar на играта) */}
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="inbox" element={<Inbox />} />
          <Route path="agency" element={<Agency />} />
          <Route path="players" element={<Players />} />
          <Route path="finance" element={<Finance />} />
          <Route path="market" element={<Market />} />
          {/* World Routes с вложени (Nested) страници */}
          <Route path="world" element={<WorldLayout />}>
            <Route index element={<Navigate to="standings" replace />} />
            <Route path="standings" element={<Standings />} />
            <Route path="scorers" element={<Scorers />} />
            <Route path="awards" element={<Awards />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;