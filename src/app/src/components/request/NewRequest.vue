<template lang="pug">
  div
    el-button(@click='showNewVisible = true') Add a New Request
    el-dialog(title='Add a New Prayer Request' :visible.sync='showNewVisible')
      el-form(:model='form' :label-position='top')
        el-form-item(label='Prayer Request')
          el-input(type='textarea' v-model.trim='form.requestText' :rows='10')
      span.dialog-footer(slot='footer')
        el-button(@click='showNewVisible = false') Cancel
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
    saveRequest: async function () {
      await this.$store.dispatch(actions.ADD_REQUEST, this.form.requestText)
      this.showNewVisible = false
    }
  }
}
</script>