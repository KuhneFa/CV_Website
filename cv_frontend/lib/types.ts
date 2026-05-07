export interface LoginResponse {
  success: boolean;
  message: string;
}

export interface PdfResponse {
  pdfContent?: string; // Base64 encoded PDF
}

export interface UploadResponse {
  success: boolean;
  message: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  isAdmin: boolean;
  loading: boolean;
  error: string | null;
}
