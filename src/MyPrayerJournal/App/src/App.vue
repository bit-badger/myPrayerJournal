<template lang="pug">
.mpj-app
  #nav
    router-link(to="/") Home
    | |
    router-link(to="/about") About
  router-view
</template>

<script setup lang="ts">
import "bootstrap/dist/css/bootstrap.min.css"
</script>

<style lang="sass">
#app
  font-family: Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
  text-align: center
  color: #2c3e50
#nav
  padding: 30px
  a
    font-weight: bold
    color: #2c3e50
    &.router-link-exact-active
      color: #42b983
</style>
