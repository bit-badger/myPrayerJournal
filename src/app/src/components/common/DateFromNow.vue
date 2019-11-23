<script lang="ts">
import { computed, createElement, onBeforeUnmount, onMounted, ref } from '@vue/composition-api'
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
  setup (props: any) {
    /** Interval ID for updating relative time */
    let intervalId: number = 0

    /** The relative time string */
    const fromNow = ref(moment(props.value).fromNow())

    /** The actual date/time (used as the title for the relative time) */
    const actual = computed(() => moment(props.value).format('LLLL'))

    /** Update the relative time string if it is different */
    const updateFromNow = () => {
      const newFromNow = moment(props.value).fromNow()
      if (newFromNow !== fromNow.value) fromNow.value = newFromNow
    }

    /** Refresh the relative time string to keep it accurate */
    onMounted(() => { intervalId = setInterval(updateFromNow, props.interval) })

    /** Cancel refreshing the time string represented with this component */
    onBeforeUnmount(() => clearInterval(intervalId))

    return () => createElement(props.tag, {
      domProps: {
        title: actual.value,
        innerText: fromNow.value
      }
    })
  }
}
</script>
