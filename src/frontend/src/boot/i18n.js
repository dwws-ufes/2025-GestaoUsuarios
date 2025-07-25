// src/boot/i18n.js
import { createI18n } from 'vue-i18n'
import messages from 'src/i18n' // Importa as mensagens de tradução

// Cria a instância de i18n
export const i18n = createI18n({
  locale: 'pt-BR',
  globalInjection: true,
  messages,
})

// Exporta default como exigido pelo Quasar
export default ({ app }) => {
  app.use(i18n)
}
