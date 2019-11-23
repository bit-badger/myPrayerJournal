import { inject, provide } from '@vue/composition-api'
import authService, { AuthService } from '@/auth'

export default {
  install (Vue: any) {
    Vue.prototype.$auth = authService

    Vue.mixin({
      created () {
        if (this.handleLoginEvent) {
          authService.addListener('loginEvent', this.handleLoginEvent)
        }
      },
      destroyed () {
        if (this.handleLoginEvent) {
          authService.removeListener('loginEvent', this.handleLoginEvent)
        }
      }
    })
  }
}

const AuthSymbol = Symbol('Auth service')

export function provideAuth (auth: AuthService) {
  provide(AuthSymbol, auth)
}

/** Use the auth service */
export function useAuth (): AuthService {
  const auth = inject(AuthSymbol)
  if (!auth) {
    throw new Error('Auth not configured!')
  }
  return auth as AuthService
}
