# Plano de deploy da Users API no AWS EKS

Este diretorio cria a infraestrutura AWS para executar a Web API .NET 8 em Kubernetes gerenciado com Amazon EKS.

> Observacao: EKS e ECS sao dois orquestradores diferentes. O plano abaixo usa EKS, porque o repositorio ja possui manifests Kubernetes. A imagem da aplicacao sera obtida pelo Docker Hub.

## Arquitetura proposta

- Amazon VPC com subnets publicas e privadas em 3 AZs.
- NAT Gateway para saida dos pods em subnets privadas.
- Amazon EKS com managed node group EC2.
- Docker Hub como registry da imagem Docker da API.
- Add-ons do EKS: CoreDNS, kube-proxy, VPC CNI e EBS CSI Driver.
- AWS Load Balancer Controller via Helm para publicar o `Ingress` como ALB.
- Metrics Server via Helm para suportar o HPA da API.
- Kubernetes manifests em `../../../k8s/eks` para namespace, secrets, Postgres, RabbitMQ, API, service, HPA e ingress.

## Plano de execucao

1. Validar a aplicacao localmente:

   ```powershell
   dotnet restore
   dotnet build
   docker build -t users-api:local .
   ```

2. Criar a infraestrutura AWS:

   ```powershell
   cd iac/terraform/aws
   Copy-Item terraform.tfvars.example terraform.tfvars
   terraform init
   terraform plan
   terraform apply
   ```

   O exemplo usa `m7i-flex.large`, que oferece mais memoria para rodar EKS, add-ons, Postgres, RabbitMQ e a API. Revise os custos antes de manter esse tamanho por muito tempo.

3. Configurar o `kubectl`:

   ```powershell
   
   ```

4. Publicar a imagem no Docker Hub:

   ```powershell
   docker login
   docker build -t adinteltidev/users-api:5 .
   docker push adinteltidev/users-api:5
   ```

5. Atualizar a imagem no manifest:

   ```powershell
   kubectl set image deployment/users-api users-api=adinteltidev/users-api:5 -n fase4 --local -o yaml
   ```

   Para persistir no repositorio, altere `../../../k8s/eks/04-users-api.yaml` para usar a tag publicada no Docker Hub.

6. Revisar secrets antes de aplicar:

   - Trocar `jwt-secret`.
   - Trocar senha do Postgres.
   - Para producao, preferir AWS Secrets Manager ou External Secrets Operator.

   Se a imagem no Docker Hub for privada, crie o secret antes de aplicar o deployment e adicione `imagePullSecrets` no `../../../k8s/eks/04-users-api.yaml`:

   ```powershell
   kubectl create secret docker-registry dockerhub-secret `
     --docker-server=https://index.docker.io/v1/ `
     --docker-username=<usuario> `
     --docker-password=<token-ou-senha> `
     --docker-email=<email> `
     -n fase4
   ```

7. Aplicar os manifests:

   ```powershell
   kubectl apply -f ../../../k8s/eks/00-namespace.yaml
   kubectl create secret generic app-secrets -n fase4 `
     --from-literal=db-connection="$env:USERS_DB_CONNECTION_STRING" `
     --from-literal=db-password="$env:POSTGRES_PASSWORD" `
     --from-literal=jwt-secret="$env:JWT_SECRET" `
     --from-literal=jwt-issuer="$env:JWT_ISSUER" `
     --from-literal=jwt-audience="$env:JWT_AUDIENCE" `
     --from-literal=jwt-key-id="$env:JWT_KEY_ID" `
     --dry-run=client -o yaml | kubectl apply -f -
   kubectl apply -f ../../../k8s/eks/02-postgres-pvc.yaml
   kubectl apply -f ../../../k8s/eks/02-postgres.yaml
   kubectl apply -f ../../../k8s/eks/03-rabbitmq.yaml
   kubectl apply -f ../../../k8s/eks/04-users-api.yaml
   kubectl apply -f ../../../k8s/eks/05-service.yaml
   kubectl apply -f ../../../k8s/eks/06-hpa.yaml
   kubectl apply -f ../../../k8s/eks/07-ingress.yaml
   ```

8. Validar o deploy:

   ```powershell
   kubectl get pods -n fase4
   kubectl get hpa -n fase4
   kubectl get ingress -n fase4
   kubectl describe ingress users-api-ingress -n fase4
   ```

## Ajustes antes de producao

- Usar Amazon RDS PostgreSQL em vez de Postgres dentro do cluster.
- Usar Amazon MQ, MSK ou um RabbitMQ gerenciado/HA em vez de `emptyDir`.
- Substituir secrets em YAML por Secrets Manager/External Secrets.
- Configurar dominio real no `Ingress` e certificado ACM valido na mesma regiao do ALB.
- Adicionar pipeline CI/CD para build, push no Docker Hub e deploy no EKS.
- Avaliar Karpenter ou Cluster Autoscaler para autoscaling de nos.
- Ativar logs e metricas do control plane do EKS se o custo for aceitavel.
