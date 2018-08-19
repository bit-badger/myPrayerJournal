<template lang="pug">
article.mpj-main-content(role='main')
  page-title(title='Full Prayer Request')
  template(v-if='request')
    p
      span(v-if='isAnswered') Answered {{ formatDate(answered) }} (#[date-from-now(:value='answered')]) &nbsp;
      small: em.mpj-muted-text prayed {{ prayedCount }} times, open {{ openDays }} days
    p.mpj-request-text {{ lastText }}
    br
    table.mpj-request-log
      thead
        tr
          th Action
          th Update / Notes
      tbody
        tr(v-for='item in log' :key='item.asOf')
          td {{ item.status }} on #[span.mpj-text-nowrap {{ formatDate(item.asOf) }}]
          td(v-if='item.text').mpj-request-text {{ item.text.fields[0] }}
          td(v-else) &nbsp;
  p(v-else) Loading request...
</template>

<script>
'use strict'

import moment from 'moment'

import api from '@/api'

const asOfDesc = (a, b) => b.asOf - a.asOf

export default {
  name: 'full-request',
  props: {
    id: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      request: null
    }
  },
  computed: {
    answered () {
      return this.request.history.find(hist => hist.status === 'Answered').asOf
    },
    isAnswered () {
      return this.request.history.filter(hist => hist.status === 'Answered').length > 0
    },
    lastText () {
      return this.request.history
        .filter(hist => hist.text)
        .sort(asOfDesc)[0].text.fields[0]
    },
    log () {
      const allHistory = (this.request.notes || [])
        .map(note => ({ asOf: note.asOf, text: { case: 'Some', fields: [ note.notes ] }, status: 'Notes' }))
        .concat(this.request.history)
        .sort(asOfDesc)
      // Skip the first entry for answered requests; that info is already displayed
      return this.isAnswered ? allHistory.slice(1) : allHistory
    },
    openDays () {
      const asOf = this.isAnswered ? this.answered : Date.now()
      return Math.floor(
        (asOf - this.request.history.find(hist => hist.status === 'Created').asOf) / 1000 / 60 / 60 / 24)
    },
    prayedCount () {
      return this.request.history.filter(hist => hist.status === 'Prayed').length
    }
  },
  async mounted () {
    this.$Progress.start()
    try {
      const req = await api.getFullRequest(this.id)
      this.request = req.data
      this.$Progress.finish()
    } catch (e) {
      console.log(e)
      this.$Progress.fail()
    }
  },
  methods: {
    formatDate (asOf) {
      return moment(asOf).format('LL')
    }
  }
}
</script>
