<template lang="pug">
p.mpj-request-text
  | {{ request.text }}
  br
  br
  button(@click='viewFull'
         title='View Full Request').
    #[md-icon description] View Full Request
  | &nbsp; &nbsp;
  template(v-if='!isAnswered')
    button(@click='editRequest'
           title='Edit Request').
      #[md-icon edit] Edit Request
    | &nbsp; &nbsp;
  template(v-if='isSnoozed')
    button(@click='cancelSnooze()').
      #[md-icon restore] Cancel Snooze
    | &nbsp; &nbsp;
  template(v-if='isPending')
    button(@click='showNow()').
      #[md-icon restore] Show Now
  br(v-if='isSnoozed || isPending || isAnswered')
  small(v-if='isSnoozed').mpj-muted-text: em.
    &nbsp; Snooze expires #[date-from-now(:value='request.snoozedUntil')]
  small(v-if='isPending').mpj-muted-text: em.
    &nbsp; Request scheduled to reappear #[date-from-now(:value='request.showAfter')]
  small(v-if='isAnswered').mpj-muted-text: em.
    &nbsp; Answered #[date-from-now(:value='request.asOf')]
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: {
    request: { required: true },
    toast: { required: true }
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
        progress: this.$Progress,
        requestId: this.request.requestId,
        until: 0
      })
      this.toast.showToast('Request un-snoozed', { theme: 'success' })
      this.$parent.$emit('requestUnsnoozed')
    },
    editRequest () {
      this.$router.push({ name: 'EditRequest', params: { id: this.request.requestId } })
    },
    async showNow () {
      await this.$store.dispatch(actions.SHOW_REQUEST_NOW, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        showAfter: Date.now()
      })
      this.toast.showToast('Recurrence skipped; request now shows in journal', { theme: 'success' })
      this.$parent.$emit('requestNowShown')
    },
    viewFull () {
      this.$router.push({ name: 'FullRequest', params: { id: this.request.requestId } })
    }
  }
}
</script>
