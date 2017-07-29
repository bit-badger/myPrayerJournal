import Vue from 'vue'
import Router from 'vue-router'

import Dashboard from '@/components/Dashboard'
import Home from '@/components/Home'
import LogOn from '@/components/user/LogOn'

Vue.use(Router)

export default new Router({
  mode: 'history',
  routes: [
    { path: '/', name: 'Home', component: Home },
    { path: '/dashboard', name: 'Dashboard', component: Dashboard },
    { path: '/user/log-on', name: 'LogOn', component: LogOn }
  ]
})
