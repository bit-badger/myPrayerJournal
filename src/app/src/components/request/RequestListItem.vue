<template lang="pug">
p.mpj-request-text
  | {{ request.text }}
  br
  br
  button(@click='viewFull'
         title='View Full Request').
    #[md-icon(icon='description')] View Full Request
  | &nbsp; &nbsp;
  button(v-if='!isAnswered'
         @click='editRequest'
         title='Edit Request').
    #[md-icon(icon='edit')] Edit Request
  | &nbsp; &nbsp;
  button(v-if='isSnoozed'
         @click='cancelSnooze()').
    #[md-icon(icon='restore')] Cancel Snooze
  br(v-if='isSnoozed || isAnswered')
  small(v-if='isSnoozed').mpj-muted-text: em.
    &nbsp; Snooze expires #[date-from-now(:value='request.snoozedUntil')]
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
    viewFull () {
      this.$router.push({ name: 'FullRequest', params: { id: this.request.requestId } })
    }
  }
}
</script>
