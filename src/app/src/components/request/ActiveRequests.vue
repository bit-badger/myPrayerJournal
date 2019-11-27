<template lang="pug">
md-content(role='main').mpj-main-content
  page-title(title='Active Requests'
             hide-on-page=true)
  template(v-if='isLoaded.value')
    md-empty-state(v-if='requests.length === 0'
                   md-icon='sentiment_dissatisfied'
                   md-label='No Active Requests'
                   md-description='Your prayer journal has no active requests')
      md-button(to='/journal').md-primary.md-raised Return to your journal
    request-list(v-if='requests.length !== 0'
                 title='Active Requests'
                 :requests='requests')
  p(v-else) Loading journal...
</template>

<script lang="ts">
import { onBeforeMount, ref } from '@vue/composition-api'
import { Store } from 'vuex'

import RequestList from '@/components/request/RequestList.vue'
import { useProgress } from '../../App.vue'

import { Actions, AppState, JournalRequest } from '../../store/types'
import { useStore } from '../../plugins/store'

export default {
  inject: ['progress'],
  components: {
    RequestList
  },
  setup () {
    /** The Vuex store */
    const store = useStore() as Store<AppState>

    /** The progress bar component instance */
    const progress = useProgress()

    /** The requests, sorted by the date they will be next shown */
    let requests: JournalRequest[] = []

    /** Whether all requests have been loaded */
    const isLoaded = ref(false)

    const ensureJournal = async () => {
      if (!Array.isArray(store.state.journal)) {
        isLoaded.value = false
        await store.dispatch(Actions.LoadJournal, progress)
      }
      requests = store.state.journal.sort((a, b) => a.showAfter - b.showAfter)
      isLoaded.value = true
    }

    onBeforeMount(async () => { await ensureJournal() })

    // TODO: is "this" what we think it is here?
    this.$on('requestUnsnoozed', ensureJournal)
    this.$on('requestNowShown', ensureJournal)

    return {
      requests,
      isLoaded
    }
  }
}
</script>
