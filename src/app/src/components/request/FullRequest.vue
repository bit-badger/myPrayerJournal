<template lang="pug">
md-content(role='main').mpj-main-content
  page-title(title='Full Prayer Request'
             hide-on-page=true)
  md-card(v-if='request')
    md-card-header
      .md-title Full Prayer Request
      .md-subhead
        span(v-if='isAnswered') Answered {{ formatDate(answered) }} (#[date-from-now(:value='answered')]) !{' &bull; '}
        | Prayed {{ prayedCount }} times &bull; Open {{ openDays }} days
    md-card-content.mpj-full-page-card
      p.mpj-request-text {{ lastText }}
      md-table
        md-table-row
          md-table-head Action
          md-table-head Update / Notes
        md-table-row(v-for='item in log'
                     :key='item.asOf')
          md-table-cell.mpj-valign-top {{ item.status }} on #[span.mpj-text-nowrap {{ formatDate(item.asOf) }}]
          md-table-cell(v-if='item.text').mpj-request-text.mpj-valign-top {{ item.text }}
          md-table-cell(v-else) &nbsp;
  p(v-else) Loading request...
</template>

<script>
'use strict'

import moment from 'moment'

import api from '@/api'

const asOfDesc = (a, b) => b.asOf - a.asOf

export default {
  name: 'full-request',
  inject: ['progress'],
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
        .sort(asOfDesc)[0].text
    },
    log () {
      const allHistory = (this.request.notes || [])
        .map(note => ({ asOf: note.asOf, text: note.notes, status: 'Notes' }))
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
    this.progress.$emit('show', 'indeterminate')
    try {
      const req = await api.getFullRequest(this.id)
      this.request = req.data
      this.progress.$emit('done')
    } catch (e) {
      console.log(e) // eslint-disable-line no-console
      this.progress.$emit('done')
    }
  },
  methods: {
    formatDate (asOf) {
      return moment(asOf).format('LL')
    }
  }
}
</script>
