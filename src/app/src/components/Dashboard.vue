<template lang="pug">
  article
    page-title(:title="title")
    p(v-if="isLoadingJournal") journal is loading...
    template(v-if="!isLoadingJournal")
      new-request
      p journal has {{ journal.length }} entries
      request-list-item(v-for="request in journal" v-bind:request="request" v-bind:key="request.requestId")
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import PageTitle from './PageTitle'
import NewRequest from './request/NewRequest'
import RequestListItem from './request/RequestListItem'

import actions from '@/store/action-types'

export default {
  name: 'dashboard',
  async data () {
    this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
    return {}
  },
  components: {
    PageTitle,
    NewRequest,
    RequestListItem
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Dashboard`
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  }
}
</script>
