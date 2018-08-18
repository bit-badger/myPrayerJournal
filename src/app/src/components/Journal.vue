<template lang="pug">
article.mpj-main-content-wide(role='main')
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-else)
    router-link(:to="{ name: 'EditRequest', params: { id: 'new' } }"
                role='button')
      md-icon(icon='add_box')
      | &nbsp; Add a New Request
    br
    .mpj-journal(v-if='journal.length > 0')
      request-card(v-for='request in journal'
                   :key='request.requestId'
                   :request='request'
                   :events='eventBus'
                   :toast='toast')
    p.text-center(v-else): em.
      No requests found; click the &ldquo;Add a New Request&rdquo; button to add one
    notes-edit(:events='eventBus'
               :toast='toast')
    snooze-request(:events='eventBus'
                   :toast='toast')
</template>

<script>
'use strict'

import Vue from 'vue'
import { mapState } from 'vuex'

import NotesEdit from './request/NotesEdit'
import RequestCard from './request/RequestCard'
import SnoozeRequest from './request/SnoozeRequest'

import actions from '@/store/action-types'

export default {
  name: 'journal',
  components: {
    NotesEdit,
    RequestCard,
    SnoozeRequest
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

<style>
.mpj-journal {
  display: flex;
  flex-flow: row wrap;
  justify-content: center;
}
</style>