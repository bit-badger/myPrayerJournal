<template lang="pug">
md-card(v-if='shouldDisplay'
        md-with-hover).mpj-request-card
  md-card-actions(md-alignment='space-between')
    md-button(@click='markPrayed()').md-icon-button.md-raised.md-primary
      md-icon done
      md-tooltip(md-direction='top'
                 md-delay=1000) Mark as Prayed
    span
      md-button(@click.stop='showEdit()').md-icon-button.md-raised
        md-icon edit
        md-tooltip(md-direction='top'
                   md-delay=1000) Edit Request
      md-button(@click.stop='showNotes()').md-icon-button.md-raised
        md-icon comment
        md-tooltip(md-direction='top'
                   md-delay=1000) Add Notes
      md-button(@click.stop='snooze()').md-icon-button.md-raised
        md-icon schedule
        md-tooltip(md-direction='top'
                   md-delay=1000) Snooze Request
  md-card-content
    p.mpj-request-text {{ request.text }}
    p.mpj-text-right: small.mpj-muted-text: em (last activity #[date-from-now(:value='request.asOf')])
</template>

<script lang="ts">
import { createComponent, computed } from '@vue/composition-api'

import { Actions, JournalRequest, UpdateRequestAction } from '../../store/types' // eslint-disable-line no-unused-vars

import { useEvents } from '../Journal.vue'
import { useStore } from '../../plugins/store'
import { useProgress, useSnackbar } from '../../App.vue'
import { useRouter } from '../../plugins/router'

export default createComponent({
  props: {
    request: {
      type: JournalRequest,
      required: true
    }
  },
  setup (props) {
    /** The Vuex store */
    const store = useStore()

    /** The application router */
    const router = useRouter()

    /** The progress bar component properties */
    const progress = useProgress()

    /** The snackbar component properties */
    const snackbar = useSnackbar()

    /** The journal event bus */
    const events = useEvents()

    /** Should this request be displayed? */
    const shouldDisplay = computed(() => {
      const now = Date.now()
      return Math.max(now, props.request.showAfter, props.request.snoozedUntil) === now
    })

    /** Mark the request as prayed */
    const markPrayed = async () => {
      const opts: UpdateRequestAction = {
        progress,
        requestId: props.request.requestId,
        status: 'Prayed',
        updateText: '',
        recurType: '',
        recurCount: 0
      }
      await store.dispatch(Actions.UpdateRequest, opts)
      snackbar.events.$emit('info', 'Request marked as prayed')
    }

    /** Show the edit page for this request */
    const showEdit = () => { router.push({ name: 'EditRequest', params: { id: props.request.requestId } }) }

    /** Show the request notes dialog */
    const showNotes = () => events.$emit('notes', props.request)

    /** Show the snooze request dialog */
    const snooze = () => events.$emit('snooze', props.request.requestId)

    return {
      markPrayed,
      request: props.request,
      shouldDisplay,
      showEdit,
      showNotes,
      snooze
    }
  }
})
</script>

<style lang="sass">
.mpj-request-card
  width: 20rem
  margin-bottom: 1rem
@media screen and (max-width: 20rem)
  .mpj-request-card
    width: 100%
</style>
