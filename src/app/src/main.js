// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import VueProgressBar from 'vue-progressbar'
import VueToast from 'vue-toast'

import 'vue-toast/dist/vue-toast.min.css'

import App from './App'
import router from './router'
import store from './store'
import DateFromNow from './components/common/DateFromNow'
import MaterialDesignIcon from './components/common/MaterialDesignIcon'
import PageTitle from './components/common/PageTitle'

Vue.config.productionTip = false

Vue.use(VueProgressBar, {
  color: 'yellow',
  failedColor: 'red',
  height: '5px',
  transition: {
    speed: '0.2s',
    opacity: '0.6s',
    termination: 1000
  }
})

Vue.component('date-from-now', DateFromNow)
Vue.component('md-icon', MaterialDesignIcon)
Vue.component('page-title', PageTitle)
Vue.component('toast', VueToast)

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  store,
  template: '<App/>',
  components: { App }
})
