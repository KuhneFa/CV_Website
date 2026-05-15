"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

export default function Home() {
  const router = useRouter();
  const [password, setPassword] = useState("");
  const [website, setWebsite] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!password) return;

    setLoading(true);
    setError(null);

    try {
      const result = await api.auth.loginAuto(password, website);
      
      if (result.success && result.role === "admin") {
        router.push("/admin");
        return;
      }

      if (result.success && result.role === "user") {
        router.push("/viewer");
        return;
      }

      setError("Passwort ungültig");
    } catch (error) {
      setError("Verbindungsfehler");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="framework-grid min-h-screen flex items-center justify-center px-4">
      <form onSubmit={handleLogin} className="w-full max-w-[255px]">
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Password"
          disabled={loading}
          autoFocus
          aria-label="Password"
          className="cta-field w-full px-6 text-center text-[30px] font-semibold outline-none"
        />
        <input
          type="text"
          name="website"
          value={website}
          onChange={(e) => setWebsite(e.target.value)}
          tabIndex={-1}
          autoComplete="off"
          className="hidden"
          aria-hidden="true"
        />
        
        {error && (
          <div className="mt-6 text-center text-sm text-red-400">
            {error}
          </div>
        )}

        <button
          type="submit"
          disabled={!password || loading}
          className="sr-only"
        >
          Enter
        </button>
      </form>
    </div>
  );
}
