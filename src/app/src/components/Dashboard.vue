<template lang="pug">
  article
    page-title(:title="title")
    p here you are!
</template>

<script>
import { mapState } from 'vuex'
import PageTitle from './PageTitle'

export default {
  name: 'dashboard',
  components: {
    PageTitle
  },
  computed: {
    title () {
      return `${this.user.given_name}'s Dashboard`
    },
    ...mapState(['user'])
  }
}
</script>
