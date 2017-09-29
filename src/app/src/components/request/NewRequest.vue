<template lang="pug">
div
  b-btn(@click='openDialog()' size='sm' variant='primary')
    icon(name='plus')
    | &nbsp; Add a New Request
  b-modal(title='Add a New Prayer Request'
          v-model='showNewVisible'
          size='lg'
          header-bg-variant='dark'
          header-text-variant='light'
          @shown='focusRequestText')
    b-form
      b-form-group(label='Prayer Request' label-for='request_text')
        b-textarea#request_text(v-model='form.requestText' :rows='10' @blur='trimText()' ref='toFocus')
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary' @click='saveRequest()') Save
      | &nbsp; &nbsp;
      b-btn(variant='outline-secondary' @click='closeDialog()') Cancel
  toast(ref='toast')
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'new-request',
  data () {
    return {
      showNewVisible: false,
      form: {
        requestText: ''
      },
      formLabelWidth: '120px'
    }
  },
  mounted () {
    this.$refs.toast.setOptions({ position: 'bottom right' })
  },
  methods: {
    closeDialog () {
      this.form.requestText = ''
      this.showNewVisible = false
    },
    focusRequestText (e) {
      this.$refs.toFocus.focus()
    },
    openDialog () {
      this.showNewVisible = true
    },
    trimText () {
      this.form.requestText = this.form.requestText.trim()
    },
    async saveRequest () {
      await this.$store.dispatch(actions.ADD_REQUEST, {
        progress: this.$Progress,
        requestText: this.form.requestText
      })
      this.$refs.toast.showToast('New prayer request added', { theme: 'success' })
      this.closeDialog()
    }
  }
}
</script>
