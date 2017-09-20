<template lang="pug">
  article
    page-title(:title="title")
    p(v-if="isLoadingJournal") journal is loading...
    template(v-if="!isLoadingJournal")
      new-request
      p journal has {{ journal.length }} entries
</template>

<script>
import { mapState } from 'vuex'
import PageTitle from './PageTitle'
import NewRequest from './request/NewRequest'

import * as actions from '@/store/action-types'

export default {
  name: 'dashboard',
  data () {
    this.$store.dispatch(actions.LOAD_JOURNAL)
    return {}
  },
  components: {
    PageTitle,
    NewRequest
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Dashboard`
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  }
}
</script>
