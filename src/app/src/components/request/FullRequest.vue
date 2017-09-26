<template lang="pug">
span
  el-button(icon='view' @click='openDialog()' title='Show History')
  el-dialog(title='Prayer Request History' :visible.sync='historyVisible')
    span(v-if='null !== full')
      full-request-history(v-for='item in full.history' :history='item' :key='item.asOf')
    span.dialog-footer(slot='footer')
      el-button(type='primary' @click='closeDialog()') Close
</template>

<script>
'use strict'

import FullRequestHistory from './FullRequestHistory'

import api from '@/api'

export default {
  name: 'full-request',
  props: [ 'request' ],
  data () {
    return {
      historyVisible: false,
      full: null
    }
  },
  components: {
    FullRequestHistory
  },
  methods: {
    closeDialog () {
      this.full = null
      this.historyVisible = false
    },
    async openDialog () {
      this.historyVisible = true
      this.$Progress.start()
      const req = await api.getFullRequest(this.request.requestId)
      this.full = req.data
      this.$Progress.finish()
    }
  }
}
</script>