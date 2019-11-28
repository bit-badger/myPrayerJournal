<template lang="pug">
md-dialog(:md-active.sync='isVisible.value').mpj-skinny
  md-dialog-title Snooze Prayer Request
  md-content.mpj-dialog-content
    span.mpj-text-muted Until
    md-datepicker(v-model='form.snoozedUntil'
                  :md-disabled-dates='datesInPast'
                  md-immediately)
  md-dialog-actions
    md-button(:disabled='!isValid.value'
              @click='snoozeRequest()').md-primary #[md-icon snooze] Snooze
    md-button(@click='closeDialog()') #[md-icon undo] Cancel
</template>

<script lang="ts">
import { createComponent, ref, computed } from '@vue/composition-api'

import { Actions, SnoozeRequestAction } from '../../store/types' // eslint-disable-line no-unused-vars

import { useProgress, useSnackbar } from '../../App.vue'
import { useEvents } from '../Journal.vue'
import { useStore } from '../../plugins/store'

/** The input form */
class SnoozeForm {
  /** The ID of the request */
  requestId = ''

  /** The date until which the request will be snoozed */
  snoozedUntil = ''
}

export default createComponent({
  setup () {
    /** The Vuex store */
    const store = useStore()

    /** The progress bar component properties */
    const progress = useProgress()

    /** The snackbar component properties */
    const snackbar = useSnackbar()

    /** The journal event bus */
    const events = useEvents()

    /** Whether this dialog is visible */
    const isVisible = ref(false)

    /** The input form */
    const form = new SnoozeForm()

    /** Is the input date valid? */
    const isValid = computed(() => !isNaN(Date.parse(form.snoozedUntil)))

    /** Close the dialog */
    const closeDialog = () => {
      form.requestId = ''
      form.snoozedUntil = ''
      isVisible.value = false
    }

    /**
     * Open the dialog
     * @param requestId The ID of the request to be snoozed
     */
    const openDialog = (requestId: string) => {
      form.requestId = requestId
      isVisible.value = true
    }

    const snoozeRequest = async () => {
      const opts: SnoozeRequestAction = {
        progress,
        requestId: form.requestId,
        until: Date.parse(form.snoozedUntil)
      }
      await store.dispatch(Actions.SnoozeRequest, opts)
      snackbar.events.$emit('info', `Request snoozed until ${form.snoozedUntil}`)
      closeDialog()
    }

    events.$on('snooze', openDialog)

    return {
      closeDialog,
      datesInPast: (date: Date) => date < new Date(),
      form,
      isValid,
      isVisible,
      openDialog,
      snoozeRequest
    }
  }
})
</script>
