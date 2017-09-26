<script>
'use strict'

import moment from 'moment'

export default {
  name: 'date-from-now',
  props: {
    tag: {
      type: String,
      default: 'span'
    },
    value: {
      type: Number,
      default: 0
    },
    interval: {
      type: Number,
      default: 10000
    }
  },
  data () {
    return {
      fromNow: moment(this.value).fromNow(),
      intervalId: null
    }
  },
  mounted () {
    this.intervalId = setInterval(this.updateFromNow, this.interval)
    this.$watch('value', this.updateFromNow)
  },
  beforeDestroy () {
    clearInterval(this.intervalId)
  },
  methods: {
    updateFromNow () {
      let newFromNow = moment(this.value).fromNow()
      if (newFromNow !== this.fromNow) this.fromNow = newFromNow
    }
  },
  render (createElement) {
    return createElement(this.tag, this.fromNow)
  }
}
</script>
