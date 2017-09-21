<template lang="pug">
  div
    p Id {{ request.requestId }} as of {{ request.asOf }}
    p {{ request.text }}
</template>

<script>
export default {
  name: 'request-list-item',
  props: ['request'],
  data: function () {
    return {}
  }
}
</script>
