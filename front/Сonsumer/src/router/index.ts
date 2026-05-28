import { createRouter, createWebHistory } from 'vue-router'

import AuthPage from '@/Page/AuthPage.vue'
import RegPage from '@/Page/RegPage.vue'
import ResPage from '@/Page/ResPage.vue'

import MainLayout from '@/layouts/MainLayout.vue'
import ProfilePage from '@/Page/ProfilePage.vue'
import ViewProfilePage from '@/Page/ViewProfilePage.vue'
import FeedPage from '@/Page/FeedPage.vue'
import MessagesPage from '@/Page/MessagesPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/profile'
    },

    {
      path: '/auth',
      name: 'auth',
      component: AuthPage
    },
    {
      path: '/reg',
      name: 'reg',
      component: RegPage
    },
    {
      path: '/res',
      name: 'res',
      component: ResPage
    },

    {
      path: '/',
      component: MainLayout,
      children: [
        {
          path: 'profile',
          name: 'profile',
          component: ProfilePage
        },
        {
        path: 'profile/:id',
        name: 'profile-view',
        component: ViewProfilePage
        },
        {
         path: 'feed/:userId?/:postId?',
         name: 'feed',
         component: FeedPage,
         props: true
        },
        {
        path: "/messages",
        component: MessagesPage
        }
      ]
    }
  ]
})

export default router
