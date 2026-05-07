"use client";

interface PasswordInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  onKeyDown?: (e: React.KeyboardEvent) => void;
}

export function PasswordInput({
  value,
  onChange,
  placeholder = "Passwort eingeben",
  disabled = false,
  onKeyDown,
}: PasswordInputProps) {
  return (
    <input
      type="password"
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      disabled={disabled}
      onKeyDown={onKeyDown}
      className="cta-field w-full px-6 text-center text-[30px] font-semibold outline-none disabled:opacity-50 disabled:cursor-not-allowed"
    />
  );
}
