<script lang="ts">
import { computed, createComponent, createElement, onBeforeUnmount, onMounted, ref } from '@vue/composition-api'
import { format, formatDistance } from 'date-fns'

export default createComponent({
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
  setup (props) {
    /** Interval ID for updating relative time */
    let intervalId: number = 0

    /** The relative time string */
    const fromNow = ref(formatDistance(props.value, Date.now(), { addSuffix: true }))

    /** The actual date/time (used as the title for the relative time) */
    const actual = computed(() => format(props.value, 'PPPPp'))

    /** Update the relative time string if it is different */
    const updateFromNow = () => {
      const newFromNow = formatDistance(props.value, Date.now(), { addSuffix: true })
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
})
</script>
