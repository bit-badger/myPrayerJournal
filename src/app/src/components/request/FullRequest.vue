<template lang="pug">
span
  b-modal(title='Prayer Request History'
          v-model='historyVisible'
          size='lg'
          header-bg-variant='dark'
          header-text-variant='light'
          @shows='focusRequestText')
    b-list-group(v-if='null !== full' flush)
      full-request-history(v-for='item in full.history' :history='item' :key='item.asOf')
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary' @click='closeDialog()') Close
</template>

<script>
'use strict'

import FullRequestHistory from './FullRequestHistory'

import api from '@/api'

export default {
  name: 'full-request',
  props: {
    events: { required: true }
  },
  data () {
    return {
      historyVisible: false,
      full: null
    }
  },
  created () {
    this.events.$on('full', this.openDialog)
  },
  components: {
    FullRequestHistory
  },
  methods: {
    closeDialog () {
      this.full = null
      this.historyVisible = false
    },
    async openDialog (requestId) {
      this.historyVisible = true
      this.$Progress.start()
      const req = await api.getFullRequest(requestId)
      this.full = req.data
      this.$Progress.finish()
    }
  }
}
</script>