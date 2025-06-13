#!/bin/bash

# Script de inicialização do ambiente de desenvolvimento para Linux/macOS

# Cores para saída no terminal
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}--- Iniciando Verificação de Pré-requisitos e Ambiente ---${NC}"

# 1. Executar o script de instalação de pré-requisitos
echo -e "${YELLOW}Executando script de verificação e instalação de pré-requisitos...${NC}"
# Usamos sudo para garantir que o script de pré-requisitos possa instalar
# Ele mesmo fará as verificações de permissão e prompts.
sudo ./install_prerequisites.sh

# Verifica o status de saída do script de pré-requisitos
if [ $? -ne 0 ]; then
    echo -e "${RED}Erro: Falha na verificação ou instalação de pré-requisitos. Abortando inicialização do ambiente.${NC}"
    echo -e "${RED}Por favor, resolva os problemas e execute este script novamente.${NC}"
    exit 1
fi

echo -e "${GREEN}Pré-requisitos verificados/instalados com sucesso.${NC}"

# 2. Iniciar o Backend (.NET)
echo -e "${CYAN}\n--- Iniciando Backend .NET ---${NC}"
echo -e "${YELLOW}Executando: dotnet run --project ./backend/UsersManager.Application/UsersManager.Application.csproj${NC}"
# Usamos 'nohup' e '&' para que o backend rode em segundo plano e não bloqueie o terminal
# 'nohup' previne que o processo seja encerrado quando o terminal for fechado.
# Redirecionamos a saída para um arquivo de log, ou para /dev/null se não quiser logs.
nohup dotnet run --project ./backend/UsersManager.Application/UsersManager.Application.csproj > backend.log 2>&1 &
backend_pid=$! # Captura o PID do processo do backend
echo -e "${GREEN}Backend .NET iniciado em segundo plano (PID: $backend_pid). Verifique 'backend.log' para detalhes.${NC}"

# 3. Navegar para a pasta do frontend e iniciar o Quasar
echo -e "${CYAN}\n--- Iniciando Frontend Quasar ---${NC}"
echo -e "${YELLOW}Navegando para ./frontend e executando: yarn quasar dev${NC}"

# Verifica se o diretório frontend existe
if [ -d "./frontend" ]; then
    cd ./frontend
    # Verifica se yarn está instalado, se não, sugere npm
    if command_exists yarn; then
        echo -e "${GREEN}Iniciando Quasar dev com yarn...${NC}"
        # Execute o comando de desenvolvimento do Quasar.
        # Ele geralmente assume o controle do terminal, por isso não usamos nohup aqui.
        yarn quasar dev
    elif command_exists npm; then
        echo -e "${YELLOW}Yarn não encontrado, usando npm...${NC}"
        npm install # Garante que as dependências estejam instaladas
        npm run dev # Ou o comando equivalente para iniciar o Quasar dev
    else
        echo -e "${RED}Erro: Nem 'yarn' nem 'npm' foram encontrados. Por favor, instale um deles.${NC}"
        echo -e "${RED}O frontend não poderá ser iniciado automaticamente.${NC}"
        # Mata o processo do backend se o frontend não puder ser iniciado
        echo -e "${YELLOW}Matando processo do backend (PID: $backend_pid) devido à falha no frontend.${NC}"
        kill $backend_pid 2>/dev/null
        exit 1
    fi
else
    echo -e "${RED}Erro: O diretório './frontend' não foi encontrado. Abortando inicialização do frontend.${NC}"
    # Mata o processo do backend se o frontend não puder ser iniciado
    echo -e "${YELLOW}Matando processo do backend (PID: $backend_pid) devido à falha no frontend.${NC}"
    kill $backend_pid 2>/dev/null
    exit 1
fi

# Se o comando `yarn quasar dev` ou `npm run dev` for encerrado,
# ele voltará para cá e poderá ser usado para finalizar o backend.
echo -e "${CYAN}\n--- Frontend Quasar encerrado. Encerrando Backend .NET...${NC}"
kill $backend_pid 2>/dev/null # Tenta matar o processo do backend
if kill -0 $backend_pid 2>/dev/null; then # Verifica se o processo ainda está rodando
    echo -e "${RED}Não foi possível encerrar o processo do backend (PID: $backend_pid) automaticamente. Por favor, encerre-o manualmente.${NC}"
else
    echo -e "${GREEN}Backend .NET (PID: $backend_pid) encerrado com sucesso.${NC}"
fi

echo -e "${CYAN}--- Ambiente de Desenvolvimento Encerrado ---${NC}"