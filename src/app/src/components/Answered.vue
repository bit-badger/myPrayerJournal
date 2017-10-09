<template lang="pug">
article
  page-title(title='Answered Requests')
  p(v-if='!loaded') Loading answered requests...
  div(v-if='loaded')
    p.mpj-request-text(v-for='req in requests' :key='req.requestId')
      b-btn(@click='showFull(req.requestId)'
            size='sm'
            variant='outline-secondary')
        icon(name='search')
        | &nbsp;View Full Request
      | &nbsp; &nbsp; {{ req.text }} &nbsp;
      small.text-muted: em.
        (Answered #[date-from-now(:value='req.asOf')])
  full-request(:events='eventBus')
</template>

<script>
'use static'

import Vue from 'vue'

import FullRequest from './request/FullRequest'

import api from '@/api'

export default {
  name: 'answered',
  components: {
    FullRequest
  },
  data () {
    return {
      eventBus: new Vue(),
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
  },
  methods: {
    showFull (requestId) {
      this.eventBus.$emit('full', requestId)
    }
  }
}
</script>
