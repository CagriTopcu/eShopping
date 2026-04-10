import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute.tsx";
import { useAuth } from "@/features/auth/hooks/useAuth.ts";
import { useProfile } from "../api/profile.queries.ts";
import {
  useCreateProfile,
  useUpdateProfile,
  useAddAddress,
  useRemoveAddress,
} from "../api/profile.mutations.ts";
import { Button } from "@/shared/components/ui/Button.tsx";
import { Input } from "@/shared/components/ui/Input.tsx";
import { Badge } from "@/shared/components/ui/Badge.tsx";
import { Skeleton } from "@/shared/components/ui/Skeleton.tsx";
import { Dialog } from "@/shared/components/ui/Dialog.tsx";

// ── Schemas ──────────────────────────────────────────────

const profileSchema = z.object({
  firstName: z.string().min(1, "Ad zorunludur"),
  lastName: z.string().min(1, "Soyad zorunludur"),
  email: z.string().email("Gecerli bir email giriniz"),
  phoneNumber: z.string().optional(),
});

const addressSchema = z.object({
  label: z.string().min(1, "Etiket zorunludur"),
  street: z.string().min(1, "Sokak zorunludur"),
  city: z.string().min(1, "Sehir zorunludur"),
  state: z.string().min(1, "Bolge zorunludur"),
  zipCode: z.string().min(1, "Posta kodu zorunludur"),
  country: z.string().min(1, "Ulke zorunludur"),
  isDefault: z.boolean().optional(),
});

type ProfileFormData = z.infer<typeof profileSchema>;
type AddressFormData = z.infer<typeof addressSchema>;

// ── Main ─────────────────────────────────────────────────

function ProfileContent() {
  const { username } = useAuth();
  const { data: profile, isLoading, isError } = useProfile();
  const createProfile = useCreateProfile();
  const updateProfile = useUpdateProfile();
  const addAddress = useAddAddress();
  const removeAddress = useRemoveAddress();

  const [showAddressForm, setShowAddressForm] = useState(false);
  const [deleteAddressId, setDeleteAddressId] = useState<string | null>(null);

  const hasProfile = !!profile && !isError;

  const profileForm = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema) as never,
    values: hasProfile
      ? {
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email,
          phoneNumber: profile.phoneNumber ?? "",
        }
      : { firstName: "", lastName: "", email: "", phoneNumber: "" },
  });

  const addressForm = useForm<AddressFormData>({
    resolver: zodResolver(addressSchema) as never,
    defaultValues: { country: "Turkiye", isDefault: false },
  });

  const onProfileSubmit = (data: ProfileFormData) => {
    if (hasProfile) {
      updateProfile.mutate({
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: data.phoneNumber || undefined,
      });
    } else {
      createProfile.mutate({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phoneNumber: data.phoneNumber || undefined,
      });
    }
  };

  const onAddressSubmit = (data: AddressFormData) => {
    addAddress.mutate(data, {
      onSuccess: () => {
        addressForm.reset();
        setShowAddressForm(false);
      },
    });
  };

  const handleDeleteAddress = () => {
    if (deleteAddressId) {
      removeAddress.mutate(deleteAddressId, {
        onSuccess: () => setDeleteAddressId(null),
      });
    }
  };

  if (isLoading) {
    return (
      <div className="max-w-3xl space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full rounded-xl" />
        <Skeleton className="h-48 w-full rounded-xl" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl animate-fade-in">
      <div className="mb-8">
        <h1 className="text-2xl font-bold">Profilim</h1>
        <p className="text-muted-foreground mt-1">
          Hesap bilgilerinizi ve adreslerinizi yonetin
        </p>
      </div>

      {/* Profile info card */}
      <form onSubmit={profileForm.handleSubmit(onProfileSubmit)} className="space-y-6">
        <div className="rounded-xl border border-border bg-card p-6 space-y-5">
          <div className="flex items-center gap-4 pb-4 border-b border-border">
            <div className="h-14 w-14 rounded-full bg-primary/10 flex items-center justify-center">
              <span className="text-xl font-bold text-primary">
                {hasProfile ? profile.firstName[0]?.toUpperCase() : username?.[0]?.toUpperCase() ?? "?"}
              </span>
            </div>
            <div>
              <p className="font-semibold">
                {hasProfile ? `${profile.firstName} ${profile.lastName}` : username}
              </p>
              <p className="text-sm text-muted-foreground">
                {hasProfile ? profile.email : "Profil henuz olusturulmadi"}
              </p>
            </div>
          </div>

          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium mb-1 block">Ad</label>
              <Input {...profileForm.register("firstName")} error={profileForm.formState.errors.firstName?.message} />
            </div>
            <div>
              <label className="text-sm font-medium mb-1 block">Soyad</label>
              <Input {...profileForm.register("lastName")} error={profileForm.formState.errors.lastName?.message} />
            </div>
          </div>

          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium mb-1 block">Email</label>
              <Input
                {...profileForm.register("email")}
                type="email"
                disabled={hasProfile}
                error={profileForm.formState.errors.email?.message}
              />
            </div>
            <div>
              <label className="text-sm font-medium mb-1 block">Telefon</label>
              <Input
                {...profileForm.register("phoneNumber")}
                placeholder="+90 5XX XXX XX XX"
                error={profileForm.formState.errors.phoneNumber?.message}
              />
            </div>
          </div>

          <div className="flex justify-end pt-2">
            <Button
              type="submit"
              disabled={createProfile.isPending || updateProfile.isPending}
            >
              {createProfile.isPending || updateProfile.isPending
                ? "Kaydediliyor..."
                : hasProfile
                  ? "Guncelle"
                  : "Profil Olustur"}
            </Button>
          </div>
        </div>
      </form>

      {/* Addresses */}
      {hasProfile && (
        <div className="mt-8 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold">Adreslerim</h2>
            <Button
              size="sm"
              variant={showAddressForm ? "ghost" : "default"}
              onClick={() => setShowAddressForm(!showAddressForm)}
            >
              {showAddressForm ? "Vazgec" : "Yeni Adres Ekle"}
            </Button>
          </div>

          {/* Add address form */}
          {showAddressForm && (
            <form
              onSubmit={addressForm.handleSubmit(onAddressSubmit)}
              className="rounded-xl border-2 border-dashed border-primary/30 bg-primary/5 p-6 space-y-4 animate-fade-in"
            >
              <div className="grid sm:grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium mb-1 block">Etiket</label>
                  <Input {...addressForm.register("label")} placeholder="Ev, Is, vb." error={addressForm.formState.errors.label?.message} />
                </div>
                <div>
                  <label className="text-sm font-medium mb-1 block">Ulke</label>
                  <Input {...addressForm.register("country")} error={addressForm.formState.errors.country?.message} />
                </div>
              </div>

              <div>
                <label className="text-sm font-medium mb-1 block">Sokak / Cadde</label>
                <Input {...addressForm.register("street")} placeholder="Ornek Mah. Ornek Sok. No: 1" error={addressForm.formState.errors.street?.message} />
              </div>

              <div className="grid sm:grid-cols-3 gap-4">
                <div>
                  <label className="text-sm font-medium mb-1 block">Sehir</label>
                  <Input {...addressForm.register("city")} error={addressForm.formState.errors.city?.message} />
                </div>
                <div>
                  <label className="text-sm font-medium mb-1 block">Bolge / Ilce</label>
                  <Input {...addressForm.register("state")} error={addressForm.formState.errors.state?.message} />
                </div>
                <div>
                  <label className="text-sm font-medium mb-1 block">Posta Kodu</label>
                  <Input {...addressForm.register("zipCode")} error={addressForm.formState.errors.zipCode?.message} />
                </div>
              </div>

              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" {...addressForm.register("isDefault")} className="rounded border-border" />
                Varsayilan adres olarak ayarla
              </label>

              <div className="flex justify-end">
                <Button type="submit" disabled={addAddress.isPending}>
                  {addAddress.isPending ? "Ekleniyor..." : "Adres Ekle"}
                </Button>
              </div>
            </form>
          )}

          {/* Address list */}
          {profile.addresses.length === 0 && !showAddressForm ? (
            <div className="rounded-xl border border-dashed border-border p-8 text-center">
              <p className="text-muted-foreground">Henuz adres eklememissiniz</p>
              <Button
                variant="outline"
                size="sm"
                className="mt-3"
                onClick={() => setShowAddressForm(true)}
              >
                Ilk Adresinizi Ekleyin
              </Button>
            </div>
          ) : (
            <div className="grid gap-3">
              {profile.addresses.map((address, i) => (
                <div
                  key={address.id}
                  className="group rounded-xl border border-border bg-card p-5 hover:shadow-sm transition-all animate-slide-up"
                  style={{ animationDelay: `${i * 50}ms`, animationFillMode: "both" }}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{address.label}</span>
                      {address.isDefault && (
                        <Badge variant="success" className="text-[10px]">Varsayilan</Badge>
                      )}
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-destructive opacity-0 group-hover:opacity-100 transition-opacity"
                      onClick={() => setDeleteAddressId(address.id)}
                    >
                      Sil
                    </Button>
                  </div>
                  <address className="text-sm text-muted-foreground not-italic leading-relaxed mt-2">
                    {address.street}<br />
                    {address.city}, {address.state} {address.zipCode}<br />
                    {address.country}
                  </address>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Delete address dialog */}
      <Dialog
        open={!!deleteAddressId}
        onOpenChange={(open) => !open && setDeleteAddressId(null)}
        title="Adresi Sil"
        description="Bu adresi silmek istediginizden emin misiniz?"
      >
        <div className="flex justify-end gap-3 mt-4">
          <Button variant="outline" onClick={() => setDeleteAddressId(null)}>Vazgec</Button>
          <Button
            variant="destructive"
            onClick={handleDeleteAddress}
            disabled={removeAddress.isPending}
          >
            {removeAddress.isPending ? "Siliniyor..." : "Sil"}
          </Button>
        </div>
      </Dialog>
    </div>
  );
}

export default function ProfilePage() {
  return (
    <ProtectedRoute>
      <ProfileContent />
    </ProtectedRoute>
  );
}
