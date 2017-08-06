<template lang="pug">
  el-menu(theme="dark" mode="horizontal" class="mpj-top-nav" router=true)
    el-menu-item(index="/")
      span(style="font-weight:100;") my
      span(style="font-weight:600;") Prayer
      span(style="font-weight:700;") Journal
    el-menu-item(v-if="isAuthenticated" index="/dashboard") Dashboard
    el-menu-item(v-if="isAuthenticated" index="3"): a(@click.stop="logOff()") Log Off
    el-menu-item(v-if="!isAuthenticated" index="4"): a(@click.stop="logOn()") Log On
</template>

<script>
import { mapState } from 'vuex'
import AuthService from '@/auth/AuthService'

export default {
  name: 'navigation',
  data () {
    return {
      auth0: new AuthService()
    }
  },
  methods: {
    logOn () {
      this.auth0.login()
    },
    logOff () {
      this.auth0.logout(this.$store, this.$router)
    }
  },
  computed: mapState(['isAuthenticated'])
}
</script>

<style scoped>
.mpj-top-nav {
  position: fixed;
  top: 0px;
  width: 100%;
}
</style>
