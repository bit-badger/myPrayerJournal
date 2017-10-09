<template lang="pug">
span
  b-modal(v-model='historyVisible'
          header-bg-variant='mpj'
          header-text-variant='light'
          size='lg'
          title='Prayer Request History'
          @shows='focusRequestText')
    b-list-group(v-if='null !== full'
                 flush)
      full-request-history(v-for='item in full.history'
                           :key='item.asOf'
                           :history='item')
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary'
            @click='closeDialog()') Close
</template>

<script>
'use strict'

import FullRequestHistory from './FullRequestHistory'

import api from '@/api'

export default {
  name: 'full-request',
  components: {
    FullRequestHistory
  },
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
