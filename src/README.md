
# UsersManager Application

Este projeto consiste em uma aplicação web completa, dividida em um **Backend** desenvolvido em .NET e um **Frontend** construído com Quasar Framework (Vue.js). O objetivo é fornecer uma solução para gerenciamento de usuários, incluindo autenticação, perfis e histórico de acessos.

---

## 🚀 Tecnologias Principais

### Backend:
- .NET 8 SDK  
- C#  
- Entity Framework  
- API RESTful  

### Frontend:
- Vue.js 3  
- Quasar Framework (v2)  
- Node.js (v22.x.x)  
- Yarn (recomendado para gerenciamento de pacotes) ou npm  
- Pinia (gerenciamento de estado)  
- Vue I18n (internacionalização)  
- Axios (requisições HTTP)  

---

## 📂 Estrutura do Projeto

```
.
├── backend/
│   └── UsersManager.Application/   # Projeto .NET (API RESTful)
├── frontend/                       # Projeto Quasar Framework (Vue.js)
├── install_prerequisites.ps1       # Script de instalação de pré-requisitos (Windows)
├── install_prerequisites.sh        # Script de instalação de pré-requisitos (Linux/macOS)
├── StartupEnv.bat                  # Script para iniciar o ambiente (Windows)
├── StartupEnv.sh                   # Script para iniciar o ambiente (Linux/macOS)
├── README.md                       # Este arquivo
└── ... (outros arquivos de configuração)
```

---

## 📋 Pré-requisitos

Para executar este projeto em sua máquina local, você precisará ter os seguintes softwares instalados:

- **Node.js:** Versão `v22.x.x` (LTS recomendada)
- **.NET SDK:** Versão `8.0` ou superior
- **Yarn** ou **npm:** Gerenciadores de pacotes para o frontend (Yarn é o preferencial usado nos scripts)  
    - Instalar Yarn: `npm install -g yarn` (se você já tiver npm)

---

## 🚀 Primeiros Passos

### 1. Clonar o Repositório

```bash
git clone <URL_DO_SEU_REPOSITORIO>
cd UsersManager-Application
```

### 2. Iniciar o Ambiente de Desenvolvimento

Temos scripts automatizados para verificar e instalar os pré-requisitos e iniciar o backend e o frontend.

#### Para Usuários **Windows**:

1. Abra o Explorador de Arquivos e navegue até a pasta raiz do projeto.  
2. Localize o arquivo `StartupEnv.bat`.  
3. Clique com o botão direito e selecione **"Executar como administrador"**.  

- O script `install_prerequisites.ps1` será executado primeiro para verificar e instalar Node.js, .NET SDK e Chocolatey (se necessário).  
- Após os pré-requisitos, o backend .NET e o frontend Quasar serão iniciados em **janelas de console separadas**.

⚠️ **Importante (Windows):**  
Você precisará **fechar manualmente** essas janelas para encerrar as aplicações.

---

#### Para Usuários **Linux / macOS**:

1. Abra o Terminal e navegue até a pasta raiz do projeto.  
2. Garanta permissões de execução aos scripts:

```bash
chmod +x install_prerequisites.sh
chmod +x StartupEnv.sh
```

3. Execute o script de inicialização com:

```bash
sudo ./StartupEnv.sh
```

- O script `install_prerequisites.sh` verificará e instalará Node.js e .NET SDK.  
- O backend será iniciado em segundo plano (log: `backend.log`).  
- O frontend será iniciado no terminal.

⚠️ **Importante (Linux/macOS):**  
Se `dotnet` não for reconhecido após instalação via script, adicione ao seu `PATH`:

```bash
export PATH="$PATH:$HOME/.dotnet"
```

Adicione isso no `~/.bashrc` ou `~/.zshrc` e rode `source ~/.bashrc` ou reinicie o terminal.

---

## 💻 Acessando a Aplicação

Após os scripts de inicialização:

- **Backend API:** http://localhost:5000 ou https://localhost:5001  
- **Frontend:** http://localhost:8080  

---

## 🛠️ Workflow de Desenvolvimento

Se quiser rodar **separadamente**:

### Backend (.NET)

```bash
cd backend/UsersManager.Application/
dotnet run
```

### Frontend (Quasar)

```bash
cd frontend/

# Para instalar dependências
yarn install
# ou
npm install

# Para iniciar o servidor de desenvolvimento
yarn quasar dev
# ou
npm run dev
```

---

## 🌐 Internacionalização (i18n)

O projeto utiliza `vue-i18n` para suportar múltiplos idiomas.  
Arquivos de tradução: `frontend/src/i18n/`.

---

## 🤝 Contribuição

Contribuições são bem-vindas! Siga estas etapas:

1. Faça um fork do repositório.  
2. Crie uma nova branch:  
```bash
git checkout -b feature/sua-feature
```
3. Faça alterações e commit:  
```bash
git commit -m 'feat: Adiciona sua nova funcionalidade'
```
4. Envie para o seu fork:  
```bash
git push origin feature/sua-feature
```
5. Abra um Pull Request.

---
