<script setup lang="ts">
import { computed, h, onBeforeUnmount, onMounted, ref } from "vue"
import { format, formatDistance } from "date-fns"

/** Properties for this component */
interface Props {
  /** The tag name to be rendered (defaults to `span`) */
  tag : string
  /** The value of the date */
  value : number
  /** The interval at which the date should be refreshed (defaults to 10 seconds) */
  interval : number
}

const props = withDefaults(defineProps<Props>(), {
  tag : "span",
  value : 0,
  interval : 10000
})

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

/** Render the element */
h(props.tag, {
  domProps: {
    title: actual.value,
    innerText: fromNow.value
  }
})
</script>
