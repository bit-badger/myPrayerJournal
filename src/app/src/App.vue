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
        #[a(href='https://github.com/bit-badger/myprayerjournal' target='_blank') Developed] and hosted by
        #[a(href='https://bitbadger.solutions' target='_blank') Bit Badger Solutions]
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
h1, h2, h3, h4, h5 {
  font-weight: 500;
  margin-top: 0;
}
h1 {
  font-size: 2.5rem;
}
h2 {
  font-size: 2rem;
}
h3 {
  font-size: 1.75rem;
}
h4 {
  font-size: 1.5rem;
}
h5 {
  font-size: 1.25rem;
}
p {
  margin-bottom: 0;
}
input, textarea {
  border-radius: .25rem;
}
button,
a[role="button"] {
  border: solid 1px #050;
  border-radius: .5rem;
  background-color: rgb(235, 235, 235);
  padding: .25rem;
  font-size: 1rem;
}
a[role="button"]:link,
a[role="button"]:visited {
  color: black;
}
button.primary,
a[role="button"].primary {
  background-color: white;
  border-width: 3px;
}
button:hover,
a[role="button"]:hover {
  cursor: pointer;
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
.mpj-request-list p {
  border-top: solid 1px lightgray;
}
.mpj-request-list p:first-child {
  border-top: none;
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
}
.mpj-muted-text {
  color: rgba(0, 0, 0, .6);
}
.mpj-narrow {
  max-width: 40rem;
  margin: auto;
}
.mpj-skinny {
  max-width: 20rem;
  margin: auto;
}
.mpj-modal {
  position: fixed;
  z-index: 8;
  left: 0;
  top: 0;
  width: 100%;
  height: 100%;
  overflow: auto;
  background-color: rgba(0, 0, 0, .4);
}
.mpj-modal-content {
  background-color: whitesmoke;
  border: solid 1px #050;
  border-radius: .5rem;
  animation-name: animatetop;
  animation-duration: 0.4s;
  padding: 1rem;
  margin-top: 4rem;
}
@keyframes animatetop {
  from {
    top: -300px;
    opacity: 0;
  }
  to {
    top: 0;
    opacity: 1;
  }
}
.mpj-modal-content header {
  margin: -1rem -1rem .5rem;
  border-radius: .4rem;
}
.mpj-modal-content header h5 {
  color: white;
  margin: 0;
  padding: 1rem;
}
.mpj-margin {
  margin-left: 1rem;
  margin-right: 1rem;
}
.material-icons {
  vertical-align: middle;
}
</style>
