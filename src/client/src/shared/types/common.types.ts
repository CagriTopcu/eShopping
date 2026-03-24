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
  stock: number;
  description?: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt?: string;
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
  stock: number;
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

export interface ProductFilters {
  category?: string;
  name?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}
