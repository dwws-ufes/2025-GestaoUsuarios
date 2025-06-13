# Script para verificar e instalar pré-requisitos para a aplicação
# Pré-requisitos: .NET 8 SDK (ou superior), Node.js v22.x.x

Write-Host "--- Verificação e Instalação de Pré-requisitos da Aplicação ---" -ForegroundColor Cyan

# --- Verifica privilégios de administrador ---
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "AVISO: Este script precisa ser executado com privilégios de administrador para instalar softwares." -ForegroundColor Red
    Write-Host "Por favor, clique com o botão direito no arquivo '.ps1' e selecione 'Executar com PowerShell' (se estiver no Explorador de Arquivos) ou abra o PowerShell como administrador e execute o script por lá." -ForegroundColor Red
    Start-Sleep -Seconds 5
    exit 1
}

# --- Pergunta ao usuário se deseja prosseguir com as instalações ---
Write-Host "`nEste script irá verificar se o Node.js (v22.x.x) e o .NET 8 SDK (ou superior) estão instalados." -ForegroundColor White
Write-Host "Se algum deles estiver faltando ou com a versão incorreta, o script tentará instalá-lo automaticamente usando o Chocolatey." -ForegroundColor White
$confirm = Read-Host "Deseja continuar com a verificação e possíveis instalações? (S/N)"

if ($confirm -notmatch "^[Ss]$") {
    Write-Host "Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

# --- Função para instalar Chocolatey ---
function Install-Chocolatey {
    Write-Host "`nChocolatey não encontrado. Tentando instalar Chocolatey..." -ForegroundColor Cyan
    try {
        Set-ExecutionPolicy Bypass -Scope Process -Force -ErrorAction Stop # Permite execução do script de instalação
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072 # Garante TLS 1.2
        iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Chocolatey instalado com sucesso!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Falha crítica ao instalar Chocolatey. Por favor, tente instalar manualmente: https://chocolatey.org/install" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "Erro durante a instalação do Chocolatey: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# --- Função para verificar a instalação do Chocolatey ---
function Test-Chocolatey {
    try {
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-Host "Chocolatey (gerenciador de pacotes) está instalado." -ForegroundColor Green
            return $true
        }
        Write-Host "Chocolatey não está instalado." -ForegroundColor Yellow
        return $false
    } catch {
        Write-Host "Erro ao verificar Chocolatey: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# --- Função para verificar e/ou instalar Node.js ---
function Test-And-Install-NodeJs {
    param (
        [string]$RequiredMajorVersion = "22",
        [bool]$ChocoAvailable = $false
    )
    Write-Host "`nVerificando Node.js..." -ForegroundColor Cyan
    $isNodeOk = $false
    try {
        $nodeVersionOutput = (node -v 2>&1)
        if ($LASTEXITCODE -eq 0 -and $nodeVersionOutput -match "^v(\d+)\.") {
            $currentMajorVersion = $Matches[1]
            Write-Host "Node.js encontrado: $nodeVersionOutput" -ForegroundColor Green
            if ($currentMajorVersion -eq $RequiredMajorVersion) {
                Write-Host "Versão $RequiredMajorVersion.x.x de Node.js (ou compatível) encontrada." -ForegroundColor Green
                $isNodeOk = $true
            } else {
                Write-Host "Versão de Node.js instalada ($nodeVersionOutput) não corresponde à versão $RequiredMajorVersion.x.x." -ForegroundColor Yellow
            }
        } else {
            Write-Host "Node.js não encontrado ou erro ao verificar a versão." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Erro ao verificar Node.js: $($_.Exception.Message)" -ForegroundColor Red
    }

    if (-not $isNodeOk) {
        if ($ChocoAvailable) {
            Write-Host "Tentando instalar Node.js LTS (versão $RequiredMajorVersion.x.x) via Chocolatey..." -ForegroundColor Yellow
            try {
                choco install nodejs-lts -y
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Node.js LTS instalado/atualizado com sucesso!" -ForegroundColor Green
                    return $true
                } else {
                    Write-Host "Falha na instalação/atualização do Node.js via Chocolatey." -ForegroundColor Red
                }
            } catch {
                Write-Host "Erro durante a instalação do Node.js via Chocolatey: $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            Write-Host "Chocolatey não está disponível para instalação automática do Node.js." -ForegroundColor Yellow
        }
        Write-Host "Para instalar ou atualizar Node.js (versão $RequiredMajorVersion.x.x LTS):" -ForegroundColor White
        Write-Host "  - Download direto: https://nodejs.org/en/download/ (selecione a versão LTS 22.x.x)" -ForegroundColor Yellow
    }
    return $isNodeOk # Retorna o status final após a tentativa de instalação
}

# --- Função para verificar e/ou instalar .NET 8 SDK (ou superior) ---
function Test-And-Install-DotNet8Sdk {
    Write-Host "`nVerificando .NET 8 SDK ou superior..." -ForegroundColor Cyan
    $requiredMajorVersion = 8 # A aplicação requer no mínimo .NET 8
    $isDotNetSdkOk = $false

    try {
        $sdkList = (dotnet --list-sdks 2>&1)
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Erro ao executar 'dotnet --list-sdks'. Verifique se o .NET SDK está instalado." -ForegroundColor Red
        } else {
            foreach ($line in $sdkList) {
                if ($line -match "^(\d+)\.\d+\.\d+") {
                    $majorVersion = [int]$Matches[1]
                    if ($majorVersion -ge $requiredMajorVersion) {
                        Write-Host ".NET SDK versão $majorVersion.x.x encontrado: $($line.Split(' ')[0])" -ForegroundColor Green
                        $isDotNetSdkOk = $true
                        break # Encontrou um SDK compatível, não precisa verificar mais
                    }
                }
            }

            if (-not $isDotNetSdkOk) {
                Write-Host ".NET SDK versão $requiredMajorVersion.x.x ou superior não encontrado." -ForegroundColor Yellow
            }
        }
    } catch {
        Write-Host "Erro inesperado ao verificar .NET SDK: $($_.Exception.Message)" -ForegroundColor Red
    }

    if (-not $isDotNetSdkOk) {
        if ($ChocoAvailable) {
            Write-Host "Tentando instalar .NET SDK 8.0 via Chocolatey..." -ForegroundColor Yellow
            try {
                choco install dotnet-sdk-8.0 -y
                if ($LASTEXITCODE -eq 0) {
                    Write-Host ".NET SDK 8.0 instalado com sucesso!" -ForegroundColor Green
                    return $true
                } else {
                    Write-Host "Falha na instalação do .NET SDK 8.0 via Chocolatey." -ForegroundColor Red
                }
            } catch {
                Write-Host "Erro durante a instalação do .NET SDK 8.0 via Chocolatey: $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            Write-Host "Chocolatey não está disponível para instalação automática do .NET SDK." -ForegroundColor Yellow
        }
        Write-Host "Para instalar o .NET 8 SDK (versão mínima recomendada):" -ForegroundColor White
        Write-Host "  - Download direto: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    }
    return $isDotNetSdkOk # Retorna o status final após a tentativa de instalação
}

# --- Execução Principal do Script ---

# 1. Tenta verificar/instalar Chocolatey
$chocoAvailable = Test-Chocolatey
if (-not $chocoAvailable) {
    $chocoAvailable = Install-Chocolatey # Tenta instalar Chocolatey se não estiver presente
}
# Dá um tempo para o PATH do choco ser atualizado (pode ser necessário em novas instalações)
if ($chocoAvailable) {
    Write-Host "Aguardando 2 segundos para o PATH do Chocolatey ser atualizado..." -ForegroundColor DarkGray
    Start-Sleep -Seconds 2
}

# 2. Verifica e instala Node.js
$nodeFinalStatus = Test-And-Install-NodeJs -RequiredMajorVersion "22" -ChocoAvailable $chocoAvailable

# 3. Verifica e instala .NET 8 SDK
$dotnetFinalStatus = Test-And-Install-DotNet8Sdk -ChocoAvailable $chocoAvailable

Write-Host "`n--- Resultado Final da Verificação/Instalação ---" -ForegroundColor Cyan

if ($nodeFinalStatus -and $dotnetFinalStatus) {
    Write-Host "Todos os pré-requisitos essenciais (.NET 8 SDK ou superior, Node.js v22.x.x) foram encontrados ou instalados com sucesso!" -ForegroundColor Green
    Write-Host "Você pode continuar com a configuração e execução da aplicação." -ForegroundColor Green
} else {
    Write-Host "ATENÇÃO: Não foi possível instalar/verificar todos os pré-requisitos." -ForegroundColor Red
    Write-Host "Por favor, revise as mensagens acima e siga as instruções para instalação manual, se necessário." -ForegroundColor Red
}

Write-Host "`n--- Fim da Verificação ---" -ForegroundColor Cyan