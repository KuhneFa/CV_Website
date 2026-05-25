"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Card } from "@/components/Card";
import { Button } from "@/components/Button";
import { PdfPreview } from "@/components/PdfPreview";
import { api } from "@/lib/api";

export default function ViewerPage() {
  const router = useRouter();
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPdf();
  }, []);

  const loadPdf = async () => {
    setLoading(true);
    setError(null);
    setPdfUrl(api.pdf.viewUrl());
    setLoading(false);
  };

  const handleLogout = async () => {
    await api.auth.logout();
    setPdfUrl(null);
    router.push("/");
  };

  if (loading) {
    return (
      <div className="framework-grid min-h-screen flex flex-col items-center justify-center text-white">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-white/20 border-t-white rounded-full animate-spin mx-auto mb-4" />
          <p className="text-white">PDF wird geladen...</p>
        </div>
      </div>
    );
  }

  if (error || !pdfUrl) {
    return (
      <div className="framework-grid min-h-screen flex flex-col items-center justify-center px-4 py-12 text-white">
        <Card className="w-full max-w-md text-center">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Fehler
          </h2>
          <p className="text-white mb-8">
            {error || "PDF konnte nicht geladen werden"}
          </p>
          <div className="space-y-3">
            <Button variant="secondary" onClick={loadPdf} fullWidth>
              Erneut Versuchen
            </Button>
            <Button variant="secondary" onClick={handleLogout} fullWidth>
              Zurück zur Startseite
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="framework-grid min-h-screen px-4 py-8 text-white">
      <div className="mx-auto flex max-w-6xl items-center justify-end">
        <Button variant="secondary" onClick={handleLogout}>
          Abmelden
        </Button>
      </div>

      <main className="mx-auto flex min-h-[calc(100vh-112px)] w-full items-center justify-center py-8">
        <Card className="pdf-shell p-2">
          <PdfPreview pdfUrl={pdfUrl} downloadUrl={api.pdf.downloadUrl()} />
        </Card>
      </main>
    </div>
  );
}
