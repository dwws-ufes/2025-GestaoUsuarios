export default {
  app: {
    title: 'UsersManager',
    greeting: 'Olá, {name}',
    guest: 'Visitante',
    manageProfile: 'Gerenciar Perfil',
    language: 'Idioma / Language',
    portuguese: 'Português (BR)',
    english: 'English (US)',
    profileLabel: 'Perfil',
    logout: 'Sair',
    logoutSuccess: 'Sessão encerrada com sucesso!',
  },
  navigation: {
    header: 'Navegação',
    menu: 'Menu',
    dashboard: 'Dashboard',
    users: 'Usuários',
    profiles: 'Perfis',
    accesses: 'Acessos',
  },
  accessesPage: {
    title: 'Histórico de Acessos',
    userColumn: 'Usuário',
    dateTimeColumn: 'Data/Hora',
    ipColumn: 'IP',
    agentColumn: 'Agente',
    resultColumn: 'Resultado',
    success: 'Sucesso',
    failure: 'Falha',
    noDataAvailable: 'Sem dados disponíveis',
    searchPlaceholder: 'Buscar por usuário ou ip ...',
    exportAccesses: 'Exportar Histórico de Acessos para CSV',
  },
  dashboardPage: {
    title: 'Dashboard',
    updateData: 'Atualizar Dados',
    activeUsers: 'Usuários Ativos',
    viewUsers: 'Ver Usuários',
    totalProfiles: 'Total de Perfis',
    registeredProfiles: 'Perfis Cadastrados',
    viewProfiles: 'Ver Perfis',
    totalPermissions: 'Total de Permissões',
    accessesToday: 'Acessos Hoje',
    viewHistory: 'Ver Histórico',
    dailyAccessesLast7Days: 'Acessos Diários (Últimos 7 dias)',
    chartPlaceholder: 'Espaço para o componente de gráfico (ex: Chart.js, ApexCharts).',
    chartData: 'Dados para o gráfico: `{acessosChartData}`',
    noRecentAccessData: 'Nenhum dado de acesso recente para exibir o gráfico.',
    recentFailedAccessAttempts: 'Últimos Acessos com Falha',
    viewAll: 'Ver Todos',
    noRecentFailedAccess: 'Nenhum acesso com falha recente',
    failure: 'Falha',
    columnDateTime: 'Data/Hora',
    columnUser: 'Usuário',
    columnIP: 'IP',
    columnAgent: 'Agente',
    columnError: 'Erro',
    columnDate: 'Data', // Nova chave para o cabeçalho do CSV do gráfico
    accesses: 'Acessos',
    accessCount: 'Quantidade de acessos',
    exportChartData: 'Exportar Dados do Gráfico para CSV', // Nova chave
    exportFailedAccesses: 'Exportar Acessos com Falha para CSV', // Nova chave
    noDataToExport: 'Nenhum dado para exportar.', // Nova chave
    exportSuccess: 'Dados exportados com sucesso para {filename}!', // Nova chave
    exportFailed: 'Falha ao exportar dados. Seu navegador pode não suportar esta funcionalidade.', // Nova chave
  },
  usersPage: {
    title: 'Usuários',
    searchPlaceholder: 'Buscar usuários...',
    newUser: 'Novo Usuário',
    editUser: 'Editar Usuário',
    name: 'Nome',
    email: 'E-mail',
    password: 'Senha', // <-- Novo
    passwordOptional: 'Senha (opcional)', // <-- Novo
    profiles: 'Perfis',
    noProfilesAvailable: 'Nenhum perfil disponível',
    cancel: 'Cancelar',
    save: 'Salvar',
    loadingUsers: 'Carregando usuários...',
    loadingUser: 'Carregando usuário...',
    userUpdated: 'Usuário atualizado com sucesso!',
    userCreated: 'Usuário criado com sucesso!',
    saveFailed: 'Falha ao salvar usuário.',
    confirmTitle: 'Confirmação',
    confirmDeleteMessage: 'Tem certeza que deseja excluir o usuário "{name}"?',
    userDeleted: 'Usuário excluído com sucesso!',
    deleteFailed: 'Falha ao excluir usuário.',
    noDataAvailable: 'Nenhum dado disponível.',
    noProfileAssigned: 'Nenhum perfil atribuído',
    exportRdf: 'Exportar para RDF',
    rdfExportSuccess: 'RDF exportado com sucesso!',
    deleteUser: 'Excluir usuário',
    columns: {
      name: 'Nome',
      login: 'Login (E-mail)',
      profile: 'Perfil Primário',
      actions: 'Ações',
    },
    errors: {
      nameRequired: 'O nome é obrigatório.',
      emailRequired: 'O e-mail é obrigatório.',
      emailInvalid: 'E-mail inválido.',
      passwordRequired: 'A senha é obrigatória.', // <-- Novo
      passwordMinLength: 'A senha deve ter pelo menos {length} caracteres.', // <-- Novo
      profilesRequired: 'Pelo menos um perfil é obrigatório.',
      loadUserFailed: 'Falha ao carregar os dados do usuário.',
    },
  },
  columns: {
    name: 'Nome',
    login: 'Email',
    profile: 'Perfil Primário',
    actions: 'Ações',
  },

  loginPage: {
    // <-- NOVA SEÇÃO
    systemDescription: 'Sistema de Administração e Gestão de Usuários e Permissões',
    emailLabel: 'E-mail',
    passwordLabel: 'Senha',
    loginButton: 'Entrar',
    closeButton: 'Fechar',
    selectLanguage: 'Selecionar Idioma',
    portuguese: 'Português (BR)',
    english: 'Inglês (US)',
    emailRequired: 'O campo E-mail é obrigatório.',
    emailInvalid: 'Por favor, insira um E-mail válido.',
    passwordRequired: 'O campo Senha é obrigatório.',
  },
  userPreferences: {
    title: 'Preferências de Interface',
    darkMode: 'Modo Escuro',
    primaryColor: 'Cor Primária da Interface',
    customColor: 'Cor Personalizada (HEX)',
    fontSize: 'Tamanho da Fonte',
    smallFont: 'Pequena',
    mediumFont: 'Média',
    largeFont: 'Grande',
    componentDensity: 'Densidade dos Componentes',
    comfyDensity: 'Confortável (Padrão)',
    compactDensity: 'Compacta',
    save: 'Salvar',
    cancel: 'Cancelar',
    preferencesSaved: 'Preferências salvas com sucesso!',
  },
  profilesPage: {
    title: 'Gerenciamento de Perfis',
    searchPlaceholder: 'Buscar perfis...',
    newProfile: 'Novo Perfil',
    editProfile: 'Editar Perfil',
    name: 'Nome do Perfil',
    description: 'Descrição do Perfil',
    permissions: 'Permissões',
    managePermissions: 'Gerenciar Permissões',
    noDataAvailable: 'Nenhum dado disponível',
    cancel: 'Cancelar',
    save: 'Salvar',
    close: 'Fechar',
    loadingProfiles: 'Carregando perfis...',
    profileUpdated: 'Perfil atualizado com sucesso!',
    profileCreated: 'Perfil criado com sucesso!',
    profileDeleted: 'Perfil deletado com sucesso!',
    confirmTitle: 'Confirmação',
    confirmDeleteMessage: "Tem certeza que deseja deletar o perfil '{name}'?",
    deleteProfile: 'Deletar Perfil',
    dbpediaInfoTitle: 'Descrição do DBpedia',
    exportRdf: 'Exportar para RDF',
    exportSuccess: 'RDF exportado com sucesso!',
    loadingExport: 'Gerando arquivo RDF ...',
    dbpediaNotFound: 'Termo não encontrado no DBpedia',
    loadingDbpedia: 'Buscando no DBpedia...',
    errors: {
      nameRequired: 'O nome do perfil é obrigatório.',
      descriptionRequired: 'A descrição do perfil é obrigatória.',
      saveFailed: 'Falha ao salvar o perfil.',
      deleteFailed: 'Falha ao deletar o perfil.',
      profileNotFound: 'Perfil não encontrado.',
      loadProfileFailed: 'Falha ao carregar o perfil para edição.',
      permissionNameRequired: 'O nome da permissão é obrigatório.',
      permissionResourceRequired: 'O recurso da permissão é obrigatório.',
      permissionActionRequired: 'A ação da permissão é obrigatória.',
      savePermissionFailed: 'Falha ao salvar a permissão.',
      deletePermissionFailed: 'Falha ao deletar a permissão.',
      allPermissionFieldsRequired: 'Por favor, preencha todos os campos da permissão.',
    },
    columns: {
      name: 'Nome',
      description: 'Descrição',
      actions: 'Ações',
    },
    permissionName: 'Nome da Permissão',
    permissionResource: 'Recurso',
    permissionAction: 'Ação',
    updatePermission: 'Atualizar Permissão',
    addPermission: 'Adicionar Permissão',
    cancelEditPermission: 'Cancelar Edição',
    loadingPermissions: 'Carregando permissões...',
    permissionUpdated: 'Permissão atualizada com sucesso!',
    permissionCreated: 'Permissão criada com sucesso!',
    permissionDeleted: 'Permissão deletada com sucesso!',
    editPermission: 'Editar Permissão',
    deletePermission: 'Deletar Permissão',
    confirmDeletePermissionMessage:
      "Tem certeza que deseja deletar a permissão '{name}' para o recurso '{recurso}' com ação '{acao}'?",
  },
  errors: {
    loginFailed: 'Não foi possível realizar o login',
    fetchFailed: 'Erro ao carregar dados: {message}',
    loadDashboardFailed: 'Falha ao carregar dados do dashboard.',
  },
}
