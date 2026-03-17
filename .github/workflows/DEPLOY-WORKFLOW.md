# Workflow de Deploy — FCG Fenix (APIs)

Este documento descreve o workflow **deploy.yml** (testes → build Docker → push ECR → deploy EC2 via reusable do repositório de infra) e o que alterar para **gamesapi** e **paymentsapi**.

---

## 1. Workflow para Users API (referência)

O arquivo **`.github/workflows/deploy.yml`** já está configurado para **usersapi**:

- **Branch de deploy:** `junonn/mvp-aws`
- **OIDC:** secret `AWS_ROLE_ARN` (role com permissão ECR + SSM)
- **Testes:** `dotnet restore` → `dotnet build` → `dotnet test` em `Fcg.Users.slnx`
- **Imagem:** build com `Dockerfile.postgres`, tag = `github.sha`
- **ECR:** repositório `fcg-fenix-usersapi-ecr`
- **Deploy:** chama o reusable do repo de infra com `aws_region`, `environment`, `service`, `repository`, `image_tag`

**Variáveis de repositório necessárias:**

| Variável      | Exemplo                         | Uso |
|---------------|----------------------------------|-----|
| `INFRA_REPO`  | `sua-org/Fase3-InfraOrchestrador` | Repositório que contém o reusable `deploy-ec2.yml` (owner/repo). |
| `AWS_REGION`  | `us-east-1`                     | Região AWS (opcional; default us-east-1). |

**Secrets:**

| Secret          | Uso |
|-----------------|-----|
| `AWS_ROLE_ARN`  | ARN da role OIDC (ECR push + SSM SendCommand + ec2:DescribeInstances). |

---

## 2. O que mudar para Games API

Copie o **deploy.yml** para o repositório da Games API e altere **apenas** o que segue.

| Onde | De (usersapi) | Para (gamesapi) |
|------|----------------|-----------------|
| **Nome do workflow** | `name: Deploy Users API` | `name: Deploy Games API` |
| **env.SERVICE** | `usersapi` | `gamesapi` |
| **env.ECR_REPOSITORY_NAME** | `fcg-fenix-usersapi-ecr` | `fcg-fenix-gamesapi-ecr` |
| **Solution para testes** | `Fcg.Users.slnx` | `Fcg.Games.slnx` (ou o nome da solution do projeto Games) |
| **deploy.with.service** | já usa `env.SERVICE` | nada (continua `${{ env.SERVICE }}`) |

Ou seja: troque o **nome do workflow**, **SERVICE**, **ECR_REPOSITORY_NAME** e o arquivo da **solution** nos passos Restore / Build / Test. O restante (branch, OIDC, formato da imagem, chamada ao reusable) permanece igual.

---

## 3. O que mudar para Payments API

Copie o **deploy.yml** para o repositório da Payments API e altere **apenas** o que segue.

| Onde | De (usersapi) | Para (paymentsapi) |
|------|----------------|---------------------|
| **Nome do workflow** | `name: Deploy Users API` | `name: Deploy Payments API` |
| **env.SERVICE** | `usersapi` | `paymentsapi` |
| **env.ECR_REPOSITORY_NAME** | `fcg-fenix-usersapi-ecr` | `fcg-fenix-paymentsapi-ecr` |
| **Solution para testes** | `Fcg.Users.slnx` | `Fcg.Payments.slnx` (ou o nome da solution do projeto Payments) |

As variáveis `INFRA_REPO` e `AWS_REGION` e o secret `AWS_ROLE_ARN` são os mesmos nos três repositórios (mesma org, mesma infra, mesma role).

---

## 4. Boas práticas (padronização entre os repositórios)

1. **Nomenclatura**
   - **SERVICE:** sempre `usersapi`, `gamesapi` ou `paymentsapi` (minúsculo, sem hífen), igual ao Terraform e ao reusable.
   - **ECR_REPOSITORY_NAME:** sempre `fcg-fenix-{service}-ecr` (conforme convenção do projeto).

2. **Branch de deploy**
   - Manter a mesma branch nos três repos (ex.: `junonn/mvp-aws`) para que o comportamento seja idêntico.

3. **Variáveis de repositório**
   - **INFRA_REPO:** mesmo valor nos três (ex.: `org/Fase3-InfraOrchestrador`).
   - **AWS_REGION:** mesmo valor (ex.: `us-east-1`) ou deixar default no workflow.

4. **Secret AWS_ROLE_ARN**
   - Uma única role OIDC pode ser usada pelos três repositórios; a role deve ter permissão para ECR (push nos três repositórios ECR), SSM SendCommand e ec2:DescribeInstances.

5. **Tag da imagem**
   - Usar **sempre** `github.sha` como `image_tag` para rastreabilidade e para o reusable aplicar o mesmo valor na EC2.

6. **Ordem dos jobs**
   - Manter: **test** → **build-and-push** → **deploy** (chamada ao reusable). Não pular testes antes do push.

7. **Dockerfile**
   - Usar o mesmo padrão de build (ex.: `Dockerfile.postgres` para imagem all-in-one) nos três; só o contexto (solution/csproj) muda por repositório.

8. **Reusable**
   - Não alterar os **inputs** do reusable (`aws_region`, `environment`, `service`, `repository`, `image_tag`); apenas preencher corretamente a partir do deploy.yml de cada API.

Seguindo isso, os três repositórios ficam alinhados e fáceis de manter.
