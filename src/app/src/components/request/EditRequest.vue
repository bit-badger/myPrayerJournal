<template lang="pug">
article.mpj-main-content(role='main')
  page-title(:title='title')
  .mpj-narrow
    label(for='request_text') Prayer Request
    br
    textarea#request_text(v-model='form.requestText'
                          :rows='10'
                          @blur='trimText()'
                          autofocus)
    br
    template(v-if='!isNew')
      label Also Mark As
      br
      input(type='radio'
            id='status_updated'
            value='Updated'
            v-model='form.status')
      label(for='status_updated')= ' Updated'
      input(type='radio'
            id='status_prayed'
            value='Prayed'
            v-model='form.status')
      label(for='status_prayed')= ' Prayed'
      input(type='radio'
            id='status_answered'
            value='Answered'
            v-model='form.status')
      label(for='status_answered')= ' Answered'
    br(v-else)
    .mpj-text-right
      button(@click.stop='saveRequest()').primary
        md-icon(icon='save')
        = ' Save'
      | &nbsp; &nbsp;
      button(@click.stop='goBack()')
        md-icon(icon='arrow_back')
        = ' Cancel'
</template>

<script>
'use strict'

import { mapState } from 'vuex'

import actions from '@/store/action-types'

export default {
  name: 'edit-request',
  props: {
    id: {
      type: String,
      required: true
    }
  },
  data () {
    return {
      title: 'Edit Prayer Request',
      isNew: false,
      form: {
        requestId: '',
        requestText: '',
        status: 'Updated'
      }
    }
  },
  computed: {
    toast () {
      return this.$parent.$refs.toast
    },
    ...mapState(['journal'])
  },
  async mounted () {
    if (this.id === 'new') {
      this.title = 'Add Prayer Request'
      this.isNew = true
      this.form.requestId = ''
      this.form.requestText = ''
      this.form.status = 'Created'
    } else {
      this.title = 'Edit Prayer Request'
      this.isNew = false
      if (this.journal.length === 0) {
        await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
      }
      const req = this.journal.filter(r => r.requestId === this.id)[0]
      this.form.requestId = this.id
      this.form.requestText = req.text
      this.form.status = 'Updated'
    }
  },
  methods: {
    goBack () {
      this.$router.go(-1)
    },
    trimText () {
      this.form.requestText = this.form.requestText.trim()
    },
    async saveRequest () {
      if (this.isNew) {
        await this.$store.dispatch(actions.ADD_REQUEST, {
          progress: this.$Progress,
          requestText: this.form.requestText
        })
        this.toast.showToast('New prayer request added', { theme: 'success' })
      } else {
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
      }
      this.goBack()
    }
  }
}
</script>

<style scoped>
#request_text {
  width: 100%;
}
</style>
