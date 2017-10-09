<template lang="pug">
h2.mpj-page-title(v-if='!hideOnPage'
                  v-html='title')
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
      document.title = `${this.title} « myPrayerJournal`
    }
  },
  created () {
    document.title = `${this.title} « myPrayerJournal`
  }
}
</script>

<style scoped>
.mpj-page-title {
  border-bottom: solid 1px lightgray;
  margin-bottom: 20px;
}
</style>
