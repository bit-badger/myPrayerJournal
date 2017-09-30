<template lang="pug">
span
  //- b-btn(@click='openDialog()' title='Edit' size='sm' variant='outline-secondary'): icon(name='pencil')
  b-modal(title='Edit Prayer Request'
          v-model='editVisible'
          size='lg'
          header-bg-variant='dark'
          header-text-variant='light'
          @edit='openDialog()'
          @shows='focusRequestText')
    b-form
      b-form-group(label='Prayer Request' label-for='request_text')
        b-textarea#request_text(v-model='form.requestText' :rows='10' @blur='trimText()' ref='toFocus')
      b-form-group(label='Also Mark As')
        b-radio-group(v-model='form.status' buttons)
          b-radio(value='Updated') Updated
          b-radio(value='Prayed') Prayed
          b-radio(value='Answered') Answered
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary' @click='saveRequest()') Save
      | &nbsp; &nbsp;
      b-btn(variant='outline-secondary' @click='closeDialog()') Cancel
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'edit-request',
  props: {
    toast: { required: true },
    events: { required: true }
  },
  data () {
    return {
      editVisible: false,
      form: {
        requestId: '',
        requestText: '',
        status: 'Updated'
      }
    }
  },
  created () {
    this.events.$on('edit', this.openDialog)
  },
  methods: {
    closeDialog () {
      this.form.requestId = ''
      this.form.requestText = ''
      this.form.status = 'Updated'
      this.editVisible = false
    },
    focusRequestText (e) {
      this.$refs.toFocus.focus()
    },
    openDialog (request) {
      this.form.requestId = request.requestId
      this.form.requestText = request.text
      this.editVisible = true
      this.focusRequestText(null)
    },
    trimText () {
      this.form.requestText = this.form.requestText.trim()
    },
    async saveRequest () {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.form.requestId,
        updateText: this.form.requestText,
        status: this.form.status
      })
      if (this.form.status === 'Answered') {
        this.toast.showToast('Request updated and removed from active journal', { theme: 'success' })
      } else {
        this.toast.showToast('Request updated', { theme: 'success' })
      }
      this.closeDialog()
    }
  }
}
</script>
