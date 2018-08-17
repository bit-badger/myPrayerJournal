<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Answered Requests')
  div(v-if='loaded').mpj-answered-list
    p.text-center(v-if='requests.length === 0'): em.
      No answered requests found; once you have marked one as &ldquo;Answered&rdquo;, it will appear here
    p.mpj-request-text(v-for='req in requests' :key='req.requestId')
      | {{ req.text }}
      br
      br
      router-link(:to='{ name: "AnsweredDetail", params: { id: req.requestId }}'
                  role='button'
                  title='View Full Request')
        md-icon(icon='description')
        = ' View Full Request'
      small.mpj-muted-text: em.
        &nbsp; Answered #[date-from-now(:value='req.asOf')]
  p(v-else) Loading answered requests...
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
