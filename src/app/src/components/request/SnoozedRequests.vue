<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Snoozed Requests')
  div(v-if='loaded').mpj-request-list
    p.mpj-text-center(v-if='requests.length === 0'): em.
      No snoozed requests found; return to #[router-link(:to='{ name: "Journal" } ') your journal]
    request-list-item(v-for='req in requests'
                      :key='req.Id'
                      :request='req'
                      :toast='toast')
  p(v-else) Loading journal...
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import actions from '@/store/action-types'

import RequestListItem from '@/components/request/RequestListItem'

export default {
  name: 'snoozed-requests',
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
    toast () {
      return this.$parent.$refs.toast
    },
    ...mapState(['journal', 'isLoadingJournal'])
  },
  created () {
    this.$on('requestUnsnoozed', this.ensureJournal)
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
    }
  },
  async mounted () {
    await this.ensureJournal()
  }
}
</script>
