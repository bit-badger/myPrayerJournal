<template lang="pug">
span
  el-button(icon='edit' @click='openDialog()' title='Edit')
  el-dialog(title='Edit Prayer Request' :visible.sync='editVisible')
    el-form(:model='form' :label-position='top')
      el-form-item(label='Prayer Request')
        el-input(type='textarea' v-model='form.requestText' :rows='10' @blur="trimText()")
      el-form-item(label='Also Mark As')
        el-radio-group(v-model='form.status')
          el-radio-button(label='Updated') Updated
          el-radio-button(label='Prayed') Prayed
          el-radio-button(label='Answered') Answered
    span.dialog-footer(slot='footer')
      el-button(@click='closeDialog()') Cancel
      el-button(type='primary' @click='saveRequest()') Save
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'edit-request',
  props: [ 'request' ],
  data () {
    return {
      editVisible: false,
      form: {
        requestText: this.request.text,
        status: 'Updated'
      },
      formLabelWidth: '120px'
    }
  },
  methods: {
    closeDialog () {
      this.form.requestText = ''
      this.form.status = 'Updated'
      this.editVisible = false
    },
    openDialog () {
      this.editVisible = true
    },
    trimText () {
      this.form.requestText = this.form.requestText.trim()
    },
    async saveRequest () {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        updateText: this.form.requestText,
        status: this.form.status
      })
      if (this.form.status === 'Answered') {
        this.$message({
          message: 'Request updated and removed from active journal',
          type: 'success'
        })
      } else {
        this.$message({
          message: 'Request updated',
          type: 'success'
        })
      }
      this.editVisible = false
    }
  }
}
</script>