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
  components: {
    RequestListItem
  },
  data () {
    return {
      requests: [],
      loaded: false
    }
  },
  computed: {
    toast () {
      return this.$parent.$refs.toast
    }
  },
  async mounted () {
    this.$Progress.start()
    try {
      const reqs = await api.getAnsweredRequests()
      this.requests = reqs.data
      this.$Progress.finish()
    } catch (err) {
      console.error(err)
      this.toast.showToast('Error loading requests; check console for details', { theme: 'danger' })
      this.$Progress.fail()
    } finally {
      this.loaded = true
    }
  }
}
</script>
