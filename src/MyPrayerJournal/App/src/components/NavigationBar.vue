<template lang="pug">
nav.navbar.navbar-dark
  .container-fluid
    router-link.navbar-brand(to="/")
      span.m my
      span.p Prayer
      span.j Journal
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

<style lang="sass" scoped>
nav
  background-color: green
.m
  font-weight: 100
.p
  font-weight: 400
.j
  font-weight: 700
.nav-item
  a:link,
  a:visited
    padding: .5rem 1rem
    margin: 0 .5rem
    border-radius: .5rem
    color: white
    text-decoration: none
  a:hover
    cursor: pointer
    background-color: rgba(255, 255, 255, .2)
.navbar-nav .router-link-exact-active
  background-color: rgba(255, 255, 255, .2)
</style>
