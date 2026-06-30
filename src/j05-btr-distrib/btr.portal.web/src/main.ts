import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import App from './App.vue'
import router from './router'
import { setUnauthorizedHandler } from '@/services/authEvents'
import { useAuthStore } from '@/stores/authStore'
import 'primeicons/primeicons.css'
import './styles/main.css'
import './styles/dashboard-tokens.css'
import './styles/investigation-workspace-tokens.css'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: false,
    },
  },
})

setUnauthorizedHandler(() => {
  useAuthStore().logout()
})

app.mount('#app')
