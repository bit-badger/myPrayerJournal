<template lang="pug">
nav.mpj-top-nav.mpj-bg(role='menubar')
  router-link.title(:to="{ name: 'Home' }"
                    role='menuitem')
    span(style='font-weight:100;') my
    span(style='font-weight:600;') Prayer
    span(style='font-weight:700;') Journal
  router-link(v-if='isAuthenticated'
              :to="{ name: 'Journal' }"
              role='menuitem') Journal
  router-link(v-if='isAuthenticated'
              :to="{ name: 'ActiveRequests' }"
              role='menuitem') Active
  router-link(v-if='hasSnoozed'
              :to="{ name: 'SnoozedRequests' }"
              role='menuitem') Snoozed
  router-link(v-if='isAuthenticated'
              :to="{ name: 'AnsweredRequests' }"
              role='menuitem') Answered
  a(v-if='isAuthenticated'
    href='#'
    role='menuitem'
    @click.stop='logOff()') Log Off
  a(v-if='!isAuthenticated'
    href='#'
    role='menuitem'
    @click.stop='logOn()') Log On
  a(href='https://bit-badger.github.io/myPrayerJournal/'
    target='_blank'
    role='menuitem'
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

<style>
.mpj-top-nav {
  position: fixed;
  display: flex;
  flex-flow: row wrap;
  align-items: center;
  top: 0;
  left: 0;
  width: 100%;
  padding-left: .5rem;
  min-height: 50px;
}
.mpj-top-nav a:link,
.mpj-top-nav a:visited {
  text-decoration: none;
  color: rgba(255, 255, 255, .75);
  padding-left: 1rem;
}
.mpj-top-nav a:link.router-link-active,
.mpj-top-nav a:visited.router-link-active,
.mpj-top-nav a:hover {
  color: white;
}
.mpj-top-nav .title {
  font-size: 1.25rem;
  color: white;
  padding-left: 1.25rem;
  padding-right: 1.25rem;
}
</style>