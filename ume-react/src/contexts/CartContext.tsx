import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import type { CartItem } from '../models/models';

interface CartContextType {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
  addToCart: (product: any, quantity?: number) => void;
  addPetToCart: (pet: any) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  removeItem: (productId: string) => void;
  clearCart: () => void;
}

const CartContext = createContext<CartContextType | null>(null);

function loadCart(): CartItem[] {
  const saved = localStorage.getItem('cart');
  return saved ? JSON.parse(saved) : [];
}

function saveCart(items: CartItem[]) {
  localStorage.setItem('cart', JSON.stringify(items));
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(loadCart);

  const totalItems = items.reduce((sum, i) => sum + i.quantity, 0);
  const totalAmount = items.reduce((sum, i) => {
    const price = i.itemType === 'pet' ? (i.product.listingPrice || 0) : (i.product.price || 0);
    return sum + price * i.quantity;
  }, 0);

  const addToCart = useCallback((product: any, quantity = 1) => {
    setItems(prev => {
      const existing = prev.find(i => i.product._id === product._id);
      let next: CartItem[];
      if (existing) {
        next = prev.map(i => i.product._id === product._id ? { ...i, quantity: i.quantity + quantity } : i);
      } else {
        next = [...prev, { product, quantity }];
      }
      saveCart(next);
      return next;
    });
  }, []);

  const addPetToCart = useCallback((pet: any) => {
    setItems(prev => {
      if (prev.find(i => i.product._id === pet._id && i.itemType === 'pet')) return prev;
      const next = [...prev, { product: pet, quantity: 1, itemType: 'pet' as const }];
      saveCart(next);
      return next;
    });
  }, []);

  const updateQuantity = useCallback((productId: string, quantity: number) => {
    setItems(prev => {
      let next: CartItem[];
      if (quantity <= 0) {
        next = prev.filter(i => i.product._id !== productId);
      } else {
        next = prev.map(i => i.product._id === productId ? { ...i, quantity } : i);
      }
      saveCart(next);
      return next;
    });
  }, []);

  const removeItem = useCallback((productId: string) => {
    setItems(prev => {
      const next = prev.filter(i => i.product._id !== productId);
      saveCart(next);
      return next;
    });
  }, []);

  const clearCart = useCallback(() => {
    setItems([]);
    saveCart([]);
  }, []);

  return (
    <CartContext.Provider value={{ items, totalItems, totalAmount, addToCart, addPetToCart, updateQuantity, removeItem, clearCart }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be inside CartProvider');
  return ctx;
}
