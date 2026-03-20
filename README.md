# 👤 Users API - Fase 3 (MVP AWS)

## 📌 Visão Geral

A **Users API** é um microsserviço responsável pelo gerenciamento de usuários dentro do ecossistema da Fase 3, projetado sob uma abordagem **cloud-native e serverless na AWS**.

O serviço foi desenvolvido para atuar como **fonte central de identidade e dados de usuários**, garantindo integração consistente com outros serviços da plataforma, como Games, Payments e Notifications.

---

## 🎯 Objetivo

* Centralizar o gerenciamento de usuários
* Garantir escalabilidade horizontal automática
* Fornecer endpoints seguros e performáticos
* Servir como base para autenticação e autorização em outros serviços

---

## 🏗️ Arquitetura

O projeto adota **Arquitetura Hexagonal (Ports & Adapters)**, promovendo isolamento entre domínio e infraestrutura.

### 🔹 Organização em Camadas

* **Domain**

  * Entidades de usuário
  * Regras de negócio
  * Interfaces (contracts)

* **Application**

  * Casos de uso (Use Cases)
  * Orquestração do domínio

* **Infrastructure**

  * Implementações de repositórios
  * Integrações com AWS

* **API (EntryPoint)**

  * AWS Lambda handlers
  * Exposição via API Gateway

---

## ☁️ Infraestrutura AWS

A aplicação utiliza serviços gerenciados da AWS para garantir alta disponibilidade e resiliência:

* **AWS Lambda**

  * Execução serverless dos endpoints

* **Amazon API Gateway**

  * Camada de exposição HTTP e roteamento

* **Amazon DynamoDB**

  * Banco NoSQL altamente escalável

* **AWS IAM**

  * Gerenciamento de permissões

* **AWS CloudWatch**

  * Observabilidade (logs e métricas)

➡️ Esse modelo permite escalar automaticamente sem necessidade de gerenciamento de servidores, característica central de arquiteturas serverless.

---

## 🔗 Funcionalidades

* 👤 Cadastro de usuários
* 📄 Consulta por ID
* 🔍 Busca por critérios (ex: email)
* ✏️ Atualização de dados
* ❌ Exclusão de usuários

---

## 🔐 Segurança

* Validação de entrada (input validation)
* Controle de acesso via IAM
* Possível integração com autenticação baseada em token (ex: JWT ou Cognito)

💡 Serviços como o **Amazon Cognito** são frequentemente utilizados para autenticação e gerenciamento de usuários em arquiteturas AWS ([Documentação AWS][1])

---

## 🚀 Stack Tecnológica

* **.NET 8**
* **C#**
* **AWS Lambda**
* **API Gateway**
* **DynamoDB**
* **xUnit + Moq**

---

## ⚙️ Execução do Projeto

### 🔧 Pré-requisitos

* .NET 8 SDK
* AWS CLI configurado
* Conta AWS ativa
* Amazon Lambda Tools

---

### ▶️ Rodando localmente

```bash
dotnet restore
dotnet build
dotnet run
```

---

### ☁️ Deploy (AWS)

```bash
dotnet lambda deploy-serverless
```

Ou utilizando infraestrutura como código:

```bash
terraform init
terraform apply
```

---

## 📦 Estrutura do Projeto

```bash
src/
 ├── Domain/
 ├── Application/
 ├── Infrastructure/
 ├── API/
 └── Shared/
```

---

## 🔄 Integração com o Ecossistema

A Users API atua como serviço base e pode ser consumida por:

* 🎮 Games API → associação usuário-jogos
* 💳 Payments API → dados de cobrança
* 🔔 Notifications API → envio de notificações
