import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/layout/Layout';

// Страници на играта
import Home from './pages/Home';
import Agency from './pages/Agency';
import Players from './pages/Players';
import Finance from './pages/Finance';

// Auth Страници
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import CreateAgency from './pages/auth/CreateAgency';

import WorldLayout from './pages/world/WorldLayout';
import Standings from './pages/world/Standings';
import Fixtures from './pages/world/Fixtures';
import SeasonStats from './pages/world/SeasonStats';
import Inbox from './pages/Inbox';
import PlayerDetails from './pages/world/PlayerDetails';

import RequireAuth from './components/layout/RequireAuth';
import RequireAgency from './components/layout/RequireAgency';

import RequireAdmin from './pages/admin/RequireAdmin';
import AdminDashboard from './pages/admin/AdminDashboard';
import ClubDetails from './pages/world/ClubDetails';
import ScoutingPool from './pages/ScoutingPool';

function App() {

  return (
    <BrowserRouter>
      <Routes>
        {/* Публични страници */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route path="/admin" element={
          <RequireAdmin>
            <AdminDashboard />
          </RequireAdmin>
        } />

        {/* Защитени страници - ПЪРВО ниво */}
        <Route element={<RequireAuth />}>
          <Route path="/create-agency" element={<CreateAgency />} />

          {/* Защитени страници - ВТОРО ниво */}
          <Route element={<RequireAgency />}>
            <Route path="/" element={<Layout />}>
              <Route index element={<Home />} />
              <Route path="inbox" element={<Inbox />} />
              <Route path="agency" element={<Agency />} />
              <Route path="players" element={<Players />} />
              <Route path="finance" element={<Finance />} />
              <Route path="scouting-pool" element={<ScoutingPool />} />

              <Route path="world">
                <Route element={<WorldLayout />}>
                  <Route index element={<Navigate to="standings" replace />} />
                  <Route path="standings" element={<Standings />} />
                  <Route path="fixtures" element={<Fixtures />} />
                  <Route path="stats" element={<SeasonStats />} /> {/* ПРОМЕНЕНО */}
                </Route>

                <Route path="club/:id" element={<ClubDetails />} />
                <Route path="player/:id" element={<PlayerDetails />} />
              </Route>
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;