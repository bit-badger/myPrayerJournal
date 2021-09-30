import { createApp } from "vue"
import App from "./App.vue"
import auth, { key as authKey } from "./plugins/auth"
import router from "./router"
import store, { key as storeKey } from "./store"

createApp(App).use(store, storeKey).use(router).use(auth, authKey).mount('#app')
