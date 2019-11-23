import VueRouter from 'vue-router'
import { inject, provide } from '@vue/composition-api'

const RouterSymbol = Symbol('Vue router')

export function provideRouter (router: VueRouter) {
  provide(RouterSymbol, router)
}

export function useRouter (): VueRouter {
  const router = inject(RouterSymbol)
  if (!router) {
    throw new Error('Router not configured!')
  }
  return router as VueRouter
}
