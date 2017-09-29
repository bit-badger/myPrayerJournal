<template lang="pug">
article
  p(v-if='!loaded') Loading answered requests...
  div(v-if='loaded')
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
  async mounted () {
    this.$Progress.start()
    try {
      const reqs = await api.getAnsweredRequests()
      this.requests = reqs.data
      this.$Progress.finish()
    } catch (err) {
      console.error(err)
      this.$message({
        message: 'Error loading requests; check console for details',
        type: 'error'
      })
      this.$Progress.fail()
    } finally {
      this.loaded = true
    }
  }  
}
</script>
