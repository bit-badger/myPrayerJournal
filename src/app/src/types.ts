import Vue from 'vue'
import { Ref } from '@vue/composition-api';

export interface ISnackbar {
  events: Vue
  visible: Ref<boolean>
  message: Ref<string>
  interval: Ref<number>
  showSnackbar: (msg: string) => void
  showInfo: (msg: string) => void
  showError: (msg: string) => void
}

export interface IProgress {
  events: Vue
  visible: Ref<boolean>
  mode: Ref<string>
  showProgress: (mod: string) => void
  hideProgress: () => void
}
