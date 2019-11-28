<template lang="pug">
h1(v-if='!hideOnPage'
   v-html='title').md-title
</template>

<script lang="ts">
import { createComponent, watch, ref } from '@vue/composition-api'

export default createComponent({
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
  setup (props) {
    watch(ref(props.title), (title: string, prevTitle: string) => {
      document.title = `${props.title.replace('&rsquo;', "'")} « myPrayerJournal`
    })
    return { }
  }
})
</script>
