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

| Variable | Obrigatório | Default | Exemplo | Uso |
|----------|-------------|---------|---------|-----|
| `AWS_REGION` | Não | **`us-east-1`** (Virginia) | `us-east-1` | Região do ECR e da role. Se não definida, usa Virginia. |
| `ECR_REPOSITORY_NAME` | Não | **`fcg/fase03`** | `fcg/fase03` ou `fcg-prod-users-api` | Nome do repositório no ECR. Se não definida, usa `fcg/fase03`. |
| `ORCHESTRATOR_REPO` | Sim (para trigger) | — | `minha-org/Fase3-InfraOrchestrador` | Repositório que recebe o `repository_dispatch` (formato `owner/repo`). |
| `SERVICE_NAME` | Não | `users-api` | `users-api` | Nome do serviço no payload; deve coincidir com o esperado pelo orquestrador. |
| `ENVIRONMENT` | Não | `prod` | `prod` | Ambiente; usado no payload e para decidir se faz push da tag `latest`. |

### Valores padrão (ECR e região)

Todos os workflows de **Publish image** (UsersAPI, GamesAPI, PaymentsAPI, NotificationLambda) usam os mesmos padrões quando as variables não estão definidas:

| Variável | Valor padrão | Observação |
|----------|--------------|------------|
| **AWS_REGION** | `us-east-1` | Região da AWS (Virginia). O ECR e a IAM Role devem estar nessa região. |
| **ECR_REPOSITORY_NAME** | `fcg/fase03` | Nome do repositório no ECR. A imagem é enviada para `registry/fcg/fase03:tag`. |

Para usar outro repositório ou outra região, defina a variable no repositório em **Settings → Secrets and variables → Actions**.

---

## 3. Como personalizar o nome do serviço e o repositório ECR

- **Nome do serviço:** defina a variable **`SERVICE_NAME`** (ex.: `users-api`, `games-api`, `payments-api`, `notification-lambda`). O mesmo valor é enviado no payload para o orquestrador. Se não definir, o default no workflow é `users-api`.
- **Repositório ECR:** defina a variable **`ECR_REPOSITORY_NAME`** para usar um repositório diferente do padrão `fcg/fase03`. O valor deve ser o **nome completo** do repositório no ECR (ex.: `fcg-prod-users-api`). Esse nome costuma ser gerado pelo Terraform no orquestrador. Consulte os outputs do Terraform para obter o nome exato.

Para outro microsserviço (ex.: Games API) no mesmo estilo de repo: copie os dois workflows, ajuste a solution no `ci.yml` (ex.: `Fcg.Games.slnx`) e defina `SERVICE_NAME` e, se necessário, `ECR_REPOSITORY_NAME` para esse serviço.

---

## 3.1. Dockerfile e contexto de build

Os repositórios **não são monorepo**: cada serviço tem seu próprio repositório. O **Dockerfile** de cada um espera que o **contexto de build seja a raiz do repositório** (onde estão a pasta `src/` e o próprio `Dockerfile`). Os caminhos no Dockerfile usam `src/...` (sem prefixo do tipo `Fase3-UsersAPI/`). No CI, o workflow faz `docker build -f Dockerfile .` na raiz; não é necessário ajustar caminhos para monorepo.

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
