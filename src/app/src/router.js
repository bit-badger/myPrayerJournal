import Vue from 'vue'
import Router from 'vue-router'

import Answered from '@/components/Answered'
import AnsweredDetail from '@/components/AnsweredDetail'
import Home from '@/components/Home'
import Journal from '@/components/Journal'
import LogOn from '@/components/user/LogOn'
import PrivacyPolicy from '@/components/legal/PrivacyPolicy'
import Snoozed from '@/components/Snoozed'
import TermsOfService from '@/components/legal/TermsOfService'

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'Home',
      component: Home
    },
    {
      path: '/answered/:id',
      name: 'AnsweredDetail',
      component: AnsweredDetail,
      props: true
    },
    {
      path: '/answered',
      name: 'Answered',
      component: Answered
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
      path: '/snoozed',
      name: 'Snoozed',
      component: Snoozed
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
