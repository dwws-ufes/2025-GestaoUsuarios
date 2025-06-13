// src/services/perfilService.js
import { api } from 'boot/axios' // Importa a instância do Axios

const PERFIL_BASE_URL = '/api/Perfil' // Base da URL para o PerfilController
const PERMISSAO_BASE_URL = PERFIL_BASE_URL + '/permissoes' // Base da URL para o PermissaoController (melhor ter um controller separado para Permissões, se houver)

// Nota: Se o backend de permissões for um sub-recurso de Perfil,
// por exemplo, /api/Perfil/{id}/Permissoes, o PERMISSAO_BASE_URL precisaria ser dinâmico
// ou as funções de permissão seriam chamadas pelo perfilService com o ID do perfil.
// Pelo seu código anterior, o endpoint '/api/Perfil/permissoes' sugere que Permissões
// são um recurso de nível superior, gerenciado de forma independente dos perfis,
// mas apenas referenciado por eles. Vou seguir essa premissa.

const perfilService = {
  /**
   * Busca todos os perfis.
   * @returns {Promise<Array>} Lista de perfis.
   * @throws {Error} Se a requisição falhar.
   */
  async getPerfis() {
    try {
      const response = await api.get(PERFIL_BASE_URL)
      return response.data
    } catch (error) {
      console.error('Erro ao buscar perfis:', error)
      throw error // Lança o erro para ser tratado pela Pinia store ou componente
    }
  },

  /**
   * Busca um perfil por ID.
   * @param {string} id - ID do perfil.
   * @returns {Promise<Object>} Dados do perfil.
   * @throws {Error} Se a requisição falhar.
   */
  async getPerfilById(id) {
    try {
      const response = await api.get(`${PERFIL_BASE_URL}/${id}`)
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar perfil com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Cria um novo perfil.
   * @param {Object} perfilData - Dados do perfil a ser criado.
   * @returns {Promise<Object>} Perfil criado.
   * @throws {Error} Se a requisição falhar.
   */
  async createPerfil(perfilData) {
    try {
      const response = await api.post(PERFIL_BASE_URL, perfilData)
      return response.data
    } catch (error) {
      console.error('Erro ao criar perfil:', error)
      throw error
    }
  },

  /**
   * Atualiza um perfil existente.
   * @param {string} id - ID do perfil a ser atualizado.
   * @param {Object} perfilData - Novos dados do perfil.
   * @returns {Promise<Object>} Perfil atualizado.
   * @throws {Error} Se a requisição falhar.
   */
  async updatePerfil(id, perfilData) {
    try {
      const response = await api.put(`${PERFIL_BASE_URL}/${id}`, perfilData)
      return response.data
    } catch (error) {
      console.error(`Erro ao atualizar perfil com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Deleta um perfil.
   * @param {string} id - ID do perfil a ser deletado.
   * @returns {Promise<void>}
   * @throws {Error} Se a requisição falhar.
   */
  async deletePerfil(id) {
    try {
      await api.delete(`${PERFIL_BASE_URL}/${id}`)
    } catch (error) {
      console.error(`Erro ao deletar perfil com ID ${id}:`, error)
      throw error
    }
  },

  // --- Funções de Permissões ---

  /**
   * Busca todas as permissões.
   * @returns {Promise<Array>} Lista de permissões.
   * @throws {Error} Se a requisição falhar.
   */
  async getPermissions() {
    try {
      // Usando PERMISSAO_BASE_URL que foi adicionado.
      // Se o endpoint for '/api/Perfil/permissoes', você pode manter como estava:
      // const response = await api.get(PERFIL_BASE_URL + '/permissoes');
      const response = await api.get(PERMISSAO_BASE_URL)
      return response.data
    } catch (error) {
      console.error('Erro ao buscar permissões:', error)
      throw error
    }
  },

  /**
   * Cria uma nova permissão.
   * @param {Object} permissionData - Dados da permissão a ser criada ({ nome, descricao }).
   * @returns {Promise<Object>} Permissão criada.
   * @throws {Error} Se a requisição falhar.
   */
  async createPermission(permissionData) {
    try {
      const response = await api.post(PERMISSAO_BASE_URL, permissionData)
      return response.data
    } catch (error) {
      console.error('Erro ao criar permissão:', error)
      throw error
    }
  },

  /**
   * Atualiza uma permissão existente.
   * @param {string} id - ID da permissão a ser atualizada.
   * @param {Object} permissionData - Novos dados da permissão ({ nome, descricao }).
   * @returns {Promise<Object>} Permissão atualizada.
   * @throws {Error} Se a requisição falhar.
   */
  async updatePermission(id, permissionData) {
    try {
      const response = await api.post(PERMISSAO_BASE_URL, permissionData)
      return response.data
    } catch (error) {
      console.error(`Erro ao atualizar permissão com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Deleta uma permissão.
   * @param {string} id - ID da permissão a ser deletada.
   * @returns {Promise<void>}
   * @throws {Error} Se a requisição falhar.
   */
  async deletePermission(id) {
    try {
      await api.delete(`${PERMISSAO_BASE_URL}/${id}`)
    } catch (error) {
      console.error(`Erro ao deletar permissão com ID ${id}:`, error)
      throw error
    }
  },
}

export default perfilService
