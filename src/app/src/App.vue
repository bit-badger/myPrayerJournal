<template>
  <div id="app">
    <navigation :auth="auth" />
    <div id="content" class="container">
      <router-view :auth="auth"></router-view>
    </div>
    <footer>
      <p class="text-right"><i>myPrayerJournal v0.8.0</i></p>
    </footer>
  </div>
</template>

<script>
import AuthService from './auth/AuthService'
import Navigation from './components/Navigation.vue'

const auth = new AuthService()
const { login, logout, authenticated, authNotifier } = auth

export default {
  name: 'app',
  data: function () {
    authNotifier.on('authChange', authState => {
      this.authenticated = authState.authenticated
    })
    return {
      auth,
      authenticated
    }
  },
  methods: {
    login,
    logout
  },
  components: {
    Navigation
  }
}
</script>

<style>
@import url('../node_modules/bootstrap/dist/css/bootstrap.css');
@import url('../node_modules/bootstrap-vue/dist/bootstrap-vue.css');

body {
  padding-top: 60px;
}
footer {
  border-top: solid 1px lightgray;
  margin-top: 1rem;
  padding: 0 1rem;

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
