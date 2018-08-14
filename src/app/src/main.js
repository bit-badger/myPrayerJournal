import Vue from 'vue'
import BootstrapVue from 'bootstrap-vue'
import Icon from 'vue-awesome/components/Icon'
import VueProgressBar from 'vue-progressbar'
import VueToast from 'vue-toast'

import 'bootstrap-vue/dist/bootstrap-vue.css'
import 'bootstrap/dist/css/bootstrap.css'
import 'vue-toast/dist/vue-toast.min.css'

// Only import the icons we need; the whole set is ~500K!
import 'vue-awesome/icons/check'
import 'vue-awesome/icons/clock-o'
import 'vue-awesome/icons/file-text-o'
import 'vue-awesome/icons/pencil'
import 'vue-awesome/icons/plus'
import 'vue-awesome/icons/search'
import 'vue-awesome/icons/times'

import App from './App.vue'
import router from './router'
import store from './store'
import DateFromNow from './components/common/DateFromNow'
import PageTitle from './components/common/PageTitle'

Vue.config.productionTip = false

Vue.use(BootstrapVue)
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

Vue.component('icon', Icon)
Vue.component('date-from-now', DateFromNow)
Vue.component('page-title', PageTitle)
Vue.component('toast', VueToast)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
