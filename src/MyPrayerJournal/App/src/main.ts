import { createApp } from "vue"
import App from "./App.vue"
import auth from "./plugins/auth"
import router from "./router"
import store from "./store"

createApp(App).use(store).use(router).use(auth).mount('#app')
