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

<script lang="ts">
import RequestListItem from './RequestListItem.vue'

export default {
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
  setup (props, { parent }) {
    this.$on('requestUnsnoozed', parent.$emit('requestUnsnoozed'))
    this.$on('requestNowShown', parent.$emit('requestNowShown'))
    return {
      title: props.title,
      requests: props.requests
    }
  },
}
</script>
