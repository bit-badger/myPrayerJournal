import { provide, inject } from '@vue/composition-api'
import { Store } from 'vuex'

import { AppState } from '@/store/types'

const StoreSymbol = Symbol('Vuex store')

/** Configure the store provided by this plugin */
export function provideStore (store: Store<AppState>) {
  provide(StoreSymbol, store)
}

/** Use the provided store */
export function useStore (): Store<AppState> {
  const store = inject(StoreSymbol)
  if (!store) {
    throw new Error('No store configured!')
  }
  return store as Store<AppState>
}
