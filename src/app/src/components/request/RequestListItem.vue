<template lang="pug">
div
  b-card-group.w-100(deck)
    b-card(v-for='(request, idx) in row' border-variant='dark' no-body)
      b-card-body.p-0
        p.card-text.request-card-text.mb-1.px-3.pt-3
          | {{ request.text }}
        p.card-text.p-0.pr-1.text-right: small.text-muted: em
          = '(last activity '
          date-from-now(:value='request.asOf')
          | )
      b-card-footer.text-center.py-1.
        #[b-btn(@click='markPrayed(idx)' variant='outline-primary' title='Pray' size='sm'): icon(name='check')]
        #[b-btn(@click.stop='showEdit(request)' variant='outline-secondary' title='Edit' size='sm'): icon(name='pencil')]
        #[b-btn(disabled variant='outline-secondary' title='Add Notes' size='sm'): icon(name='file-text-o')]
        #[b-btn(@click.stop='showFull(idx)' variant='outline-secondary' title='View Full Request' size='sm'): icon(name='search')]
    b-card(v-for='it in 3 - row.length')
  br
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'request-list-item',
  props: {
    row: { required: true },
    toast: { required: true },
    events: { required: true }
  },
  methods: {
    async markPrayed (idx) {
      await this.$store.dispatch(actions.UPDATE_REQUEST, {
        progress: this.$Progress,
        requestId: this.row[idx].requestId,
        status: 'Prayed',
        updateText: ''
      })
      this.toast.showToast('Request marked as prayed', { theme: 'success' })
    },
    showEdit (request) {
      this.events.$emit('edit', request)
    },
    showFull (idx) {
      this.events.$emit('full', this.row[idx].requestId)
    }
  }
}
</script>

<style>
.request-card-text {
  white-space: pre-line;
}
</style>
