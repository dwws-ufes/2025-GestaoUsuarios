<template>
  <q-page class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5">{{ t('profilesPage.title') }}</div>
      <div class="row items-center">
        <q-input
          dense
          debounce="300"
          v-model="profileFilter"
          :placeholder="t('profilesPage.searchPlaceholder')"
          class="q-ml-sm"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
        <q-btn
          color="primary"
          icon="add"
          :label="t('profilesPage.newProfile')"
          class="q-ml-md"
          @click="toggleProfileForm(true)"
        />
      </div>
    </div>

    <q-card class="q-mb-md" v-show="showProfileForm">
      <q-card-section>
        <div class="text-h6 q-mb-md">
          {{ editModeProfile ? t('profilesPage.editProfile') : t('profilesPage.newProfile') }}
        </div>
        <q-form @submit.prevent="saveProfile">
          <q-input
            filled
            v-model="profileForm.nome"
            :label="t('profilesPage.name')"
            lazy-rules
            :rules="[(val) => !!val || t('profilesPage.errors.nameRequired')]"
            class="q-mb-md"
          />
          <q-input
            filled
            v-model="profileForm.descricao"
            :label="t('profilesPage.description')"
            lazy-rules
            :rules="[(val) => !!val || t('profilesPage.errors.descriptionRequired')]"
            class="q-mb-md"
          />

          <div class="q-mb-md">
            <div class="text-subtitle1">{{ t('profilesPage.permissions') }}</div>
            <q-select
              filled
              v-model="profileForm.permissoes"
              :options="commonStore.permissionsData"
              option-value="id"
              option-label="nome"
              multiple
              use-chips
              :label="t('profilesPage.permissions')"
              :loading="commonStore.isPermissionsLoading"
              :disable="commonStore.isPermissionsLoading"
            >
              <template #no-option>
                <q-item>
                  <q-item-section class="text-grey">
                    {{ t('profilesPage.noDataAvailable') }}
                  </q-item-section>
                </q-item>
              </template>
              <template #append>
                <q-btn round dense flat icon="settings" @click.stop="showPermissionManager = true">
                  <q-tooltip>{{ t('profilesPage.managePermissions') }}</q-tooltip>
                </q-btn>
              </template>
            </q-select>
          </div>

          <q-card-actions align="right" class="q-mt-md">
            <q-btn flat :label="t('profilesPage.cancel')" @click="toggleProfileForm(false)" />
            <q-btn color="primary" :label="t('profilesPage.save')" type="submit" />
          </q-card-actions>
        </q-form>
      </q-card-section>
    </q-card>

    <q-table
      :rows="filteredProfiles"
      :columns="profileColumns"
      row-key="id"
      flat
      bordered
      dense
      :loading="commonStore.isProfilesLoading"
      :no-data-label="t('profilesPage.noDataAvailable')"
    >
      <template #body-cell-acoes="props">
        <q-td :props="props" align="center">
          <q-btn
            flat
            round
            dense
            icon="edit"
            color="primary"
            @click="editProfile(props.row)"
            :aria-label="t('profilesPage.editProfile')"
          >
            <q-tooltip>{{ t('profilesPage.editProfile') }}</q-tooltip>
          </q-btn>
          <q-btn
            flat
            round
            dense
            icon="delete"
            color="negative"
            @click="confirmDeleteProfile(props.row)"
            :aria-label="t('profilesPage.confirmDeleteMessage')"
          >
            <q-tooltip>{{ t('profilesPage.deleteProfile') }}</q-tooltip>
          </q-btn>
        </q-td>
      </template>
      <template #loading>
        <q-inner-loading showing color="primary" />
      </template>
    </q-table>

    <q-dialog v-model="showPermissionManager" persistent>
      <q-card style="width: 700px; max-width: 80vw">
        <q-toolbar>
          <q-toolbar-title>{{ t('profilesPage.managePermissions') }}</q-toolbar-title>
          <q-btn flat round dense icon="close" v-close-popup @click="onPermissionManagerClose">
            <q-tooltip>{{ t('profilesPage.close') }}</q-tooltip>
          </q-btn>
        </q-toolbar>

        <q-card-section>
          <div class="row q-col-gutter-sm q-mb-md items-start">
            <q-input
              filled
              dense
              v-model="permissionForm.nome"
              :label="t('profilesPage.permissionName')"
              class="col-12 col-sm-4"
              :rules="[(val) => !!val || t('profilesPage.errors.permissionNameRequired')]"
            />
            <q-input
              filled
              dense
              v-model="permissionForm.recurso"
              :label="t('profilesPage.permissionResource')"
              class="col-12 col-sm-4"
              :rules="[(val) => !!val || t('profilesPage.errors.permissionResourceRequired')]"
            />
            <q-select
              filled
              dense
              v-model="permissionForm.acaoActionEnum"
              :options="actionEnumOptions"
              :label="t('profilesPage.permissionAction')"
              class="col-12 col-sm-4"
              :rules="[(val) => !!val || t('profilesPage.errors.permissionActionRequired')]"
            />

            <div class="col-12 row no-wrap items-start q-mt-sm">
              <q-btn
                color="primary"
                icon="add"
                :label="
                  editModePermission
                    ? t('profilesPage.updatePermission')
                    : t('profilesPage.addPermission')
                "
                @click="savePermission"
                :disable="
                  !permissionForm.nome || !permissionForm.recurso || !permissionForm.acaoActionEnum
                "
              />
              <q-btn
                v-if="editModePermission"
                flat
                dense
                icon="cancel"
                color="grey-7"
                @click="resetPermissionForm"
                class="q-ml-sm"
              >
                <q-tooltip>{{ t('profilesPage.cancelEditPermission') }}</q-tooltip>
              </q-btn>
            </div>
          </div>

          <q-table
            :rows="commonStore.permissionsData"
            :columns="permissionColumns"
            row-key="id"
            flat
            bordered
            dense
            :loading="commonStore.isPermissionsLoading"
            :no-data-label="t('profilesPage.noDataAvailable')"
          >
            <template #no-data>
              <q-item>
                <q-item-section class="text-grey">
                  {{ t('profilesPage.noDataAvailable') }}
                </q-item-section>
              </q-item>
            </template>

            <template #body-cell-acoes="props">
              <q-td :props="props" align="center">
                <q-btn
                  flat
                  round
                  dense
                  icon="edit"
                  color="primary"
                  @click="editPermission(props.row)"
                >
                  <q-tooltip>{{ t('profilesPage.editPermission') }}</q-tooltip>
                </q-btn>
                <q-btn
                  flat
                  round
                  dense
                  icon="delete"
                  color="negative"
                  @click="confirmDeletePermission(props.row)"
                >
                  <q-tooltip>{{ t('profilesPage.deletePermission') }}</q-tooltip>
                </q-btn>
              </q-td>
            </template>
            <template #loading>
              <q-inner-loading showing color="primary" />
            </template>
          </q-table>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn
            flat
            :label="t('profilesPage.close')"
            color="primary"
            v-close-popup
            @click="onPermissionManagerClose"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useCommonStore } from 'stores/common-store'

const { t } = useI18n()
const $q = useQuasar()
const route = useRoute()
const router = useRouter()
const commonStore = useCommonStore()

const profileFilter = ref('')
const showProfileForm = ref(false)
const editModeProfile = ref(false)

// profileForm agora armazenará objetos PermissaoDTO completos em 'permissoes'
const profileForm = ref({
  id: null,
  nome: '',
  descricao: '',
  permissoes: [],
})

// Permissão CRUD Dialog State
const showPermissionManager = ref(false)
const permissionForm = ref({
  id: null,
  nome: '',
  recurso: '',
  acaoActionEnum: null,
})
const editModePermission = ref(false)

// Opções para o q-select de acaoActionEnum (garanta que estes correspondam ao seu enum no backend)
const actionEnumOptions = ['Update', 'Delete', 'Read', 'Create']

const profileColumns = computed(() => [
  { name: 'nome', label: t('profilesPage.columns.name'), field: 'nome', sortable: true },
  {
    name: 'descricao',
    label: t('profilesPage.columns.description'),
    field: 'descricao',
    sortable: true,
  },
  {
    name: 'acoes',
    label: t('profilesPage.columns.actions'),
    field: 'id',
    sortable: false,
    align: 'center',
  },
])

const permissionColumns = computed(() => [
  {
    name: 'nome',
    label: t('profilesPage.permissionName'),
    field: 'nome',
    sortable: true,
    align: 'left',
  },
  {
    name: 'recurso',
    label: t('profilesPage.permissionResource'),
    field: 'recurso',
    sortable: true,
    align: 'left',
  },
  {
    name: 'acaoActionEnum',
    label: t('profilesPage.permissionAction'),
    field: 'acaoActionEnum',
    sortable: true,
    align: 'left',
  },
  {
    name: 'acoes',
    label: t('profilesPage.columns.actions'),
    field: 'id',
    sortable: false,
    align: 'center',
  },
])

const filteredProfiles = computed(() => {
  if (!profileFilter.value) return commonStore.profilesData
  const searchTerm = profileFilter.value.toLowerCase()
  return commonStore.profilesData.filter(
    (p) =>
      p.nome.toLowerCase().includes(searchTerm) || p.descricao.toLowerCase().includes(searchTerm),
  )
})

// --- Funções do CRUD de Perfis ---

function resetProfileForm() {
  profileForm.value = { id: null, nome: '', descricao: '', permissoes: [] }
}

async function toggleProfileForm(open, profile = null) {
  if (open) {
    if (profile) {
      editModeProfile.value = true
      // Ao carregar um perfil para edição, `profile.permissoes` já deve ser um array de objetos PermissaoDTO.
      // O q-select com `option-value="id"` e `option-label="nome"` espera objetos no v-model.
      profileForm.value = {
        ...profile,
        permissoes: profile.permissoes || [], // Garante que é um array, mesmo que vazio
      }
    } else {
      editModeProfile.value = false
      resetProfileForm()
    }
    showProfileForm.value = true
  } else {
    resetProfileForm()
    showProfileForm.value = false
    if (route.query.idPerfil) {
      router.replace({ query: {} })
    }
  }
}

async function saveProfile() {
  try {
    $q.loading.show({ message: t('profilesPage.loadingProfiles') })

    // Payload para enviar para a API. `profileForm.permissoes` já contém os objetos completos.
    const payload = {
      ...profileForm.value,
    }

    if (editModeProfile.value) {
      await commonStore.updateProfile(payload)
      $q.notify({ type: 'positive', message: t('profilesPage.profileUpdated') })
    } else {
      await commonStore.createProfile(payload)
      $q.notify({ type: 'positive', message: t('profilesPage.profileCreated') })
    }
    toggleProfileForm(false)
    await commonStore.fetchProfiles()
  } catch (error) {
    console.error('Error saving profile:', error)
    $q.notify({ type: 'negative', message: error.message || t('profilesPage.errors.saveFailed') })
  } finally {
    $q.loading.hide()
  }
}

function editProfile(profile) {
  router.push({ query: { idPerfil: profile.id } })
}

function confirmDeleteProfile(profile) {
  $q.dialog({
    title: t('profilesPage.confirmTitle'),
    message: t('profilesPage.confirmDeleteMessage', { name: profile.nome }),
    cancel: true,
    persistent: true,
  }).onOk(() => deleteProfile(profile.id))
}

async function deleteProfile(id) {
  try {
    $q.loading.show({ message: t('profilesPage.loadingProfiles') })
    await commonStore.deleteProfile(id)
    $q.notify({ type: 'positive', message: t('profilesPage.profileDeleted') })
    if (profileForm.value.id === id && editModeProfile.value) {
      toggleProfileForm(false)
    }
    await commonStore.fetchProfiles()
  } catch (error) {
    console.error('Error deleting profile:', error)
    $q.notify({ type: 'negative', message: error.message || t('profilesPage.errors.deleteFailed') })
  } finally {
    $q.loading.hide()
  }
}

// --- Funções do CRUD de Permissões (Aninhado) ---

function resetPermissionForm() {
  permissionForm.value = { id: null, nome: '', recurso: '', acaoActionEnum: null }
  editModePermission.value = false
}

async function savePermission() {
  try {
    if (
      !permissionForm.value.nome ||
      !permissionForm.value.recurso ||
      !permissionForm.value.acaoActionEnum
    ) {
      $q.notify({ type: 'negative', message: t('profilesPage.errors.allPermissionFieldsRequired') })
      return
    }

    $q.loading.show({ message: t('profilesPage.loadingPermissions') })
    const payload = { ...permissionForm.value }

    if (editModePermission.value) {
      await commonStore.updatePermission(payload)
      $q.notify({ type: 'positive', message: t('profilesPage.permissionUpdated') })
    } else {
      await commonStore.createPermission(payload)
      $q.notify({ type: 'positive', message: t('profilesPage.permissionCreated') })
    }
    resetPermissionForm()
    await commonStore.fetchPermissions()
  } catch (error) {
    console.error('Error saving permission:', error)
    $q.notify({
      type: 'negative',
      message: error.message || t('profilesPage.errors.savePermissionFailed'),
    })
  } finally {
    $q.loading.hide()
  }
}

function editPermission(permission) {
  editModePermission.value = true
  permissionForm.value = { ...permission }
}

function confirmDeletePermission(permission) {
  $q.dialog({
    title: t('profilesPage.confirmTitle'),
    message: t('profilesPage.confirmDeletePermissionMessage', {
      name: permission.nome,
      recurso: permission.recurso,
      acao: permission.acaoActionEnum,
    }),
    cancel: true,
    persistent: true,
  }).onOk(() => deletePermission(permission.id))
}

async function deletePermission(id) {
  try {
    $q.loading.show({ message: t('profilesPage.loadingPermissions') })
    await commonStore.deletePermission(id)
    $q.notify({ type: 'positive', message: t('profilesPage.permissionDeleted') })

    // Remove a permissão da seleção atual do perfil se ela foi excluída
    // Agora filtramos por objeto de permissão com base no Id
    profileForm.value.permissoes = profileForm.value.permissoes.filter(
      (permissionItem) => permissionItem.id !== id,
    )

    if (permissionForm.value.id === id && editModePermission.value) {
      resetPermissionForm()
    }

    await commonStore.fetchPermissions()
  } catch (error) {
    console.error('Error deleting permission:', error)
    $q.notify({
      type: 'negative',
      message: error.message || t('profilesPage.errors.deletePermissionFailed'),
    })
  } finally {
    $q.loading.hide()
  }
}

// Função chamada ao fechar o diálogo de gerenciamento de permissões
function onPermissionManagerClose() {
  resetPermissionForm()
}

// --- Watchers e Mounted ---

watch(
  () => route.query.idPerfil,
  async (id) => {
    if (id) {
      $q.loading.show({ message: t('profilesPage.loadingProfiles') })
      try {
        const profileToEdit = await commonStore.fetchProfileById(id)
        if (profileToEdit) {
          // O fetchProfileById deve retornar o perfil com permissoes como array de objetos completos.
          profileToEdit.permissoes = profileToEdit.permissoes || [] // Garante que é um array
          toggleProfileForm(true, profileToEdit)
        } else {
          $q.notify({ type: 'negative', message: t('profilesPage.errors.profileNotFound') })
          router.replace({ query: {} })
        }
      } catch (error) {
        console.error('Error loading profile for editing:', error)
        $q.notify({
          type: 'negative',
          message: error.message || t('profilesPage.errors.loadProfileFailed'),
        })
        router.replace({ query: {} })
      } finally {
        $q.loading.hide()
      }
    } else {
      toggleProfileForm(false)
    }
  },
  { immediate: true },
)

onMounted(async () => {
  await commonStore.fetchProfiles()
  await commonStore.fetchPermissions()
})
</script>

<style lang="scss" scoped>
/* Adicione estilos específicos para esta página aqui, se necessário */
</style>
