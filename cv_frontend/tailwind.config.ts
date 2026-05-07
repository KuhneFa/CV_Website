import type { Config } from "tailwindcss";

const config: Config = {
  theme: {
    extend: {
      colors: {
        // Modernes Next.js Style: Schwarz/Dunkelblau + Silber/Weiß
        dark: {
          bg: "#0a0e27", // Sehr dunkles Blau-Schwarz
          card: "#151b33", // Dunkelblau für Cards
          border: "#2a3550", // Grauerer Dunkelblau für Borders
        },
        accent: {
          silver: "#e5e7eb", // Silber/Hellgrau
          white: "#ffffff",
          muted: "#9ca3af", // Gedimmtes Grau
        },
      },
      backgroundColor: {
        primary: "#0a0e27",
        secondary: "#151b33",
      },
    },
  },
  plugins: [],
};

export default config;
