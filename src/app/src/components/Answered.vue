<template lang="pug">
article
  page-title(title='Answered Requests')
  p(v-if='!loaded') Loading answered requests...
  div(v-if='loaded')
    p(v-for='req in requests')
      b-btn(@click='showFull(req.requestId)' size='sm' variant='outline-secondary')
        icon(name='search')
        | &nbsp;View Full Request
      | &nbsp; &nbsp; {{ req.text }}
  full-request(:events='eventBus')
</template>

<script>
'use static'

import Vue from 'vue'

import FullRequest from './request/FullRequest'

import api from '@/api'

export default {
  name: 'answered',
  data () {
    return {
      eventBus: new Vue(),
      requests: [],
      loaded: false
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
  },
  components: {
    FullRequest
  },
  computed: {
    toast () {
      return this.$parent.$refs.toast
    }
  },
  methods: {
    showFull (requestId) {
      this.eventBus.$emit('full', requestId)
    }
  }
}
</script>
