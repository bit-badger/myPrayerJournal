<template lang="pug">
md-content(role='main').mpj-narrow
  page-title(:title='title.value')
  md-field
    label(for='request_text') Prayer Request
    md-textarea(v-model='form.requestText'
                @blur='trimText()'
                md-autogrow
                autofocus).mpj-full-width
  br
  template(v-if='!isNew.value')
    label Also Mark As
    br
    md-radio(v-model='form.status'
             value='Updated') Updated
    md-radio(v-model='form.status'
             value='Prayed') Prayed
    md-radio(v-model='form.status'
             value='Answered') Answered
    br
  label Recurrence
  | &nbsp; &nbsp;
  em.mpj-muted-text After prayer, request reappears...
  br
  .md-layout
    .md-layout-item.md-size-30
      md-radio(v-model='form.recur.typ'
              value='Immediate') Immediately
    .md-layout-item.md-size-20
      md-radio(v-model='form.recur.typ'
              value='other') Every...
    .md-layout-item.md-size-10
      md-field(md-inline)
        label Count
        md-input(v-model='form.recur.count'
                type='number'
                :disabled='!showRecurrence.value')
    .md-layout-item.md-size-20
      md-field
        label Interval
        md-select(v-model='form.recur.other'
                  :disabled='!showRecurrence.value')
          md-option(value='Hours') hours
          md-option(value='Days') days
          md-option(value='Weeks') weeks
  .mpj-text-right
    md-button(:disabled='!isValidRecurrence.value'
              @click.stop='saveRequest()').md-primary.md-raised #[md-icon save] Save
    md-button(@click.stop='goBack()').md-raised #[md-icon arrow_back] Cancel
</template>

<script lang="ts">
import { createComponent, ref, computed, onMounted } from '@vue/composition-api'
import { Store } from 'vuex' // eslint-disable-line no-unused-vars

import { Actions, AppState, AddRequestAction, UpdateRequestAction } from '../../store/types' // eslint-disable-line no-unused-vars
import { useProgress, useSnackbar } from '../../App.vue'
import { useStore } from '../../plugins/store'
import { useRouter } from '../../plugins/router'

/** The recurrence settings for the request */
class RecurrenceForm {
  /** The type of recurrence */
  typ = 'Immediate'

  /** The type of recurrence (other than immediate) */
  other = ''

  /** The count of non-immediate intervals */
  count = ''

  /**
   * The recurrence represented by the given form
   * @param x The recurrence form
   */
  static recurrence = (x: RecurrenceForm) => x.typ === 'Immediate' ? 'Immediate' : x.other

  /**
   * The interval represented by the given form
   * @param x The recurrence form
   */
  static interval = (x: RecurrenceForm) => x.typ === 'Immediate' ? 0 : Number.parseInt(x.count)
}

/** The form for editing the request */
class EditForm {
  /** The ID of the request */
  requestId = ''

  /** The text of the request */
  requestText = ''

  /** The status associated with this update */
  status = 'Updated'

  /** The recurrence for the request */
  recur = new RecurrenceForm()
}

export default createComponent({
  props: {
    id: {
      type: String,
      required: true
    }
  },
  setup (props) {
    /** The Vuex store */
    const store = useStore() as Store<AppState>

    /** The snackbar component properties */
    const snackbar = useSnackbar()

    /** The progress bar component properties */
    const progress = useProgress()

    /** The application router */
    const router = useRouter()

    /** The page title */
    const title = ref('Edit Prayer Request')

    /** Whether this is a new request */
    const isNew = ref(false)

    /** The input form */
    const form = new EditForm()

    /** Is the selected recurrence a valid recurrence? */
    const isValidRecurrence = computed(() => {
      if (form.recur.typ === 'Immediate') return true
      const count = Number.parseInt(form.recur.count)
      if (isNaN(count) || form.recur.other === '') return false
      if (form.recur.other === 'Hours' && count > (365 * 24)) return false
      if (form.recur.other === 'Days' && count > 365) return false
      if (form.recur.other === 'Weeks' && count > 52) return false
      return true
    })

    /** Whether the recurrence should be shown */
    const showRecurrence = computed(() => form.recur.typ !== 'Immediate')

    /** Go back 1 in browser history */
    const goBack = () => { router.go(-1) }

    /** Trim the request text */
    const trimText = () => { form.requestText = form.requestText.trim() }

    /** Save the edited request */
    const saveRequest = async () => {
      if (isNew.value) {
        const opts: AddRequestAction = {
          progress,
          requestText: form.requestText,
          recurType: RecurrenceForm.recurrence(form.recur),
          recurCount: RecurrenceForm.interval(form.recur)
        }
        await store.dispatch(Actions.AddRequest, opts)
        snackbar.events.$emit('info', 'New prayer request added')
      } else {
        const opts: UpdateRequestAction = {
          progress,
          requestId: form.requestId,
          updateText: form.requestText,
          status: form.status,
          recurType: RecurrenceForm.recurrence(form.recur),
          recurCount: RecurrenceForm.interval(form.recur)
        }
        await store.dispatch(Actions.UpdateRequest, opts)
        if (form.status === 'Answered') {
          snackbar.events.$emit('info', 'Request updated and removed from active journal')
        } else {
          snackbar.events.$emit('info', 'Request updated')
        }
      }
      goBack()
    }

    onMounted(async () => {
      if (!Array.isArray(store.state.journal)) {
        await store.dispatch(Actions.LoadJournal, progress)
      }
      if (props.id === 'new') {
        title.value = 'Add Prayer Request'
        isNew.value = true
        form.requestId = ''
        form.requestText = ''
        form.status = 'Created'
        form.recur.typ = 'Immediate'
        form.recur.other = ''
        form.recur.count = ''
      } else {
        title.value = 'Edit Prayer Request'
        isNew.value = false
        const req = store.state.journal.filter(r => r.requestId === props.id)[0]
        form.requestId = props.id
        form.requestText = req.text
        form.status = 'Updated'
        if (req.recurType === 'Immediate') {
          form.recur.typ = 'Immediate'
          form.recur.other = ''
          form.recur.count = ''
        } else {
          form.recur.typ = 'other'
          form.recur.other = req.recurType
          form.recur.count = req.recurCount.toString()
        }
      }
    })

    return {
      form,
      goBack,
      isNew,
      isValidRecurrence,
      journal: store.state.journal,
      saveRequest,
      showRecurrence,
      title,
      trimText
    }
  }
})
</script>
