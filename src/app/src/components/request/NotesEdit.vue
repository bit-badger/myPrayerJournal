<template lang="pug">
md-dialog(:md-active.sync='notesVisible').mpj-note-dialog
  md-dialog-title Add Notes to Prayer Request
  md-content.mpj-dialog-content
    md-field
      label Notes
      md-textarea(v-model='form.notes'
                  md-autogrow
                  @blur='trimText()')
  md-dialog-actions
    md-button(@click='saveNotes()').md-primary #[md-icon save] Save
    md-button(@click='closeDialog()') #[md-icon undo] Cancel
  md-dialog-content(md-scrollbar='true').mpj-dialog-content
    div(v-if='hasPriorNotes.value')
      p.mpj-text-center: strong Prior Notes for This Request
      .mpj-note-list
        p(v-for='note in priorNotes'
          :key='note.asOf')
          small.mpj-muted-text: date-from-now(:value='note.asOf')
          br
          span.mpj-request-text {{ note.notes }}
    div(v-else-if='noPriorNotes.value').mpj-text-center.mpj-muted-text There are no prior notes for this request
    div(v-else).mpj-text-center
      hr
      md-button(@click='loadNotes()') #[md-icon cloud_download] Load Prior Notes
</template>

<script lang="ts">
import { computed, createComponent, ref } from '@vue/composition-api'

import api from '../../api'

import { useEvents } from '../Journal.vue'
import { NotesEntry, JournalRequest } from '../../store/types' // eslint-disable-line no-unused-vars
import { useProgress, useSnackbar } from '../../App.vue'

/** The input form for the notes dialog */
class NotesForm {
  /** The ID of the request */
  requestId = ''
  /** The actual notes */
  notes = ''
}

/** The prior notes for this request */
class PriorNotes {
  /** The prior notes */
  notes: NotesEntry[] = []
  /** Have the prior notes been loaded? */
  isLoaded = false
}

export default createComponent({
  setup () {
    /** The event bus for the journal page */
    const events = useEvents()

    /** The snackbar component properties */
    const snackbar = useSnackbar()

    /** The progress bar component properties */
    const progress = useProgress()

    /** Is this dialog visible? */
    const notesVisible = ref(false)

    /** The input form */
    const form = new NotesForm()

    /** The prior notes */
    const prior = new PriorNotes()

    /** Are there prior notes? */
    const hasPriorNotes = computed(() => prior.isLoaded && prior.notes.length > 0)

    /** Are there no prior notes? */
    const noPriorNotes = computed(() => prior.isLoaded && prior.notes.length === 0)

    /** Close this dialog */
    const closeDialog = () => {
      form.requestId = ''
      form.notes = ''
      prior.notes = []
      prior.isLoaded = false
      notesVisible.value = false
    }

    /** Load the notes for this request */
    const loadNotes = async () => {
      progress.events.$emit('show', 'indeterminate')
      try {
        const notes = await api.getNotes(form.requestId)
        prior.notes = (notes.data as NotesEntry[]).sort((a, b) => b.asOf - a.asOf)
      } catch (e) {
        console.error(e) // eslint-disable-line no-console
      } finally {
        progress.events.$emit('done')
        prior.isLoaded = true
      }
    }

    /** Open this dialog */
    const openDialog = (request: JournalRequest) => {
      form.requestId = request.requestId
      notesVisible.value = true
    }

    /** Save the notes entered on this dialog */
    const saveNotes = async () => {
      progress.events.$emit('show', 'indeterminate')
      try {
        await api.addNote(form.requestId, form.notes)
        snackbar.events.$emit('info', 'Added notes')
        closeDialog()
      } catch (e) {
        console.error(e) // eslint-disable-line no-console
      } finally {
        progress.events.$emit('done')
      }
    }

    /** Trim the note text */
    const trimText = () => { form.notes = form.notes.trim() }

    events.$on('notes', openDialog)

    return {
      closeDialog,
      form,
      hasPriorNotes,
      loadNotes,
      noPriorNotes,
      notesVisible,
      openDialog,
      prior,
      saveNotes,
      trimText
    }
  }
})
</script>

<style lang="sass">
.mpj-note-dialog
  width: 40rem
  padding-bottom: 1.5rem
@media screen and (max-width: 40rem)
  @media screen and (max-width: 20rem)
    .mpj-note-dialog
      width: 100%
  .mpj-note-dialog
    width: 20rem
.mpj-note-list p
  border-top: dotted 1px lightgray
</style>
