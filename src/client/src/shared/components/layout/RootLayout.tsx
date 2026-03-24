import type { ReactNode } from "react";
import { Header } from "./Header.tsx";
import { Footer } from "./Footer.tsx";

interface RootLayoutProps {
  children: ReactNode;
}

export function RootLayout({ children }: RootLayoutProps) {
  return (
    <>
      <Header />
      <main className="flex-1 mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 py-6">
        {children}
      </main>
      <Footer />
    </>
  );
}
