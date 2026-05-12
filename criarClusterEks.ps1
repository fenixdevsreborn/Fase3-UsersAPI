$EKS_CLUSTER_NAME = "fase4-cluster"
$AWS_REGION = "us-east-1"
$NODE_TYPE = "t3.medium"
$NODES_COUNT = 2

$OIDC_ID = "D2F9F8B1CAB65B9799BEECB5AF5EA143"

aws iam create-role --role-name AmazonEKS_EBS_CSI_DriverRole --assume-role-policy-document '{"Version": "2012-10-17", "Statement": [ { "Effect": "Allow", "Principal": { "Federated": "arn:aws:iam::123456789:oidc-provider/oidc.eks.us-east-1.amazonaws.com/id/'"D2F9F8B1CAB65B9799BEECB5AF5EA143"'" }, "Action": "sts:AssumeRoleWithWebIdentity", "Condition": { "StringEquals": { "oidc.eks.us-east-1.amazonaws.com/id/'"D2F9F8B1CAB65B9799BEECB5AF5EA143"':sub": "system:serviceaccount:kube-system:ebs-csi-controller-sa" } } } ] }'

aws iam attach-role-policy --role-name AmazonEKS_EBS_CSI_DriverRole --policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy

aws eks create-addon --cluster-name "fase4-cluster" --addon-name aws-ebs-csi-driver --addon-version v1.23.0 --region "us-east-1"

kubectl get deployment -n kube-system ebs-csi-controller