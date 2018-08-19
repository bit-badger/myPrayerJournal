'use strict'

import Vue from 'vue'
import Router from 'vue-router'

import ActiveRequests from '@/components/request/ActiveRequests'
import AnsweredRequests from '@/components/request/AnsweredRequests'
import EditRequest from '@/components/request/EditRequest'
import FullRequest from '@/components/request/FullRequest'
import Home from '@/components/Home'
import Journal from '@/components/Journal'
import LogOn from '@/components/user/LogOn'
import PrivacyPolicy from '@/components/legal/PrivacyPolicy'
import SnoozedRequests from '@/components/request/SnoozedRequests'
import TermsOfService from '@/components/legal/TermsOfService'

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
  routes: [
    {
      path: '/',
      name: 'Home',
      component: Home
    },
    {
      path: '/journal',
      name: 'Journal',
      component: Journal
    },
    {
      path: '/legal/privacy-policy',
      name: 'PrivacyPolicy',
      component: PrivacyPolicy
    },
    {
      path: '/legal/terms-of-service',
      name: 'TermsOfService',
      component: TermsOfService
    },
    {
      path: '/request/:id/edit',
      name: 'EditRequest',
      component: EditRequest,
      props: true
    },
    {
      path: '/request/:id/full',
      name: 'FullRequest',
      component: FullRequest,
      props: true
    },
    {
      path: '/requests/active',
      name: 'ActiveRequests',
      component: ActiveRequests
    },
    {
      path: '/requests/answered',
      name: 'AnsweredRequests',
      component: AnsweredRequests
    },
    {
      path: '/requests/snoozed',
      name: 'SnoozedRequests',
      component: SnoozedRequests
    },
    {
      path: '/user/log-on',
      name: 'LogOn',
      component: LogOn
    }
//    {
//      path: '/about',
//      name: 'about',
//      // route level code-splitting
//      // this generates a separate chunk (about.[hash].js) for this route
//      // which is lazy-loaded when the route is visited.
//      component: () => import(/* webpackChunkName: "about" */ './views/About.vue')
//    }
  ]
})
