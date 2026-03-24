import { Button } from "../ui/Button.tsx";
import { Link } from "@tanstack/react-router";

export function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh] gap-4 p-8 animate-fade-in">
      <div className="text-8xl font-bold bg-gradient-to-b from-muted-foreground to-muted bg-clip-text text-transparent">
        404
      </div>
      <h2 className="text-xl font-semibold">Sayfa Bulunamadı</h2>
      <p className="text-muted-foreground text-center max-w-md">
        Aradığınız sayfa mevcut değil veya taşınmış olabilir.
      </p>
      <Button asChild className="mt-2">
        <Link to="/">Ana Sayfaya Dön</Link>
      </Button>
    </div>
  );
}
