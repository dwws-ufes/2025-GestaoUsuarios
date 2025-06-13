<template>
  <q-dialog ref="dialogRef">
    <q-card class="q-dialog-plugin" style="width: 700px; max-width: 80vw">
      <q-card-section>
        <div class="text-h6">{{ $t('userPreferences.title') }}</div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        <q-form @submit="onOKClick" class="q-gutter-md">
          <q-toggle
            v-model="darkTheme"
            :label="$t('userPreferences.darkMode')"
            checked-icon="dark_mode"
            unchecked-icon="light_mode"
            color="primary"
          />

          <div class="q-mt-md">
            <div class="text-subtitle1">{{ $t('userPreferences.primaryColor') }}</div>

            <div class="row q-gutter-sm q-mt-sm">
              <q-btn
                v-for="colorName in primaryColors"
                :key="colorName"
                :color="colorName"
                round
                size="md"
                class="color-option"
                :class="{ 'color-option--selected': selectedPrimaryColor === colorName }"
                @click="selectedPrimaryColor = colorName"
              >
                <q-icon
                  v-if="selectedPrimaryColor === colorName"
                  name="check_circle"
                  color="white"
                />
              </q-btn>
            </div>

            <div class="q-mt-md">
              <q-input
                v-model="customPrimaryColor"
                :label="$t('userPreferences.customColor')"
                type="color"
                style="width: 150px"
                @update:model-value="selectedPrimaryColor = customPrimaryColor"
              />
            </div>
          </div>
        </q-form>
      </q-card-section>

      <q-card-actions align="right">
        <q-btn flat :label="$t('userPreferences.cancel')" @click="onCancelClick" />
        <q-btn color="primary" :label="$t('userPreferences.save')" @click="onOKClick" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useQuasar } from 'quasar'
import { useI18n } from 'vue-i18n'
import { useDialogPluginComponent, colors } from 'quasar'
import { LocalStorage } from 'quasar'

const { dialogRef, onDialogOK, onDialogCancel } = useDialogPluginComponent()
const $q = useQuasar()
const { t } = useI18n()

const darkTheme = ref($q.dark.isActive)
const selectedPrimaryColor = ref('primary')
const customPrimaryColor = ref('#1976D2')
const currentPrimaryColor = ref('')

const primaryColors = [
  'primary',
  'red-5',
  'pink-5',
  'purple-5',
  'deep-purple-5',
  'indigo-5',
  'blue-5',
  'light-blue-5',
  'cyan-5',
  'teal-5',
  'green-5',
  'light-green-5',
  'lime-5',
  'yellow-5',
  'amber-5',
  'orange-5',
  'deep-orange-5',
  'brown-5',
  'grey-7',
  'blue-grey-5',
]

watch(darkTheme, (newValue) => {
  console.log(`[Dialog] Modo escuro alterado para: ${newValue}`)
  $q.dark.set(newValue)
  setTimeout(() => {
    applyPrimaryColor(selectedPrimaryColor.value)
  }, 100)
})

watch(selectedPrimaryColor, (newColor) => {
  console.log(`[Dialog] Cor primária selecionada alterada para: ${newColor}`)
  applyPrimaryColor(newColor)
})

const applyPrimaryColor = (colorToApply) => {
  console.log(`[Dialog - applyPrimaryColor] Chamado com: ${colorToApply}`)
  const rootStyles = getComputedStyle(document.documentElement)

  let finalColorHex = ''

  if (colorToApply && colorToApply.startsWith('#')) {
    finalColorHex = colorToApply
    console.log(`[Dialog - applyPrimaryColor] Aplicando cor HEX: ${finalColorHex}`)
  } else {
    if (colorToApply === 'primary') {
      finalColorHex =
        rootStyles.getPropertyValue('--q-primary').trim() || colors.getPaletteColor('blue-7')
      console.log(`[Dialog - applyPrimaryColor] Cor 'primary' padrão/computada: ${finalColorHex}`)
    } else {
      finalColorHex = colors.getPaletteColor(colorToApply)
      console.log(
        `[Dialog - applyPrimaryColor] Cor predefinida (${colorToApply}) convertida para HEX: ${finalColorHex}`,
      )
    }
  }

  if (finalColorHex) {
    document.documentElement.style.setProperty('--q-primary', finalColorHex)
    currentPrimaryColor.value = finalColorHex
    console.log(`[Dialog - applyPrimaryColor] --q-primary setado para: ${finalColorHex}`)
  } else {
    document.documentElement.style.removeProperty('--q-primary')
    requestAnimationFrame(() => {
      currentPrimaryColor.value = rootStyles.getPropertyValue('--q-primary').trim() || '#1976D2'
    })
    console.log('[Dialog - applyPrimaryColor] Nenhuma cor válida, resetando --q-primary.')
  }
}

const savePreferences = () => {
  console.log('[Dialog] Salvando preferências...')
  LocalStorage.set('darkTheme', darkTheme.value)
  LocalStorage.set('primaryColor', selectedPrimaryColor.value)
  $q.notify({
    type: 'positive',
    message: t('userPreferences.preferencesSaved'),
    timeout: 1000,
  })
}

const loadPreferences = () => {
  console.log('[Dialog] Carregando preferências...')
  const savedDarkTheme = LocalStorage.getItem('darkTheme')
  if (savedDarkTheme !== null) {
    darkTheme.value = savedDarkTheme
    $q.dark.set(savedDarkTheme)
  }

  const savedPrimaryColor = LocalStorage.getItem('primaryColor')
  if (savedPrimaryColor) {
    selectedPrimaryColor.value = savedPrimaryColor
    if (savedPrimaryColor.startsWith('#')) {
      customPrimaryColor.value = savedPrimaryColor
    } else {
      customPrimaryColor.value = '#1976D2'
    }
    applyPrimaryColor(savedPrimaryColor)
  } else {
    selectedPrimaryColor.value = 'primary'
    customPrimaryColor.value = '#1976D2'
    const rootStyles = getComputedStyle(document.documentElement)
    requestAnimationFrame(() => {
      currentPrimaryColor.value = rootStyles.getPropertyValue('--q-primary').trim() || '#1976D2'
    })
  }
}

onMounted(() => {
  console.log('[Dialog] UserPreferencesDialog mounted.')
  loadPreferences()
})

function onOKClick() {
  savePreferences()
  onDialogOK()
}

function onCancelClick() {
  onDialogCancel()
}
</script>

<style scoped>
.color-option {
  border: 2px solid transparent;
  transition: border 0.2s ease;
}
.color-option--selected {
  border: 2px solid #333;
  box-shadow: 0 0 0 2px rgba(0, 0, 0, 0.2);
}

/* Remove a linha de estilo inline-flex, pois o input agora está em sua própria linha */
/* .q-card-section .q-input {
  display: inline-flex;
  vertical-align: middle;
} */
</style>
