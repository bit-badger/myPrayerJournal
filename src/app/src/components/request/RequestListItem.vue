<template lang="pug">
md-table-row
  md-table-cell.mpj-action-cell.mpj-valign-top
    md-button(@click='viewFull').md-icon-button.md-raised
      md-icon description
      md-tooltip(md-direction='top'
                 md-delay=250) View Full Request
    template(v-if='!isAnswered.value')
      md-button(@click='editRequest').md-icon-button.md-raised
        md-icon edit
        md-tooltip(md-direction='top'
                   md-delay=250) Edit Request
    template(v-if='isSnoozed.value')
      md-button(@click='cancelSnooze()').md-icon-button.md-raised
        md-icon restore
        md-tooltip(md-direction='top'
                   md-delay=250) Cancel Snooze
    template(v-if='isPending.value')
      md-button(@click='showNow()').md-icon-button.md-raised
        md-icon restore
        md-tooltip(md-direction='top'
                   md-delay=250) Show Now
  md-table-cell.mpj-valign-top
    p.mpj-request-text {{ request.text }}
    br(v-if='isSnoozed.value || isPending.value || isAnswered.value')
    small(v-if='isSnoozed.value').mpj-muted-text: em Snooze expires #[date-from-now(:value='request.snoozedUntil')]
    small(v-if='isPending.value').mpj-muted-text: em Request appears next #[date-from-now(:value='request.showAfter')]
    small(v-if='isAnswered.value').mpj-muted-text: em Answered #[date-from-now(:value='request.asOf')]
</template>

<script lang="ts">
import { computed } from '@vue/composition-api'

import { Actions, JournalRequest, ISnoozeRequestAction, IShowRequestAction } from '../../store/types'
import { useStore } from '../../plugins/store'
import { useRouter } from '../../plugins/router'
import { useProgress, useSnackbar } from '../../App.vue'

export default {
  props: {
    request: { required: true }
  },
  setup (props, { parent }) {
    /** The request to be rendered */
    const request = props.request as JournalRequest

    /** The Vuex store */
    const store = useStore()

    /** The application router */
    const router = useRouter()

    /** The snackbar instance */
    const snackbar = useSnackbar()

    /** The progress bar component instance */
    const progress = useProgress()

    /** Whether the request has been answered */
    const isAnswered = computed(() => request.lastStatus === 'Answered')

    /** Whether the request is snoozed */
    const isSnoozed = computed(() => request.snoozedUntil > Date.now())

    /** Whether the request is not shown because of an interval */
    const isPending = computed(() => !isSnoozed.value && request.showAfter > Date.now())

    /** Cancel the snooze period for this request */
    const cancelSnooze = async () => {
      const opts: ISnoozeRequestAction = {
        progress: progress,
        requestId: request.requestId,
        until: 0
      }
      await store.dispatch(Actions.SnoozeRequest, opts)
      snackbar.events.$emit('info', 'Request un-snoozed')
      parent.$emit('requestUnsnoozed')
    }

    /** Edit the given request */
    const editRequest = () => { router.push({ name: 'EditRequest', params: { id: request.requestId } }) }
    
    /** Show the request now */
    const showNow = async () => {
      const opts: IShowRequestAction = {
        progress: this.progress,
        requestId: this.request.requestId,
        showAfter: 0
      }
      await store.dispatch(Actions.ShowRequestNow, opts)
      snackbar.events.$emit('info', 'Recurrence skipped; request now shows in journal')
      parent.$emit('requestNowShown')
    }

    /** View the full request */
    const viewFull = () => { router.push({ name: 'FullRequest', params: { id: request.requestId } }) }
    
    return {
      cancelSnooze,
      editRequest,
      isAnswered,
      isPending,
      isSnoozed,
      showNow,
      viewFull
    }
  }
}
</script>

<style lang="sass">
.mpj-action-cell
  width: 1%
  white-space: nowrap
</style>
