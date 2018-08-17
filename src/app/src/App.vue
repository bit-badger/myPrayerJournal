<template lang="pug">
#app(role='application')
  navigation
  #content
    router-view
    vue-progress-bar
    toast(ref='toast')
  footer.mpj-text-right.mpj-muted-text
    p
      | myPrayerJournal v{{ version }}
      br
      em: small.
        #[router-link(:to="{ name: 'PrivacyPolicy' }") Privacy Policy] &bull;
        #[router-link(:to="{ name: 'TermsOfService' }") Terms of Service] &bull;
        #[a(href='https://github.com/bit-badger/myprayerjournal') Developed] and hosted by
        #[a(href='https://bitbadger.solutions') Bit Badger Solutions]
</template>

<script>
'use strict'

import Navigation from './components/Navigation.vue'

import { version } from '../package.json'

export default {
  name: 'app',
  components: {
    Navigation
  },
  data () {
    return { version }
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
  font-family: -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Oxygen-Sans,Ubuntu,Cantarell,"Helvetica Neue",sans-serif;
  font-size: 1rem;
}
body {
  padding-top: 50px;
  margin: 0;
}
p {
  margin-bottom: 0;
}
button,
a[role="button"] {
  border: solid 1px #050;
  border-radius: .5rem;
  background-color: whitesmoke;
}
button:hover,
a[role="button"]:hover {
  background-color: #050;
  color: white;
  text-decoration: none;
}
footer {
  border-top: solid 1px lightgray;
  margin-top: 1rem;
  padding: 0 1rem;
}
footer p {
  margin: 0;
}
a:link, a:visited {
  color: #050;
  text-decoration: none;
}
a:hover {
  text-decoration: underline;
}
.mpj-main-content {
  max-width: 60rem;
  margin: auto;
}
.mpj-main-content-wide {
  margin: .5rem;
}
@media screen and (max-width: 21rem) {
  .mpj-main-content-wide {
    margin: 0;
  }
}
.mpj-request-text {
  white-space: pre-line;
}
.mpj-request-log {
  width: 100%;
}
.mpj-request-log thead th {
  border-top: solid 1px lightgray;
  border-bottom: solid 2px lightgray;
  text-align: left;
}
.mpj-request-log tbody td {
  border-bottom: dotted 1px lightgray;
  vertical-align: top;
}
.mpj-bg {
  background-image: -webkit-gradient(linear, left top, left bottom, from(#050), to(whitesmoke));
  background-image: -webkit-linear-gradient(top, #050, whitesmoke);
  background-image: -moz-linear-gradient(top, #050, whitesmoke);
  background-image: linear-gradient(to bottom, #050, whitesmoke);
}
.mpj-text-center {
  text-align: center;
}
.mpj-text-nowrap {
  white-space: nowrap;
}
.mpj-text-right {
  text-align: right;
  color: rgba(0, 0, 0, .60);
}
.mpj-muted-text {
  color: rgba(0, 0, 0, .60);
}
.mpj-margin {
  margin-left: 1rem;
  margin-right: 1rem;
}
</style>
