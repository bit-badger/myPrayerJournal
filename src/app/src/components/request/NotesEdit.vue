<template lang="pug">
b-modal(v-model='notesVisible'
        header-bg-variant='mpj'
        header-text-variant='light'
        size='lg'
        title='Add Notes to Prayer Request'
        @edit='openDialog()'
        @shows='focusNotes')
  b-form
    b-form-group(label='Notes'
                 label-for='notes')
      b-textarea#notes(ref='toFocus'
                       v-model='form.notes'
                       :rows='10'
                       @blur='trimText()')
  div(v-if='hasPriorNotes')
    p.text-center: strong Prior Notes for This Request
    b-list-group(flush)
      b-list-group-item(v-for='note in priorNotes'
                        :key='note.asOf')
        small.text-muted: date-from-now(:value='note.asOf')
        br
        div.mpj-request-text {{ note.notes }}
  div(v-else-if='noPriorNotes').text-center.text-muted There are no prior notes for this request
  div(v-else).text-center
    b-btn(variant='outline-secondary'
          @click='loadNotes()') Load Prior Notes
  div.w-100.text-right(slot='modal-footer')
    b-btn(variant='primary'
          @click='saveNotes()') Save
    | &nbsp; &nbsp;
    b-btn(variant='outline-secondary'
          @click='closeDialog()') Cancel
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
    focusNotes (e) {
      this.$refs.toFocus.focus()
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
      this.form.requestId = request.requestId
      this.notesVisible = true
      this.focusNotes(null)
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
