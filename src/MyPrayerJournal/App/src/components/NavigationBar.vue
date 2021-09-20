<template lang="pug">
.md-toolbar-row
  md-tabs(md-sync-route).md-primary
    template(v-if="isAuthenticated")
      md-tab(md-label="Journal" to="/journal")
      md-tab(md-label="Active" to="/requests/active")
      md-tab(v-if="hasSnoozed" md-label="Snoozed" to="/requests/snoozed")
      md-tab(md-label="Answered" to="/requests/answered")
      md-tab(md-label="Log Off" href="/user/log-off" @click.prevent="logOff()")
    template(v-else)
      md-tab(md-label="Log On" href="/user/log-on" @click.prevent="logOn()")
    md-tab(md-label="Docs" href="https://docs.prayerjournal.me" @click.prevent="showHelp()")
</template>

<script setup lang="ts">
import { computed, inject } from "vue"
import { useRouter } from "vue-router"

import { useStore } from "@/store"
import { AuthService } from "@/auth"
import { AuthSymbol } from "@/App.vue"

const store = useStore()
const router = useRouter()

/** The auth service */
const auth = inject(AuthSymbol) as AuthService

/** Whether a user is authenticated */
const isAuthenticated = computed(() => store.state.isAuthenticated)

/** Whether the user has any snoozed requests */
const hasSnoozed = computed(() =>
  store.state.isAuthenticated &&
    Array.isArray(store.state.journal) &&
    store.state.journal.filter(req => req.snoozedUntil > Date.now()).length > 0)

/** Log a user on using Auth0 */
const logOn = () => auth.login()

/** Log a user off using Auth0 */
const logOff = () => {
  auth.logout(store)
  router.push("/")
}

/** Open a new window/tab with help */
const showHelp = () => { window.open("https://docs.prayerjournal.me", "_blank") }
</script>
