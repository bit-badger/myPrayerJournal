<template lang="pug">
article.mpj-main-content-wide(role='main')
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-else)
    .mpj-text-center
      md-button(:to="{ name: 'EditRequest', params: { id: 'new' } }"
                role='button').
        #[md-icon add_box] Add a New Request
    br
    .mpj-journal(v-if='journal.length > 0')
      request-card(v-for='request in journal'
                   :key='request.requestId'
                   :request='request')
    p.text-center(v-else): em.
      No requests found; click the &ldquo;Add a New Request&rdquo; button to add one
    notes-edit
    snooze-request
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
  inject: [
    'messages',
    'progress'
  ],
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
    snackbar () {
      return this.$parent.$refs.snackbar
    },
    ...mapState(['user', 'journal', 'isLoadingJournal'])
  },
  async created () {
    await this.$store.dispatch(actions.LOAD_JOURNAL, this.progress)
    this.messages.$emit('info', `Loaded ${this.journal.length} prayer requests`)
  },
  provide () {
    return {
      journalEvents: this.eventBus
    }
  }
}
</script>

<style>
.mpj-journal {
  display: flex;
  flex-flow: row wrap;
  justify-content: center;
  align-items: flex-start;
}
</style>
