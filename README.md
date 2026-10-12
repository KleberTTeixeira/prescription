# Prescrição médica – Avaliação Desenvolvedor Fullstack .NET + Angular

Sistema de prescrição médica (receita) com banco MySQL, Web API em .NET 9 e frontend em Angular 21, seguindo os requisitos SBIS de prescrição eletrônica (ECF.10).

## Pré-requisito

Somente **Docker** com **Docker Compose**. Não é preciso instalar MySQL, .NET nem Node: tudo roda em contêineres.

Portas usadas na máquina: **3306** (MySQL), **5000** (Web API) e **4200** (Angular). Elas precisam estar livres.

## Como executar

Na pasta raiz do projeto (onde está o `docker-compose.yml`):

```bash
docker compose up -d --build
```

A primeira execução demora alguns minutos (download das imagens, compilação da API e instalação dos pacotes do Angular). Acompanhar com:

```bash
docker compose ps
docker compose logs -f
```

Quando os três serviços estiverem no ar:

| O quê | Endereço |
|---|---|
| Sistema (Angular) | http://localhost:4200 |
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

1. Abrir http://localhost:4200 e escolher um profissional (o sistema não tem login; o profissional escolhido faz o papel do usuário logado).
2. **Receitas**: lista as receitas do profissional, com filtro por cliente, ordenação ao clicar no título das colunas e paginação. Daqui é possível criar, editar, visualizar, imprimir e excluir.
3. **Textos padrão** e **Favoritos**: cadastros próprios de cada profissional, usados no formulário da receita.

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

| Requisito | Onde ver |
|---|---|
| ECF.10.01 – receita não estruturada (texto livre) | Nova receita → formato "Não estruturada" |
| ECF.10.02 – textos padrão com título, usados como base e editáveis | Menu "Textos padrão"; no formulário, "Usar texto padrão" |
| ECF.10.03 – receita estruturada: busca de medicamento por nome, medicamento não cadastrado, todos os campos do item e prazo de utilização | Nova receita → formato "Estruturada". A data limite de uso é sugerida pela data de emissão + prazo do tipo de medicamento e pode ser alterada |
| ECF.10.04 – impressão com dados do cliente, profissional, instituição (CNES), data/hora e assinatura; endereço do cliente em controle especial; idade e sexo em antimicrobiano | Botão "Imprimir" da receita (ex.: nº 2 antimicrobiano, nº 4 controle especial) |
| ECF.10.07 – lista de medicamentos com linhas distintas e destaque ao passar o mouse ou selecionar | Busca de medicamento no formulário e em "Favoritos" |
| ECF.10.10 – medicamentos favoritos por profissional, usados na prescrição | Menu "Favoritos"; no item da receita, seletor "Favoritos..." |

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

| Pasta | Conteúdo |
|---|---|
| `db` | Scripts SQL (`01_schema.sql`, `02_seed.sql`) e DER (`der.md` em Mermaid e `der.png`) |
| `business/Prescription.Domain` | Domain: entidades, value objects e regras |
| `business/Prescription.Application` | Application: casos de uso e contratos dos repositórios |
| `data` | Infrastructure: Entity Framework Core (MySQL) e repositórios |
| `web/backend` | Web API .NET 9 (controllers, OData, Swagger) e `Dockerfile` |
| `web/frontend` | Angular 21 (camadas `domain`, `application`, `infrastructure`, `presentation`) e `Dockerfile` |
| `testes` | Testes unitários xUnit (Domain e Application) |
| `testes/integration` | Testes de integração xUnit (repositórios e API contra MySQL) |

## Testes (opcional, fora do Docker)

Exigem .NET 9 SDK e Node.js 24 instalados.

Backend, na pasta raiz:

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
