<template lang="pug">
b-list-group-item
  | {{ history.status }} {{ asOf }}
  span(v-if='0 < history.text.length') &nbsp;&raquo; {{ history.text }}
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
    }
  }
}
</script>
