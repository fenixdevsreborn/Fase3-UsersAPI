$NAMESPACE = "fase4"
$AWS_REGION = "us-east-1"
$EKS_CLUSTER = "fase4-users-cluster"

$ErrorActionPreference = "Stop"
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

Write-Host "Conectando ao EKS cluster..."
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER

Write-Host "Aplicando StorageClass e PVC..."
kubectl apply -f k8s/eks/02-postgres-pvc.yaml

Write-Host "Aplicando manifests EKS da Users API..."
kubectl apply -f k8s/eks/00-namespace.yaml
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
kubectl apply -f k8s/eks/02-postgres.yaml
kubectl apply -f k8s/eks/03-rabbitmq.yaml
kubectl apply -f k8s/eks/04-users-api.yaml
kubectl apply -f k8s/eks/05-service.yaml
kubectl apply -f k8s/eks/06-hpa.yaml
kubectl apply -f k8s/eks/07-ingress.yaml

Write-Host "Aguardando postgres..."
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=300s

Write-Host "Aguardando rabbitmq compartilhado..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n $NAMESPACE --timeout=300s

Write-Host "Aguardando users-api..."
kubectl wait --for=condition=ready pod -l app=users-api -n $NAMESPACE --timeout=300s

Write-Host "Deploy EKS da Users API completo."
kubectl get pods -n $NAMESPACE -o wide
kubectl get svc -n $NAMESPACE
kubectl get pvc -n $NAMESPACE
