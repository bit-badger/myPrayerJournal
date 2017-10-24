<template lang="pug">
b-col(md='6' lg='4')
  .mpj-request-card
    b-card-header.text-center.py-1.
      #[b-btn(@click='markPrayed()' variant='outline-primary' title='Pray' size='sm'): icon(name='check')]
      #[b-btn(@click.stop='showEdit()' variant='outline-secondary' title='Edit' size='sm'): icon(name='pencil')]
      #[b-btn(@click.stop='showNotes()' variant='outline-secondary' title='Add Notes' size='sm'): icon(name='file-text-o')]
      #[b-btn(@click.stop='showFull()' variant='outline-secondary' title='View Full Request' size='sm'): icon(name='search')]
    b-card-body.p-0
      p.card-text.mpj-request-text.mb-1.px-3.pt-3
        | {{ request.text }}
      p.card-text.p-0.pr-1.text-right: small.text-muted: em
        = '(last activity '
        date-from-now(:value='request.asOf')
        | )
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: {
    request: { required: true },
    toast: { required: true },
    events: { required: true }
  },
  methods: {
    async markPrayed () {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.request.requestId,
        status: 'Prayed',
        updateText: ''
      })
      this.toast.showToast('Request marked as prayed', { theme: 'success' })
    },
    showEdit () {
      this.events.$emit('edit', this.request)
    },
    showFull () {
      this.events.$emit('full', this.request.requestId)
    },
    showNotes () {
      this.events.$emit('notes', this.request)
    }
  }
}
</script>

<style>
.mpj-request-card {
  border: solid 1px darkgray;
  border-radius: 5px;
  margin-bottom: 15px;
}
</style>
