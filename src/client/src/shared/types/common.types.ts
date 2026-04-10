export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiError {
  code: string;
  description: string;
  type:
    | "Failure"
    | "Validation"
    | "NotFound"
    | "Conflict"
    | "Unauthorized"
    | "Forbidden";
  status: number;
}

export interface ProductResponse {
  id: string;
  name: string;
  category: string;
  price: number;
  currency: string;
  description?: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface StockResponse {
  productId: string;
  availableQuantity: number;
}

export interface BasketResponse {
  username: string;
  items: BasketItemResponse[];
  totalPrice: number;
}

export interface BasketItemResponse {
  productId: string;
  productName: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  lineTotal: number;
}

export interface OrderResponse {
  id: string;
  customerId: string;
  status: number;
  statusName: string;
  items: OrderItemResponse[];
  totalAmount: number;
  shippingStreet: string;
  shippingCity: string;
  shippingState: string;
  shippingCountry: string;
  shippingZipCode: string;
  placedAt: string;
}

export interface OrderItemResponse {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CreateProductRequest {
  name: string;
  category: string;
  price: number;
  currency: string;
  initialStock: number;
  description?: string;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  name: string;
  category: string;
  price: number;
  currency: string;
  description?: string;
  imageUrl?: string;
}

export interface UpsertBasketItemRequest {
  productId: string;
  quantity: number;
}

export interface PlaceOrderRequest {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  items: OrderItemRequest[];
}

export interface OrderItemRequest {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface CatalogStatsResponse {
  totalProducts: number;
  categories: string[];
}

export interface OrderStatsResponse {
  totalOrders: number;
  pendingOrders: number;
  confirmedOrders: number;
  cancelledOrders: number;
  totalRevenue: number;
}

export interface ProductFilters {
  category?: string;
  name?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}

// ── Profile ──────────────────────────────────────────────

export interface ProfileResponse {
  id: string;
  keycloakUserId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  addresses: AddressResponse[];
}

export interface AddressResponse {
  id: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  label: string;
  isDefault: boolean;
}

export interface CreateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface AddAddressRequest {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  label: string;
  isDefault?: boolean;
}

// ── Shipping ─────────────────────────────────────────────

export interface ShipmentResponse {
  id: string;
  orderId: string;
  customerId: string;
  orderTotal: number;
  status: number;
  trackingNumber?: string;
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
}
