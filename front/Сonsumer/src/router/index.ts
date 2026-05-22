import { createRouter, createWebHistory } from 'vue-router'
import AuthPage from '@/Page/AuthPage.vue'
import RegPage from '@/Page/RegPage.vue'
import ResPage from '@/Page/ResPage.vue'
import MainLayout from '@/layouts/MainLayout.vue'
import ProfilePage from '@/Page/ProfilePage.vue'
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
    },
    {
      path:'/res',
      component:ResPage
    },
    {
      path:"/",
      component:MainLayout,
      children: [
        { path:"profile",
          component:ProfilePage
        }
      ]
    }
  ]
})

export default router
