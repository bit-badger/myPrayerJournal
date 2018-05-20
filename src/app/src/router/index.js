import Vue from 'vue'
import Router from 'vue-router'

import Answered from '@/components/Answered'
import AnsweredDetail from '@/components/AnsweredDetail'
import Home from '@/components/Home'
import Journal from '@/components/Journal'
import LogOn from '@/components/user/LogOn'
import PrivacyPolicy from '@/components/legal/PrivacyPolicy'
import TermsOfService from '@/components/legal/TermsOfService'

Vue.use(Router)

export default new Router({
  mode: 'history',
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
      path: '/user/log-on',
      name: 'LogOn',
      component: LogOn
    }
  ]
})
