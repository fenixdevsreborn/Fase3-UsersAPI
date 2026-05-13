$NAMESPACE = "fase4"

Write-Host "Aplicando manifests locais da Users API..." -ForegroundColor Green
kubectl apply -f k8s/local/00-namespace.yaml
kubectl apply -f k8s/local/01-secrets.yaml
kubectl apply -f k8s/local/01-rabbitmq-secrets.yaml
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
Write-Host "Username: admin"
Write-Host "Password: admin123"
Write-Host "VHost: fiap`n"
