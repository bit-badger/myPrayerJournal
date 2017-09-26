<template lang="pug">
el-row.journal-request
  el-col(:span='4'): p
    el-button(icon='check' @click='markPrayed()' title='Pray')
    edit-request(:request='request')
    full-request(:request='request')
  el-col(:span='16'): p {{ text }}
  el-col(:span='4'): p: date-from-now(:value='request.asOf')
</template>

<script>
'use strict'

import moment from 'moment'

import DateFromNow from '../common/DateFromNow'
import EditRequest from './EditRequest'
import FullRequest from './FullRequest'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: [ 'request' ],
  data () {
    return { interval: null }
  },
  components: {
    DateFromNow,
    EditRequest,
    FullRequest
  },
  methods: {
    async markPrayed () {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        status: 'Prayed',
        updateText: ''
      })
      this.$message({
        message: 'Request marked as prayed',
        type: 'success'
      })
    }
  },
  computed: {
    asOf () {
      return moment(this.request.asOf).fromNow()
    },
    text () {
      return this.request.text.split('\n').join('<br>')
    }
  }
}
</script>

<style>
.journal-request {
  border-bottom: dotted 1px lightgray;
}
</style>
