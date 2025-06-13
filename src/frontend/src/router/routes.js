import { authGuard } from './index'

const routes = [
  // Página de login (agora usando um layout minimalista)
  {
    path: '/login',
    component: () => import('layouts/EmptyLayout.vue'), // <-- Aponta para o novo layout
    children: [
      { path: '', component: () => import('pages/LoginPage.vue') }, // <-- LoginPage.vue como filho
    ],
  },

  // Rotas protegidas com layout principal
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    beforeEnter: authGuard,
    children: [
      { path: '', redirect: '/dashboard' },

      {
        path: 'dashboard',
        component: () => import('pages/DashboardPage.vue'),
      },
      {
        path: 'usuarios',
        component: () => import('pages/UsuariosPage.vue'),
      },
      {
        path: 'perfis',
        component: () => import('pages/PerfisPage.vue'),
      },
      {
        path: 'acessos',
        component: () => import('pages/AcessosPage.vue'),
      },
    ],
  },

  // Rota de erro 404
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
]

export default routes
