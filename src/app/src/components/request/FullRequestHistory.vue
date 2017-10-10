<template lang="pug">
b-list-group-item
  | {{ history.status }}
  |
  small.text-muted {{ asOf }}
  div(v-if='hasText').mpj-request-text {{ history.text }}
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
    hasText () {
      return this.history.text.length > 0
    }
  }
}
</script>
