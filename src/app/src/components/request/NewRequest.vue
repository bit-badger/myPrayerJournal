<template lang="pug">
div
  button(@click='openDialog()')
    md-icon(icon='add_box')
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
      b-form-group(label='Recurrence')
        | After prayer, request reappears
        b-radio(v-model='form.recur.typ'
                label='Immediately'
                value='immediately'
                checked='checked'
                @click='checkRadios')
        b-radio(v-model='form.recur.typ'
                label='Every...'
                value='other'
                @click='checkRadios')
        b-input(v-model='form.recur.count'
                placeholder='##')
        b-select(v-model='form.recur.other')
          b-option(value='hours') hours
          b-option(value='days') days
          b-option(value='weeks') weeks
    div.w-100.text-right(slot='modal-footer')
      b-btn(variant='primary'
            :disabled='!isValid'
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
        requestText: '',
        recur: {
          typ: 'immediate',
          other: '',
          count: ''
        }
      },
      formLabelWidth: '120px'
    }
  },
  mounted () {
    this.$refs.toast.setOptions({ position: 'bottom right' })
  },
  computed: {
    isValid () {
      // TODO disallow submission if recurrence is too long
      return true
    }
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
