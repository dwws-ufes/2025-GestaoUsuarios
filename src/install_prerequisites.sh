#!/bin/bash

# Script para verificar e instalar pré-requisitos para a aplicação em Linux/macOS
# Pré-requisitos: .NET 8 SDK (ou superior), Node.js v22.x.x

# Cores para saída no terminal
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}--- Verificação e Instalação de Pré-requisitos da Aplicação ---${NC}"

# --- Verifica privilégios de administrador ---
check_root_privileges() {
    if [[ $EUID -ne 0 ]]; then
        echo -e "${RED}AVISO: Este script precisa de privilégios de administrador (sudo) para instalar softwares.${NC}"
        echo -e "${RED}Por favor, execute-o novamente usando 'sudo ./nome_do_script.sh'.${NC}"
        exit 1
    fi
}

# --- Pergunta ao usuário se deseja prosseguir com as instalações ---
read_confirmation() {
    echo -e "\nEste script irá verificar se o Node.js (v22.x.x) e o .NET 8 SDK (ou superior) estão instalados."
    echo "Se algum deles estiver faltando ou com a versão incorreta, o script tentará instalá-lo automaticamente."
    read -p "Deseja continuar com a verificação e possíveis instalações? (S/N): " confirm

    if [[ ! "$confirm" =~ ^[Ss]$ ]]; then
        echo -e "${YELLOW}Operação cancelada pelo usuário.${NC}"
        exit 0
    fi
}

# --- Função para verificar a existência de um comando ---
command_exists() {
    command -v "$1" &> /dev/null
}

# --- Função para verificar e/ou instalar Node.js ---
test_and_install_nodejs() {
    local required_major_version="22"
    echo -e "${CYAN}\nVerificando Node.js...${NC}"
    local is_node_ok=false

    if command_exists node; then
        local node_version_output=$(node -v)
        echo -e "${GREEN}Node.js encontrado: ${node_version_output}${NC}"
        if [[ "$node_version_output" =~ ^v([0-9]+)\. ]]; then
            local current_major_version="${BASH_REMATCH[1]}"
            if [[ "$current_major_version" -eq "$required_major_version" ]]; then
                echo -e "${GREEN}Versão ${required_major_version}.x.x de Node.js (ou compatível) encontrada.${NC}"
                is_node_ok=true
            else
                echo -e "${YELLOW}Versão de Node.js instalada (${node_version_output}) não corresponde à versão ${required_major_version}.x.x.${NC}"
            fi
        else
            echo -e "${YELLOW}Não foi possível extrair a versão do Node.js. Verifique a instalação manual.${NC}"
        fi
    else
        echo -e "${YELLOW}Node.js não encontrado.${NC}"
    fi

    if [[ "$is_node_ok" == "false" ]]; then
        echo -e "${YELLOW}Tentando instalar Node.js LTS (versão ${required_major_version}.x.x)...${NC}"
        if command_exists brew; then
            echo -e "${CYAN}Instalando via Homebrew (macOS)...${NC}"
            brew install node@$required_major_version
            brew link node@$required_major_version --force --overwrite # Link it to be default
            if [[ $? -eq 0 ]]; then
                echo -e "${GREEN}Node.js instalado/atualizado com sucesso via Homebrew!${NC}"
                is_node_ok=true
            else
                echo -e "${RED}Falha na instalação do Node.js via Homebrew.${NC}"
            fi
        elif command_exists apt; then
            echo -e "${CYAN}Instalando via APT (Debian/Ubuntu)...${NC}"
            # Add Node.js 22.x repository from NodeSource
            curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -
            sudo apt-get install -y nodejs
            if [[ $? -eq 0 ]]; then
                echo -e "${GREEN}Node.js instalado/atualizado com sucesso via APT!${NC}"
                is_node_ok=true
            else
                echo -e "${RED}Falha na instalação do Node.js via APT.${NC}"
            fi
        else
            echo -e "${YELLOW}Nenhum gerenciador de pacotes conhecido (Homebrew, APT) encontrado para instalação automática do Node.js.${NC}"
        fi

        if [[ "$is_node_ok" == "false" ]]; then
            echo -e "${WHITE}Para instalar Node.js manualmente (versão ${required_major_version}.x.x LTS):${NC}"
            echo -e "${YELLOW}  - Utilize um gerenciador de versão como NVM (Node Version Manager): https://github.com/nvm-sh/nvm${NC}"
            echo -e "${YELLOW}  - Download direto: https://nodejs.org/en/download/ (selecione a versão LTS ${required_major_version}.x.x)${NC}"
        fi
    fi
    return "$is_node_ok"
}

# --- Função para verificar e/ou instalar .NET 8 SDK (ou superior) ---
test_and_install_dotnet_sdk() {
    local required_major_version="8"
    echo -e "${CYAN}\nVerificando .NET ${required_major_version} SDK ou superior...${NC}"
    local is_dotnet_sdk_ok=false

    if command_exists dotnet; then
        local sdk_list=$(dotnet --list-sdks 2>&1)
        if [[ $? -ne 0 ]]; then
            echo -e "${RED}Erro ao executar 'dotnet --list-sdks'. Verifique se o .NET SDK está instalado.${NC}"
        else
            while IFS= read -r line; do
                if [[ "$line" =~ ^([0-9]+)\.[0-9]+\.[0-9]+ ]]; then
                    local major_version="${BASH_REMATCH[1]}"
                    if [[ "$major_version" -ge "$required_major_version" ]]; then
                        echo -e "${GREEN}.NET SDK versão ${major_version}.x.x encontrado: ${line%% *}${NC}"
                        is_dotnet_sdk_ok=true
                        break
                    fi
                fi
            done <<< "$sdk_list"

            if [[ "$is_dotnet_sdk_ok" == "false" ]]; then
                echo -e "${YELLOW}.NET SDK versão ${required_major_version}.x.x ou superior não encontrado.${NC}"
            fi
        fi
    else
        echo -e "${YELLOW}Comando 'dotnet' não encontrado. .NET SDK não está instalado.${NC}"
    fi

    if [[ "$is_dotnet_sdk_ok" == "false" ]]; then
        echo -e "${YELLOW}Tentando instalar .NET SDK ${required_major_version}.0...${NC}"
        if command_exists brew; then
            echo -e "${CYAN}Instalando via Homebrew (macOS)...${NC}"
            brew install dotnet
            if [[ $? -eq 0 ]]; then
                echo -e "${GREEN}.NET SDK ${required_major_version}.0 instalado com sucesso via Homebrew!${NC}"
                is_dotnet_sdk_ok=true
            else
                echo -e "${RED}Falha na instalação do .NET SDK via Homebrew.${NC}"
            fi
        elif command_exists apt; then
            echo -e "${CYAN}Instalando via APT (Debian/Ubuntu)...${NC}"
            # Add Microsoft package signing key and repository
            sudo wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
            sudo dpkg -i packages-microsoft-prod.deb
            rm packages-microsoft-prod.deb
            sudo apt-get update
            sudo apt-get install -y dotnet-sdk-$required_major_version.0 # Installs 8.0.x
            if [[ $? -eq 0 ]]; then
                echo -e "${GREEN}.NET SDK ${required_major_version}.0 instalado com sucesso via APT!${NC}"
                is_dotnet_sdk_ok=true
            else
                echo -e "${RED}Falha na instalação do .NET SDK via APT.${NC}"
            fi
        else
            echo -e "${YELLOW}Nenhum gerenciador de pacotes conhecido (Homebrew, APT) encontrado para instalação automática do .NET SDK.${NC}"
        fi

        if [[ "$is_dotnet_sdk_ok" == "false" ]]; then
            echo -e "${WHITE}Para instalar o .NET SDK ${required_major_version}.0 manualmente:${NC}"
            echo -e "${YELLOW}  - Visite o site oficial: https://dotnet.microsoft.com/download/dotnet/${required_major_version}.0${NC}"
            echo -e "${YELLOW}  - Utilize o script de instalação para Linux/macOS: https://learn.microsoft.com/pt-br/dotnet/core/install/linux-scripted-manual#scripted-install${NC}"
            echo -e "${YELLOW}    Exemplo: curl -fSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0${NC}"
        fi
    fi
    return "$is_dotnet_sdk_ok"
}

# --- Execução Principal do Script ---

# 1. Verifica privilégios de administrador
check_root_privileges

# 2. Pergunta ao usuário se deseja prosseguir
read_confirmation

# 3. Tenta verificar e instalar Node.js
test_and_install_nodejs
node_final_status=$? # Captura o status de retorno da função

# 4. Tenta verificar e instalar .NET SDK
test_and_install_dotnet_sdk
dotnet_final_status=$? # Captura o status de retorno da função

echo -e "${CYAN}\n--- Resultado Final da Verificação/Instalação ---${NC}"

if [[ "$node_final_status" -eq 0 && "$dotnet_final_status" -eq 0 ]]; then
    echo -e "${GREEN}Todos os pré-requisitos essenciais (.NET 8 SDK ou superior, Node.js v22.x.x) foram encontrados ou instalados com sucesso!${NC}"
    echo -e "${GREEN}Você pode continuar com a configuração e execução da aplicação.${NC}"
    echo -e "${YELLOW}OBS: Se o .NET SDK foi instalado manualmente ou via script, pode ser necessário reiniciar o terminal ou adicionar ~/.dotnet à sua variável PATH.${NC}"
else
    echo -e "${RED}ATENÇÃO: Não foi possível instalar/verificar todos os pré-requisitos.${NC}"
    echo -e "${RED}Por favor, revise as mensagens acima e siga as instruções para instalação manual, se necessário.${NC}"
fi

echo -e "${CYAN}\n--- Fim da Verificação ---${NC}"