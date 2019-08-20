import Vue from 'vue'
// eslint-disable-next-line
// import { MdApp, MdContent, MdProgress, MdSnackbar, MdTabs, MdToolbar } from 'vue-material/dist/components'
import { VueMaterial } from 'vue-material/dist/components'
import VueProgressBar from 'vue-progressbar'
import VueToast from 'vue-toast'

import 'vue-material/dist/vue-material.min.css'
import 'vue-material/dist/theme/default.css'
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

// Vue.use(MdApp)
// Vue.use(MdContent)
// Vue.use(MdProgress)
// Vue.use(MdSnackbar)
// Vue.use(MdTabs)
// Vue.use(MdToolbar)
Vue.use(VueMaterial)
Vue.component('date-from-now', DateFromNow)
Vue.component('md-icon', MaterialDesignIcon)
Vue.component('page-title', PageTitle)
Vue.component('toast', VueToast)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
