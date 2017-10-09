<template lang="pug">
div
  b-btn(@click='openDialog()' size='sm' variant='primary')
    icon(name='plus')
    | &nbsp; Add a New Request
  b-modal(v-model='showNewVisible'
          header-bg-variant='mpj'
          header-text-variant='light'
          size='lg'
          title='Add a New Prayer Request'
          @shown='focusRequestText')
    b-form
      b-form-group(label='Prayer Request'
                   label-for='request_text')
        b-textarea#request_text(ref='toFocus'
                                v-model='form.requestText'
                                :rows='10'
                                @blur='trimText()')
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary'
            @click='saveRequest()') Save
      | &nbsp; &nbsp;
      b-btn(variant='outline-secondary'
            @click='closeDialog()') Cancel
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
