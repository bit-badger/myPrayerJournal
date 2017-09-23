<template lang="pug">
  el-row.journal-request
    el-col(:span='4')
      p
        el-button(icon='check' @click='markPrayed()' title='Pray')
        el-button(icon='edit' @click='editRequest()' title='Edit')
        el-button(icon='document' @click='viewHistory()' title='Show History')
    el-col(:span='16'): p {{ request.text }}
    el-col(:span='4'): p {{ asOf }}
</template>

<script>
import moment from 'moment'

export default {
  name: 'request-list-item',
  props: ['request'],
  data () {
    return {}
  },
  methods: {
    markPrayed () {
      alert(`Marking ${this.request.requestId} prayed`)
      // TODO: write function
    }
  },
  computed: {
    asOf () {
      // FIXME: why isn't this already a number?
      return moment(parseInt(this.request.asOf)).fromNow()
    }
  }
}
</script>

<style>
.journal-request {
  border-bottom: dotted 1px lightgray;
}
</style>
