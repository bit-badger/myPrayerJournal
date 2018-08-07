<template lang="pug">
b-navbar(toggleable='sm'
         type='dark'
         variant='mpj'
         fixed='top')
  b-nav-toggle(target='nav_collapse')
  b-navbar-brand(to='/')
    span(style='font-weight:100;') my
    span(style='font-weight:600;') Prayer
    span(style='font-weight:700;') Journal
  b-collapse#nav_collapse(is-nav)
    b-navbar-nav
      b-nav-item(v-if='isAuthenticated'
                 to='/journal') Journal
      b-nav-item(v-if='hasSnoozed'
                 to='/snoozed') Snoozed
      b-nav-item(v-if='isAuthenticated'
                 to='/answered') Answered
      b-nav-item(v-if='isAuthenticated'): a(@click.stop='logOff()') Log Off
      b-nav-item(v-if='!isAuthenticated'): a(@click.stop='logOn()') Log On
      b-nav-item(href='https://bit-badger.github.io/myPrayerJournal/'
                 target='_blank'
                 @click.stop='') Docs
</template>

<script>
'use strict'

import { mapState } from 'vuex'
import AuthService from '@/auth/AuthService'

export default {
  name: 'navigation',
  data () {
    return {
      auth0: new AuthService()
    }
  },
  computed: {
    hasSnoozed () {
      return this.isAuthenticated &&
        Array.isArray(this.journal) &&
        this.journal.filter(req => req.snoozedUntil > Date.now()).length > 0
    },
    ...mapState([ 'journal', 'isAuthenticated' ])
  },
  methods: {
    logOn () {
      this.auth0.login()
    },
    logOff () {
      this.auth0.logout(this.$store, this.$router)
    }
  }
}
</script>
