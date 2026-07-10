
# 📝 Todo List

Uma API RESTful robusta desenvolvida para o gerenciamento inteligente de tarefas e categorias de usuários, trazendo recursos avançados de autenticação, segurança e controle de dados. 

---

## 🎯 Objetivo do Projeto

O propósito desta API é ir além do CRUD tradicional. O sistema garante o isolamento dos dados por usuário, com validação de payloads, paginação inteligente de recursos e segurança no tráfego de informações através de tokens de autenticação.

## 🛠️ Tecnologias Utilizadas

*   **Back-end:** .NET 9 / ASP.NET com Minimal API
*   **Front-end:** .NET 9 / ASP.NET MVC
*   **Persistência & ORM:** Entity Framework Core
*   **Banco de Dados:** PostgreSQL 
*   **Segurança & Autenticação:** JWT (JSON Web Tokens) & BCrypt para Hash de Senhas
*   **Documentação:** Swagger (OpenAPI)

---

## ⚙️ Funcionalidades Principais

*   **Autenticação & Segurança:** 
    *   Registro de novos usuários com senha criptografada com BCrypt.Net-Next.
    *   Geração de Tokens JWT expiráveis no Login.
    *   Bloqueio e isolamento de rotas por usuarios.
*   **Gerenciamento de Tarefas (Todos):**
    *   CRUD completo de tarefas com proteção de escopo por Token.
    *   Vínculo flexível de uma tarefa a múltiplas categorias.
*   **Organização por Categorias:**
    *   Criação e gerenciamento de categorias personalizadas por usuário.
*   **Performance:**
    *   Paginação dinâmica e filtros via Query Strings nos endpoints de listagem (`GET /todos`).

---

## Como Executar o Projeto Localmente
Pré-requisitos
- .NET SDK 9
- PostgreSQL

**Clonar o Repositório:**
```txt
git clone https://github.com/DanielCamposOliveira/To-do-List.git
```

**Configurar as Variáveis de Ambiente:**
Atualize a Connection String e a Secret Key do JWT no arquivo appsettings.json

**Rodar as Migrations do Entity Framework:**
```bash
dotnet ef database update
```

**Executar a Aplicação:**
```bash
dotnet run
```

A API estará disponível em http://localhost:8585 (ou na porta configurada). Acesse /swagger no navegador para testar pela interface gráfica.


## Documentação das Rotas  Backend API

Todas as rotas protegidas exigem o envio do token JWT no cabeçalho da requisição: 
**Authorization: Bearer Token**

---

### 1. Autenticação de Usuários


#### POST /register
 **Descrição:** Registra um novo usuário com senha criptografada e retorna o token JWT de acesso.

```txt
http://localhost:8585/register
```

**Corpo da Requisição (JSON):**
```json
{
	"name": "string",
	"email": "string",
	"password": "string"
}
```

**Responses (JSON):**
**Code:** 200
```json
{
	"token": "eyJhbGciOiJIUzI1NikpXVCJ9..."
}
```


#### POST /login
**Descrição:** Autentica o usuário e retorna o token JWT para as próximas requisições.

```txt
http://localhost:8585/login
```

**Corpo da Requisição (JSON):**
```json
{
	"email": "string",
	"password": "string"
}
```

**Responses (JSON):**
**Code:** 200
```json
{
  "token": "1lajptrwvdpgkgçfQiOiIiwibmJmIjoxNzgz..."
}
```


---

### 2. Gerenciamento de Tarefas

#### GET /todos
**Descrição:** Retorna uma lista paginada de tarefas do usuário logado.

**Parâmetros de Query:** page (página atual) e **limit** (itens por página).
```txt
http://localhost:8585/todos?page=1&limit=10
```

**Responses (JSON):**
**Code:** 200
```json
{
  "data": [
    {
      "id": int,
      "title": "string",
      "description": "string",
      "completed": false
    }
  ],
  "page": 1,
  "limit": 10,
  "total": 1
}
```



#### POST /todos
**Descrição:** Cria uma nova tarefa vinculada ao usuário logado.

```txt
http://localhost:8585/todos
```

**Corpo da Requisição (JSON):**
```json
{
  "title": "string",
  "description": "string"
}
```

**Responses (JSON):**
**Code:** 201
```json
{
	"id": int,
	"title": "string",
	"description": "string",
	"completed": false
}
```



#### PUT /todos/{id}
**Descrição:** Atualiza o título e a descrição da tarefa informada no ID. Apenas o dono do registro pode alterá-lo.

**Parâmetros de Path:** id (int).
```txt
http://localhost:8585/todos/6
```
**Corpo da Requisição (JSON):**
```json
{
  "title": "string",
  "description": "string"
}
```

**Responses (JSON):**
**Code:** 200
```json
{
	"id": int,
	"title": "string",
	"description": "string",
	"completed": bool
}
```



#### PATCH /todos/{id}/complete
**Descrição:** Inverte o status de conclusão (Completed) da tarefa informada no ID. Apenas o dono da tarefa pode alterá-la.

**Parâmetros de Path:** `id` (int).
```txt
http://localhost:8585/todos/6/complete
```

**Responses (JSON):**
**Code:** 200
```json
{ 
	"message": "Tarefa marcada como concluída." 
}
```




#### DELETE /todos/{id}
**Descrição:** Exclui permanentemente a tarefa informada no ID, validando se o requisitante é o criador.

**Parâmetros de Path:** `id` (int).
```txt
http://localhost:8585/todos/6
```

**Responses (JSON):**
**Code:** 204



---

### Respostas de Segurança e Erros

A API valida a propriedade dos recursos. Se um usuário tentar alterar ou excluir um dado que não o pertence, o sistema barra a requisição:

*   **403 Forbidden (Acesso Negado):**
```json
    {
      "message": "Forbidden"
    }
```
*   **401 Unauthorized:** Token ausente ou inválido.
