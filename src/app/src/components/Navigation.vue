<template>
  <b-navbar toggleable type="inverse" variant="inverse" fixed="top">
    <b-nav-toggle target="navCollapse"></b-nav-toggle>
    <b-link class="navbar-brand" :to="{ name: 'Home' }">
      <span style="font-weight:100;">my</span><span style="font-weight:600;">Prayer</span><span style="font-weight:700;">Journal</span>
    </b-link>
    <b-collapse is-nav id="navCollapse">
      <b-nav is-nav-bar>
        <b-nav-item v-if="auth.authenticated" :to="{ name: 'Dashboard' }">Dashboard</b-nav-item>
        <b-nav-item v-if="auth.authenticated" @click="auth.logout()">Log Off</b-nav-item>
        <b-nav-item v-if="!auth.authenticated" @click="auth.login()">Log On</b-nav-item>
      </b-nav>
    </b-collapse>
  </b-navbar>
</template>

<script>
export default {
  name: 'navigation',
  props: ['auth'],
  data: function () {
    return { }
  }
}
</script>
