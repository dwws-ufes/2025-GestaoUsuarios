import { createI18n } from 'vue-i18n'
import messages from 'src/i18n' // Importa o index.js da pasta i18n

export default ({ app }) => {
  const i18n = createI18n({
    locale: 'pt-BR', // Idioma padrão ao iniciar
    globalInjection: true, // Para usar $t no template sem importar em cada componente (Vue Options API)
    messages, // Suas mensagens de tradução
  })

  // Torna o i18n globalmente disponível
  app.use(i18n)
}
