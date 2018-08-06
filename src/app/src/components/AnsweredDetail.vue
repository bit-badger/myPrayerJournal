<template lang="pug">
article
  page-title(title='Answered Request')
  p(v-if='!request') Loading request...
  template(v-if='request')
    p.
      Answered {{ formatDate(answered) }} (#[date-from-now(:value='answered')]) &nbsp;
      #[small: em.text-muted prayed {{ prayedCount }} times, open {{ openDays }} days]
    p.mpj-request-text {{ lastText }}
    b-table(small hover :fields='fields' :items='log')
      template(slot='action' scope='data').
        {{ data.item.status }} on #[span.text-nowrap {{ formatDate(data.item.asOf) }}]
      template(slot='text' scope='data' v-if='data.item.text') {{ data.item.text.fields[0] }}
</template>

<script>
'use strict'

import moment from 'moment'

import api from '@/api'

const asOfDesc = (a, b) => b.asOf - a.asOf

export default {
  name: 'answer-detail',
  props: {
    id: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      request: null,
      fields: [
        { key: 'action', label: 'Action' },
        { key: 'text', label: 'Update / Notes' }
      ]
    }
  },
  computed: {
    answered () {
      return this.request.history.find(hist => hist.status === 'Answered').asOf
    },
    lastText () {
      return this.request.history
        .filter(hist => hist.text)
        .sort(asOfDesc)[0].text.fields[0]
    },
    log () {
      return (this.request.notes || [])
        .map(note => ({ asOf: note.asOf, text: { case: 'Some', fields: [ note.notes ] }, status: 'Notes' }))
        .concat(this.request.history)
        .sort(asOfDesc)
        .slice(1)
    },
    openDays () {
      return Math.floor(
        (this.answered - this.request.history.find(hist => hist.status === 'Created').asOf) / 1000 / 60 / 60 / 24)
    },
    prayedCount () {
      return this.request.history.filter(hist => hist.status === 'Prayed').length
    }
  },
  async mounted () {
    this.$Progress.start()
    try {
      const req = await api.getRequestComplete(this.id)
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
