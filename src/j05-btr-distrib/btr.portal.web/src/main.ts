import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import { definePreset } from '@primevue/themes'
import App from './App.vue'
import router from './router'
import { setUnauthorizedHandler } from '@/services/authEvents'
import { useAuthStore } from '@/stores/authStore'
import 'primeicons/primeicons.css'
import './styles/main.css'
import './styles/portal-brand-tokens.css'
import './styles/dashboard-tokens.css'
import './styles/investigation-workspace-tokens.css'

const BtrAura = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#fbf5f6',
      100: '#f4e4e8',
      200: '#e5c5cd',
      300: '#c98a98',
      400: '#a84d63',
      500: '#8b1e3a',
      600: '#7a1832',
      700: '#6b0f1a',
      800: '#4a0e1c',
      900: '#2a0a12',
      950: '#1a050a',
    },
  },
  components: {
    button: {
      colorScheme: {
        light: {
          root: {
            primary: {
              background: '#d4af37',
              hoverBackground: '#c9a227',
              activeBackground: '#c9a227',
              borderColor: '#d4af37',
              hoverBorderColor: '#c9a227',
              activeBorderColor: '#c9a227',
              color: '#3f2e0a',
              hoverColor: '#3f2e0a',
              activeColor: '#3f2e0a',
              focusRing: {
                color: '#3f2e0a',
                shadow: 'none',
              },
            },
          },
        },
      },
    },
  },
})

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: BtrAura,
    options: {
      darkModeSelector: false,
    },
  },
})

setUnauthorizedHandler(() => {
  useAuthStore().logout()
})

app.mount('#app')
