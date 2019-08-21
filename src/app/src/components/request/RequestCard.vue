<template lang="pug">
md-card.mpj-request-card(v-if='shouldDisplay'
                         md-with-hover)
  md-card-actions(md-alignment='space-between')
    md-button.md-icon-button.md-raised.md-primary(@click='markPrayed()')
      md-icon done
      md-tooltip(md-direction='top'
                 md-delay=1000) Mark as Prayed
    span
      md-button.md-icon-button.md-raised(@click.stop='showEdit()')
        md-icon edit
        md-tooltip(md-direction='top'
                   md-delay=1000) Edit Request
      md-button.md-icon-button.md-raised(@click.stop='showNotes()')
        md-icon comment
        md-tooltip(md-direction='top'
                   md-delay=1000) Add Notes
      md-button.md-icon-button.md-raised(@click.stop='snooze()')
        md-icon schedule
        md-tooltip(md-direction='top'
                   md-delay=1000) Snooze Request
  md-card-content
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
  inject: [
    'journalEvents',
    'messages',
    'progress'
  ],
  props: {
    request: { required: true }
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
        progress: this.progress,
        requestId: this.request.requestId,
        status: 'Prayed',
        updateText: ''
      })
      this.messages.$emit('info', 'Request marked as prayed')
    },
    showEdit () {
      this.$router.push({ name: 'EditRequest', params: { id: this.request.requestId } })
    },
    showNotes () {
      this.journalEvents.$emit('notes', this.request)
    },
    snooze () {
      this.journalEvents.$emit('snooze', this.request.requestId)
    }
  }
}
</script>

<style>
.mpj-request-card {
/*  border: solid 1px darkgray;
  border-radius: 5px; */
  width: 20rem;
  margin-bottom: 1rem;
/*  margin: .5rem; */
}
@media screen and (max-width: 20rem) {
  .mpj-request-card {
    width: 100%;
  }
}
/*
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
} */
</style>
