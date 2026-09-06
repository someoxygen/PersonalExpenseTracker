import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";
export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: "/", redirect: "/dashboard" },
    {
      path: "/login",
      name: "login",
      meta: { guest: true },
      component: () => import("../features/auth/AuthView.vue"),
    },
    {
      path: "/register",
      name: "register",
      meta: { guest: true },
      component: () => import("../features/auth/AuthView.vue"),
    },
    {
      path: "/",
      component: () => import("../layouts/MainLayout.vue"),
      meta: { requiresAuth: true },
      children: [
        {
          path: "accounts",
          name: "accounts",
          component: () => import("../features/accounts/AccountsView.vue"),
        },
        {
          path: "categories",
          name: "categories",
          component: () => import("../features/categories/CategoriesView.vue"),
        },
        {
          path: "budgets",
          name: "budgets",
          component: () => import("../features/budgets/BudgetsView.vue"),
        },
        {
          path: "transactions",
          name: "transactions",
          component: () =>
            import("../features/transactions/TransactionsView.vue"),
        },
        {
          path: "transactions/new",
          component: () =>
            import("../features/transactions/TransactionEditorView.vue"),
        },
        {
          path: "transactions/:id/edit",
          component: () =>
            import("../features/transactions/TransactionEditorView.vue"),
        },
        {
          path: "dashboard",
          name: "dashboard",
          component: () => import("../features/dashboard/DashboardView.vue"),
        },
        {
          path: "profile",
          name: "profile",
          component: () => import("../features/auth/ProfileView.vue"),
        },
      ],
    },
    {
      path: "/:pathMatch(.*)*",
      component: () => import("../views/NotFoundView.vue"),
    },
  ],
  scrollBehavior: () => ({ top: 0 }),
});
router.beforeEach((to) => {
  const auth = useAuthStore();
  if (to.meta.requiresAuth && !auth.isAuthenticated)
    return { name: "login", query: { redirect: to.fullPath } };
  if (to.meta.guest && auth.isAuthenticated) return { name: "dashboard" };
});
