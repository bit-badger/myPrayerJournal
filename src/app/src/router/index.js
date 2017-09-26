import Vue from 'vue'
import Router from 'vue-router'

import Home from '@/components/Home'
import Journal from '@/components/Journal'
import LogOn from '@/components/user/LogOn'

Vue.use(Router)

export default new Router({
  mode: 'history',
  routes: [
    { path: '/', name: 'Home', component: Home },
    { path: '/journal', name: 'Journal', component: Journal },
    { path: '/user/log-on', name: 'LogOn', component: LogOn }
  ]
})
