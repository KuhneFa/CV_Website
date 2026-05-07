"use client";

interface CardProps {
  children: React.ReactNode;
  className?: string;
}

export function Card({ children, className = "" }: CardProps) {
  return (
    <div className={`bg-black/75 border border-dashed border-white/20 p-6 backdrop-blur-sm ${className}`}>
      {children}
    </div>
  );
}
