<template lang="pug">
.mpj-request-card
  header.mpj-card-header(role='toolbar').
    #[button.primary(@click='markPrayed()' title='Pray'): md-icon(icon='done')]
    #[button(@click.stop='showEdit()' title='Edit'): md-icon(icon='edit')]
    #[button(@click.stop='showNotes()' title='Add Notes'): md-icon(icon='comment')]
    #[button(@click.stop='snooze()' title='Snooze Request'): md-icon(icon='schedule')]
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
      const now = Date.now()
      return Math.max(now, this.request.showAfter, this.request.snoozedUntil) === now
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
      this.$router.push({ name: 'EditRequest', params: { id: this.request.requestId } })
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
  background-image: -webkit-gradient(linear, left top, left bottom, from(lightgray), to(whitesmoke));
  background-image: -webkit-linear-gradient(top, lightgray, whitesmoke);
  background-image: -moz-linear-gradient(top, lightgray, whitesmoke);
  background-image: linear-gradient(to bottom, lightgray, whitesmoke);
}
.mpj-card-header button {
  margin: .25rem;
  padding: 0 .25rem;
}
.mpj-card-header button .material-icons {
  font-size: 1.3rem;
}
.mpj-request-card .card-text {
  margin-left: 1rem;
  margin-right: 1rem;
}
.mpj-request-card .as-of {
  margin-right: .25rem;
}
</style>
