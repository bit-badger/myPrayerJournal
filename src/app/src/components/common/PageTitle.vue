<template lang="pug">
h1(v-if='!hideOnPage'
   v-html='title').md-title
</template>

<script lang="ts">
import { watch } from '@vue/composition-api'

export default {
  props: {
    title: {
      type: String,
      required: true
    },
    hideOnPage: {
      type: Boolean,
      default: false
    }
  },
  setup (props: any) {
    watch(props.title, (title, prevTitle) => {
      document.title = `${props.title.replace('&rsquo;', "'")} « myPrayerJournal`
    })
    return { }
  }
}
</script>
