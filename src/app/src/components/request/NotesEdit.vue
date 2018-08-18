<template lang="pug">
.mpj-modal(v-show='notesVisible'
           @edit='openDialog()')
  .mpj-modal-content.mpj-narrow
    header.mpj-bg
      h3 Add Notes to Prayer Request
    label(for='notes') Notes
    br
    textarea#notes(v-model='form.notes'
                   :rows='10'
                   @blur='trimText()')
    .mpj-text-right
      button(@click='saveNotes()').primary.
        #[md-icon(icon='save')] Save
      | &nbsp; &nbsp;
      button(@click='closeDialog()').
        #[md-icon(icon='undo')] Cancel
    hr
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
      button(@click='loadNotes()').
        #[md-icon(icon='cloud_download')] Load Prior Notes
</template>

<script>
'use strict'

import api from '@/api'

export default {
  name: 'notes-edit',
  props: {
    toast: { required: true },
    events: { required: true }
  },
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
    this.events.$on('notes', this.openDialog)
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
      this.$Progress.start()
      try {
        const notes = await api.getNotes(this.form.requestId)
        this.priorNotes = notes.data
        console.log(this.priorNotes)
        this.$Progress.finish()
      } catch (e) {
        console.error(e)
        this.$Progress.fail()
      } finally {
        this.priorNotesLoaded = true
      }
    },
    openDialog (request) {
      console.log('Received openDialog event')
      this.form.requestId = request.requestId
      this.notesVisible = true
    },
    async saveNotes () {
      this.$Progress.start()
      try {
        await api.addNote(this.form.requestId, this.form.notes)
        this.$Progress.finish()
        this.toast.showToast('Added notes', { theme: 'success' })
        this.closeDialog()
      } catch (e) {
        console.error(e)
        this.$Progress.fail()
      }
    },
    trimText () {
      this.form.notes = this.form.notes.trim()
    }
  }
}
</script>

<style scoped>
#notes {
  width: 100%;
}
.mpj-note-list p {
  border-top: dotted 1px lightgray;
}
</style>
