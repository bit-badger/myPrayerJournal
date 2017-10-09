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
    b-nav(is-nav-bar)
      b-nav-item(v-if='isAuthenticated'
                 to='/journal') Journal
      b-nav-item(v-if='isAuthenticated'
                 to='/answered') Answered
      b-nav-item(v-if='isAuthenticated'): a(@click.stop='logOff()') Log Off
      b-nav-item(v-if='!isAuthenticated'): a(@click.stop='logOn()') Log On
      b-nav-item(href='https://danieljsummers.github.io/myPrayerJournal/'
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
    ...mapState([ 'isAuthenticated' ])
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
