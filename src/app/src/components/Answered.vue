<template lang="pug">
article
  page-title(title='Answered Requests')
  p(v-if='!loaded') Loading answered requests...
  div(v-if='loaded').mpj-answered-list
    p.mpj-request-text(v-for='req in requests' :key='req.requestId')
      | {{ req.text }}
      br
      br
      b-btn(:to='{ name: "AnsweredDetail", params: { id: req.requestId }}'
            size='sm'
            variant='outline-secondary')
        icon(name='search')
        = ' View Full Request'
      small.text-muted: em.
        &nbsp; Answered #[date-from-now(:value='req.asOf')]
</template>

<script>
'use static'

import api from '@/api'

export default {
  name: 'answered',
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

<style>
.mpj-answered-list p {
  border-top: solid 1px lightgray;
}
.mpj-answered-list p:first-child {
  border-top: none;
}
</style>
