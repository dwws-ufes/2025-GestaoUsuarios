<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          :aria-label="t('navigation.menu')"
          @click="toggleLeftDrawer"
        />

        <q-toolbar-title> {{ t('app.title') }} </q-toolbar-title>

        <q-space />

        <div class="q-pr-md text-weight-bold">
          {{ t('app.greeting', { name: commonStore.user?.name || t('app.guest') }) }}
        </div>

        <q-btn flat round dense icon="person">
          <q-menu>
            <q-list style="min-width: 200px">
              <q-item clickable v-ripple @click="showPreferencesDialog">
                <q-item-section avatar>
                  <q-icon name="settings" />
                </q-item-section>
                <q-item-section>{{ t('userPreferences.title') }}</q-item-section>
              </q-item>

              <q-separator />

              <q-item-label class="text-subtitle2 text-grey-8 q-pa-sm">
                {{ t('app.language') }}
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
                <q-item-section>{{ t('app.portuguese') }}</q-item-section>
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
                <q-item-section>{{ t('app.english') }}</q-item-section>
              </q-item>

              <q-separator />

              <q-item v-if="commonStore.currentProfile" dense>
                <q-item-section>
                  <q-item-label caption
                    >{{ t('app.profileLabel') }}: {{ commonStore.currentProfile }}</q-item-label
                  >
                </q-item-section>
              </q-item>

              <q-separator />

              <q-item clickable v-ripple @click="handleLogout">
                <q-item-section avatar>
                  <q-icon name="logout" />
                </q-item-section>
                <q-item-section>{{ t('app.logout') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above bordered>
      <q-list>
        <q-item-label header> {{ t('navigation.header') }} </q-item-label>

        <q-item clickable v-ripple to="/dashboard">
          <q-item-section avatar>
            <q-icon name="dashboard" />
          </q-item-section>
          <q-item-section> {{ t('navigation.dashboard') }} </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/usuarios">
          <q-item-section avatar>
            <q-icon name="people" />
          </q-item-section>
          <q-item-section> {{ t('navigation.users') }} </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/perfis">
          <q-item-section avatar>
            <q-icon name="badge" />
          </q-item-section>
          <q-item-section> {{ t('navigation.profiles') }} </q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/acessos">
          <q-item-section avatar>
            <q-icon name="history" />
          </q-item-section>
          <q-item-section> {{ t('navigation.accesses') }} </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCommonStore } from 'stores/common-store'
import { useI18n } from 'vue-i18n'
// *** IMPORTANTE: Adicione 'colors' aqui ***
import { useQuasar, LocalStorage, colors } from 'quasar'
import UserPreferencesDialog from 'components/UserPreferencesDialog.vue'

const { locale, t } = useI18n()
const $q = useQuasar()
const leftDrawerOpen = ref(false)
const router = useRouter()
const commonStore = useCommonStore()

function changeLanguage(lang) {
  locale.value = lang
  LocalStorage.set('locale', lang)
}

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value
}

async function handleLogout() {
  await commonStore.logout()
  router.push('/login')
}

function showPreferencesDialog() {
  console.log('[MainLayout] Tentando abrir UserPreferencesDialog...')
  $q.dialog({
    component: UserPreferencesDialog,
  })
    .onOk(() => {
      console.log(
        '[MainLayout] Diálogo de Preferências: Salvo! Reaplicando preferências no layout...',
      )
      applySavedPreferences() // Garante que as preferências são aplicadas após salvar
    })
    .onCancel(() => {
      console.log('[MainLayout] Diálogo de Preferências: Cancelado.')
    })
    .onDismiss(() => {
      console.log('[MainLayout] Diálogo de Preferências: Escondido (fechado por qualquer motivo).')
    })
}

// *** FUNÇÃO CHAVE PARA A PERSISTÊNCIA - LÓGICA ATUALIZADA ***
function applySavedPreferences() {
  console.log('[MainLayout] applySavedPreferences: Iniciando aplicação de preferências salvas.')

  // 1. Aplicar Modo Escuro
  const savedDarkTheme = LocalStorage.getItem('darkTheme')
  if (savedDarkTheme !== null) {
    console.log(`[MainLayout] Aplicando modo escuro salvo: ${savedDarkTheme}`)
    $q.dark.set(savedDarkTheme)
  } else {
    console.log(
      '[MainLayout] Nenhuma preferência de modo escuro salva. Usando padrão do sistema/navegador.',
    )
  }

  // 2. Aplicar Cor Primária
  const savedPrimaryColor = LocalStorage.getItem('primaryColor')
  if (savedPrimaryColor) {
    console.log(`[MainLayout] Tentando aplicar cor primária salva: ${savedPrimaryColor}`)
    const rootStyles = getComputedStyle(document.documentElement)
    let finalColorHex = ''

    if (savedPrimaryColor.startsWith('#')) {
      // Se a cor salva é um HEX
      finalColorHex = savedPrimaryColor
      console.log(`[MainLayout] Cor primária é HEX: ${finalColorHex}`)
    } else {
      // Se a cor salva é um nome de cor Quasar (ex: 'red-5', 'primary')
      if (savedPrimaryColor === 'primary') {
        // Para 'primary', tenta pegar o valor computado atual ou um fallback para o azul padrão do Quasar
        finalColorHex =
          rootStyles.getPropertyValue('--q-primary').trim() || colors.getPaletteColor('blue-7')
        console.log(`[MainLayout] Cor primária é 'primary' (computada): ${finalColorHex}`)
      } else {
        // Para outras cores nomeadas (red-5, blue-5, etc.), use colors.getPaletteColor para obter o HEX
        finalColorHex = colors.getPaletteColor(savedPrimaryColor)
        console.log(
          `[MainLayout] Cor primária é nomeada (${savedPrimaryColor}), convertida para HEX: ${finalColorHex}`,
        )
      }
    }

    if (finalColorHex) {
      // *** IMPORTANTE: Pequeno atraso para garantir que o Quasar já carregou suas cores padrão
      // antes de sobrescrever com a cor salva. Isso pode resolver problemas de timing. ***
      setTimeout(() => {
        document.documentElement.style.setProperty('--q-primary', finalColorHex)
        console.log(`[MainLayout] --q-primary setado para: ${finalColorHex}`)
      }, 50) // Atraso de 50ms pode ser suficiente
    } else {
      console.warn(
        `[MainLayout] Não foi possível determinar HEX para a cor salva: ${savedPrimaryColor}. Resetando para padrão.`,
      )
      // Se por algum motivo não conseguimos uma cor válida, voltamos ao padrão do Quasar
      document.documentElement.style.removeProperty('--q-primary')
    }
  } else {
    // Se não há cor primária salva no LocalStorage, garanta que qualquer customização anterior seja removida
    // para que o Quasar use a cor primária padrão de seu tema.
    document.documentElement.style.removeProperty('--q-primary')
    console.log('[MainLayout] Nenhuma preferência de cor primária salva. Usando padrão do Quasar.')
  }

  // Lógica para fontSize (se aplicável, com base nos seus últimos códigos)
  const savedFontSize = LocalStorage.getItem('fontSize')
  if (savedFontSize) {
    document.body.classList.forEach((cls) => {
      if (cls.startsWith('text-')) {
        document.body.classList.remove(cls)
      }
    })
    document.body.classList.add(savedFontSize)
    console.log(`[MainLayout] Tamanho da fonte aplicado: ${savedFontSize}`)
  }
}

// *** CHAMADA IMPORTANTE: APLICAR PREFERÊNCIAS AO MONTAR O LAYOUT ***
onMounted(() => {
  console.log('[MainLayout] Layout montado. Inicializando store e aplicando preferências.')
  commonStore.initializeStore()
  applySavedPreferences() // Isso deve ser chamado aqui!
})
</script>

<style lang="scss" scoped>
.q-toolbar-title {
  font-weight: 500;
  font-size: 1.5rem;
}
.text-weight-bold {
  font-weight: bold;
}
</style>
