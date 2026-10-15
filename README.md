# Prescrição médica – Avaliação Desenvolvedor Fullstack .NET + Blazor/Angular

Sistema de prescrição médica (receita) com banco MySQL, Web API em .NET 9 e dois frontends que usam a mesma API: Angular 21 e Blazor (.NET 10 com MudBlazor), seguindo os requisitos SBIS de prescrição eletrônica (ECF.10).

Os dois frontends são independentes e têm as mesmas funções e regras (escolha do profissional, receitas, textos padrão, favoritos, impressão e emissão); muda apenas o visual. Uma receita criada em um aparece no outro, porque ambos usam a mesma Web API e o mesmo banco.

## Pré-requisito

Somente **Docker** com **Docker Compose**. Não é preciso instalar MySQL, .NET nem Node: tudo roda em contêineres.

Portas usadas na máquina: **3306** (MySQL), **5000** (Web API), **4200** (Angular) e **5100** (Blazor). Elas precisam estar livres.

## Como executar

Na pasta raiz do projeto (onde está o `docker-compose.yml`):

```bash
docker compose up -d --build
```

A primeira execução demora alguns minutos (download das imagens, compilação da API e do Blazor e instalação dos pacotes do Angular). Acompanhar com:

```bash
docker compose ps
docker compose logs -f
```

Serviços do `docker-compose.yml`:

| Serviço | Imagem | Porta | Conteúdo | Sobe depois de |
|---|---|---|---|---|
| `mysql` | `mysql:8.0` | 3306 | Banco, criado pelos scripts de `db` na primeira execução | – |
| `api` | `prescription-api` (`web/backend/Dockerfile`) | 5000 | Web API .NET 9 | `mysql` saudável |
| `web` | `prescription-web` (`web/frontend/Dockerfile`) | 4200 | Angular (`ng serve`) | `api` |
| `blazor` | `prescription-blazor` (`web/blazor/Dockerfile`) | 5100 | Blazor .NET 10 | `api` |

Para subir só um frontend (o banco e a API sobem junto):

```bash
docker compose up -d --build web      # só Angular
docker compose up -d --build blazor   # só Blazor
```

Quando os serviços estiverem no ar:

| O quê | Endereço |
|---|---|
| Sistema (Angular) | http://localhost:4200 |
| Sistema (Blazor) | http://localhost:5100 |
| Documentação interativa da API (Swagger) | http://localhost:5000/swagger |
| Verificação de saúde da API e do banco | http://localhost:5000/health |
| MySQL | `localhost:3306`, banco `prescription_db`, usuário `prescription_app`, senha `prescription_dev_2026` |

Parar:

```bash
docker compose down
```

Os dados ficam guardados no volume `mysql-data` entre execuções. Para voltar o banco aos dados iniciais:

```bash
docker compose down -v
docker compose up -d
```

## Roteiro de avaliação

1. Abrir http://localhost:4200 (Angular) ou http://localhost:5100 (Blazor) e escolher um profissional (o sistema não tem login; o profissional escolhido faz o papel do usuário logado).
2. **Receitas**: lista as receitas do profissional, com filtro por cliente, ordenação ao clicar no título das colunas e paginação. Daqui é possível criar, editar, visualizar, imprimir e excluir.
3. **Textos padrão** e **Favoritos**: cadastros próprios de cada profissional, usados no formulário da receita.

Diferenças de uso no Blazor (as regras são as mesmas):

- A lista de receitas filtra pelo nome do cliente enquanto se digita (sem botão "Filtrar").
- A busca de medicamento é um campo de autocompletar: digitar parte do nome ou do princípio ativo e escolher na lista.
- No formulário, os favoritos aparecem como etiquetas em "Favoritos:"; ao clicar, o medicamento vai para o primeiro item sem medicamento ou para um item novo.
- Os itens da receita ficam em painéis que abrem e fecham; erros aparecem embaixo de cada campo.
- Textos padrão são criados e editados em uma janela; remover um favorito pede confirmação.
- Na impressão, o profissional aparece no corpo da receita e a área de assinatura tem espaço para o carimbo, com o texto "Assinatura e carimbo do profissional".

Dados iniciais (fictícios):

| Profissional | Receitas |
|---|---|
| Ana Beatriz Moreira (CRM 123456/SP) | nº 1 – não estruturada, comum, já emitida |
| Carlos Eduardo Lima (CRM 234567/SP) | nº 2 – estruturada, antimicrobiano, já emitida; nº 4 – estruturada, controle especial, não emitida |
| Fernanda Souza Ramos (CRM 345678/MG) | nº 3 – estruturada, comum, não emitida |
| Ricardo Alves Pinto (CRO 456789/RJ) | nenhuma |

### Regras de negócio

- **Emissão**: a receita é considerada emitida quando é impressa pela primeira vez e o usuário confirma "A receita foi impressa?". A partir daí fica bloqueada: só visualizar, imprimir de novo ou excluir. Antes disso tudo é editável, inclusive o formato.
- **Categoria** (comum, controle especial, antimicrobiano): sugerida pelo sistema conforme os medicamentos e escolhida pelo usuário.
- Cada profissional vê e altera apenas as próprias receitas, textos padrão e favoritos. Receitas de outro profissional podem ser visualizadas e reimpressas.

### Requisitos SBIS atendidos (ECF.10)

| Requisito | Onde ver no Angular | Onde ver no Blazor |
|---|---|---|
| ECF.10.01 – receita não estruturada (texto livre) | Nova receita → formato "Não estruturada" | Nova receita → formato "Não estruturada (texto livre)" |
| ECF.10.02 – textos padrão com título, usados como base e editáveis | Menu "Textos padrão"; no formulário, "Usar texto padrão" | Menu "Textos padrão" ("Novo texto padrão" abre uma janela); no formulário, campo "Texto padrão" e botão "Usar texto padrão" |
| ECF.10.03 – receita estruturada: busca de medicamento por nome, medicamento não cadastrado, todos os campos do item e prazo de utilização | Nova receita → formato "Estruturada" | Nova receita → formato "Estruturada (itens)"; opção "Medicamento não cadastrado" no item |
| ECF.10.04 – impressão com dados do cliente, profissional, instituição (CNES), data/hora e assinatura; endereço do cliente em controle especial; idade e sexo em antimicrobiano | Botão "Imprimir" da receita (ex.: nº 2 antimicrobiano, nº 4 controle especial) | Botão "Imprimir" da receita; área "Assinatura e carimbo do profissional" com espaço para o carimbo |
| ECF.10.07 – lista de medicamentos com linhas distintas e destaque ao passar o mouse ou selecionar | Busca de medicamento no formulário e em "Favoritos" | Autocompletar de medicamento no formulário e em "Favoritos" |
| ECF.10.10 – medicamentos favoritos por profissional, usados na prescrição | Menu "Favoritos"; no item da receita, seletor "Favoritos..." | Menu "Favoritos"; no formulário, etiquetas em "Favoritos:" |

Nos dois, a data limite de uso de cada item (ECF.10.03) é sugerida pela data de emissão + prazo do tipo de medicamento e pode ser alterada.

## Web API

Todas as rotas do profissional exigem o cabeçalho HTTP `X-Professional-Id` com o id do profissional (1 a 4 nos dados iniciais). No Swagger há um campo para informá-lo.

| Método e rota | Descrição |
|---|---|
| `GET /odata/Prescriptions` | Lista de receitas com OData: `$filter`, `$orderby`, `$select`, `$top` (máx. 100), `$skip`, `$count` |
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

Erros seguem o padrão ProblemDetails (RFC 7807): regra de negócio violada → 400, registro inexistente → 404, mensagem em português no campo `detail`.

## Estrutura do projeto

| Pasta ou arquivo | Conteúdo |
|---|---|
| `docker-compose.yml` | Sobe MySQL, Web API, Angular e Blazor |
| `.dockerignore` | Pastas que não entram nas imagens (`node_modules`, `bin`, `obj` etc.) |
| `.github/workflows` | Pipelines CI/CD (`backend.yml`, `frontend.yml`, `blazor.yml`) |
| `Prescription.sln` | Solução .NET com todos os projetos .NET (API, camadas, Blazor e testes) |
| `db` | Scripts SQL (`01_schema.sql`, `02_seed.sql`) e DER (`der.md` em Mermaid e `der.png`) |
| `business/Prescription.Domain` | Domain: entidades, value objects e regras |
| `business/Prescription.Application` | Application: casos de uso e contratos dos repositórios |
| `data` | Infrastructure: Entity Framework Core (MySQL) e repositórios |
| `web/backend` | Web API .NET 9 (controllers, OData, Swagger) e `Dockerfile` |
| `web/frontend` | Angular 21 (camadas `domain`, `application`, `infrastructure`, `presentation`) e `Dockerfile` |
| `web/blazor` | Blazor .NET 10 com MudBlazor (Interactive Server) e `Dockerfile` |
| `testes` | Testes unitários xUnit (Domain e Application) |
| `testes/integration` | Testes de integração xUnit (repositórios e API contra MySQL) |
| `testes/blazor` | Testes de componentes do Blazor (xUnit + bUnit) |

## Testes (opcional, fora do Docker)

Exigem .NET 9 SDK, .NET 10 SDK e Node.js 24 instalados.

Backend e Blazor, na pasta raiz (roda os testes unitários, os de integração e os do Blazor com bUnit):

```bash
dotnet test
```

Os testes de integração rodam somente com a variável `PRESCRIPTION_TEST_DB` apontando para o MySQL (por exemplo, o do `docker compose`); sem ela ficam ignorados. Cada teste roda numa transação desfeita no final, sem alterar os dados.

```bash
# Linux/macOS
export PRESCRIPTION_TEST_DB="server=localhost;port=3306;database=prescription_db;user=prescription_app;password=prescription_dev_2026"
# Windows PowerShell
$env:PRESCRIPTION_TEST_DB = "server=localhost;port=3306;database=prescription_db;user=prescription_app;password=prescription_dev_2026"
```

Frontend, na pasta `web/frontend`:

```bash
npm ci
npx ng test --watch=false
```

## Blazor fora do Docker (opcional)

Exige .NET 10 SDK e a Web API no ar (por exemplo, a do `docker compose`). Na pasta raiz:

```bash
dotnet run --project web/blazor/Prescription.Blazor.csproj
```

Abre em http://localhost:5100 e usa a API em `http://localhost:5000` (`web/blazor/appsettings.Development.json`). Para outra API, informar o endereço na variável de ambiente `ApiBaseUrl`.

## CI/CD (GitHub Actions)

Três pipelines em `.github/workflows`, um por microserviço. Cada um roda em push e pull request para a `main` quando mudam as pastas indicadas. O job `deploy` roda só em push na `main` e publica a imagem no GHCR (GitHub Container Registry) com as tags `latest` e o SHA do commit.

| Pipeline | Dispara quando mudam | Job `build-test` | Artefatos | Imagem (`deploy`) |
|---|---|---|---|---|
| `backend.yml` | `business`, `data`, `web/backend`, `testes`, `db`, `Prescription.sln`, `.dockerignore`, o próprio arquivo | MySQL 8.0 temporário com os scripts de `db`; .NET 9 e .NET 10; build e testes de toda a solução (unitários, integração e Blazor) | `backend-test-results`, `prescription-api` | `ghcr.io/<dono>/prescription-api` |
| `frontend.yml` | `web/frontend`, `.dockerignore`, o próprio arquivo | Node 24; `npm ci`, testes (Vitest) e `ng build` | `prescription-web` | `ghcr.io/<dono>/prescription-web` |
| `blazor.yml` | `web/blazor`, `testes/blazor`, `.dockerignore`, o próprio arquivo | .NET 10; build e testes do Blazor (bUnit); publicação | `blazor-test-results`, `prescription-blazor` | `ghcr.io/<dono>/prescription-blazor` |

`<dono>` é o dono do repositório no GitHub, em minúsculas.
