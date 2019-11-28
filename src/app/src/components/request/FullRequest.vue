<template lang="pug">
md-content(role='main').mpj-main-content
  page-title(title='Full Prayer Request'
             hide-on-page=true)
  md-card(v-if='request')
    md-card-header
      .md-title Full Prayer Request
      .md-subhead
        span(v-if='isAnswered.value') Answered {{ formatDate(answered) }} (#[date-from-now(:value='answered')]) !{' &bull; '}
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

<script lang="ts">
import { computed, createComponent, onMounted } from '@vue/composition-api'
import moment from 'moment'

import api from '../../api'
import { useProgress } from '../../App.vue'
import { HistoryEntry, JournalRequest } from '../../store/types' // eslint-disable-line no-unused-vars

/** Sort history entries in descending order */
const asOfDesc = (a: HistoryEntry, b: HistoryEntry) => b.asOf - a.asOf

export default createComponent({
  props: {
    id: {
      type: String,
      required: true
    }
  },
  setup (props) {
    /** The progress bar component instance */
    const progress = useProgress()

    /** The request being displayed */
    let request = new JournalRequest()

    /** The history entry where the request was marked as answered */
    const answer = computed(() => request.history.find(hist => hist.status === 'Answered'))

    /** Whether this request is answered */
    const isAnswered = computed(() => answer.value !== undefined)

    /** The date/time this request was answered */
    const answered = computed(() => answer.value ? answer.value.asOf : undefined)

    /** The last recorded text for the request */
    const lastText = computed(() => request.history.filter(hist => hist.text).sort(asOfDesc)[0].text)

    /** The history log including notes (and excluding the final entry for answered requests) */
    const log = computed(() => {
      const allHistory = (request.notes || [])
        .map(note => ({ asOf: note.asOf, text: note.notes, status: 'Notes' } as HistoryEntry))
        .concat(request.history)
        .sort(asOfDesc)
      // Skip the first entry for answered requests; that info is already displayed
      return isAnswered.value ? allHistory.slice(1) : allHistory
    })

    /** The number of days this request [was|has been] open */
    const openDays = computed(() => {
      const asOf = answered.value ? answered.value : Date.now()
      return Math.floor(
        (asOf - request.history.filter(hist => hist.status === 'Created')[0].asOf) / 1000 / 60 / 60 / 24)
    })

    /** How many times this request has been prayed for */
    const prayedCount = computed(() => request.history.filter(hist => hist.status === 'Prayed').length)

    /** Format a date */
    const formatDate = (asOf: number) => moment(asOf).format('LL')

    onMounted(async () => {
      progress.events.$emit('show', 'indeterminate')
      try {
        const req = await api.getFullRequest(props.id)
        request = req.data as JournalRequest
      } catch (e) {
        console.log(e) // eslint-disable-line no-console
      } finally {
        progress.events.$emit('done')
      }
    })
    return {
      answered,
      formatDate,
      isAnswered,
      lastText,
      log,
      openDays,
      prayedCount,
      request
    }
  }
})
</script>
