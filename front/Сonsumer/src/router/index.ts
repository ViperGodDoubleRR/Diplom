import { createRouter, createWebHistory } from 'vue-router'
import AuthPage from '@/Page/AuthPage.vue'
import RegPage from '@/Page/RegPage.vue'
const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/auth',
      component: AuthPage
    },
    {
      path:'/reg',
      component:RegPage
    }
  ]
})

export default router
