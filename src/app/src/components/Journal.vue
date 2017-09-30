<template lang="pug">
article
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-if='!isLoadingJournal')
    new-request
    br
    request-list-item(v-if='journal.length > 0'
                      v-for='row in journalCardRows'
                      :row='row'
                      :events='eventBus'
                      :toast='toast'
                      :key='row[0].requestId')
    p.text-center(v-if='journal.length === 0'): em No requests found; click the "Add a New Request" button to add one
    edit-request(:events='eventBus' :toast='toast' )
    full-request(:events='eventBus')
</template>

<script>
'use strict'

import Vue from 'vue'
import { mapState } from 'vuex'
import _ from 'lodash'

import EditRequest from './request/EditRequest'
import FullRequest from './request/FullRequest'
import NewRequest from './request/NewRequest'
import RequestListItem from './request/RequestListItem'

import actions from '@/store/action-types'

export default {
  name: 'journal',
  data () {
    return {
      eventBus: new Vue()
    }
  },
  components: {
    EditRequest,
    FullRequest,
    NewRequest,
    RequestListItem
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Prayer Journal`
    },
    journalCardRows () {
      return _.chunk(this.journal, 3)
    },
    toast () {
      return this.$parent.$refs.toast
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  },
  async created () {
    await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
    this.toast.showToast(`Loaded ${this.journal.length} prayer requests`, { theme: 'success' })
  }
}
</script>
