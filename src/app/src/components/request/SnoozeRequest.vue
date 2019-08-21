<template lang="pug">
.mpj-modal(v-show='snoozeVisible')
  .mpj-modal-content.mpj-skinny
    header.mpj-bg
      h5 Snooze Prayer Request
    p.mpj-text-center
      label
        = 'Until '
        input(v-model='form.snoozedUntil'
              type='date'
              autofocus)
    br
    .mpj-text-right
      button.primary(:disabled='!isValid'
                     @click='snoozeRequest()').
        #[md-icon snooze] Snooze
      | &nbsp; &nbsp;
      button(@click='closeDialog()').
        #[md-icon undo] Cancel
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'snooze-request',
  inject: ['messages'],
  props: {
    events: { required: true }
  },
  data () {
    return {
      snoozeVisible: false,
      form: {
        requestId: '',
        snoozedUntil: ''
      }
    }
  },
  created () {
    this.events.$on('snooze', this.openDialog)
  },
  computed: {
    isValid () {
      return !isNaN(Date.parse(this.form.snoozedUntil))
    }
  },
  methods: {
    closeDialog () {
      this.form.requestId = ''
      this.form.snoozedUntil = ''
      this.snoozeVisible = false
    },
    openDialog (requestId) {
      this.form.requestId = requestId
      this.snoozeVisible = true
    },
    async snoozeRequest () {
      await this.$store.dispatch(actions.SNOOZE_REQUEST, {
        progress: this.$Progress,
        requestId: this.form.requestId,
        until: Date.parse(this.form.snoozedUntil)
      })
      this.messages.$emit('info', `Request snoozed until ${this.form.snoozedUntil}`)
      this.closeDialog()
    }
  }
}
</script>
