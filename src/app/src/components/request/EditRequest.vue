<template lang="pug">
article.mpj-main-content(role='main')
  page-title(:title='title')
  .mpj-narrow
    label(for='request_text')
      | Prayer Request
      br
      textarea(v-model='form.requestText'
               :rows='10'
               @blur='trimText()'
               autofocus).mpj-full-width
    br
    template(v-if='!isNew')
      label Also Mark As
      br
      label.normal
        input(v-model='form.status'
              type='radio'
              name='status'
              value='Updated')
        | Updated
      | &nbsp; &nbsp;
      label.normal
        input(v-model='form.status'
              type='radio'
              name='status'
              value='Prayed')
        | Prayed
      | &nbsp; &nbsp;
      label.normal
        input(v-model='form.status'
              type='radio'
              name='status'
              value='Answered')
        | Answered
      br
    label Recurrence
    | &nbsp; &nbsp;
    em.mpj-muted-text After prayer, request reappears...
    br
    label.normal
      input(v-model='form.recur.typ'
            type='radio'
            name='recur'
            value='immediate')
      | Immediately
    | &nbsp; &nbsp;
    label.normal
      input(v-model='form.recur.typ'
            type='radio'
            name='recur'
            value='other')
      | Every...
    input(v-model='form.recur.count'
          type='number'
          :disabled='!showRecurrence').mpj-recur-count
    select(v-model='form.recur.other'
           :disabled='!showRecurrence').mpj-recur-type
      option(value='hours') hours
      option(value='days') days
      option(value='weeks') weeks
    .mpj-text-right
      button(:disabled='!isValidRecurrence'
             @click.stop='saveRequest()').primary.
        #[md-icon(icon='save')] Save
      | &nbsp; &nbsp;
      button(@click.stop='goBack()').
        #[md-icon(icon='arrow_back')] Cancel
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
        status: 'Updated',
        recur: {
          typ: 'immediate',
          other: '',
          count: ''
        }
      }
    }
  },
  computed: {
    isValidRecurrence () {
      if (this.form.recur.typ === 'immediate') return true
      const count = Number.parseInt(this.form.recur.count)
      if (isNaN(count) || this.form.recur.other === '') return false
      if (this.form.recur.other === 'hours' && count > (365 * 24)) return false
      if (this.form.recur.other === 'days' && count > 365) return false
      if (this.form.recur.other === 'weeks' && count > 52) return false
      return true
    },
    showRecurrence () {
      return this.form.recur.typ !== 'immediate'
    },
    toast () {
      return this.$parent.$refs.toast
    },
    ...mapState(['journal'])
  },
  async mounted () {
    await this.ensureJournal()
    if (this.id === 'new') {
      this.title = 'Add Prayer Request'
      this.isNew = true
      this.form.requestId = ''
      this.form.requestText = ''
      this.form.status = 'Created'
      this.form.recur.typ = 'immediate'
      this.form.recur.other = ''
      this.form.recur.count = ''
    } else {
      this.title = 'Edit Prayer Request'
      this.isNew = false
      if (this.journal.length === 0) {
        await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
      }
      const req = this.journal.filter(r => r.Id === this.id)[0]
      this.form.requestId = this.id
      this.form.requestText = req.text
      this.form.status = 'Updated'
      if (req.recurType === 'immediate') {
        this.form.recur.typ = 'immediate'
        this.form.recur.other = ''
        this.form.recur.count = ''
      } else {
        this.form.recur.typ = 'other'
        this.form.recur.other = req.recurType
        this.form.recur.count = req.recurCount
      }
    }
  },
  methods: {
    goBack () {
      this.$router.go(-1)
    },
    trimText () {
      this.form.requestText = this.form.requestText.trim()
    },
    async ensureJournal () {
      if (!Array.isArray(this.journal)) {
        await this.$store.dispatch(actions.LOAD_JOURNAL, this.$Progress)
      }
    },
    async saveRequest () {
      if (this.isNew) {
        await this.$store.dispatch(actions.ADD_REQUEST, {
          progress: this.$Progress,
          requestText: this.form.requestText,
          recurType: this.form.recur.typ === 'immediate' ? 'immediate' : this.form.recur.other,
          recurCount: this.form.recur.typ === 'immediate' ? 0 : Number.parseInt(this.form.recur.count)
        })
        this.toast.showToast('New prayer request added', { theme: 'success' })
      } else {
        await this.$store.dispatch(actions.UPDATE_REQUEST, {
          progress: this.$Progress,
          requestId: this.form.requestId,
          updateText: this.form.requestText,
          status: this.form.status,
          recurType: this.form.recur.typ === 'immediate' ? 'immediate' : this.form.recur.other,
          recurCount: this.form.recur.typ === 'immediate' ? 0 : Number.parseInt(this.form.recur.count)
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

<style>
.mpj-recur-count {
  width: 3rem;
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
.mpj-recur-type {
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
}
</style>
