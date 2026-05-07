"use client";

interface ButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  variant?: "primary" | "secondary" | "danger";
  loading?: boolean;
  type?: "button" | "submit" | "reset";
  fullWidth?: boolean;
}

export function Button({
  children,
  onClick,
  disabled = false,
  variant = "primary",
  loading = false,
  type = "button",
  fullWidth = false,
}: ButtonProps) {
  const baseStyle = "px-6 py-3 rounded-[14px] border font-medium transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed";
  
  const variants = {
    primary: "bg-white border-white text-black hover:bg-[#e5e5e5]",
    secondary: "bg-black border-white text-white hover:bg-white hover:text-black",
    danger: "bg-black border-red-500 text-red-400 hover:bg-red-500 hover:text-white",
  };

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled || loading}
      className={`${baseStyle} ${variants[variant]} ${fullWidth ? "w-full" : ""}`}
    >
      {loading ? (
        <div className="flex items-center justify-center gap-2">
          <div className="w-4 h-4 border-2 border-transparent border-t-current rounded-full animate-spin" />
          Wird geladen...
        </div>
      ) : (
        children
      )}
    </button>
  );
}
