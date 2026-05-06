// src/store/useAgencyStore.ts
import { create } from 'zustand';

interface AgencyState {
  budget: number | null;
  // Функция за първоначално задаване на бюджета
  setBudget: (amount: number) => void;
  // Функция за добавяне/вадене на пари (напр. -50000 за ъпгрейд)
  updateBudget: (amount: number) => void;
}

export const useAgencyStore = create<AgencyState>((set) => ({
  budget: null,
  
  setBudget: (amount) => set({ budget: amount }),
  
  updateBudget: (amount) => set((state) => ({ 
    budget: state.budget !== null ? state.budget + amount : null 
  })),
}));