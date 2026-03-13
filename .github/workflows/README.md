# Workflows — CI e publicação de imagem

Este repositório usa dois workflows: **CI** (restore, build, test) e **Publish image** (Docker → ECR → disparo do orquestrador).

---

## 1. Resumo das etapas

### ci.yml

| Etapa | Ação |
|-------|------|
| Checkout | Baixa o código do repositório. |
| Setup .NET | Instala o SDK .NET (versão em `env.DOTNET_VERSION`). |
| Restore | `dotnet restore` na solution (ex.: `Fcg.Users.slnx`). |
| Build | `dotnet build -c Release --no-restore`. |
| Test | `dotnet test -c Release --no-build`; o job **falha** se algum teste falhar. |
| Upload test results | Opcional: envia artefato com os `.trx` para download na aba Actions. |

### publish-image.yml

| Etapa | Ação |
|-------|------|
| Checkout | Baixa o código. |
| Configure AWS (OIDC) | Assume a IAM Role via OIDC (sem access key); necessário `id-token: write`. |
| Login to ECR | Autentica no Amazon ECR; o registry fica em `steps.ecr.outputs.registry`. |
| Docker meta | Monta a tag (SHA curto) e a URI completa da imagem (registry/repo:tag). |
| Build image | `docker build -f Dockerfile -t <uri>` no contexto da raiz do repo. |
| Push image | `docker push` da imagem com tag SHA. |
| Push latest | Se `ENVIRONMENT == 'prod'`, faz push também da tag `latest`. |
| Trigger orchestrator | Envia `repository_dispatch` (evento `deploy-request`) para o repo do orquestrador com o payload (service_name, image_tag, image_uri, commit_sha, environment). |

---

## 2. GitHub Secrets e Variables

Configure em **Settings → Secrets and variables → Actions**.

### Secrets (obrigatórios para publish-image)

| Secret | Obrigatório | Uso |
|--------|-------------|-----|
| `AWS_ROLE_ARN_ECR` | Sim (para publish) | ARN da IAM Role que o GitHub assume via OIDC. A role deve ter permissão para `ecr:GetAuthorizationToken` e, no ECR, `ecr:BatchCheckLayerAvailability`, `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`. |
| `ORCHESTRATOR_REPO_TOKEN` | Sim (para trigger) | PAT (Personal Access Token) com escopo `repo` (ou pelo menos acesso ao repositório do orquestrador) para chamar a API de `repository_dispatch`. |

### Variables (recomendado)

| Variable | Obrigatório | Exemplo | Uso |
|----------|-------------|---------|-----|
| `AWS_REGION` | Não (default us-east-1) | `us-east-1` | Região do ECR e da role. |
| `ECR_REPOSITORY_NAME` | Sim (para publish) | `fcg-prod-users-api` | Nome completo do repositório no ECR (conforme criado pelo Terraform: `name_prefix` + sufixo do serviço). |
| `ORCHESTRATOR_REPO` | Sim (para trigger) | `minha-org/Fase3-InfraOrchestrador` | Repositório que recebe o `repository_dispatch` (formato `owner/repo`). |
| `SERVICE_NAME` | Não (default users-api) | `users-api` | Nome do serviço enviado no payload; deve coincidir com o esperado pelo orquestrador (users-api, games-api, payments-api, notification-lambda). |
| `ENVIRONMENT` | Não (default prod) | `prod` | Ambiente; usado no payload e para decidir se faz push da tag `latest`. |

---

## 3. Como personalizar o nome do serviço e o repositório ECR

- **Nome do serviço:** defina a variable **`SERVICE_NAME`** (ex.: `users-api`, `games-api`, `payments-api`, `notification-lambda`). O mesmo valor é enviado no payload para o orquestrador. Se não definir, o default no workflow é `users-api`.
- **Repositório ECR:** defina a variable **`ECR_REPOSITORY_NAME`**. O valor deve ser o **nome completo** do repositório no ECR (ex.: `fcg-prod-users-api`). Esse nome costuma ser gerado pelo Terraform no orquestrador (`name_prefix` + sufixo, ex.: `fcg-prod` + `-users-api`). Consulte os outputs do Terraform (`ecr_repository_urls` ou o nome do recurso) para obter o nome exato.

Para outro microsserviço (ex.: Games API) no mesmo estilo de repo: copie os dois workflows, ajuste a solution no `ci.yml` (ex.: `Fcg.Games.slnx`) e defina `SERVICE_NAME` e `ECR_REPOSITORY_NAME` para esse serviço.

---

## 4. Exemplo do payload enviado no `repository_dispatch`

O orquestrador (`Fase3-InfraOrchestrador`) recebe um evento **repository_dispatch** com:

- **event_type:** `deploy-request`
- **client_payload:** objeto JSON com os campos abaixo.

Exemplo de **client_payload**:

```json
{
  "service_name": "users-api",
  "image_tag": "a1b2c3d",
  "image_uri": "123456789012.dkr.ecr.us-east-1.amazonaws.com/fcg-prod-users-api:a1b2c3d",
  "commit_sha": "a1b2c3d4e5f6789012345678901234567890abcd",
  "environment": "prod"
}
```

- **service_name:** identificador do serviço no orquestrador (deve bater com a variável Terraform, ex.: users-api).
- **image_tag:** tag da imagem que foi publicada (ex.: 7 primeiros caracteres do commit).
- **image_uri:** URI completa da imagem no ECR (registry/repositório:tag).
- **commit_sha:** SHA completo do commit que originou o build.
- **environment:** ambiente (prod, staging, demo) para o qual a imagem foi publicada / será implantada.

O workflow **deploy-from-service-update** no orquestrador escuta esse evento e aplica o Terraform atualizando a tag do serviço correspondente.
