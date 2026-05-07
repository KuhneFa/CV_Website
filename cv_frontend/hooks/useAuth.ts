"use client";

import { useState } from "react";
import { AuthState } from "@/lib/types";
import { api } from "@/lib/api";

export function useAuth() {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    isAdmin: false,
    loading: false,
    error: null,
  });

  const login = async (password: string, website = "") => {
    setState((prev) => ({ ...prev, loading: true, error: null }));
    try {
      const result = await api.auth.login(password, website);
      if (result.success) {
        setState((prev) => ({
          ...prev,
          isAuthenticated: true,
          loading: false,
        }));
        return true;
      } else {
        setState((prev) => ({
          ...prev,
          error: result.message || "Login fehlgeschlagen",
          loading: false,
        }));
        return false;
      }
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error: "Verbindungsfehler",
        loading: false,
      }));
      return false;
    }
  };

  const adminLogin = async (password: string) => {
    setState((prev) => ({ ...prev, loading: true, error: null }));
    try {
      const result = await api.auth.adminLogin(password);
      if (result.success) {
        setState((prev) => ({
          ...prev,
          isAuthenticated: true,
          isAdmin: true,
          loading: false,
        }));
        return true;
      } else {
        setState((prev) => ({
          ...prev,
          error: result.message || "Admin-Login fehlgeschlagen",
          loading: false,
        }));
        return false;
      }
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error: "Verbindungsfehler",
        loading: false,
      }));
      return false;
    }
  };

  const logout = () => {
    setState({
      isAuthenticated: false,
      isAdmin: false,
      loading: false,
      error: null,
    });
  };

  return { ...state, login, adminLogin, logout };
}
