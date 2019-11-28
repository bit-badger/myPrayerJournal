<template lang="pug">
.md-toolbar-row
  md-tabs(md-sync-route).md-primary
    template(v-if='isAuthenticated')
      md-tab(md-label='Journal'
             to='/journal')
      md-tab(md-label='Active'
             to='/requests/active')
      md-tab(v-if='hasSnoozed.value'
             md-label='Snoozed'
             to='/requests/snoozed')
      md-tab(md-label='Answered'
             to='/requests/answered')
      md-tab(md-label='Log Off'
             href='/user/log-off'
             @click.prevent='logOff()')
      md-tab(md-label='Docs'
             href='https://docs.prayerjournal.me'
             @click.prevent='showHelp()')
    template(v-else)
      md-tab(md-label='Log On'
             href='/user/log-on'
             @click.prevent='logOn()')
      md-tab(md-label='Docs'
             href='https://docs.prayerjournal.me'
             @click.prevent='showHelp()')
</template>

<script lang="ts">
import { computed, createComponent } from '@vue/composition-api'
import { Store } from 'vuex' // eslint-disable-line no-unused-vars

import { AppState } from '../../store/types' // eslint-disable-line no-unused-vars
import { AuthService } from '../../auth' // eslint-disable-line no-unused-vars
import { useAuth } from '../../plugins/auth'
import { useRouter } from '../../plugins/router'
import { useStore } from '../../plugins/store'

export default createComponent({
  setup () {
    /** The Vuex store */
    const store = useStore() as Store<AppState>

    /** The auth service */
    const auth = useAuth() as AuthService

    /** The router for myPrayerJournal */
    const router = useRouter()

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
      router.push('/')
    }

    /** Open a new window/tab with help */
    const showHelp = () => { window.open('https://docs.prayerjournal.me', '_blank') }

    return {
      hasSnoozed,
      logOn,
      logOff,
      showHelp
    }
  }
})
</script>
