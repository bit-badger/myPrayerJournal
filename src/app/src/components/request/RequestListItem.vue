<template lang="pug">
md-table-row
  md-table-cell.mpj-action-cell
    md-button(@click='viewFull').md-icon-button.md-raised
      md-icon description
      md-tooltip(md-direction='top'
                 md-delay=250) View Full Request
    template(v-if='!isAnswered')
      md-button(@click='editRequest').md-icon-button.md-raised
        md-icon edit
        md-tooltip(md-direction='top'
                   md-delay=250) Edit Request
    template(v-if='isSnoozed')
      md-button(@click='cancelSnooze()').md-icon-button.md-raised
        md-icon restore
        md-tooltip(md-direction='top'
                   md-delay=250) Cancel Snooze
    template(v-if='isPending')
      md-button(@click='showNow()').md-icon-button.md-rasied
        md-icon restore
        md-tooltip(md-direction='top'
                   md-delay=250) Show Now
  md-table-cell.mpj-request-cell
    p.mpj-request-text
      | {{ request.text }}
    br(v-if='isSnoozed || isPending || isAnswered')
    small(v-if='isSnoozed').mpj-muted-text: em.
      Snooze expires #[date-from-now(:value='request.snoozedUntil')]
    small(v-if='isPending').mpj-muted-text: em.
      Request scheduled to reappear #[date-from-now(:value='request.showAfter')]
    small(v-if='isAnswered').mpj-muted-text: em.
      Answered #[date-from-now(:value='request.asOf')]
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  inject: [
    'messages',
    'progress'
  ],
  props: {
    request: { required: true }
  },
  data () {
    return {}
  },
  computed: {
    answered () {
      return this.request.history.find(hist => hist.status === 'Answered').asOf
    },
    isAnswered () {
      return this.request.lastStatus === 'Answered'
    },
    isPending () {
      return !this.isSnoozed && this.request.showAfter > Date.now()
    },
    isSnoozed () {
      return this.request.snoozedUntil > Date.now()
    }
  },
  methods: {
    async cancelSnooze () {
      await this.$store.dispatch(actions.SNOOZE_REQUEST, {
        progress: this.progress,
        requestId: this.request.requestId,
        until: 0
      })
      this.messages.$emit('info', 'Request un-snoozed')
      this.$parent.$emit('requestUnsnoozed')
    },
    editRequest () {
      this.$router.push({ name: 'EditRequest', params: { id: this.request.requestId } })
    },
    async showNow () {
      await this.$store.dispatch(actions.SHOW_REQUEST_NOW, {
        progress: this.progress,
        requestId: this.request.requestId,
        showAfter: Date.now()
      })
      this.messages.$emit('info', 'Recurrence skipped; request now shows in journal')
      this.$parent.$emit('requestNowShown')
    },
    viewFull () {
      this.$router.push({ name: 'FullRequest', params: { id: this.request.requestId } })
    }
  }
}
</script>

<style lang="sass">
.mpj-action-cell
  width: 1%
  white-space: nowrap
  vertical-align: top
.mpj-request-cell
  vertical-align: top
  p
    margin-top: 0
</style>