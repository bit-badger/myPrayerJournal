<template lang="pug">
md-table(md-card)
  md-table-toolbar
    h1.md-title {{ title }}
  md-table-row
    md-table-head Actions
    md-table-head Request
  request-list-item(v-for='req in requests'
                    :key='req.requestId'
                    :request='req')

</template>

<script>
'use strict'

import RequestListItem from '@/components/request/RequestListItem'

export default {
  name: 'request-list',
  components: { RequestListItem },
  props: {
    title: {
      type: String,
      required: true
    },
    requests: {
      type: Array,
      required: true
    }
  },
  data () {
    return { }
  },
  created () {
    this.$on('requestUnsnoozed', this.$parent.$emit('requestUnsnoozed'))
    this.$on('requestNowShown', this.$parent.$emit('requestNowShown'))
  }
}
</script>
