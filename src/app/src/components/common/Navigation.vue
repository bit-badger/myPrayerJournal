<template lang="pug">
.md-toolbar-row
  md-tabs(md-sync-route).md-primary
    template(v-if='isAuthenticated')
      md-tab(md-label='Journal'
             to='/journal')
      md-tab(md-label='Active'
             to='/requests/active')
      md-tab(v-if='hasSnoozed'
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

<script>
'use strict'

import { mapState } from 'vuex'

export default {
  name: 'navigation',
  data () {
    return {}
  },
  computed: {
    hasSnoozed () {
      return this.isAuthenticated &&
        Array.isArray(this.journal) &&
        this.journal.filter(req => req.snoozedUntil > Date.now()).length > 0
    },
    ...mapState([ 'isAuthenticated', 'journal' ])
  },
  methods: {
    logOn () {
      this.$auth.login()
    },
    logOff () {
      this.$auth.logout(this.$store, this.$router)
    },
    showHelp () {
      window.open('https://docs.prayerjournal.me', '_blank')
    }
  }
}
</script>
