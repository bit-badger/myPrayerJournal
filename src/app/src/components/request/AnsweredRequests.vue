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

<script>
'use strict'

import api from '@/api'

import RequestList from '@/components/request/RequestList'

export default {
  name: 'answered-requests',
  inject: [
    'messages',
    'progress'
  ],
  components: {
    RequestList
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
