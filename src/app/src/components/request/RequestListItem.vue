<template lang="pug">
  el-row.journal-request
    el-col(:span='4')
      p
        el-button(icon='check' @click='markPrayed()' title='Pray')
        edit-request(:request='request')
        el-button(icon='document' @click='viewHistory()' title='Show History')
    el-col(:span='16'): p {{ request.text }}
    el-col(:span='4'): p {{ asOf }}
</template>

<script>
'use strict'

import moment from 'moment'

import EditRequest from './EditRequest'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: [ 'request' ],
  data () {
    return {}
  },
  components: {
    EditRequest
  },
  methods: {
    markPrayed () {
      this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        status: 'Prayed',
        updateText: ''
      })
    }
  },
  computed: {
    asOf () {
      return moment(this.request.asOf).fromNow()
    }
  }
}
</script>

<style>
.journal-request {
  border-bottom: dotted 1px lightgray;
}
</style>
