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
    div(v-if='hasPriorNotes')
      p.mpj-text-center: strong Prior Notes for This Request
      .mpj-note-list
        p(v-for='note in priorNotes'
          :key='note.asOf')
          small.mpj-muted-text: date-from-now(:value='note.asOf')
          br
          span.mpj-request-text {{ note.notes }}
    div(v-else-if='noPriorNotes').mpj-text-center.mpj-muted-text There are no prior notes for this request
    div(v-else).mpj-text-center
      hr
      md-button(@click='loadNotes()') #[md-icon cloud_download] Load Prior Notes
</template>

<script>
'use strict'

import api from '@/api'

export default {
  name: 'notes-edit',
  inject: [
    'journalEvents',
    'messages',
    'progress'
  ],
  data () {
    return {
      notesVisible: false,
      form: {
        requestId: '',
        notes: ''
      },
      priorNotes: [],
      priorNotesLoaded: false
    }
  },
  computed: {
    hasPriorNotes () {
      return this.priorNotesLoaded && this.priorNotes.length > 0
    },
    noPriorNotes () {
      return this.priorNotesLoaded && this.priorNotes.length === 0
    }
  },
  created () {
    this.journalEvents.$on('notes', this.openDialog)
  },
  methods: {
    closeDialog () {
      this.form.requestId = ''
      this.form.notes = ''
      this.priorNotes = []
      this.priorNotesLoaded = false
      this.notesVisible = false
    },
    async loadNotes () {
      this.progress.$emit('show', 'indeterminate')
      try {
        const notes = await api.getNotes(this.form.requestId)
        this.priorNotes = notes.data.sort((a, b) => b.asOf - a.asOf)
        this.progress.$emit('done')
      } catch (e) {
        console.error(e)
        this.progress.$emit('done')
      } finally {
        this.priorNotesLoaded = true
      }
    },
    openDialog (request) {
      this.form.requestId = request.requestId
      this.notesVisible = true
    },
    async saveNotes () {
      this.progress.$emit('show', 'indeterminate')
      try {
        await api.addNote(this.form.requestId, this.form.notes)
        this.progress.$emit('done')
        this.messages.$emit('info', 'Added notes')
        this.closeDialog()
      } catch (e) {
        console.error(e)
        this.progress.$emit('done')
      }
    },
    trimText () {
      this.form.notes = this.form.notes.trim()
    }
  }
}
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
