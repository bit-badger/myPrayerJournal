<template lang="pug">
b-list-group-item
  | {{ history.status }}
  |
  small.text-muted(:title='actualDate') {{ asOf }}
  div(v-if='history.text').mpj-request-text {{ history.text.fields[0] }}
</template>

<script>
'use strict'

import moment from 'moment'

export default {
  name: 'full-request-history',
  props: {
    history: { required: true }
  },
  computed: {
    asOf () {
      return moment(this.history.asOf).fromNow()
    },
    actualDate () {
      return moment(this.history.asOf).format('LLLL')
    }
  }
}
</script>
