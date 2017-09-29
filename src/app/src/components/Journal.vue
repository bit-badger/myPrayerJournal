<template lang="pug">
article
  page-title(:title='title')
  toast(ref='toast')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-if='!isLoadingJournal')
    new-request
    br
    b-row
      request-list-item(v-if='journal.length > 0' v-for='request in journal' :request='request' :key='request.requestId')
    p.text-center(v-if='journal.length === 0'): em No requests found; click the "Add a New Request" button to add one
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import NewRequest from './request/NewRequest'
import RequestListItem from './request/RequestListItem'

import actions from '@/store/action-types'

export default {
  name: 'journal',
  components: {
    NewRequest,
    RequestListItem
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Prayer Journal`
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  },
  async created () {
    await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
    this.$refs.toast.setOptions({ position: 'bottom right' })
    this.$refs.toast.showToast(`Loaded ${this.journal.length} prayer requests`, { theme: 'success' })
  }
}
/*
    b-row
      b-col(cols='2'): strong Actions
      b-col(cols='8'): strong Request
      b-col(cols='2'): strong As Of
 */
</script>
