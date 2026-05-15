import Link from "next/link";

export function Footer() {
  return (
    <footer className="fixed inset-x-0 bottom-0 z-20 flex justify-center px-4 pb-5 text-sm text-white/55">
      <nav className="flex items-center gap-4 rounded-full border border-white/10 bg-black/70 px-5 py-2 backdrop-blur-sm">
        <Link href="/impressum" className="transition-colors hover:text-white">
          Impressum
        </Link>
        <span aria-hidden="true" className="text-white/25">
          /
        </span>
        <Link href="/datenschutz" className="transition-colors hover:text-white">
          Datenschutz
        </Link>
      </nav>
    </footer>
  );
}
