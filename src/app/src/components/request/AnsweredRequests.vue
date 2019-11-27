<template lang="pug">
md-content(role='main').mpj-main-content
  page-title(title='Answered Requests'
             hide-on-page=true)
  template(v-if='loaded')
    md-empty-state(v-if='requests.length === 0'
                   md-icon='sentiment_dissatisfied'
                   md-label='No Answered Requests'
                   md-description='Your prayer journal has no answered requests; once you have marked one as “Answered”, it will appear here')
    request-list(v-if='requests.length !== 0'
                 title='Answered Requests'
                 :requests='requests')
  p(v-else) Loading answered requests...
</template>

<script lang="ts">
import { ref, onMounted } from '@vue/composition-api'

import RequestList from './RequestList.vue'

import api from '../../api'
import { JournalRequest } from '../../store/types' // eslint-disable-line no-unused-vars
import { useProgress, useSnackbar } from '../../App.vue'

export default {
  components: {
    RequestList
  },
  setup () {
    /** The answered requests */
    let requests: JournalRequest[] = []

    /** Whether the requests have been loaded */
    const isLoaded = ref(false)

    /** The snackbar component instance */
    const snackbar = useSnackbar()

    /** The progress bar instance */
    const progress = useProgress()

    onMounted(async () => {
      progress.events.$emit('show', 'query')
      try {
        const reqs = await api.getAnsweredRequests()
        requests = reqs.data
        progress.events.$emit('done')
      } catch (err) {
        console.error(err) // eslint-disable-line no-console
        snackbar.events.$emit('error', 'Error loading requests; check console for details')
        progress.events.$emit('done')
      } finally {
        isLoaded.value = true
      }
    })

    return {
      requests,
      isLoaded
    }
  }
}
</script>
