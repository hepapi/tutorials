## Create a User in a Kubernetes Cluster and Grant Access
We’ll illustrate the steps required to create a user, generate necessary certificates, and configure access using a kubeconfig file within a Kubernetes cluster.

- Create minikube cluster

```bash
minikube start
```

- Create folder for serviceaccount hands-on

```bash
  mkdir -p Kubernetes/examples/kubernetes-security/user
  cd Kubernetes/examples/kubernetes-security/user
```

### Generating a Key Pair and Certificate Signing Request (CSR)

- Generate a key pair and a Certificate Signing Request (CSR) using OpenSSL:

```bash
openssl genrsa -out user.key 2048
openssl req -new -key user.key -out user.csr -subj "/CN=user/O=group"
```

- create a CSR YAML file named “user-csr.yaml” to submit to Kubernetes and copy below code
- Encode the CSR file in base64 and prepare it for the Kubernetes YAML.

```yaml
apiVersion: certificates.k8s.io/v1
kind: CertificateSigningRequest
metadata:
  name: user-csr
spec:
  request: <base64_encoded_csr>
  signerName: kubernetes.io/kube-apiserver-client
  usages:
  - client auth
```

```bash
cat user.csr | base64 | tr -d '\n'
```
-Replace <base64_encoded_csr> with the base64 output from the previous step.

- Apply the CSR YAML file to Kubernetes

```bash
kubectl apply -f user-csr.yaml
```

- Approve the CSR and retrieve the approved certificate:

```bash
kubectl get csr
kubectl certificate approve user-csr
kubectl get csr
#get user certificate
kubectl get csr user-csr -o jsonpath='{.status.certificate}' | base64 --decode > user.crt
```
### Configure a kubeconfig File

To access the Kubernetes cluster, it’s essential to generate a configuration file tailored for the user. This file needs to encompass critical information, including the Kubernetes API access specifics, the Cluster CA certificate, as well as the user’s certificate and context name. Initially, we’ll generate the kubeconfig file specifically for the user.

```bash
kubectl config set-credentials user --client-certificate=user.crt --client-key=user.key
kubectl config set-context user-context --cluster=minikube --namespace=test --user=user
```

### Test new user
- Change context 

```bash
kubectl config use-context user-context
kubectl get po
```
You can not access any resources in kubernetes cluster yes you There is a user but he/she doesn't have any permission

### Create Role and RoleBinding

Create a Role and RoleBinding in the test namespace.

- Create role.yaml and copy below code

```yaml
# role.yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: test
  name: pod-reader
rules:
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "watch", "list"]
```
- Create rolebindings.yaml and copy below code

```yaml
# rolebinding.yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: read-pods
  namespace: test
subjects:
- kind: User
  name: user
  apiGroup: rbac.authorization.k8s.io
roleRef:
  kind: Role
  name: pod-reader
  apiGroup: rbac.authorization.k8s.io
```

```bash
kubectl config use-context minikube
kubectl create ns test
kubectl apply -f role.yaml
kubectl apply -f rolebindings.yaml
```

- Test again 

```bash
kubectl config use-context user-context
kubectl get po
```