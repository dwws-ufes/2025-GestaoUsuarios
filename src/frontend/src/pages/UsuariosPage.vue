<template>
  <q-page class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5">{{ t('usersPage.title') }}</div>
      <div class="row items-center">
        <q-input
          dense
          debounce="300"
          v-model="userFilter"
          :placeholder="t('usersPage.searchPlaceholder')"
          class="q-ml-sm"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
        <q-btn
          color="primary"
          icon="add"
          :label="t('usersPage.newUser')"
          class="q-ml-md"
          @click="toggleUserForm(true)"
        />
      </div>
    </div>

    <q-card class="q-mb-md" v-show="showUserForm">
      <q-card-section>
        <div class="text-h6 q-mb-md">
          {{ editModeUser ? t('usersPage.editUser') : t('usersPage.newUser') }}
        </div>
        <q-form @submit.prevent="saveUser">
          <q-input
            filled
            v-model="userForm.nome"
            :label="t('usersPage.name')"
            lazy-rules
            :rules="[(val) => !!val || t('usersPage.errors.nameRequired')]"
            class="q-mb-md"
          />
          <q-input
            filled
            v-model="userForm.email"
            :label="t('usersPage.email')"
            type="email"
            lazy-rules
            :rules="[
              (val) => !!val || t('usersPage.errors.emailRequired'),
              (val) => /.+@.+\..+/.test(val) || t('usersPage.errors.emailInvalid'),
            ]"
            class="q-mb-md"
          />

          <q-input
            filled
            v-model="userForm.senha"
            :label="t(editModeUser ? 'usersPage.passwordOptional' : 'usersPage.password')"
            type="password"
            lazy-rules
            :rules="[
              // Senha é obrigatória apenas para NOVOS usuários
              (val) => (editModeUser ? true : !!val) || t('usersPage.errors.passwordRequired'),
              // Regra opcional: mínimo de 6 caracteres se preenchido
              (val) =>
                !val || val.length >= 6 || t('usersPage.errors.passwordMinLength', { length: 6 }),
            ]"
            class="q-mb-md"
          />

          <q-select
            filled
            v-model="userForm.perfis"
            :options="commonStore.profilesData"
            option-value="id"
            option-label="nome"
            multiple
            use-chips
            :label="t('usersPage.profiles')"
            :loading="commonStore.isProfilesLoading"
            :disable="commonStore.isProfilesLoading"
            lazy-rules
            :rules="[(val) => (val && val.length > 0) || t('usersPage.errors.profilesRequired')]"
            class="q-mb-md"
          >
            <template #no-option>
              <q-item>
                <q-item-section class="text-grey">
                  {{ t('usersPage.noProfilesAvailable') }}
                </q-item-section>
              </q-item>
            </template>
          </q-select>

          <q-card-actions align="right" class="q-mt-md">
            <q-btn flat :label="t('usersPage.cancel')" @click="toggleUserForm(false)" />
            <q-btn color="primary" :label="t('usersPage.save')" type="submit" />
          </q-card-actions>
        </q-form>
      </q-card-section>
    </q-card>

    <q-table
      :rows="filteredUsers"
      :columns="userColumns"
      row-key="id"
      flat
      bordered
      dense
      :loading="commonStore.isUsersLoading"
      :no-data-label="t('usersPage.noDataAvailable')"
    >
      <template #body-cell-nomePerfil="props">
        <q-td :props="props" align="left">
          <div v-if="props.row.nomePerfil">
            <q-badge color="primary"> {{ props.row.nomePerfil }} </q-badge>
          </div>
          <div v-else class="text-grey">
            {{ t('usersPage.noProfileAssigned') }}
          </div>
        </q-td>
      </template>

      <template #body-cell-acoes="props">
        <q-td :props="props" align="center">
          <q-btn
            flat
            round
            dense
            icon="edit"
            color="primary"
            @click="editUser(props.row)"
            :aria-label="t('usersPage.editUser')"
          >
            <q-tooltip>{{ t('usersPage.editUser') }}</q-tooltip>
          </q-btn>
          <q-btn
            flat
            round
            dense
            icon="delete"
            color="negative"
            @click="confirmDeleteUser(props.row)"
            :aria-label="t('usersPage.deleteUser')"
          >
            <q-tooltip>{{ t('usersPage.deleteUser') }}</q-tooltip>
          </q-btn>
        </q-td>
      </template>
      <template #loading>
        <q-inner-loading showing color="primary" />
      </template>
      <template #no-data>
        <div class="full-width row flex-center text-accent q-gutter-sm">
          <q-icon size="2em" name="sentiment_dissatisfied" />
          <span>{{ t('usersPage.noDataAvailable') }}</span>
          <span v-if="commonStore.usersErrorMessage" class="text-negative">{{
            commonStore.usersErrorMessage
          }}</span>
        </div>
      </template>
    </q-table>
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

const allUsers = computed(() => commonStore.usersData)
const userFilter = ref('')
const showUserForm = ref(false)
const editModeUser = ref(false)

const userForm = ref({
  id: null,
  nome: '',
  email: '',
  senha: '', // Restaurado o campo senha
  perfis: [],
})

const userColumns = [
  { name: 'nome', label: t('usersPage.columns.name'), field: 'nome', sortable: true },
  { name: 'email', label: t('usersPage.columns.login'), field: 'email', sortable: true },
  {
    name: 'nomePerfil',
    label: t('usersPage.columns.profile'),
    field: 'nomePerfil',
    sortable: true,
  },
  {
    name: 'acoes',
    label: t('usersPage.columns.actions'),
    field: 'id',
    sortable: false,
    align: 'center',
  },
]

const filteredUsers = computed(() => {
  if (!userFilter.value) {
    return allUsers.value
  }

  const searchTerm = userFilter.value.toLowerCase()
  return allUsers.value.filter(
    (u) =>
      u.nome.toLowerCase().includes(searchTerm) ||
      u.email.toLowerCase().includes(searchTerm) ||
      (u.nomePerfil && u.nomePerfil.toLowerCase().includes(searchTerm)),
  )
})

function resetUserForm() {
  userForm.value = { id: null, nome: '', email: '', senha: '', perfis: [] }
}

function toggleUserForm(open, user = null) {
  if (open) {
    if (user) {
      editModeUser.value = true
      userForm.value.id = user.id
      userForm.value.nome = user.nome
      userForm.value.email = user.email
      userForm.value.senha = '' // <--- ESTA LINHA É CRUCIAL! Ela limpa a senha no modo de edição.
      userForm.value.perfis = user.perfis ? [...user.perfis] : []
    } else {
      editModeUser.value = false
      resetUserForm() // resetUserForm também limpa userForm.senha
    }
    showUserForm.value = true
  } else {
    resetUserForm()
    showUserForm.value = false
    if (route.query.idUsuario) {
      router.replace({ query: {} })
    }
  }
}

async function fetchUsersFromStore() {
  await commonStore.fetchUsers()
  if (commonStore.usersErrorMessage) {
    $q.notify({ type: 'negative', message: commonStore.usersErrorMessage })
  }
}

async function fetchProfilesFromStore() {
  await commonStore.fetchProfiles()
  if (commonStore.profilesErrorMessage) {
    $q.notify({ type: 'negative', message: commonStore.profilesErrorMessage })
  }
}

async function saveUser() {
  try {
    $q.loading.show({ message: t('usersPage.loadingUsers') })
    const payload = { ...userForm.value }

    // === Lógica CONDICIONAL para a Senha no Payload ===
    // Se estiver em modo de edição e a senha estiver vazia,
    // remova a propriedade 'senha' do payload para não enviar.
    // O backend então manterá a senha existente.
    if (editModeUser.value && payload.senha === '') {
      delete payload.senha
    }

    if (!payload.perfis || payload.perfis.length === 0) {
      $q.notify({ type: 'negative', message: t('usersPage.errors.profilesRequired') })
      return // Impede o envio se nenhum perfil for selecionado
    }

    if (editModeUser.value) {
      await commonStore.updateUser(payload) // Envia o payload modificado (sem senha, se vazia)
      $q.notify({ type: 'positive', message: t('usersPage.userUpdated') })
    } else {
      await commonStore.createUser(payload) // Envia a senha (obrigatória para novo usuário)
      $q.notify({ type: 'positive', message: t('usersPage.userCreated') })
    }
    toggleUserForm(false)
    await fetchUsersFromStore()
  } catch (error) {
    console.error('Error saving user:', error)
    $q.notify({ type: 'negative', message: error.message || t('usersPage.errors.saveFailed') })
  } finally {
    $q.loading.hide()
  }
}

function editUser(user) {
  router.push({ query: { idUsuario: user.id } })
}

function confirmDeleteUser(user) {
  $q.dialog({
    title: t('usersPage.confirmTitle'),
    message: t('usersPage.confirmDeleteMessage', { name: user.nome }),
    cancel: true,
    persistent: true,
  }).onOk(() => deleteUser(user.id))
}

async function deleteUser(id) {
  try {
    $q.loading.show({ message: t('usersPage.loadingUsers') })
    await commonStore.deleteUser(id)
    $q.notify({ type: 'positive', message: t('usersPage.userDeleted') })
    if (userForm.value.id === id && editModeUser.value) {
      toggleUserForm(false)
    }
    await fetchUsersFromStore()
  } catch (error) {
    console.error('Error deleting user:', error)
    $q.notify({ type: 'negative', message: error.message || t('usersPage.errors.deleteFailed') })
  } finally {
    $q.loading.hide()
  }
}

watch(
  () => route.query.idUsuario,
  async (id) => {
    if (id) {
      $q.loading.show({ message: t('usersPage.loadingUser') })
      try {
        const userData = await commonStore.fetchUserById(id)
        // Garante que 'perfis' é um array vazio se não vier da API.
        // É crucial que commonStore.fetchUserById retorne PerfilDTO completos.
        userData.perfis = userData.perfis || []
        toggleUserForm(true, userData)
      } catch (error) {
        console.error('Error loading user for editing:', error)
        $q.notify({
          type: 'negative',
          message: error.message || t('usersPage.errors.loadUserFailed'),
        })
        router.replace({ query: {} })
      } finally {
        $q.loading.hide()
      }
    } else {
      toggleUserForm(false)
    }
  },
  { immediate: true },
)

onMounted(() => {
  fetchProfilesFromStore()
  fetchUsersFromStore()
})
</script>

<style lang="scss" scoped>
/* Estilos existentes */
</style>
