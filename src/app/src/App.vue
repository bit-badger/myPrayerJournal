<template lang="pug">
#app
  navigation
  #content.container
    router-view
    vue-progress-bar
  footer
    p.text-right: i myPrayerJournal v0.8.0
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
@import url('../node_modules/element-ui/lib/theme-default/index.css');
body {
  font-family: -apple-system,system-ui,BlinkMacSystemFont,"Segoe UI","Roboto","Helvetica Neue", Arial, sans-serif;
  padding-top: 60px;
  margin: 0;
}
#content {
  padding: 0 10px;
}
footer {
  border-top: solid 1px lightgray;
  margin-top: 1rem;
  padding: 0 1rem;

}
footer p {
  margin: 0;
}
.text-right {
  text-align: right;
}
.material-icons.md-18 {
  font-size: 18px;
}
.material-icons.md-24 {
  font-size: 24px;
}
.material-icons.md-36 {
  font-size: 36px;
}
.material-icons.md-48 {
  font-size: 48px;
}
.material-icons {
  vertical-align: middle;
}
.mpj-page-title {
  border-bottom: solid 1px lightgray;
  margin-bottom: 20px;
}
</style>
