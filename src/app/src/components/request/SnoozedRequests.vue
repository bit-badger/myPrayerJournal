<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Snoozed Requests'
             hide-on-page=true)
  template(v-if='isLoaded.value')
    md-empty-state(v-if='requests.length === 0'
                   md-icon='sentiment_dissatisfied'
                   md-label='No Snoozed Requests'
                   md-description='Your prayer journal has no snoozed requests')
      md-button(to='/journal').md-primary.md-raised Return to your journal
    request-list(v-if='requests.length !== 0'
                 title='Snoozed Requests'
                 :requests='requests')
  p(v-else) Loading journal...
</template>

<script lang="ts">
import { createComponent, ref, onMounted } from '@vue/composition-api'
import { Store } from 'vuex' // eslint-disable-line no-unused-vars

import RequestList from './RequestList.vue'

import { Actions, AppState, JournalRequest } from '../../store/types' // eslint-disable-line no-unused-vars
import { useStore } from '../../plugins/store'
import { useProgress } from '../../App.vue'

export default createComponent({
  components: { RequestList },
  setup () {
    /** The Vuex store */
    const store = useStore() as Store<AppState>

    /** The progress bar component properties */
    const progress = useProgress()

    /** The snoozed requests */
    let requests: JournalRequest[] = []

    /** Have snoozed requests been loaded? */
    const isLoaded = ref(false)

    /** Ensure the latest journal is loaded, and filter it to snoozed requests */
    const ensureJournal = async () => {
      if (!Array.isArray(store.state.journal)) {
        isLoaded.value = false
        await store.dispatch(Actions.LoadJournal, progress)
      }
      requests = store.state.journal
        .filter(req => req.snoozedUntil > Date.now())
        .sort((a, b) => a.snoozedUntil - b.snoozedUntil)
      isLoaded.value = true
    }

    onMounted(ensureJournal)

    // this.$on('requestUnsnoozed', ensureJournal)

    return {
      requests,
      isLoaded
    }
  }
})
</script>
