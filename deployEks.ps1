#!/bin/bash

set -e

NAMESPACE="fase4"
AWS_REGION="us-east-1"
EKS_CLUSTER="fase4-users-cluster"

echo "🔗 Conectando ao EKS cluster..."
aws eks update-kubeconfig \
  --region $AWS_REGION \
  --name $EKS_CLUSTER

echo "🧹 Limpando namespace antigo..."
kubectl delete namespace $NAMESPACE --ignore-not-found || true
sleep 10

echo "🔧 Criando namespace..."
kubectl create namespace $NAMESPACE

echo "📦 Aplicando StorageClass e PVC..."
kubectl apply -f k8s/eks/02-postgres-pvc.yaml

echo "📦 Aplicando manifests (EKS)..."
kubectl apply -f k8s/eks/00-namespace.yaml
kubectl apply -f k8s/eks/01-secrets.yaml
kubectl apply -f k8s/eks/02-postgres.yaml
kubectl apply -f k8s/eks/03-rabbitmq.yaml
kubectl apply -f k8s/eks/04-users-api.yaml
kubectl apply -f k8s/eks/05-service.yaml
kubectl apply -f k8s/eks/06-hpa.yaml
kubectl apply -f k8s/eks/07-ingress.yaml

echo "⏳ Aguardando postgres..."
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=300s

echo "⏳ Aguardando rabbitmq..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n $NAMESPACE --timeout=300s

echo "⏳ Aguardando users-api..."
kubectl wait --for=condition=ready pod -l app=users-api -n $NAMESPACE --timeout=300s

echo "✅ Deploy EKS completo!"
echo ""
kubectl get pods -n $NAMESPACE -o wide
echo ""
kubectl get svc -n $NAMESPACE
echo ""
kubectl get pvc -n $NAMESPACE