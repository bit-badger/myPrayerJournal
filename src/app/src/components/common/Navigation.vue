<template lang="pug">
.md-toolbar-row
  md-tabs(md-sync-route).md-primary
    md-tab(v-if='isAuthenticated'
           md-label='Journal'
           to='/journal')
    md-tab(v-if='isAuthenticated'
           md-label='Active'
           to='/requests/active')
    md-tab(v-if='hasSnoozed'
           md-label='Snoozed'
           to='/requests/snoozed')
    md-tab(v-if='isAuthenticated'
           md-label='Answered'
           to='/requests/answered')
    md-tab(v-if='isAuthenticated'
           md-label='Log Off'
           href='#'
           @click.stop='logOff()')
    md-tab(v-if='!isAuthenticated'
           md-label='Log On'
           href='#'
           @click.stop='logOn()')
    md-tab(md-label='Docs'
           href='https://docs.prayerjournal.me'
           target='_blank'
           @click.stop='')
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
    }
  }
}
</script>
