<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import { useAuthStore } from '@/stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({
  userId: '',
  password: '',
})

const validationError = ref<string | null>(null)

function validate(): boolean {
  if (!form.userId.trim()) {
    validationError.value = 'User ID is required.'
    return false
  }

  if (!form.password) {
    validationError.value = 'Password is required.'
    return false
  }

  validationError.value = null
  return true
}

async function onSubmit(): Promise<void> {
  auth.error = null

  if (!validate()) {
    return
  }

  const success = await auth.login(form.userId, form.password)
  if (!success) {
    return
  }

  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/dashboard'
  await router.push(redirect)
}
</script>

<template>
  <div class="login-page">
    <Card class="login-card">
      <template #title>
        <div class="login-card__header">
          <i class="pi pi-building login-card__icon" />
          <div>
            <h1>BTR Portal</h1>
            <p>Sign in to continue</p>
          </div>
        </div>
      </template>

      <template #content>
        <form class="login-form" @submit.prevent="onSubmit">
          <Message
            v-if="validationError || auth.error"
            severity="error"
            :closable="false"
          >
            {{ validationError ?? auth.error }}
          </Message>

          <div class="field">
            <label for="userId">User ID</label>
            <InputText
              id="userId"
              v-model="form.userId"
              autocomplete="username"
              class="w-full"
              :disabled="auth.loading"
            />
          </div>

          <div class="field">
            <label for="password">Password</label>
            <Password
              id="password"
              v-model="form.password"
              :feedback="false"
              toggle-mask
              autocomplete="current-password"
              class="w-full"
              input-class="w-full"
              :disabled="auth.loading"
            />
          </div>

          <Button
            type="submit"
            label="Login"
            icon="pi pi-sign-in"
            class="w-full"
            :loading="auth.loading"
          />
        </form>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
  background:
    linear-gradient(180deg, var(--p-primary-50) 0%, var(--p-surface-50) 45%, var(--p-surface-100) 100%);
}

.login-card {
  width: 100%;
  max-width: 420px;
}

.login-card__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.login-card__header h1 {
  margin: 0;
  font-size: 1.5rem;
}

.login-card__header p {
  margin: 0.25rem 0 0;
  color: var(--p-text-muted-color);
}

.login-card__icon {
  font-size: 2rem;
  color: var(--p-primary-color);
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field label {
  font-weight: 600;
}

.w-full {
  width: 100%;
}
</style>
