$NAMESPACE = "fase4"

Write-Host "🧹 Limpando..." -ForegroundColor Yellow
kubectl delete namespace $NAMESPACE --ignore-not-found 2>$null
Start-Sleep -Seconds 5

Write-Host "🔧 Criando namespace..." -ForegroundColor Green
kubectl create namespace $NAMESPACE

Write-Host "📦 Aplicando manifests (local)..." -ForegroundColor Green
kubectl apply -f k8s/local/00-namespace.yaml
kubectl apply -f k8s/local/01-secrets.yaml
kubectl apply -f k8s/local/02-postgres.yaml
kubectl apply -f k8s/local/03-rabbitmq.yaml
kubectl apply -f k8s/local/04-users-api.yaml
kubectl apply -f k8s/local/05-service.yaml

Write-Host "⏳ Aguardando postgres..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=120s 2>$null

Write-Host "⏳ Aguardando rabbitmq..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=rabbitmq -n $NAMESPACE --timeout=120s 2>$null

Write-Host "⏳ Aguardando users-api..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=users-api -n $NAMESPACE --timeout=120s 2>$null

Write-Host "`n✅ Deploy local completo!`n" -ForegroundColor Green
kubectl get pods -n $NAMESPACE -o wide

Write-Host "`n🌐 Acessar aplicação:" -ForegroundColor Cyan
Write-Host "kubectl port-forward svc/users-api 8080:80 -n $NAMESPACE"
Write-Host "http://localhost:8080/health`n"

Write-Host "🐰 Acessar RabbitMQ Management:" -ForegroundColor Cyan
Write-Host "kubectl port-forward svc/rabbitmq 15672:15672 -n $NAMESPACE"
Write-Host "http://localhost:15672"
Write-Host "Username: admin"
Write-Host "Password: admin123`n"