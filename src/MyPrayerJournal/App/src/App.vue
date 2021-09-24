<template lang="pug">
.mpj-app
  navigation-bar
  router-view
</template>

<script setup lang="ts">
import "bootstrap/dist/css/bootstrap.min.css"

import NavigationBar from "@/components/NavigationBar.vue"

</script>

<style lang="sass">
#app
  font-family: Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
  color: #2c3e50
#nav
  padding: 30px
  a
    font-weight: bold
    color: #2c3e50
    &.router-link-exact-active
      color: #42b983
</style>
