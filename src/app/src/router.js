'use strict'

/* eslint-disable */
import Vue    from 'vue'
import Router from 'vue-router'

import auth from './auth/AuthService'
import Home from '@/components/Home'
/* eslint-enable */

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  scrollBehavior (to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { x: 0, y: 0 }
    }
  },
  beforeEach (to, from, next) {
    if (to.path === '/' || to.path === '/user/log-on' || auth.isAuthenticated()) {
      return next()
    }
    auth.login({ target: to.path })
  },
  routes: [
    {
      path: '/',
      name: 'Home',
      component: Home
    },
    {
      path: '/journal',
      name: 'Journal',
      component: () => import('@/components/Journal')
    },
    {
      path: '/legal/privacy-policy',
      name: 'PrivacyPolicy',
      component: () => import('@/components/legal/PrivacyPolicy')
    },
    {
      path: '/legal/terms-of-service',
      name: 'TermsOfService',
      component: () => import('@/components/legal/TermsOfService')
    },
    {
      path: '/request/:id/edit',
      name: 'EditRequest',
      component: () => import('@/components/request/EditRequest'),
      props: true
    },
    {
      path: '/request/:id/full',
      name: 'FullRequest',
      component: () => import('@/components/request/FullRequest'),
      props: true
    },
    {
      path: '/requests/active',
      name: 'ActiveRequests',
      component: () => import('@/components/request/ActiveRequests')
    },
    {
      path: '/requests/answered',
      name: 'AnsweredRequests',
      component: () => import('@/components/request/AnsweredRequests')
    },
    {
      path: '/requests/snoozed',
      name: 'SnoozedRequests',
      component: () => import('@/components/request/SnoozedRequests')
    },
    {
      path: '/user/log-on',
      name: 'LogOn',
      component: () => import('@/components/user/LogOn')
    }
  ]
})
