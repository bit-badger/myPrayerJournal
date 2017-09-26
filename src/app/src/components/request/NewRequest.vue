<template lang="pug">
div
  el-button(icon='plus' @click='openDialog()') Add a New Request
  el-dialog(title='Add a New Prayer Request' :visible.sync='showNewVisible')
    el-form(:model='form' :label-position='top')
      el-form-item(label='Prayer Request')
        el-input(type='textarea' v-model='form.requestText' :rows='10' @blur='trimText()')
    span.dialog-footer(slot='footer')
      el-button(@click='closeDialog()') Cancel
      el-button(type='primary' @click='saveRequest()') Save
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
  methods: {
    closeDialog () {
      this.form.requestText = ''
      this.showNewVisible = false
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
      this.$message({
        message: 'New prayer request added',
        type: 'success'
      })
      this.closeDialog()
    }
  }
}
</script>
