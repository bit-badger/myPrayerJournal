<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Active Requests')
  div(v-if='loaded').mpj-request-list
    p.mpj-text-center(v-if='requests.length === 0'): em.
      No active requests found; return to #[router-link(:to='{ name: "Journal" } ') your journal]
    request-list-item(v-for='req in requests'
                      :key='req.requestId'
                      :request='req')
  p(v-else) Loading journal...
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import RequestListItem from '@/components/request/RequestListItem'

import actions from '@/store/action-types'

export default {
  name: 'active-requests',
  inject: ['progress'],
  components: {
    RequestListItem
  },
  data () {
    return {
      requests: [],
      loaded: false
    }
  },
  computed: {
    ...mapState(['journal', 'isLoadingJournal'])
  },
  created () {
    this.$on('requestUnsnoozed', this.ensureJournal)
    this.$on('requestNowShown', this.ensureJournal)
  },
  methods: {
    async ensureJournal () {
      if (!Array.isArray(this.journal)) {
        this.loaded = false
        await this.$store.dispatch(actions.LOAD_JOURNAL, this.progress)
      }
      this.requests = this.journal
        .sort((a, b) => a.showAfter - b.showAfter)
      this.loaded = true
    }
  },
  async mounted () {
    await this.ensureJournal()
  }
}
</script>
