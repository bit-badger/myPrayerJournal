<template lang="pug">
#app
  navigation
  #content.container
    router-view
    vue-progress-bar
    toast(ref='toast')
  footer
    p.text-right: i myPrayerJournal v0.8.4
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
html, body {
  background-color: whitesmoke;
}
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
.mpj-request-text {
  white-space: pre-line;
}
.bg-mpj {
  background-image: -webkit-gradient(linear, left top, left bottom, from(#050), to(whitesmoke));
  background-image: -webkit-linear-gradient(top, #050, whitesmoke);
  background-image: -moz-linear-gradient(top, #050, whitesmoke);
  background-image: linear-gradient(to bottom, #050, whitesmoke);
}
</style>
