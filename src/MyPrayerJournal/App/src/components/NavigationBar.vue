<template lang="pug">
nav.navbar
  .container-fluid
    router-link.navbar-brand(to="/") myPrayerJournal
    ul.navbar-nav.me-auto.d-flex.flex-row
      template(v-if="isAuthenticated")
        li.nav-item: router-link(to="/journal") Journal
        li.nav-item: router-link(to="/requests/active") Active
        li.nav-item(v-if="hasSnoozed"): router-link(to="/requests/snoozed") Snoozed
        li.nav-item: router-link(to="/requests/answered") Answered
        li.nav-item: a(href="/user/log-off" @click.prevent="logOff()") Log Off
      template(v-else)
        li.nav-item: a(href="/user/log-on" @click.prevent="logOn()") Log On
      li.nav-item: a(href="https://docs.prayerjournal.me" target="_blank") Docs
</template>

<script setup lang="ts">
import { computed } from "vue"
import { useRouter } from "vue-router"

import { useAuth } from "@/plugins/auth"
import { useStore } from "@/store"

const store = useStore()
const router = useRouter()
const auth = useAuth()

/** Whether a user is authenticated */
const isAuthenticated = computed(() => store?.state.isAuthenticated ?? false)

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
</script>
