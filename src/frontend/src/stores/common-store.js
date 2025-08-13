// src/stores/common-store.js
import { defineStore } from 'pinia'
import { api } from 'boot/axios'
import dayjs from 'dayjs'
import localizedFormat from 'dayjs/plugin/localizedFormat'
import { i18n } from 'boot/i18n'

dayjs.extend(localizedFormat)

import perfilService from '../services/perfilService'
import usuarioService from '../services/usuarioService'

export const useCommonStore = defineStore('common', {
  state: () => ({
    // Estado de Autenticação
    token: localStorage.getItem('token') || null,
    isAuthenticated: !!localStorage.getItem('token'),
    user: JSON.parse(localStorage.getItem('user')) || { name: null, profiles: [] },
    currentProfile: localStorage.getItem('currentProfile') || null,
    authError: null,
    authLoading: false,

    // Estado para Acessos
    isAccessesLoading: false,
    accessesErrorMessage: null,
    accessesData: [],

    // Estado para Usuários
    isUsersLoading: false,
    usersErrorMessage: null,
    usersData: [],

    // Estado para o Dashboard
    isDashboardLoading: false,
    dashboardErrorMessage: null,
    dashboardStats: {
      activeUsers: 0,
      totalProfiles: 0,
      totalPermissions: 0,
      accessesToday: 0,
    },
    dashboardChartData: {
      labels: [],
      data: [],
    },
    dashboardFailedAccesses: [],

    // Estado para Perfis
    isProfilesLoading: false,
    profilesErrorMessage: null,
    profilesData: [],

    // Estado para Permissões
    isPermissionsLoading: false,
    permissionsErrorMessage: null,
    permissionsData: [],
  }),

  actions: {
    _handleApiError(error, contextMessage = 'Operation error:', type = 'action') {
      const message = error.response?.data?.message || error.message || 'An unknown error occurred.'
      console.error(`${contextMessage}`, error)
      if (type === 'fetch') {
        return message
      } else {
        throw new Error(message)
      }
    },

    async login(credentials, t) {
      this.authLoading = true
      this.authError = null
      try {
        const { token, nomeUsuario, perfis, recursos } = await usuarioService.login(credentials)

        localStorage.setItem('token', `Bearer ${token}`)
        this.token = `Bearer ${token}`

        const receivedProfiles = Array.isArray(perfis) ? perfis : []

        const userInfo = { name: nomeUsuario, profiles: receivedProfiles, roles: recursos }
        localStorage.setItem('user', JSON.stringify(userInfo))
        this.user = userInfo

        this.currentProfile = receivedProfiles.length > 0 ? receivedProfiles[0] : null
        localStorage.setItem('currentProfile', this.currentProfile)

        this.isAuthenticated = true
        api.defaults.headers.common['Authorization'] = this.token

        return true
      } catch (err) {
        this.authError = err.response?.data?.message || t('errors.loginFailed')
        this.isAuthenticated = false
        this.user = { name: null, profiles: [] }
        this.currentProfile = null
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        localStorage.removeItem('currentProfile')
        return false
      } finally {
        this.authLoading = false
      }
    },

    logout() {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      localStorage.removeItem('currentProfile')
      this.token = null
      this.isAuthenticated = false
      this.user = { name: null, profiles: [] }
      this.currentProfile = null
      delete api.defaults.headers.common['Authorization']
    },

    setCurrentProfile(profileName) {
      if (
        this.user &&
        Array.isArray(this.user.profiles) &&
        this.user.profiles.includes(profileName)
      ) {
        this.currentProfile = profileName
        localStorage.setItem('currentProfile', profileName)
      } else {
        console.warn(
          `Attempt to set an invalid or unavailable profile for the user: ${profileName}`,
        )
      }
    },

    initializeStore() {
      this.token = localStorage.getItem('token') || null
      this.isAuthenticated = !!this.token
      try {
        const storedUser = JSON.parse(localStorage.getItem('user'))
        this.user =
          storedUser && typeof storedUser === 'object' ? storedUser : { name: null, profiles: [] }
      } catch (e) {
        console.error('Failed to parse user from localStorage:', e)
        this.user = { name: null, profiles: [] }
      }

      if (!this.user.profiles || !Array.isArray(this.user.profiles)) {
        this.user.profiles = []
      }

      this.currentProfile = localStorage.getItem('currentProfile') || null
      if (!this.currentProfile || !this.user.profiles.includes(this.currentProfile)) {
        this.currentProfile = this.user.profiles.length > 0 ? this.user.profiles[0] : null
      }

      if (this.currentProfile) {
        localStorage.setItem('currentProfile', this.currentProfile)
      } else {
        localStorage.removeItem('currentProfile')
      }

      if (this.isAuthenticated && this.token) {
        api.defaults.headers.common['Authorization'] = this.token
      } else {
        delete api.defaults.headers.common['Authorization']
      }
    },

    clearAuthError() {
      this.authError = null
    },

    async fetchAccesses() {
      this.isAccessesLoading = true
      this.accessesErrorMessage = null
      try {
        const data = await usuarioService.listarAcessosAsync({
          dataInicial: dayjs().subtract(30, 'day').toISOString(),
          dataFinal: dayjs().toISOString(),
          size: 100,
          sort: 'dataHora,desc',
        })
        this.accessesData = data || [] // Garante que é um array, mesmo que 'data' seja null/undefined
      } catch (err) {
        this.accessesErrorMessage = this._handleApiError(err, 'Error loading accesses:', 'fetch')
      } finally {
        this.isAccessesLoading = false
      }
    },

    async fetchUsers() {
      this.isUsersLoading = true
      this.usersErrorMessage = null
      try {
        const data = await usuarioService.getUsuarios()
        this.usersData = data.map((user) => ({
          ...user,
          perfis: user.perfis || [],
        }))
      } catch (err) {
        this.usersErrorMessage = this._handleApiError(err, 'Error loading users:', 'fetch')
      } finally {
        this.isUsersLoading = false
      }
    },

    async fetchUserById(id) {
      this.isUsersLoading = true
      this.usersErrorMessage = null
      try {
        const data = await usuarioService.getUsuarioById(id)
        data.perfis = data.perfis || []
        return data
      } catch (err) {
        this.usersErrorMessage = this._handleApiError(
          err,
          `Error loading user with ID ${id}:`,
          'fetch',
        )
        throw err
      } finally {
        this.isUsersLoading = false
      }
    },

    async createUser(user) {
      try {
        const payload = {
          nome: user.nome,
          email: user.email,
          senha: user.senha,
          perfis: user.perfis,
        }
        const newUserData = await usuarioService.createUsuario(payload)
        await this.fetchUsers()
        return newUserData
      } catch (err) {
        this._handleApiError(err, 'Error creating user:', 'crud')
        throw err
      }
    },

    async updateUser(user) {
      try {
        const payload = {
          id: user.id,
          nome: user.nome,
          senha: user.senha,
          email: user.email,
          perfis: user.perfis,
        }
        await usuarioService.updateUsuario(payload.id, payload)
        await this.fetchUsers()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error updating user:', 'crud')
        throw err
      }
    },

    async deleteUser(id) {
      try {
        await usuarioService.deleteUsuario(id)
        await this.fetchUsers()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error deleting user:', 'crud')
        throw err
      }
    },

    async fetchProfiles() {
      this.isProfilesLoading = true
      this.profilesErrorMessage = null
      try {
        const data = await perfilService.getPerfis()
        this.profilesData = data
      } catch (err) {
        this.profilesErrorMessage = this._handleApiError(err, 'Error loading profiles:', 'fetch')
      } finally {
        this.isProfilesLoading = false
      }
    },

    async fetchProfileById(id) {
      this.isProfilesLoading = true
      this.profilesErrorMessage = null
      try {
        const data = await perfilService.getPerfilById(id)
        return data
      } catch (err) {
        this.profilesErrorMessage = this._handleApiError(
          err,
          `Error loading profile with ID ${id}:`,
          'fetch',
        )
        throw err
      } finally {
        this.isProfilesLoading = false
      }
    },

    async createProfile(profile) {
      try {
        const newProfileData = await perfilService.createPerfil(profile)
        await this.fetchProfiles()
        return newProfileData
      } catch (err) {
        this._handleApiError(err, 'Error creating profile:', 'crud')
        throw err
      }
    },

    async updateProfile(profile) {
      try {
        await perfilService.updatePerfil(profile.id, profile)
        await this.fetchProfiles()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error updating profile:', 'crud')
        throw err
      }
    },

    async deleteProfile(id) {
      try {
        await perfilService.deletePerfil(id)
        await this.fetchProfiles()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error deleting profile:', 'crud')
        throw err
      }
    },

    async fetchPermissions() {
      this.isPermissionsLoading = true
      this.permissionsErrorMessage = null
      try {
        const data = await perfilService.getPermissions()
        this.permissionsData = data
      } catch (err) {
        this.permissionsErrorMessage = this._handleApiError(
          err,
          'Error loading permissions:',
          'fetch',
        )
      } finally {
        this.isPermissionsLoading = false
      }
    },

    async createPermission(permission) {
      try {
        const newPermissionData = await perfilService.createPermission(permission)
        await this.fetchPermissions()
        return newPermissionData
      } catch (err) {
        this._handleApiError(err, 'Error creating permission:', 'crud')
        throw err
      }
    },

    async updatePermission(permission) {
      try {
        await perfilService.updatePermission(permission.id, permission)
        await this.fetchPermissions()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error updating permission:', 'crud')
        throw err
      }
    },

    async deletePermission(id) {
      try {
        await perfilService.deletePermission(id)
        await this.fetchPermissions()
        return true
      } catch (err) {
        this._handleApiError(err, 'Error deleting permission:', 'crud')
        throw err
      }
    },

    async fetchDashboardData(forceReload = false) {
      this.isDashboardLoading = true
      this.dashboardErrorMessage = null
      try {
        if (forceReload || this.usersData.length === 0) {
          await this.fetchUsers()
        }
        if (forceReload || this.profilesData.length === 0) {
          await this.fetchProfiles()
        }
        if (forceReload || this.permissionsData.length === 0) {
          await this.fetchPermissions()
        }

        const today = dayjs()
        const sevenDaysAgo = today.subtract(6, 'day')

        // Garante que o resultado de cada Promise é um array, mesmo que a API retorne null/undefined
        const [accessesTodayRes, recentAccessesRes, lastFailedAccessesRes] = await Promise.all([
          usuarioService
            .listarAcessosAsync({
              dataInicial: today.startOf('day').toISOString(),
              dataFinal: today.endOf('day').toISOString(),
              size: 1000,
            })
            .then((res) => res || []), // <--- Adicionado .then(res => res || [])
          usuarioService
            .listarAcessosAsync({
              dataInicial: sevenDaysAgo.startOf('day').toISOString(),
              dataFinal: today.endOf('day').toISOString(),
              sucesso: true,
              size: 1000,
            })
            .then((res) => res || []), // <--- Adicionado .then(res => res || [])
          usuarioService
            .listarAcessosAsync({
              falhou: true,
              size: 5,
              sort: 'dataHora,desc',
            })
            .then((res) => res || []), // <--- Adicionado .then(res => res || [])
        ])

        // --- ATUALIZAÇÃO DOS DADOS ESTATÍSTICOS (cards de topo) ---
        this.dashboardStats.activeUsers = this.usersData.length
        this.dashboardStats.totalProfiles = this.profilesData.length
        this.dashboardStats.totalPermissions = this.permissionsData.length
        // Agora 'accessesTodayRes' tem a garantia de ser um array, então .length é seguro
        this.dashboardStats.accessesToday = accessesTodayRes.length

        // --- PREPARAÇÃO DOS DADOS PARA O GRÁFICO DE BARRAS (ApexCharts) ---
        const accessesPerDay = {}
        for (let i = 0; i < 7; i++) {
          const date = dayjs().subtract(i, 'day')
          accessesPerDay[date.format('DD/MM')] = 0
        }

        // 'recentAccessesRes' tem a garantia de ser um array, então o forEach é seguro
        recentAccessesRes.forEach((access) => {
          const formattedDate = dayjs(access.dataHora).format('DD/MM')
          if (accessesPerDay[formattedDate] !== undefined) {
            accessesPerDay[formattedDate]++
          }
        })

        const sortedDates = Object.keys(accessesPerDay).sort(
          (a, b) => dayjs(a, 'DD/MM').valueOf() - dayjs(b, 'DD/MM').valueOf(),
        )

        this.dashboardChartData.labels = sortedDates
        this.dashboardChartData.data = sortedDates.map((date) => accessesPerDay[date])

        // --- ATUALIZAÇÃO DOS ACESSOS FALHOS RECENTES (tabela) ---
        // 'lastFailedAccessesRes' tem a garantia de ser um array, então a atribuição é segura
        this.dashboardFailedAccesses = lastFailedAccessesRes
      } catch (err) {
        this.dashboardErrorMessage = this._handleApiError(
          err,
          'Error loading dashboard data:',
          'fetch',
        )
        this.dashboardStats = {
          activeUsers: 0,
          totalProfiles: 0,
          totalPermissions: 0,
          accessesToday: 0,
        }
        this.dashboardChartData = { labels: [], data: [] }
        this.dashboardFailedAccesses = []
      } finally {
        this.isDashboardLoading = false
      }
    },
    async openOntology() {
      usuarioService.openOntologyLink()
    },
    async fetchDescricaoPerfil(nomePerfil) {
      try {
        const { descricao } = await usuarioService.getDescricaoCargo(
          nomePerfil,
          i18n?.global?.locale?.value || 'pt',
        )
        return descricao || null
      } catch (err) {
        this._handleApiError(err, 'Erro ao buscar descrição do perfil via DBpedia:', 'fetch')
        return null
      }
    },
  },
})
