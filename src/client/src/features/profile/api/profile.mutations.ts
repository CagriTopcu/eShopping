import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/api/client.ts";
import { profileKeys } from "./profile.queries.ts";
import { toast } from "@/shared/components/feedback/GlobalToast.tsx";
import type {
  ProfileResponse,
  AddressResponse,
  CreateProfileRequest,
  UpdateProfileRequest,
  AddAddressRequest,
} from "@/shared/types/common.types.ts";

export function useCreateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProfileRequest) =>
      api.post<ProfileResponse>("/api/v1/profile", data),
    onSuccess: (data) => {
      queryClient.setQueryData(profileKeys.me(), data);
      toast("Profil olusturuldu", "success");
    },
    onError: () => {
      toast("Profil olusturulurken hata olustu", "error");
    },
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateProfileRequest) =>
      api.put<ProfileResponse>("/api/v1/profile/me", data),
    onSuccess: (data) => {
      queryClient.setQueryData(profileKeys.me(), data);
      toast("Profil guncellendi", "success");
    },
    onError: () => {
      toast("Profil guncellenirken hata olustu", "error");
    },
  });
}

export function useAddAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddAddressRequest) =>
      api.post<AddressResponse>("/api/v1/profile/me/addresses", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: profileKeys.me() });
      toast("Adres eklendi", "success");
    },
    onError: () => {
      toast("Adres eklenirken hata olustu", "error");
    },
  });
}

export function useRemoveAddress() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (addressId: string) =>
      api.delete<void>(`/api/v1/profile/me/addresses/${addressId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: profileKeys.me() });
      toast("Adres silindi", "success");
    },
    onError: () => {
      toast("Adres silinirken hata olustu", "error");
    },
  });
}
