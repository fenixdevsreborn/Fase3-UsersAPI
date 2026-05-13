$NAMESPACE = "fase4"
$AWS_REGION = "us-east-1"
$EKS_CLUSTER = "fase4-users-cluster"

$ErrorActionPreference = "Stop"

Write-Host "Conectando ao EKS cluster..."
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER

Write-Host "Aplicando StorageClass e PVC..."
kubectl apply -f k8s/eks/02-postgres-pvc.yaml

Write-Host "Aplicando manifests EKS da Users API..."
kubectl apply -f k8s/eks/00-namespace.yaml
kubectl apply -f k8s/eks/01-secrets.yaml
kubectl apply -f k8s/eks/01-rabbitmq-secrets.yaml
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
