@echo off
setlocal

REM Script de inicialização do ambiente de desenvolvimento para Windows

REM Cores para o terminal (configuração básica para cmd.exe)
color 0B 

echo --- Iniciando Verificação de Pré-requisitos e Ambiente ---

REM Define o diretório de trabalho para o diretório onde o script .bat está localizado.
REM Isso garante que todos os caminhos relativos (.\frontend, .\backend) funcionem corretamente.
cd /d "%~dp0"

REM 1. Executar o script de instalação de pré-requisitos (PowerShell)
echo Executando script de verificacao e instalacao de pre-requisitos...
echo.
echo ATENCAO: Este script .bat e o script de pre-requisitos (install_prerequisites.ps1)
echo          precisam ser executados COM PRIVILEGIOS DE ADMINISTRADOR.
echo          Por favor, clique com o botao direito neste arquivo .bat e selecione
echo          "Executar como administrador" para garantir que tudo funcione corretamente.
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install_prerequisites.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRO: Falha na verificacao ou instalacao de pre-requisitos. Abortando inicializacao do ambiente.
    echo Por favor, resolva os problemas e execute este script novamente COMO ADMINISTRADOR.
    pause
    exit /b %ERRORLEVEL%
)

echo Pré-requisitos verificados/instalados com sucesso.

REM 2. Iniciar o Backend (.NET)
echo.
echo --- Iniciando Backend .NET ---
echo Executando: dotnet run --project ./backend/UsersManager.Application/UsersManager.Application.csproj
start "UsersManager Backend" dotnet run --project "./backend/UsersManager.Application/UsersManager.Application.csproj"

timeout /t 5 >nul

REM 3. Navegar para a pasta do frontend e iniciar o Quasar
echo.
echo --- Iniciando Frontend Quasar ---
echo Navegando para ./frontend e executando: yarn quasar dev

if not exist ".\frontend\" (
    echo ERRO: O diretorio '.\frontend' nao foi encontrado. Abortando inicializacao do frontend.
    goto :end
)

cd .\frontend
if %ERRORLEVEL% NEQ 0 (
    echo ERRO: Falha ao navegar para o diretorio './frontend'. Abortando inicializacao do frontend.
    goto :end
)

where /q yarn
if %ERRORLEVEL% EQU 0 (
    echo Iniciando Quasar dev com yarn...
    start "UsersManager Frontend" yarn quasar dev
) else (
    where /q npm
    if %ERRORLEVEL% EQU 0 (
        echo Yarn nao encontrado, usando npm...
        start "UsersManager Frontend" npm run dev
    ) else (
        echo ERRO: Nem 'yarn' nem 'npm' foram encontrados. Por favor, instale um deles.
        echo O frontend nao podera ser iniciado automaticamente.
        goto :end
    )
)

:end
echo.
echo --- Processos de Aplicacao Iniciados ---
echo.
echo O backend .NET e o frontend Quasar foram iniciados em novas janelas de console.
echo Voce precisara fechar as janelas do backend e frontend manualmente quando terminar.
echo Pressione qualquer tecla para sair deste script de inicializacao.
pause
endlocal