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
   * Abre a ontologia do projeto.
   * @throws {Error} Se a requisição falhar.
   */
  async openOntologyLink() {
    try {
      window.open(api.getUri() + '/api/ontology/vocabulary', '_blank')
      //this.openUsuarioRdf(0, true)
    } catch (error) {
      console.error(`Erro ao abrir link`, error)
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
   * Abre uma nova aba, busca o conteúdo RDF de um usuário e o exibe em uma
   * página HTML com os URIs convertidos em links clicáveis.
   * @param {number} id O ID do usuário.
   * @param {boolean} showOntology Se true abre a ontologia do projeto.
   */
  async openUsuarioRdf(id, showOntology) {
    try {
      // 1. Constrói a URL para o endpoint de RDF puro do usuário
      const url =
        showOntology === true
          ? `${api.defaults.baseURL}/api/ontology/vocabulary`
          : `${api.defaults.baseURL}${USUARIO_BASE_URL}/${id}.rdf`

      // 2. Busca o conteúdo RDF em texto
      const response = await fetch(url)
      if (!response.ok) {
        throw new Error(`Erro ao buscar o conteúdo RDF: ${response.statusText}`)
      }
      const rdfContent = await response.text()

      // 3. Converte os URIs em links clicáveis antes de construir o HTML
      // Expressão regular para encontrar URIs que começam com http ou https.
      const uriRegex = /(https?:\/\/[^\s>]+)/g
      const linkedText = rdfContent.replace(uriRegex, (url) => {
        // Remove pontuações comuns no final do URI
        const cleanUrl = url.replace(/[.,;]$/, '')
        return `<a href="${cleanUrl}" target="_blank" rel="noopener noreferrer">${cleanUrl}</a>`
      })

      // 4. Constrói a página HTML completa com o conteúdo processado
      const htmlContent = `
<!DOCTYPE html>
<html lang="pt-br">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>RDV Viewer ${id}</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; }
        h1 { color: #333; }
        pre { background-color: #f4f4f4; border: 1px solid #ddd; padding: 10px; white-space: pre-wrap; word-wrap: break-word; }
        .rdf-container { margin-top: 20px; }
    </style>
</head>
<body>
    <div class="rdf-container"><pre>${linkedText}</pre></div>
</body>
</html>`

      // 5. Cria um Blob a partir do conteúdo HTML e uma URL temporária para ele
      const blob = new Blob([htmlContent], { type: 'text/html' })
      const blobUrl = URL.createObjectURL(blob)

      // 6. Abre uma nova aba com a URL do Blob
      const newTab = window.open(blobUrl, '_blank')
      if (!newTab) {
        console.error(
          'Não foi possível abrir a nova aba. Verifique se o bloqueador de pop-ups está ativado.',
        )
        return
      }

      // IMPORTANTE: Revogar a URL do Blob após um curto período para liberar memória
      // O evento 'unload' da nova aba é uma forma mais robusta de fazer isso.
      newTab.addEventListener('unload', () => {
        URL.revokeObjectURL(blobUrl)
      })
    } catch (error) {
      console.error('Erro ao abrir o RDF do usuário:', error)
      alert(`Não foi possível carregar o conteúdo RDF. Erro: ${error.message}`)
    }
  },
}

export default usuarioService
