<template lang="pug">
  article
    page-title(:title="title")
    p here you are!
    p(v-if="isLoadingJournal") journal is loading...
    p(v-if="!isLoadingJournal") journal has {{ journal.length }} entries
</template>

<script>
import { mapState } from 'vuex'
import PageTitle from './PageTitle'

import * as actions from '@/store/action-types'

export default {
  name: 'dashboard',
  data () {
    this.$store.dispatch(actions.LOAD_JOURNAL)
    return {}
  },
  components: {
    PageTitle
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Dashboard`
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  }
}
</script>
