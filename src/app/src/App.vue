<template lang="pug">
#app
  md-app(md-waterfall md-mode='fixed-last' role='application')
    md-app-toolbar.md-large.md-dense.md-primary
      .md-toolbar-row
        .md-toolbar-section-start
          span.md-title
            span(style='font-weight:100;') my
            span(style='font-weight:400;') Prayer
            span(style='font-weight:700;') Journal
      navigation
    md-app-content
      md-progress-bar(v-if='progress.visible'
                      :md-mode='progress.mode')
      router-view
      md-snackbar(:md-active.sync='snackbar.visible'
                  md-position='center'
                  :md-duration='snackbar.interval'
                  ref='snackbar')
        | {{ snackbar.message }}
      p.mpj-muted-text.mpj-text-right
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

import Vue from 'vue'

import Navigation from '@/components/common/Navigation'

import { version } from '../package.json'

export default {
  name: 'app',
  components: {
    Navigation
  },
  data () {
    return {
      progress: {
        events: new Vue(),
        visible: false,
        mode: 'query'
      },
      snackbar: {
        events: new Vue(),
        visible: false,
        message: '',
        interval: 4000
      }
    }
  },
  mounted () {
    this.progress.events.$on('show', this.showProgress)
    this.progress.events.$on('done', this.hideProgress)
    this.snackbar.events.$on('info', this.showInfo)
    this.snackbar.events.$on('error', this.showError)
  },
  computed: {
    version () {
      return version.endsWith('.0')
        ? version.endsWith('.0.0')
          ? version.substr(0, version.length - 4)
          : version.substr(0, version.length - 2)
        : version
    }
  },
  methods: {
    showSnackbar (message) {
      this.snackbar.message = message
      this.snackbar.visible = true
    },
    showInfo (message) {
      this.snackbar.interval = 4000
      this.showSnackbar(message)
    },
    showError (message) {
      this.snackbar.interval = Infinity
      this.showSnackbar(message)
    },
    showProgress (mode) {
      this.progress.mode = mode
      this.progress.visible = true
    },
    hideProgress () {
      this.progress.visible = false
    }
  },
  provide () {
    return {
      messages: this.snackbar.events,
      progress: this.progress.events
    }
  }
}
</script>

<style>
html, body {
/*  background-color: whitesmoke; */
  font-family: -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Oxygen-Sans,Ubuntu,Cantarell,"Helvetica Neue",sans-serif;
  font-size: 1rem;
} /*
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
} */
p {
  margin-bottom: 0;
} /*
input, textarea, select {
  border-radius: .25rem;
  font-size: 1rem;
}
textarea {
  font-family: "SFMono-Regular",Consolas,"Liberation Mono",Menlo,Courier,monospace;
}
input, select {
  font-family: inherit;
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
label {
  font-variant: small-caps;
  font-size: 1.1rem;
}
label.normal {
  font-variant: unset;
  font-size: unset;
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
} */
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
.mpj-full-width {
  width: 100%;
} /*
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
} */
.md-progress-bar {
  margin: 24px;
}
</style>
