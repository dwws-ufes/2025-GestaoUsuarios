// boot/axios.js
import { defineBoot } from '#q-app/wrappers'
import { useCommonStore } from 'stores/common-store'
import axios from 'axios'
import { useRouter } from 'vue-router' // Importe o router

// Altere conforme o endereço real da sua API backend
const api = axios.create({ baseURL: process.env.API_URL || 'http://localhost:5001' })

// Interceptor: adiciona o token JWT nas requisições
api.interceptors.request.use(
  (config) => {
    const commonStore = useCommonStore()
    if (commonStore.token) {
      // Garanta que o token tem o prefixo 'Bearer '
      config.headers.Authorization = `Bearer ${commonStore.token.startsWith('Bearer ') ? commonStore.token.substring(7) : commonStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

// Lida com erros 401/403 (Autenticação/Autorização)
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    // <--- Adicione 'async' aqui se useRouter for assíncrono
    if (error.response && error.response.status === 401) {
      const commonStore = useCommonStore()
      const router = useRouter()

      commonStore.logout() // Limpa o token e o estado de autenticação (implemente essa lógica na sua store)
      await router.push('/login') // Redireciona para a página de login. Use `await` para garantir que o redirecionamento ocorra.
      return Promise.reject(error) // Rejeita o erro para que outros handlers possam pegá-lo
    }
    // Para outros erros (403, 500, etc.), apenas rejeita a promise
    return Promise.reject(error)
  },
)

export default defineBoot(({ app }) => {
  app.config.globalProperties.$axios = axios
  app.config.globalProperties.$api = api
})

export { api }
