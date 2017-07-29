<template>
  <article>
    <page-title title="Your Dashboard" />
    <p>here you are! {{ JSON.stringify(this.user) }}</p>
  </article>
</template>

<script>
import PageTitle from './PageTitle'

export default {
  name: 'dashboard',
  props: ['user'],
  data () {
    return {}
  },
  components: {
    PageTitle
  }
}
</script>
