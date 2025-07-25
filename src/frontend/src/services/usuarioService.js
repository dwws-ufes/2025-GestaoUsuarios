// src/services/usuarioService.js
import { api } from 'boot/axios' // Importa a instância do Axios

const USUARIO_BASE_URL = '/api/Usuario' // Base da URL para o UsuarioController

const usuarioService = {
  /**
   * Realiza o login do usuário.
   * @param {Object} credentials - Credenciais de login (email, password).
   * @returns {Promise<Object>} Dados da resposta do login (token, nomeUsuario, perfis).
   * @throws {Error} Se o login falhar.
   */
  async login(credentials) {
    // A rota de login parece ser um pouco diferente, ajustando aqui
    try {
      const response = await api.post(USUARIO_BASE_URL + '/Login', credentials)
      return response.data
    } catch (error) {
      console.error('Erro no login:', error)
      throw error
    }
  },

  /**
   * Busca todos os usuários.
   * @returns {Promise<Array>} Lista de usuários.
   * @throws {Error} Se a requisição falhar.
   */
  async getUsuarios() {
    try {
      const response = await api.get(USUARIO_BASE_URL)
      return response.data
    } catch (error) {
      console.error('Erro ao buscar usuários:', error)
      throw error
    }
  },

  /**
   * Busca um usuário por ID.
   * @param {string} id - ID do usuário.
   * @returns {Promise<Object>} Dados do usuário.
   * @throws {Error} Se a requisição falhar.
   */
  async getUsuarioById(id) {
    try {
      const response = await api.get(`${USUARIO_BASE_URL}/${id}`)
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar usuário com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Cria um novo usuário.
   * @param {Object} userData - Dados do usuário a ser criado.
   * @returns {Promise<Object>} Usuário criado.
   * @throws {Error} Se a requisição falhar.
   */
  async createUsuario(userData) {
    try {
      const response = await api.post(USUARIO_BASE_URL, userData)
      return response.data
    } catch (error) {
      console.error('Erro ao criar usuário:', error)
      throw error
    }
  },

  /**
   * Atualiza um usuário existente.
   * @param {string} id - ID do usuário a ser atualizado.
   * @param {Object} userData - Novos dados do usuário.
   * @returns {Promise<Object>} Usuário atualizado.
   * @throws {Error} Se a requisição falhar.
   */
  async updateUsuario(id, userData) {
    try {
      const response = await api.put(`${USUARIO_BASE_URL}/${id}`, userData)
      return response.data
    } catch (error) {
      console.error(`Erro ao atualizar usuário com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Deleta um usuário.
   * @param {string} id - ID do usuário a ser deletado.
   * @returns {Promise<void>}
   * @throws {Error} Se a requisição falhar.
   */
  async deleteUsuario(id) {
    try {
      await api.delete(`${USUARIO_BASE_URL}/${id}`)
    } catch (error) {
      console.error(`Erro ao deletar usuário com ID ${id}:`, error)
      throw error
    }
  },

  /**
   * Busca dados de acessos com filtros.
   * @param {Object} params - Parâmetros de filtro (dataInicial, dataFinal, sucesso, size, sort).
   * @returns {Promise<Object>} Dados de acessos paginados.
   * @throws {Error} Se a requisição falhar.
   */
  async listarAcessosAsync(params = {}) {
    try {
      const response = await api.get(USUARIO_BASE_URL + '/acessos', { params })
      return response.data
    } catch (error) {
      console.error('Erro ao buscar acessos:', error)
      throw error
    }
  },
  async getDescricaoCargo(termo, lang) {
    const { data } = await api.get(USUARIO_BASE_URL + '/descricaoCargo', {
      params: { termo, lang },
    })
    return data
  },

  /**
   * Busca o registro RDF de um usuário por ID.
   * @param {string} id - ID do usuário.
   * @returns {Promise<string>} O conteúdo RDF do usuário.
   * @throws {Error} Se a requisição falhar.
   */
  async getUsuarioRdf(id) {
    try {
      // O backend retorna diretamente o conteúdo RDF como text/turtle
      const response = await api.get(`${USUARIO_BASE_URL}/${id}.rdf`, {
        headers: {
          Accept: 'text/turtle', // Solicita o formato Turtle
        },
      })
      return response.data
    } catch (error) {
      console.error(`Erro ao buscar RDF do usuário com ID ${id}:`, error)
      throw error
    }
  },
}

export default usuarioService
