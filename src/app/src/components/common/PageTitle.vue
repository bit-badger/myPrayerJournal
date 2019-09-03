<template lang="pug">
h1(v-if='!hideOnPage'
   v-html='title').md-title
</template>

<script>
export default {
  name: 'page-title',
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
  watch: {
    title () {
      document.title = `${this.title.replace('&rsquo;', "'")} « myPrayerJournal`
    }
  },
  created () {
    document.title = `${this.title.replace('&rsquo;', "'")} « myPrayerJournal`
  }
}
</script>
