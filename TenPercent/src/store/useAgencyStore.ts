// src/store/useAgencyStore.ts
import { create } from 'zustand';

interface AgencyState {
  budget: number | null;
  unreadMessages: number; // НОВО
  
  setBudget: (amount: number) => void;
  updateBudget: (amount: number) => void;
  
  // НОВО: Управление на съобщенията
  setUnreadMessages: (count: number) => void;
  decrementUnreadMessages: () => void;
}

export const useAgencyStore = create<AgencyState>((set) => ({
  budget: null,
  unreadMessages: 0,
  
  setBudget: (amount) => set({ budget: amount }),
  updateBudget: (amount) => set((state) => ({ 
    budget: state.budget !== null ? state.budget + amount : null 
  })),

  // НОВО: Методи за Inbox
  setUnreadMessages: (count) => set({ unreadMessages: count }),
  decrementUnreadMessages: () => set((state) => ({ 
    unreadMessages: Math.max(0, state.unreadMessages - 1) 
  })),
}));