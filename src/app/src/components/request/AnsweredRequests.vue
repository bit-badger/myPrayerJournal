<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Answered Requests')
  div(v-if='loaded').mpj-request-list
    p.text-center(v-if='requests.length === 0'): em.
      No answered requests found; once you have marked one as &ldquo;Answered&rdquo;, it will appear here
    request-list-item(v-for='req in requests'
                      :key='req.requestId'
                      :request='req')
  p(v-else) Loading answered requests...
</template>

<script>
'use strict'

import api from '@/api'

import RequestListItem from '@/components/request/RequestListItem'

export default {
  name: 'answered-requests',
  inject: [
    'messages',
    'progress'
  ],
  components: {
    RequestListItem
  },
  data () {
    return {
      requests: [],
      loaded: false
    }
  },
  async mounted () {
    this.progress.$emit('show', 'query')
    try {
      const reqs = await api.getAnsweredRequests()
      this.requests = reqs.data
      this.progress.$emit('done')
    } catch (err) {
      console.error(err)
      this.messages.$emit('error', 'Error loading requests; check console for details')
      this.progress.$emit('done')
    } finally {
      this.loaded = true
    }
  }
}
</script>
