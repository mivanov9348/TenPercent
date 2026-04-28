import { BrowserRouter, Routes, Route } from 'react-router-dom';
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
          <Route path="agency" element={<Agency />} />
          <Route path="players" element={<Players />} />
          <Route path="finance" element={<Finance />} />
          <Route path="market" element={<Market />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;