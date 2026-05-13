variable "project_name" {
  description = "Nome base usado nos recursos AWS."
  type        = string
  default     = "fase4-users-api"
}

variable "environment" {
  description = "Ambiente da implantacao."
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "Regiao AWS onde a infraestrutura sera criada."
  type        = string
  default     = "us-east-1"
}

variable "vpc_cidr" {
  description = "CIDR da VPC."
  type        = string
  default     = "10.40.0.0/16"
}

variable "cluster_version" {
  description = "Versao do Kubernetes no EKS."
  type        = string
  default     = "1.30"
}

variable "node_instance_types" {
  description = "Tipos de instancias EC2 para o managed node group."
  type        = list(string)
  default     = ["m7i-flex.large"]
}

variable "node_min_size" {
  description = "Quantidade minima de nos no node group."
  type        = number
  default     = 2
}

variable "node_max_size" {
  description = "Quantidade maxima de nos no node group."
  type        = number
  default     = 4
}

variable "node_desired_size" {
  description = "Quantidade desejada de nos no node group."
  type        = number
  default     = 2
}
