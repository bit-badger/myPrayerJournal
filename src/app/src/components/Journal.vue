<template lang="pug">
article
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-if='!isLoadingJournal')
    new-request
    br
    b-row(v-if='journal.length > 0')
      request-card(v-for='request in journal'
                   :key='request.requestId'
                   :request='request'
                   :events='eventBus'
                   :toast='toast')
    p.text-center(v-if='journal.length === 0'): em.
      No requests found; click the &ldquo;Add a New Request&rdquo; button to add one
    edit-request(:events='eventBus'
                 :toast='toast')
    notes-edit(:events='eventBus'
               :toast='toast')
    full-request(:events='eventBus')
</template>

<script>
'use strict'

import Vue from 'vue'
import { mapState } from 'vuex'
import chunk from 'lodash/chunk'

import EditRequest from './request/EditRequest'
import FullRequest from './request/FullRequest'
import NewRequest from './request/NewRequest'
import NotesEdit from './request/NotesEdit'
import RequestCard from './request/RequestCard'

import actions from '@/store/action-types'

export default {
  name: 'journal',
  components: {
    EditRequest,
    FullRequest,
    NewRequest,
    NotesEdit,
    RequestCard
  },
  data () {
    return {
      eventBus: new Vue()
    }
  },
  computed: {
    title () {
      return `${this.user.given_name}&rsquo;s Prayer Journal`
    },
    journalCardRows () {
      return chunk(this.journal, 3)
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
