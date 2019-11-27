<template lang="pug">
#app.page-container
  md-app(md-waterfall md-mode='fixed-last' role='application')
    md-app-toolbar.md-large.md-dense.md-primary
      .md-toolbar-row
        .md-toolbar-section-start
          router-link(to='/').md-title
            span(style='font-weight:100;') my
            span(style='font-weight:400;') Prayer
            span(style='font-weight:700;') Journal
      navigation
    md-app-content
      md-progress-bar(v-if='progress.visible.value'
                      :md-mode='progress.mode.value')
      router-view
      md-snackbar(:md-active.sync='snackbar.visible.value'
                  md-position='center'
                  :md-duration='snackbar.interval.value'
                  ref='snackbar') {{ snackbar.message.value }}
      footer
        p.mpj-muted-text.mpj-text-right
          | myPrayerJournal v{{ version }}
          br
          em: small.
            #[router-link(to='/legal/privacy-policy') Privacy Policy] &bull;
            #[router-link(to='/legal/terms-of-service') Terms of Service] &bull;
            #[a(href='https://github.com/bit-badger/myprayerjournal' target='_blank') Developed] and hosted by
            #[a(href='https://bitbadger.solutions' target='_blank') Bit Badger Solutions]
</template>

<script lang="ts">
import Vue from 'vue'
import { computed, createComponent, inject, onMounted, provide, ref } from '@vue/composition-api'

import Navigation from '@/components/common/Navigation.vue'

import auth from './auth'
import router from './router'
import store from './store'
import { Actions } from './store/types'
import { SnackbarProps, ProgressProps } from './types' // eslint-disable-line no-unused-vars

import { provideAuth } from './plugins/auth'
import { provideRouter } from './plugins/router'
import { provideStore } from './plugins/store'

function setupSnackbar (): SnackbarProps {
  const events = new Vue()
  const visible = ref(false)
  const message = ref('')
  const interval = ref(4000)

  const showSnackbar = (msg: string) => {
    message.value = msg
    visible.value = true
  }

  const showInfo = (msg: string) => {
    interval.value = 4000
    showSnackbar(msg)
  }

  const showError = (msg: string) => {
    interval.value = Infinity
    showSnackbar(msg)
  }

  onMounted(() => {
    events.$on('info', showInfo)
    events.$on('error', showError)
  })

  return {
    events,
    visible,
    message,
    interval,
    showSnackbar,
    showInfo,
    showError
  }
}

function setupProgress (): ProgressProps {
  const events = new Vue()
  const visible = ref(false)
  const mode = ref('query')

  const showProgress = (mod: string) => {
    mode.value = mod
    visible.value = true
  }

  const hideProgress = () => { visible.value = false }

  onMounted(() => {
    events.$on('show', showProgress)
    events.$on('done', hideProgress)
  })

  return {
    events,
    visible,
    mode,
    showProgress,
    hideProgress
  }
}

const SnackbarSymbol = Symbol('Snackbar events')
const ProgressSymbol = Symbol('Progress events')

export default createComponent({
  name: 'app',
  components: {
    Navigation
  },
  setup () {
    const pkg = require('../package.json')

    provideAuth(auth)
    provideRouter(router)
    provideStore(store)

    const version = computed(() =>
      pkg.version.endsWith('.0')
        ? pkg.version.endsWith('.0.0')
          ? pkg.version.substr(0, pkg.version.length - 4)
          : pkg.version.substr(0, pkg.version.length - 2)
        : pkg.version)

    const progress = setupProgress()
    const snackbar = setupSnackbar()

    onMounted(async () => store.dispatch(Actions.CheckAuthentication))

    provide(SnackbarSymbol, snackbar.events)
    provide(ProgressSymbol, progress.events)

    return {
      version,
      progress,
      snackbar
    }
  }
})

export function useSnackbar () {
  const snackbar = inject(SnackbarSymbol)
  if (!snackbar) {
    throw new Error('Snackbar not configured')
  }
  return snackbar as SnackbarProps
}

export function useProgress () {
  const progress = inject(ProgressSymbol)
  if (!progress) {
    throw new Error('Progress not configured')
  }
  return progress as ProgressProps
}
</script>

<style lang="sass">
@import "~vue-material/dist/theme/engine"
@include md-register-theme("default", (primary: md-get-palette-color(green, 800), accent: md-get-palette-color(gray, 700)))
@import "~vue-material/dist/theme/all"

html, body
  font-family: -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Oxygen-Sans,Ubuntu,Cantarell,"Helvetica Neue",sans-serif;
  font-size: 1rem
p
  margin-bottom: 0
footer
  border-top: solid 1px lightgray
  margin: 1rem -1rem 0
  padding: 0 1rem
footer p
  margin: 0
.mpj-full-page-card
  font-size: 1rem
  line-height: 1.25rem
.mpj-main-content
  max-width: 60rem
  margin: auto
.mpj-request-text
  white-space: pre-line
p.mpj-request-text
  margin-top: 0
.mpj-text-center
  text-align: center
.mpj-text-nowrap
  white-space: nowrap
.mpj-text-right
  text-align: right
.mpj-muted-text
  color: rgba(0, 0, 0, .6)
.mpj-valign-top
  vertical-align: top
.mpj-narrow
  max-width: 40rem
  margin: auto
.mpj-skinny
  max-width: 20rem
  margin: auto
.mpj-full-width
  width: 100%
.md-progress-bar
  margin: 24px
</style>
