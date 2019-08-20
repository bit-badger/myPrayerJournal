<template lang="pug">
md-toolbar.md-large.md-dense.md-primary
  .md-toolbar-row
    .md-toolbar-section-start
      | howdy
      span.md-title
        span(style='font-weight:100;') my
        span(style='font-weight:600;') Prayer
        span(style='font-weight:700;') Journal
    .md-toolbar-section-end
      | ""
  .md-toolbar-row
    md-tabs.md-primary
      md-tab#mpj-home(md-label='Home'
                     :to="{ name: 'Home' }")
      md-tab#mpj-journal(v-if='isAuthenticated'
                         md-label='Journal'
                         :to="{ name: 'Journal' }")
      md-tab#mpj-active(v-if='isAuthenticated'
                        md-label='Active'
                        :to="{ name: 'ActiveRequests' }")
      md-tab#mpj-snoozed(v-if='hasSnoozed'
                         md-label='Snoozed'
                         :to="{ name: 'SnoozedRequests' }")
      md-tab#mpj-answered(v-if='isAuthenticated'
                          md-label='Answered'
                          :to="{ name: 'AnsweredRequests' }")
      md-tab#mpj-log-off(v-if='isAuthenticated'
                         md-label='Log Off'
                         href='#'
                         @click.stop='logOff()')
      md-tab#mpj-log-on(v-if='!isAuthenticated'
                        md-label='Log On'
                        href='#'
                        @click.stop='logOn()')
      md-tab#mpj-docs(md-label='Docs'
                      href='https://docs.prayerjournal.me'
                      target='_blank'
                      @click.stop='')
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
