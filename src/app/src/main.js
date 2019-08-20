// Vue packages and components
import Vue            from 'vue'
import VueMaterial    from 'vue-material'
import VueProgressBar from 'vue-progressbar'
import VueToast       from 'vue-toast'

// myPrayerJournal components
import App         from './App'
import router      from './router'
import store       from './store'
import DateFromNow from './components/common/DateFromNow'
import PageTitle   from './components/common/PageTitle'

// Styles
import 'vue-material/dist/vue-material.min.css'
import 'vue-material/dist/theme/default.css'
import 'vue-toast/dist/vue-toast.min.css'

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

Vue.use(VueMaterial)
Vue.component('date-from-now', DateFromNow)
Vue.component('page-title',    PageTitle)
Vue.component('toast',         VueToast)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
