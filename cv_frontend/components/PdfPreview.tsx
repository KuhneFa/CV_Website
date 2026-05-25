"use client";

import { useEffect, useState } from "react";

interface PdfPreviewProps {
  pdfUrl: string;
  downloadUrl?: string;
  title?: string;
}

export function PdfPreview({ pdfUrl, downloadUrl, title = "PDF Viewer" }: PdfPreviewProps) {
  const [useExternalViewer, setUseExternalViewer] = useState(false);

  useEffect(() => {
    const userAgent = window.navigator.userAgent;
    const isIOS = /iPad|iPhone|iPod/.test(userAgent);
    const isSmallScreen = window.matchMedia("(max-width: 768px)").matches;
    setUseExternalViewer(isIOS || isSmallScreen);
  }, []);

  if (useExternalViewer) {
    return (
      <div className="mobile-pdf-panel">
        <a
          href={pdfUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="cta-field flex w-full max-w-[255px] items-center justify-center px-6 text-center text-[28px] font-semibold"
        >
          PDF öffnen
        </a>
        <a
          href={downloadUrl || pdfUrl}
          download="Lebenslauf.pdf"
          className="text-sm text-white/70 underline-offset-4 hover:text-white hover:underline"
        >
          PDF herunterladen
        </a>
      </div>
    );
  }

  return (
    <div className="overflow-hidden bg-black">
      <iframe
        src={pdfUrl}
        className="pdf-frame"
        title={title}
      />
    </div>
  );
}
