<template>
  <q-page class="flex flex-center bg-grey-2">
    <q-card class="q-pa-lg" style="min-width: 350px; max-width: 400px">
      <q-card-section>
        <div class="text-center">
          <div class="text-h4 text-weight-bold text-primary">UsersManager</div>
          <div class="text-subtitle1 text-grey-8 q-mt-sm">
            {{ $t('loginPage.systemDescription') }}
          </div>
        </div>
      </q-card-section>

      <q-card-section>
        <q-form @submit.prevent="handleLogin" class="q-gutter-md">
          <q-input
            filled
            v-model="email"
            :label="$t('loginPage.emailLabel')"
            type="email"
            required
            :rules="[
              (val) => !!val || $t('loginPage.emailRequired'),
              (val) => /.+@.+\..+/.test(val) || $t('loginPage.emailInvalid'),
            ]"
          />
          <q-input
            filled
            v-model="senha"
            :label="$t('loginPage.passwordLabel')"
            type="password"
            required
            :rules="[(val) => !!val || $t('loginPage.passwordRequired')]"
          />

          <div class="row items-center justify-between q-mt-md">
            <div class="col-grow q-pr-sm">
              <q-btn
                type="submit"
                :label="$t('loginPage.loginButton')"
                color="primary"
                class="full-width"
                :loading="commonStore.authLoading"
              />
            </div>
            <div class="col-auto">
              <q-btn flat round dense icon="language" :aria-label="t('app.language')">
                <q-tooltip>{{ $t('app.language') }}</q-tooltip>
                <q-menu>
                  <q-list style="min-width: 150px">
                    <q-item-label class="text-subtitle2 text-grey-8 q-pa-sm">
                      {{ $t('app.language') }}
                    </q-item-label>

                    <q-item
                      clickable
                      v-ripple
                      @click="changeLanguage('pt-BR')"
                      :active="locale === 'pt-BR'"
                      active-class="bg-primary text-white"
                    >
                      <q-item-section avatar>
                        <q-avatar size="24px">
                          <img src="https://flagcdn.com/w40/br.png" alt="Português" />
                        </q-avatar>
                      </q-item-section>
                      <q-item-section>{{ $t('app.portuguese') }}</q-item-section>
                    </q-item>

                    <q-item
                      clickable
                      v-ripple
                      @click="changeLanguage('en-US')"
                      :active="locale === 'en-US'"
                      active-class="bg-primary text-white"
                    >
                      <q-item-section avatar>
                        <q-avatar size="24px">
                          <img src="https://flagcdn.com/w40/us.png" alt="English" />
                        </q-avatar>
                      </q-item-section>
                      <q-item-section>{{ $t('app.english') }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-btn>
            </div>
          </div>
        </q-form>

        <q-banner v-if="commonStore.authError" class="q-mt-md text-negative bg-red-1">
          {{ commonStore.authError }}
          <template v-slot:action>
            <q-btn
              flat
              color="negative"
              :label="$t('loginPage.closeButton')"
              @click="commonStore.clearAuthError"
            />
          </template>
        </q-banner>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCommonStore } from 'stores/common-store'
import { useI18n } from 'vue-i18n'
import { LocalStorage } from 'quasar'

const email = ref('')
const senha = ref('')

const router = useRouter()
const commonStore = useCommonStore()
const { t, locale } = useI18n()

const changeLanguage = (newLang) => {
  locale.value = newLang
  LocalStorage.set('locale', newLang)
}

onMounted(() => {
  const savedLocale = LocalStorage.getItem('locale')
  if (savedLocale && savedLocale !== locale.value) {
    locale.value = savedLocale
  }
})

async function handleLogin() {
  const credentials = {
    email: email.value,
    senha: senha.value,
  }

  const success = await commonStore.login(credentials, t)

  if (success) {
    router.push('/dashboard')
  }
}
</script>

<style scoped>
.q-page {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}
</style>
