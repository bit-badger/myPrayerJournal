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
import { createComponent, onMounted } from '@vue/composition-api'

import RequestListItem from './RequestListItem.vue'

import { JournalRequest } from '../../store/types' // eslint-disable-line no-unused-vars

export default createComponent({
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
    // TODO: custom events; does this work?
    onMounted(function () {
      this.$on('requestUnsnoozed', parent.$emit('requestUnsnoozed'))
      this.$on('requestNowShown', parent.$emit('requestNowShown'))
    })
    return {
      title: props.title,
      requests: props.requests as JournalRequest[]
    }
  }
})
</script>
