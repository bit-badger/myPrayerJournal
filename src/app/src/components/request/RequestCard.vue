<template lang="pug">
.mpj-request-card
  header.mpj-card-header.mpj-bg(role='toolbar').
    #[button.pray(@click='markPrayed()' title='Pray' size='sm'): md-icon(icon='done')]
    #[button(@click.stop='showEdit()' title='Edit' size='sm'): md-icon(icon='edit')]
    #[button(@click.stop='showNotes()' title='Add Notes' size='sm'): md-icon(icon='comment')]
    #[button(@click.stop='snooze()' title='Snooze Request' size='sm'): md-icon(icon='schedule')]
  div
    p.card-text.mpj-request-text
      | {{ request.text }}
    p.as-of.mpj-text-right: small.mpj-muted-text: em
      = '(last activity '
      date-from-now(:value='request.asOf')
      | )
</template>

<script>
'use strict'

import actions from '@/store/action-types'

export default {
  name: 'request-card',
  props: {
    request: { required: true },
    toast: { required: true },
    events: { required: true }
  },
  computed: {
    shouldDisplay () {
      return Date.now() >= this.request.showAfter
    }
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
    },
    snooze () {
      this.events.$emit('snooze', this.request.requestId)
    }
  }
}
</script>

<style>
.mpj-request-card {
  border: solid 1px darkgray;
  border-radius: 5px;
  width: 20rem;
  margin: .5rem;
}
@media screen and (max-width: 20rem) {
  .mpj-request-card {
    width: 100%;
  }
}
.mpj-card-header {
  display: flex;
  flex-flow: row;
  justify-content: center;
}
.mpj-card-header button {
  background-color: rgba(255, 255, 255, .75);
  border-radius: .25rem;
  margin: .25rem;
  border: solid #050 1px;
  font-size: .8rem;
}
.mpj-card-header button:hover {
  cursor: pointer;
  background-color: white;
  color: #050;
}
.mpj-card-header button.pray {
  background-color: white;
}
.mpj-request-card .card-text {
  margin-left: 1rem;
  margin-right: 1rem;
}
.mpj-request-card .as-of {
  margin-right: .25rem;
}
</style>
