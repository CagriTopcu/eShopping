import { Component, type ErrorInfo, type ReactNode } from "react";
import { Button } from "../ui/Button.tsx";

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("ErrorBoundary caught:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback;

      return (
        <div className="flex flex-col items-center justify-center min-h-[50vh] gap-4 p-8">
          <h1 className="text-2xl font-semibold">Bir şeyler ters gitti</h1>
          <p className="text-muted-foreground text-center max-w-md">
            Beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyin veya daha sonra
            tekrar deneyin.
          </p>
          <Button onClick={() => window.location.reload()}>
            Sayfayı Yenile
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}
