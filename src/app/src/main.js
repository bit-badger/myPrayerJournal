/* eslint-disable */

// Vue packages and components
import Vue         from 'vue'
import VueMaterial from 'vue-material'

// myPrayerJournal components
import App         from './App'
import router      from './router'
import store       from './store'
import DateFromNow from './components/common/DateFromNow'
import PageTitle   from './components/common/PageTitle'

/* eslint-enable */

// Styles
import 'vue-material/dist/vue-material.min.css'
import 'vue-material/dist/theme/default.css'

Vue.config.productionTip = false

Vue.use(VueMaterial)
Vue.component('date-from-now', DateFromNow)
Vue.component('page-title', PageTitle)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
