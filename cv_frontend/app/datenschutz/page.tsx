import Link from "next/link";

export default function DatenschutzPage() {
  return (
    <main className="framework-grid min-h-screen px-6 py-20 text-white">
      <section className="mx-auto max-w-3xl space-y-8 pb-24">
        <Link href="/" className="text-sm text-white/55 transition-colors hover:text-white">
          Zurueck
        </Link>

        <div className="space-y-5">
          <h1 className="text-4xl font-semibold tracking-normal">Datenschutzerklaerung</h1>
          <p className="text-white/65">
            Kurze Datenschutzhinweise fuer kathercv.de
          </p>
        </div>

        <div className="space-y-7 border border-dashed border-white/20 bg-black/70 p-6 leading-7 text-white/75 backdrop-blur-sm">
          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Verantwortlicher</h2>
            <p>
              Fabian Kuhne
              <br />
              Alaunstraße 83, 01099 Dresden
              <br />
              E-Mail: fabiankuhne@outlook.de
            </p>
          </section>

          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Zweck der Website</h2>
            <p>
              Diese Website stellt einen passwortgeschuetzten Lebenslauf bereit.
              Der Zugriff erfolgt nur fuer ausgewaehlte Empfaengerinnen und
              Empfaenger, denen das Passwort mitgeteilt wurde.
            </p>
          </section>

          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Hosting und Serverlogs</h2>
            <p>
              Die Website und API werden bei Railway gehostet. Beim Aufruf der
              Seite koennen technisch notwendige Daten wie IP-Adresse, Zeitpunkt
              des Zugriffs, angeforderte URL und User-Agent in Serverlogs
              verarbeitet werden. Diese Verarbeitung dient dem sicheren Betrieb
              der Website.
            </p>
          </section>

          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Cookies</h2>
            <p>
              Die Website verwendet technisch notwendige Cookies fuer Login,
              Session-Verwaltung und CSRF-Schutz. Diese Cookies sind erforderlich,
              um den geschuetzten Bereich sicher bereitzustellen. Es werden keine
              Marketing- oder Tracking-Cookies eingesetzt.
            </p>
          </section>

          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Schriftarten und externe Dienste</h2>
            <p>
              Die verwendeten Schriftarten werden durch die Anwendung selbst
              ausgeliefert. Beim Besuch der Website werden nach aktuellem Stand
              keine Google-Fonts-Anfragen vom Browser an Google gesendet.
            </p>
          </section>

          <section>
            <h2 className="mb-2 text-lg font-semibold text-white">Rechte betroffener Personen</h2>
            <p>
              Betroffene Personen haben im Rahmen der gesetzlichen Voraussetzungen
              Rechte auf Auskunft, Berichtigung, Loeschung, Einschraenkung der
              Verarbeitung, Datenuebertragbarkeit und Widerspruch. Zudem besteht
              ein Beschwerderecht bei einer Datenschutzaufsichtsbehoerde.
            </p>
          </section>

          <p className="text-sm text-white/55">
            Hinweis: Diese Datenschutzerklaerung ist eine kompakte Vorlage und
            ersetzt keine Rechtsberatung. Bitte die Platzhalter vor dem Livegang
            ergaenzen.
          </p>
        </div>
      </section>
    </main>
  );
}
