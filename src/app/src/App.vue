<template lang="pug">
#app
  navigation
  #content.container
    router-view
    vue-progress-bar
  footer
    p.text-right: i myPrayerJournal v0.8.1
</template>

<script>
'use strict'

import Navigation from './components/Navigation.vue'

export default {
  name: 'app',
  components: {
    Navigation
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
</style>
