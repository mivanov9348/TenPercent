import { Outlet } from 'react-router-dom';
import Navbar from './Navbar'; // Вече импортваме Navbar вместо Sidebar

export default function Layout() {
  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gray-900">
      {/* Горно меню */}
      <Navbar />
      
      {/* Основна част за съдържанието на страниците */}
      <main className="flex-1 overflow-y-auto p-8">
        <div className="max-w-7xl mx-auto">
          <Outlet /> {/* Тук React Router ще рендира съответната страница */}
        </div>
      </main>
    </div>
  );
}