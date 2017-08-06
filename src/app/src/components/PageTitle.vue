<template lang="pug">
  h2.mpj-page-title(v-if="!hideOnPage" v-html="title")
</template>

<script>
export default {
  name: 'page-title',
  props: ['title', 'hideOnPage'],
  created () {
    document.title = `${this.title} « myPrayerJournal`
  },
  watch: {
    title () {
      document.title = `${this.title} « myPrayerJournal`
    }
  }
}
</script>
