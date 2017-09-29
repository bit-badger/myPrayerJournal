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
    const dt = moment(this.value)
    return {
      dt,
      fromNow: dt.fromNow(),
      actual: dt.format('LLLL'),
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
      let newFromNow = this.dt.fromNow()
      if (newFromNow !== this.fromNow) this.fromNow = newFromNow
    }
  },
  render (createElement) {
    return createElement(this.tag, {
      domProps: {
        title: this.actual,
        innerText: this.fromNow
      }
    })
  }
}
</script>
