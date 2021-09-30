<template lang="pug">
main
  p(v-if="isLoadingJournal") Loading your prayer journal&hellip;
  template(v-else)
    .card(v-if="journal.length === 0").no-requests.mt-3
      .card-body
        h5.card-title No Active Requests
        p.card-text.
          You have no requests to be shown; see the &ldquo;Active&rdquo; link above for snoozed or deferred requests,
          and the &ldquo;Answered&rdquo; link for answered requests
        router-link(:to="{ name: 'EditRequest', params: { id: 'new' } }").btn.btn-primary Add a Request
    p(v-else) There are requests
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue"
import { Actions, Mutations, useStore } from "@/store"

const store = useStore()

/** The user's prayer journal */
const journal = computed(() => store.state.journal)

/** Whether the journal is loading */
const isLoadingJournal = computed(() => store.state.isLoadingJournal)

onMounted(async () => {
  try {
    await store.dispatch(Actions.LoadJournal)
  } finally {
    store.commit(Mutations.SetTitle, `${store.state.user.given_name}&rsquo;s Prayer Journal`)
  }
})

</script>

<style lang="sass">
.no-requests
  max-width: 40rem
  margin: auto
</style>
