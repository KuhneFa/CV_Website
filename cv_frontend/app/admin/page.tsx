"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Card } from "@/components/Card";
import { Button } from "@/components/Button";
import { api } from "@/lib/api";

export default function AdminPage() {
  const router = useRouter();
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [uploadLoading, setUploadLoading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadMessage, setUploadMessage] = useState<string | null>(null);

  useEffect(() => {
    loadPdf();
  }, []);

  const loadPdf = async () => {
    try {
      const blob = await api.pdf.download();
      const url = URL.createObjectURL(blob);
      setPdfUrl(url);
    } catch (error) {
      console.log("PDF noch nicht vorhanden");
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.name.toLowerCase().endsWith(".pdf")) {
      setUploadError("Nur PDF-Dateien erlaubt");
      setUploadMessage(null);
      return;
    }

    setUploadLoading(true);
    setUploadError(null);
    setUploadMessage(null);

    try {
      const result = await api.pdf.upload(file);
      if (result.success) {
        await loadPdf();
        setUploadError(null);
        setUploadMessage("PDF hochgeladen");
      } else {
        setUploadError(result.message || "Upload fehlgeschlagen");
      }
    } catch (error) {
      setUploadError("Fehler beim Hochladen");
    } finally {
      setUploadLoading(false);
      e.target.value = "";
    }
  };

  const handleDelete = async () => {
    if (!confirm("PDF wirklich löschen?")) return;

    setUploadLoading(true);
    try {
      const result = await api.pdf.delete();
      if (result.success) {
        setPdfUrl(null);
        setUploadMessage("PDF gelöscht");
      } else {
        setUploadError(result.message || "Löschen fehlgeschlagen");
      }
    } catch (error) {
      setUploadError("Fehler beim Löschen");
    } finally {
      setUploadLoading(false);
    }
  };

  const handleLogout = async () => {
    await api.auth.logout();
    setPdfUrl(null);
    router.push("/");
  };

  return (
    <div className="framework-grid min-h-screen px-4 py-8 text-white">
      <div className="flex items-center justify-end mb-12">
        <Button variant="secondary" onClick={handleLogout}>
          Abmelden
        </Button>
      </div>

      <div className="mx-auto max-w-6xl space-y-10">
        <section className="flex min-h-[38vh] flex-col items-center justify-center gap-4">
          <input
            type="file"
            accept=".pdf"
            onChange={handleFileUpload}
            disabled={uploadLoading}
            className="hidden"
            id="pdf-upload"
          />
          <label
            htmlFor="pdf-upload"
            className="cta-field flex w-full max-w-[255px] cursor-pointer items-center justify-center px-6 text-center text-[30px] font-semibold"
          >
            {uploadLoading ? "Uploading..." : "Upload PDF"}
          </label>

          <div className="min-h-6 text-center text-sm text-white/70">
            {uploadError && <p className="text-red-400">{uploadError}</p>}
            {uploadMessage && !uploadError && (
              <p className="text-white">{uploadMessage}</p>
            )}
          </div>

          {pdfUrl && (
            <div className="flex justify-center gap-3">
              <Button variant="danger" onClick={handleDelete} disabled={uploadLoading}>
                PDF Löschen
              </Button>
              <Button variant="secondary" onClick={loadPdf}>
                PDF Neu Laden
              </Button>
            </div>
          )}
        </section>

        {pdfUrl && (
          <section className="flex justify-center pb-12">
            <Card className="pdf-shell p-2">
              <div className="overflow-hidden bg-black">
                <iframe
                  src={pdfUrl}
                  className="pdf-frame"
                  title="PDF Viewer"
                />
              </div>
            </Card>
          </section>
        )}

      </div>
    </div>
  );
}
