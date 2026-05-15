import Link from "next/link";

export default function ImpressumPage() {
  return (
    <main className="framework-grid min-h-screen px-6 py-20 text-white">
      <section className="mx-auto max-w-2xl space-y-8 pb-24">
        <Link href="/" className="text-sm text-white/55 transition-colors hover:text-white">
          Zurueck
        </Link>

        <div className="space-y-5">
          <h1 className="text-4xl font-semibold tracking-normal">Impressum</h1>
          <p className="text-white/65">
            Angaben gemaess § 5 DDG
          </p>
        </div>

        <div className="space-y-6 border border-dashed border-white/20 bg-black/70 p-6 backdrop-blur-sm">
          <div>
            <h2 className="mb-2 text-lg font-semibold">Verantwortlich</h2>
            <p className="text-white/75">
              Fabian Kuhne
              <br />
              Alaunstraße 83
              <br />
              01099 Dresden
            </p>
          </div>

          <div>
            <h2 className="mb-2 text-lg font-semibold">Kontakt</h2>
            <p className="text-white/75">
              E-Mail: fabiankuhne@outlook.de
            </p>
          </div>

          <p className="text-sm text-white/55">
            Hinweis: Diese Seite ist als Vorlage angelegt. Bitte die Platzhalter
            vor dem Livegang durch korrekte Angaben ersetzen.
          </p>
        </div>
      </section>
    </main>
  );
}
