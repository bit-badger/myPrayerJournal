<template lang="pug">
  el-row.journal-request(:id="request.requestId")
    el-col(:span='4') Action buttons
    el-col(:span='20')
      p Id {{ request.requestId }} as of {{ request.asOf }}
      p {{ request.text }}
</template>

<script>
export default {
  name: 'request-list-item',
  props: ['request'],
  data: function () {
    return {}
  },
  methods: {
    markPrayed: function (requestId) {
      // TODO: write function
    }
  }
}
</script>

<style>
.journal-request {
  border-bottom: dotted 1px lightgray;
}
</style>
