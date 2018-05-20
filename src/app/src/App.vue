<template lang="pug">
#app
  navigation
  #content.container
    router-view
    vue-progress-bar
    toast(ref='toast')
  footer
    p.text-right.text-muted
      | myPrayerJournal v{{ version }}
      br
      em: small.
        #[router-link(:to="{ name: 'PrivacyPolicy' }") Privacy Policy] &bull;
        #[router-link(:to="{ name: 'TermsOfService' }") Terms of Service] &bull;
        #[a(href='https://github.com/danieljsummers/myprayerjournal') Developed] and hosted by
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
a:link, a:visited {
  color: #050;
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
