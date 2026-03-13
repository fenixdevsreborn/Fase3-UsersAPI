# Workflows (Users API)

Estes workflows são do **projeto Users API** e devem rodar quando este projeto for a **raiz do repositório**.

- **`ci.yml`** — Restore, build (Release) e testes em `push` e `pull_request` na `junonn/mvp-aws`.
- **`publish-image.yml`** — Em `push` na `junonn/mvp-aws`: build da imagem Docker, push no ECR, disparo do `Fase3-InfraOrchestrador`.

Se o repositório for um **monorepo** (vários projetos na mesma raiz), o GitHub Actions só executa workflows que estejam em **`.github/workflows` na raiz do repositório**. Nesse caso, copie estes arquivos para a raiz (ex.: `.github/workflows/ci-users-api.yml`) e ajuste os caminhos (ex.: `Fase3-UsersAPI/Fcg.Users.slnx`).

Variables e secrets necessários para `publish-image.yml`: ver `docs/CI-CD.md` na raiz do repositório (se existir) ou a documentação do `Fase3-InfraOrchestrador`.
