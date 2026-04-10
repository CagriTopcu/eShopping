import {
  createRouter,
  createRootRoute,
  createRoute,
  Outlet,
  lazyRouteComponent,
} from "@tanstack/react-router";
import { RootLayout } from "@/shared/components/layout/RootLayout.tsx";
import { NotFound } from "@/shared/components/feedback/NotFound.tsx";

const rootRoute = createRootRoute({
  component: () => (
    <RootLayout>
      <Outlet />
    </RootLayout>
  ),
  notFoundComponent: NotFound,
});

const homeRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/",
  component: lazyRouteComponent(
    () => import("@/features/catalog/pages/HomePage.tsx"),
  ),
});

const catalogRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/products",
  component: lazyRouteComponent(
    () => import("@/features/catalog/pages/CatalogPage.tsx"),
  ),
});

const productDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/products/$productId",
  component: lazyRouteComponent(
    () => import("@/features/catalog/pages/ProductDetailPage.tsx"),
  ),
});

const basketRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/basket",
  component: lazyRouteComponent(
    () => import("@/features/basket/pages/BasketPage.tsx"),
  ),
});

const checkoutRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/checkout",
  component: lazyRouteComponent(
    () => import("@/features/order/pages/CheckoutPage.tsx"),
  ),
});

const ordersRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/orders",
  component: lazyRouteComponent(
    () => import("@/features/order/pages/OrdersPage.tsx"),
  ),
});

const orderDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/orders/$orderId",
  component: lazyRouteComponent(
    () => import("@/features/order/pages/OrderDetailPage.tsx"),
  ),
});

const adminRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminDashboardPage.tsx"),
  ),
});

const adminProductsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/products",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminProductsPage.tsx"),
  ),
});

const adminProductEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/products/$productId/edit",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminProductEditPage.tsx"),
  ),
});

const adminOrdersRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/orders",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminOrdersPage.tsx"),
  ),
});

const adminOrderDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/orders/$orderId",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminOrderDetailPage.tsx"),
  ),
});

const profileRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/profile",
  component: lazyRouteComponent(
    () => import("@/features/profile/pages/ProfilePage.tsx"),
  ),
});

const adminStockRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/stock",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminStockPage.tsx"),
  ),
});

const adminShippingRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/admin/shipping",
  component: lazyRouteComponent(
    () => import("@/features/admin/pages/AdminShippingPage.tsx"),
  ),
});

const authCallbackRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/auth/callback",
  component: () => (
    <div className="flex items-center justify-center min-h-[50vh]">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
    </div>
  ),
});

const routeTree = rootRoute.addChildren([
  homeRoute,
  catalogRoute,
  productDetailRoute,
  basketRoute,
  checkoutRoute,
  ordersRoute,
  orderDetailRoute,
  adminRoute,
  adminProductsRoute,
  adminProductEditRoute,
  adminOrdersRoute,
  adminOrderDetailRoute,
  adminStockRoute,
  adminShippingRoute,
  profileRoute,
  authCallbackRoute,
]);

export const router = createRouter({
  routeTree,
  defaultNotFoundComponent: NotFound,
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
