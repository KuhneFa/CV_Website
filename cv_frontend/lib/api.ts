const API_BASE = process.env.NEXT_PUBLIC_API_BASE || "http://localhost:8080/api";

let csrfToken: string | null = null;

async function getCsrfToken(): Promise<string> {
  if (csrfToken) {
    return csrfToken;
  }

  const response = await fetch(`${API_BASE}/auth/csrf`, {
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("CSRF token could not be loaded");
  }

  const data = await response.json();
  const token = String(data.token);
  csrfToken = token;
  return token;
}

async function csrfHeaders(contentType?: string): Promise<Record<string, string>> {
  const token = await getCsrfToken();
  return {
    ...(contentType ? { "Content-Type": contentType } : {}),
    "X-CSRF-TOKEN": token,
  };
}

export const api = {
  auth: {
    loginAuto: async (password: string, website = "") => {
      const response = await fetch(`${API_BASE}/auth/login-auto`, {
        method: "POST",
        headers: await csrfHeaders("application/json"),
        credentials: "include",
        body: JSON.stringify({ password, website }),
      });
      return response.json();
    },

    login: async (password: string, website = "") => {
      const response = await fetch(`${API_BASE}/auth/login`, {
        method: "POST",
        headers: await csrfHeaders("application/json"),
        credentials: "include",
        body: JSON.stringify({ password, website }),
      });
      return response.json();
    },

    adminLogin: async (password: string, website = "") => {
      const response = await fetch(`${API_BASE}/auth/admin-login`, {
        method: "POST",
        headers: await csrfHeaders("application/json"),
        credentials: "include",
        body: JSON.stringify({ password, website }),
      });
      return response.json();
    },

    logout: async () => {
      const response = await fetch(`${API_BASE}/auth/logout`, {
        method: "POST",
        headers: await csrfHeaders(),
        credentials: "include",
      });
      return response.json();
    },
  },

  pdf: {
    downloadUrl: () => `${API_BASE}/pdf/download?v=${Date.now()}`,

    download: async () => {
      const response = await fetch(`${API_BASE}/pdf/download`, {
        credentials: "include",
      });
      if (!response.ok) throw new Error("PDF konnte nicht geladen werden");
      return response.blob();
    },

    upload: async (file: File) => {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(`${API_BASE}/pdf/upload`, {
        method: "POST",
        headers: await csrfHeaders(),
        credentials: "include",
        body: formData,
      });
      return response.json();
    },

    delete: async () => {
      const response = await fetch(`${API_BASE}/pdf/delete`, {
        method: "DELETE",
        headers: await csrfHeaders(),
        credentials: "include",
      });
      return response.json();
    },
  },
};
