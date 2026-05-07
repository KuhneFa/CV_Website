"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/Button";
import { PasswordInput } from "@/components/PasswordInput";
import { useAuth } from "@/hooks/useAuth";

export default function LoginPage() {
  const router = useRouter();
  const { login, loading, error } = useAuth();
  const [password, setPassword] = useState("");
  const [website, setWebsite] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!password) {
      return;
    }

    const success = await login(password, website);
    if (success) {
      router.push("/viewer");
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && password && !loading) {
      handleSubmit(e as any);
    }
  };

  return (
    <div className="framework-grid min-h-screen flex flex-col items-center justify-center px-4 py-12 text-white">
      <div className="w-full max-w-[255px]">
        <form onSubmit={handleSubmit} className="space-y-6">
          <PasswordInput
            value={password}
            onChange={setPassword}
            placeholder="Password"
            disabled={loading}
            onKeyDown={handleKeyPress}
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
            <div className="text-center text-red-400 text-sm">
              {error}
            </div>
          )}

          <Button
            type="submit"
            variant="primary"
            fullWidth
            loading={loading}
            disabled={!password || loading}
          >
            Anmelden
          </Button>
        </form>
      </div>
    </div>
  );
}
