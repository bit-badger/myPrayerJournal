<template lang="pug">
#app
  navigation
  #content.container
    router-view
    vue-progress-bar
    toast(ref='toast')
  footer
    p.text-right: i myPrayerJournal v0.8.2
</template>

<script>
'use strict'

import Navigation from './components/Navigation.vue'

export default {
  name: 'app',
  components: {
    Navigation
  },
  mounted () {
    this.$refs.toast.setOptions({ position: 'bottom right' })
  },
  computed: {
    toast () {
      return this.$refs.toast
    }
  }
}
</script>

<style>
body {
  padding-top: 60px;
}
footer {
  border-top: solid 1px lightgray;
  margin-top: 1rem;
  padding: 0 1rem;
}
footer p {
  margin: 0;
}
.mpj-page-title {
  border-bottom: solid 1px lightgray;
  margin-bottom: 20px;
}
.mpj-request-text {
  white-space: pre-line;
}
</style>
