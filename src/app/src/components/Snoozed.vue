<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Snoozed Requests')
  div(v-if='loaded').mpj-snoozed-list
    p.mpj-text-center(v-if='requests.length === 0'): em.
      No snoozed requests found; return to #[router-link(:to='{ name: "Journal" } ') your journal]
    p.mpj-request-text(v-for='req in requests' :key='req.requestId')
      | {{ req.text }}
      br
      br
      button(@click='cancelSnooze(req.requestId)')
        md-icon(icon='restore')
        = ' Cancel Snooze'
      small.mpj-muted-text: em.
        &nbsp; Snooze expires #[date-from-now(:value='req.snoozedUntil')]
  p(v-else) Loading journal...
</template>

<script>
'use static'

import { mapState } from 'vuex'

import actions from '@/store/action-types'

export default {
  name: 'answered',
  data () {
    return {
      requests: [],
      loaded: false
    }
  },
  computed: {
    toast () {
      return this.$parent.$refs.toast
    },
    ...mapState(['journal', 'isLoadingJournal'])
  },
  methods: {
    async ensureJournal () {
      if (!Array.isArray(this.journal)) {
        this.loaded = false
        await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
      }
      this.requests = this.journal
        .filter(req => req.snoozedUntil > Date.now())
        .sort((a, b) => a.snoozedUntil - b.snoozedUntil)
      this.loaded = true
    },
    async cancelSnooze (requestId) {
      await this.$store.dispatch(actions.SNOOZE_REQUEST, {
        progress: this.$Progress,
        requestId: requestId,
        until: 0
      })
      this.toast.showToast('Request un-snoozed', { theme: 'success' })
      this.ensureJournal()
    }
  },
  async mounted () {
    await this.ensureJournal()
  }
}
</script>

<style>
.mpj-snoozed-list p {
  border-top: solid 1px lightgray;
}
.mpj-snoozed-list p:first-child {
  border-top: none;
}
</style>
