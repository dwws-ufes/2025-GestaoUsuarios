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
   * @param {Object} profileData - Dados do perfil a ser criado.
   * @returns {Promise<Object>} Perfil criado.
   * @throws {Error} Se a requisição falhar.
   */
  async createPerfil(profileData) {
    try {
      const response = await api.post(PERFIL_BASE_URL, profileData)
      return response.data
    } catch (error) {
      console.error('Erro ao criar perfil:', error)
      throw error
    }
  },

  /**
   * Atualiza um perfil existente.
   * @param {string} id - ID do perfil a ser atualizado.
   * @param {Object} profileData - Novos dados do perfil.
   * @returns {Promise<Object>} Perfil atualizado.
   * @throws {Error} Se a requisição falhar.
   */
  async updatePerfil(id, profileData) {
    try {
      const response = await api.put(`${PERFIL_BASE_URL}/${id}`, profileData)
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

  /**
   * Lista todas as permissões disponíveis.
   * @returns {Promise<Array>} Lista de permissões.
   * @throws {Error} Se a requisição falhar.
   */
  async getPermissions() {
    try {
      const response = await api.get(PERMISSAO_BASE_URL)
      return response.data
    } catch (error) {
      console.error('Erro ao buscar permissões:', error)
      throw error
    }
  },

  /**
   * Salva permissões (cria ou atualiza).
   * @param {Object|Array} permissionData - Dados da permissão ou array de permissões.
   * @returns {Promise<Object|Array>} Permissão(ões) salva(s).
   * @throws {Error} Se a requisição falhar.
   */
  async savePermissions(permissionData) {
    try {
      // O backend espera um array, então encapsulamos se for um único objeto
      const payload = Array.isArray(permissionData) ? permissionData : [permissionData]
      const response = await api.post(PERMISSAO_BASE_URL, payload) // O método SavePermissoesAsync no backend é POST
      return response.data
    } catch (error) {
      console.error('Erro ao salvar permissão(ões):', error)
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

  /**
   * Abre uma nova aba, busca o conteúdo RDF de um perfil e o exibe em uma
   * página HTML com os URIs convertidos em links clicáveis.
   * @param {number} id O ID do perfil.
   */
  async openPerfilRdf(id) {
    try {
      // 1. Constrói a URL para o endpoint de RDF puro
      const url = `${api.defaults.baseURL}${PERFIL_BASE_URL}/${id}.rdf`

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
      console.error('Erro ao abrir o RDF do perfil:', error)
      alert(`Não foi possível carregar o conteúdo RDF. Erro: ${error.message}`)
    }
  },
}

export default perfilService
