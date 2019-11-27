<template lang="pug">
md-content(role='main').mpj-main-content-wide
  page-title(:title='title')
  p(v-if='isLoadingJournal') Loading your prayer journal...
  template(v-else)
    md-empty-state(v-if='journal.length === 0'
                   md-icon='done_all'
                   md-label='No Requests to Show'
                   md-description='You have no requests to be shown; see the “Active” link above for snoozed/deferred requests, and the “Answered” link for answered requests')
      md-button(:to="{ name: 'EditRequest', params: { id: 'new' } }").md-primary.md-raised Add a New Request
    template(v-else)
      .mpj-text-center
        md-button(:to="{ name: 'EditRequest', params: { id: 'new' } }"
                  role='button').md-raised.md-accent #[md-icon add_box] Add a New Request
      br
      .mpj-journal
        request-card(v-for='request in journal'
                    :key='request.requestId'
                    :request='request')
    notes-edit
    snooze-request
</template>

<script lang="ts">
import Vue from 'vue'
import { computed, createComponent, inject, onBeforeMount, provide } from '@vue/composition-api'
import { Store } from 'vuex' // eslint-disable-line no-unused-vars

import NotesEdit from './request/NotesEdit.vue'
import RequestCard from './request/RequestCard.vue'
import SnoozeRequest from './request/SnoozeRequest.vue'

import { Actions, AppState } from '../store/types' // eslint-disable-line no-unused-vars
import { useStore } from '../plugins/store'
import { useSnackbar, useProgress } from '../App.vue'

const EventSymbol = Symbol('Journal events')

export default createComponent({
  components: {
    NotesEdit,
    RequestCard,
    SnoozeRequest
  },
  setup () {
    /** The Vuex store */
    const store = useStore() as Store<AppState>

    /** The title of the page */
    const title = computed(() => `${store.state.user.given_name}&rsquo;s Prayer Journal`)

    /** Events to which the journal will respond */
    const eventBus = new Vue()

    /** Reference to the application's snackbar component */
    const snackbar = useSnackbar()

    /** Reference to the application's progress bar component */
    const progress = useProgress()

    /** Provide the event bus for child components */
    provide(EventSymbol, eventBus)

    onBeforeMount(async () => {
      await store.dispatch(Actions.LoadJournal, progress)
      snackbar.events.$emit('info', `Loaded ${store.state.journal.length} prayer requests`)
    })

    return {
      title,
      journal: store.state.journal,
      isLoadingJournal: store.state.isLoadingJournal
    }
  }
})

export function useEvents () {
  const events = inject(EventSymbol)
  if (!events) {
    throw new Error('Event bus not configured')
  }
  return events as Vue
}
</script>

<style lang="sass">
.mpj-journal
  display: flex
  flex-flow: row wrap
  justify-content: center
  align-items: flex-start
.mpj-dialog-content
  padding: 0 1rem
</style>
