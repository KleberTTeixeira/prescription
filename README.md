# Prescrição Eletrônica — Estudo aplicado de SOLID, Clean Code e Clean Architecture em .NET

Projeto de atualização e consolidação prática de **SOLID**, **Clean Code** e **Clean Architecture**, aplicado a um domínio real: prescrição médica eletrônica seguindo os requisitos **SBIS ECF.10**.

A solução tem uma **Web API em .NET 9**, banco **MySQL** e **dois frontends independentes** que consomem a mesma API, com as mesmas regras de negócio:

- **Blazor** (.NET 10, Interactive Server, MudBlazor)
- **Angular 21**

> **Nota sobre o uso de IA:** o frontend Angular foi gerado com IA para validar que a Web API atende dois clientes diferentes. Meu trabalho neste projeto é o backend .NET, a arquitetura, o frontend Blazor, os testes e o CI/CD.

Inclui testes unitários, de integração e de componentes, além de pipelines **CI/CD** com GitHub Actions que publicam imagens Docker no GHCR.

---

## Tecnologias

| Camada | Tecnologias |
| --- | --- |
| Backend | C#, .NET 9, ASP.NET Core Web API, OData, Swagger, Health Checks, ProblemDetails (RFC 7807) |
| Dados | Entity Framework Core, MySQL 8.0 |
| Frontend | Blazor .NET 10 + MudBlazor; Angular 21 |
| Testes | xUnit, bUnit, testes de integração com `WebApplicationFactory`, Vitest |
| DevOps | Docker, Docker Compose, GitHub Actions, GHCR |

---

## Arquitetura

A solução segue a **Clean Architecture**: as dependências apontam sempre para o domínio, e a infraestrutura depende das abstrações, nunca o contrário.

```
web/backend (API)  ─┐
                    ├──►  business/Prescription.Application  ──►  business/Prescription.Domain
data (Infra/EF)    ─┘
```

| Projeto | Responsabilidade | Depende de |
| --- | --- | --- |
| `Prescription.Domain` | Entidades, value objects, enums e regras de negócio | nenhum projeto |
| `Prescription.Application` | Casos de uso e contratos (interfaces) dos repositórios | Domain |
| `Prescription.Data` | Implementação dos repositórios com EF Core e MySQL | Application, Domain |
| `Prescription.Api` | Controllers, OData, Swagger e composição da injeção de dependência | Application, Data |
| `Prescription.Blazor` | Interface Blazor, consumindo a Web API | — |

O frontend Angular reproduz a mesma separação em camadas: `domain`, `application`, `infrastructure` e `presentation`.

### Princípios aplicados

- **Inversão de dependência (SOLID - D):** os contratos dos repositórios ficam na camada Application e são implementados na camada Data. Domínio e casos de uso não conhecem o EF Core nem o MySQL.
- **Responsabilidade única (SOLID - S):** cada camada e cada caso de uso tem um papel definido. Regras de negócio ficam no domínio, e não nos controllers ou na interface.
- **Domínio rico:** entidades e value objects protegem as próprias regras, como o bloqueio de edição após a emissão da receita.
- **Clean Code:** nomes explícitos, métodos curtos e erros padronizados com ProblemDetails (400 para regra violada, 404 para registro inexistente).
- **Testabilidade:** o domínio e os casos de uso são testados isoladamente, sem banco de dados.

---

## Funcionalidades

- Escolha do profissional, que faz o papel do usuário logado (o sistema não tem login).
- **Receitas:** listagem com filtro, ordenação e paginação; criação, edição, visualização, impressão e exclusão.
- **Textos padrão** e **medicamentos favoritos** por profissional.
- Receita **estruturada** (itens de medicamento) ou **não estruturada** (texto livre).
- **Categoria** (comum, controle especial, antimicrobiano) sugerida pelo sistema conforme os medicamentos.
- **Emissão:** a receita é emitida na primeira impressão confirmada e, a partir daí, fica bloqueada para edição.

Uma receita criada em um frontend aparece no outro, porque ambos usam a mesma Web API e o mesmo banco.

### Requisitos SBIS atendidos (ECF.10)

| Requisito | Descrição |
| --- | --- |
| ECF.10.01 | Receita não estruturada (texto livre) |
| ECF.10.02 | Textos padrão com título, usados como base e editáveis |
| ECF.10.03 | Receita estruturada: busca de medicamento, medicamento não cadastrado, campos do item e prazo de utilização |
| ECF.10.04 | Impressão com dados do cliente, do profissional, da instituição (CNES), data/hora e assinatura; endereço do cliente em controle especial; idade e sexo em antimicrobiano |
| ECF.10.07 | Lista de medicamentos com linhas distintas e destaque ao passar o mouse ou selecionar |
| ECF.10.10 | Medicamentos favoritos por profissional, usados na prescrição |

---

## Como executar

**Pré-requisito:** somente **Docker** com **Docker Compose**. Não é preciso instalar MySQL, .NET ou Node.

Portas usadas: **3306** (MySQL), **5000** (Web API), **4200** (Angular) e **5100** (Blazor).

Na pasta raiz do projeto:

```
docker compose up -d --build
```

Para subir só um frontend (o banco e a API sobem junto):

```
docker compose up -d --build web      # só Angular
docker compose up -d --build blazor   # só Blazor
```

| O quê | Endereço |
| --- | --- |
| Sistema (Blazor) | <http://localhost:5100> |
| Sistema (Angular) | <http://localhost:4200> |
| Documentação da API (Swagger) | <http://localhost:5000/swagger> |
| Verificação de saúde da API e do banco | <http://localhost:5000/health> |
| MySQL | `localhost:3306`, banco `prescription_db`, usuário `prescription_app`, senha `prescription_dev_2026` |

Parar:

```
docker compose down
```

Voltar o banco aos dados iniciais:

```
docker compose down -v
docker compose up -d
```

### Dados iniciais (fictícios)

| Profissional | Receitas |
| --- | --- |
| Ana Beatriz Moreira (CRM 123456/SP) | nº 1 – não estruturada, comum, emitida |
| Carlos Eduardo Lima (CRM 234567/SP) | nº 2 – estruturada, antimicrobiano, emitida; nº 4 – estruturada, controle especial, não emitida |
| Fernanda Souza Ramos (CRM 345678/MG) | nº 3 – estruturada, comum, não emitida |
| Ricardo Alves Pinto (CRO 456789/RJ) | nenhuma |

---

## Web API

As rotas do profissional exigem o cabeçalho `X-Professional-Id` (1 a 4 nos dados iniciais). O Swagger tem um campo para informá-lo.

| Método e rota | Descrição |
| --- | --- |
| `GET /odata/Prescriptions` | Lista de receitas com OData (`$filter`, `$orderby`, `$select`, `$top` máx. 100, `$skip`, `$count`) |
| `GET /api/prescriptions/{id}` | Obter receita |
| `POST /api/prescriptions` | Criar receita |
| `PUT /api/prescriptions/{id}` | Alterar receita (somente não emitida) |
| `DELETE /api/prescriptions/{id}` | Excluir receita |
| `GET /api/prescriptions/{id}/print-data` | Dados para impressão (ECF.10.04) |
| `POST /api/prescriptions/{id}/print-confirmation` | Confirmar impressão (emite a receita) |
| `GET /api/prescriptions/suggested-category` | Categoria sugerida a partir dos medicamentos |
| `GET /api/medications?search=` | Busca de medicamento por nome ou princípio ativo |
| `GET /api/medications/{id}/usage-deadline?issueDate=` | Data limite de uso sugerida |
| `GET/POST/PUT/DELETE /api/templates` | Textos padrão do profissional |
| `GET /api/favorites`, `PUT/DELETE /api/favorites/{medicationId}` | Favoritos do profissional |
| `GET /api/clients`, `professionals`, `institutions`, `pharmaceutical-forms`, `administration-routes`, `dose-units`, `frequencies`, `validity-periods` | Tabelas de apoio |

Exemplo OData:

```
GET http://localhost:5000/odata/Prescriptions?$filter=Category eq 'SpecialControl'&$orderby=IssuedAt desc&$select=Id,ClientName,IssuedAt&$count=true
X-Professional-Id: 2
```

---

## Testes

| Projeto | Tipo | Ferramentas |
| --- | --- | --- |
| `testes` | Unitários (Domain e Application) | xUnit |
| `testes/integration` | Integração (repositórios e API contra MySQL) | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |
| `testes/blazor` | Componentes Blazor | xUnit, bUnit |
| `web/frontend` | Frontend Angular | Vitest |

Backend e Blazor (exige .NET 9 SDK e .NET 10 SDK), na pasta raiz:

```
dotnet test
```

Os testes de integração rodam somente com a variável `PRESCRIPTION_TEST_DB` apontando para o MySQL; sem ela, ficam ignorados. Cada teste roda numa transação desfeita no final.

```
# Linux/macOS
export PRESCRIPTION_TEST_DB="server=localhost;port=3306;database=prescription_db;user=prescription_app;password=prescription_dev_2026"
# Windows PowerShell
$env:PRESCRIPTION_TEST_DB = "server=localhost;port=3306;database=prescription_db;user=prescription_app;password=prescription_dev_2026"
```

Frontend (exige Node.js 24), na pasta `web/frontend`:

```
npm ci
npx ng test --watch=false
```

---

## CI/CD (GitHub Actions)

Três pipelines em `.github/workflows`, um por serviço. Cada um roda em push e pull request para a `main` quando mudam as pastas do serviço. O job `deploy` roda só em push na `main` e publica a imagem no **GHCR** com as tags `latest` e o SHA do commit.

| Pipeline | Job `build-test` | Imagem publicada |
| --- | --- | --- |
| `backend.yml` | MySQL 8.0 temporário; build e testes da solução (unitários, integração e Blazor) | `ghcr.io/<dono>/prescription-api` |
| `frontend.yml` | Node 24; `npm ci`, testes (Vitest) e `ng build` | `ghcr.io/<dono>/prescription-web` |
| `blazor.yml` | .NET 10; build, testes bUnit e publicação | `ghcr.io/<dono>/prescription-blazor` |

---

## Estrutura do projeto

| Pasta ou arquivo | Conteúdo |
| --- | --- |
| `docker-compose.yml` | Sobe MySQL, Web API, Angular e Blazor |
| `.github/workflows` | Pipelines CI/CD |
| `Prescription.sln` | Solução .NET (API, camadas, Blazor e testes) |
| `db` | Scripts SQL (`01_schema.sql`, `02_seed.sql`) e DER (`der.md` em Mermaid e `der.png`) |
| `business/Prescription.Domain` | Domínio |
| `business/Prescription.Application` | Casos de uso e contratos |
| `data` | Infraestrutura com EF Core e MySQL |
| `web/backend` | Web API .NET 9 |
| `web/blazor` | Blazor .NET 10 com MudBlazor |
| `web/frontend` | Angular 21 |
| `testes` | Testes unitários, de integração e de componentes |

---

## Autor

**Kleber T Teixeira** — Desenvolvedor .NET Sênior, Full Stack C# / Blazor, especialista em modernização de sistemas legados.

[LinkedIn](https://www.linkedin.com/in/kleber-t-teixeira/)
