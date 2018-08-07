<template lang="pug">
b-modal(v-model='snoozeVisible'
        header-bg-variant='mpj'
        header-text-variant='light'
        size='lg'
        title='Snooze Prayer Request'
        @edit='openDialog()')
  b-form
    b-form-group(label='Until'
                 label-for='until')
      b-input#until(type='date'
                    v-model='form.snoozedUntil'
                    autofocus)
  div.w-100.text-right(slot='modal-footer')
    b-btn(variant='primary'
          :disabled='!isValid'
          @click='snoozeRequest()') Snooze
    | &nbsp; &nbsp;
    b-btn(variant='outline-secondary'
          @click='closeDialog()') Cancel
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'snooze-request',
  props: {
    toast: { required: true },
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
      this.toast.showToast(`Request snoozed until ${this.form.snoozedUntil}`, { theme: 'success' })
      this.closeDialog()
    }
  }
}
</script>
