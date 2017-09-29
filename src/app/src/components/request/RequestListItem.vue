<template lang="pug">
b-col(xs='12' sm='6' md='4')
  b-card(border-variant='dark' no-body)
    div.card-body.p-0
      p.card-text.mb-1.px-3.pt-3
        | {{ text }}
      p.card-text.p-0.pr-1.text-right: small.text-muted: em
        = '(last activity '
        date-from-now(:value='request.asOf')
        | )
    //-
      edit-request(:request='request')
      full-request(:request='request')
    b-card-footer.text-center.py-1.
      #[b-btn(@click='markPrayed()' variant='outline-primary' title='Pray' size='sm'): icon(name='check')]
      #[b-btn(variant='outline-secondary' title='Edit' size='sm'): icon(name='pencil')]
      #[b-btn(variant='outline-secondary' title='Add Notes' size='sm'): icon(name='file-text-o')]
      #[b-btn(variant='outline-secondary' title='View Full Request' size='sm'): icon(name='search')]
  br
  toast(ref='toast')
</template>

<script>
'use strict'

import moment from 'moment'

import EditRequest from './EditRequest'
import FullRequest from './FullRequest'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: {
    request: { required: true }
  },
  components: {
    EditRequest,
    FullRequest
  },
  mounted () {
    this.$refs.toast.setOptions({ position: 'bottom right' })
  },
  methods: {
    async markPrayed () {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        status: 'Prayed',
        updateText: ''
      })
      this.$refs.toast.showToast('Request marked as prayed', { theme: 'success' })
    }
  },
  computed: {
    asOf () {
      return moment(this.request.asOf).fromNow()
    },
    text () {
      return this.request.text.split('\n').join('<br>')
    }
  }
}
/*
b-row.journal-request
  b-col(cols='2'): p
    b-btn(@click='markPrayed()' size='sm' variant='outline-primary' title='Pray'): icon(name='check')
    edit-request(:request='request')
    full-request(:request='request')
  b-col(cols='8'): p {{ text }}
  b-col(cols='2'): p: date-from-now(:value='request.asOf')
 */
</script>
