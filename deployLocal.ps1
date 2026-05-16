$NAMESPACE = "fase4"

function Require-Env([string]$Name) {
  $value = [Environment]::GetEnvironmentVariable($Name)
  if ([string]::IsNullOrWhiteSpace($value)) {
    throw "Environment variable '$Name' is required to create Kubernetes secrets."
  }
  return $value
}

function Apply-Secret([string]$Name, [string[]]$Literals) {
  kubectl create secret generic $Name -n $NAMESPACE @Literals --dry-run=client -o yaml | kubectl apply -f -
}

Write-Host "Aplicando manifests locais da Users API..." -ForegroundColor Green
kubectl apply -f k8s/local/00-namespace.yaml
Apply-Secret "app-secrets" @(
  "--from-literal=db-connection=$(Require-Env 'USERS_DB_CONNECTION_STRING')",
  "--from-literal=db-password=$(Require-Env 'POSTGRES_PASSWORD')",
  "--from-literal=jwt-secret=$(Require-Env 'JWT_SECRET')",
  "--from-literal=jwt-issuer=$(Require-Env 'JWT_ISSUER')",
  "--from-literal=jwt-audience=$(Require-Env 'JWT_AUDIENCE')",
  "--from-literal=jwt-key-id=$(Require-Env 'JWT_KEY_ID')"
)
Apply-Secret "rabbitmq-secrets" @(
  "--from-literal=rabbitmq-user=$(Require-Env 'RABBITMQ_USERNAME')",
  "--from-literal=rabbitmq-pass=$(Require-Env 'RABBITMQ_PASSWORD')",
  "--from-literal=rabbitmq-vhost=$(Require-Env 'RABBITMQ_VHOST')"
)
kubectl apply -f k8s/local/02-postgres.yaml
kubectl apply -f k8s/local/03-rabbitmq.yaml
kubectl apply -f k8s/local/04-users-api.yaml
kubectl apply -f k8s/local/05-service.yaml

Write-Host "Aguardando postgres..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=120s 2>$null

Write-Host "Aguardando rabbitmq compartilhado..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=rabbitmq -n $NAMESPACE --timeout=120s 2>$null

Write-Host "Aguardando users-api..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=users-api -n $NAMESPACE --timeout=120s 2>$null

Write-Host "`nDeploy local da Users API completo.`n" -ForegroundColor Green
kubectl get pods -n $NAMESPACE -o wide

Write-Host "`nAcessar aplicacao:" -ForegroundColor Cyan
Write-Host "kubectl port-forward svc/users-api 8080:80 -n $NAMESPACE"
Write-Host "http://localhost:8080/health`n"

Write-Host "Acessar RabbitMQ Management compartilhado:" -ForegroundColor Cyan
Write-Host "kubectl port-forward svc/rabbitmq 15672:15672 -n $NAMESPACE"
Write-Host "http://localhost:15672"
Write-Host "Use as credenciais definidas nas variaveis RABBITMQ_USERNAME/RABBITMQ_PASSWORD/RABBITMQ_VHOST.`n"
