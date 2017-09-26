<template lang="pug">
article
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-if='!isLoadingJournal')
    new-request
    el-row
      el-col(:span='4'): strong Actions
      el-col(:span='16'): strong Request
      el-col(:span='4'): strong As Of
    request-list-item(v-if='journal.length > 0' v-for='request in journal' :request='request' :key='request.requestId')
    p.text-center(v-if='journal.length === 0'): em No requests found; click the "Add a New Request" button to add one
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import PageTitle from './PageTitle'
import NewRequest from './request/NewRequest'
import RequestListItem from './request/RequestListItem'

import actions from '@/store/action-types'

export default {
  name: 'journal',
  components: {
    PageTitle,
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
    this.$message({
      message: `Loaded ${this.journal.length} prayer requests`,
      type: 'success'
    })
  }
}
</script>
