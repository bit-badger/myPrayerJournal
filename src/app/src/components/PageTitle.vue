<template>
  <h2 v-if="!hideOnPage" class="mpj-page-title" v-html="title"></h2>
</template>

<script>
export default {
  name: 'page-title',
  props: ['title', 'hideOnPage'],
  data () {
    return {}
  },
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
