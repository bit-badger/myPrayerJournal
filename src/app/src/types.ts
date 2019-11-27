import Vue from 'vue'
import { Ref } from '@vue/composition-api';

export interface SnackbarProps {
  events: Vue
  visible: Ref<boolean>
  message: Ref<string>
  interval: Ref<number>
  showSnackbar: (msg: string) => void
  showInfo: (msg: string) => void
  showError: (msg: string) => void
}

export interface ProgressProps {
  events: Vue
  visible: Ref<boolean>
  mode: Ref<string>
  showProgress: (mod: string) => void
  hideProgress: () => void
}
