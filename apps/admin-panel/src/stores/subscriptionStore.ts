import { create } from 'zustand';

interface SubscriptionState {
  isBlocked: boolean;
  message: string;
  setBlocked: (message: string) => void;
  clearBlocked: () => void;
}

export const useSubscriptionStore = create<SubscriptionState>((set) => ({
  isBlocked: false,
  message: '',
  setBlocked: (message) => set({ isBlocked: true, message }),
  clearBlocked: () => set({ isBlocked: false, message: '' }),
}));
