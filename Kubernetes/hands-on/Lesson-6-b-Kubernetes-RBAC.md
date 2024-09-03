# RBAC
- Role-based access control (RBAC) is a method of regulating access to computer or network resources based on the roles of individual users within your organization.

- RBAC authorization uses the rbac.authorization.k8s.io API group to drive authorization decisions, allowing you to dynamically configure policies through the Kubernetes API.

### API objects

- The RBAC API declares four kinds of Kubernetes object: Role, ClusterRole, RoleBinding and ClusterRoleBinding

## ServiceAccount

te minikube cluster with calico CNI

```bash
minikube start --network-plugin=cni --cni=calico
```

```bash
  mkdir -p Kubernetes/examples/rbac
  cd Kubernetes/examples/rbac
  touch serviceaccount.yaml
```

```bash
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-serviceaccount
```

```bash
kubectl apply -f serviceaccount.yaml
kubectl get sa
```


### Role and ClusterRole

- An RBAC Role or ClusterRole contains rules that represent a set of permissions. Permissions are purely additive (there are no "deny" rules).

- A Role always sets permissions within a particular namespace; when you create a Role, you have to specify the namespace it belongs in.

- ClusterRole, by contrast, is a non-namespaced resource. The resources have different names (Role and ClusterRole) because a Kubernetes object always has to be either namespaced or not namespaced; it can't be both.

- ClusterRoles have several uses. You can use a ClusterRole to:

  - define permissions on namespaced resources and be granted access within individual namespace(s)
  - define permissions on namespaced resources and be granted access across all namespaces
  - define permissions on cluster-scoped resources

- If you want to define a role within a namespace, use a Role; if you want to define a role cluster-wide, use a ClusterRole.

### Role example

- Create role.yaml file and copy beloy code

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: default
  name: pod-reader
rules:
- apiGroups: [""] # "" indicates the core API group
  resources: ["pods"]
  verbs: ["get", "watch", "list"]
```

```bash
kubectl apply -f role.yaml
kubectl get role
```

### ClusterRole example

- A ClusterRole can be used to grant the same permissions as a Role. Because ClusterRoles are cluster-scoped, you can also use them to grant access to:

  - cluster-scoped resources (like nodes)
  - non-resource endpoints (like /healthz)
  - namespaced resources (like Pods), across all namespaces

- For example: you can use a ClusterRole to allow a particular user to run kubectl get pods --all-namespaces

- Create clusterrole.yaml file and copy below code.

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  # "namespace" omitted since ClusterRoles are not namespaced
  name: secret-reader
rules:
- apiGroups: [""]
  # at the HTTP level, the name of the resource for accessing Secret
  # objects is "secrets"
  resources: ["secrets"]
  verbs: ["get", "watch", "list"]
```

```bash
kubectl apply -f clusterrole.yaml
kubectl get clusterrole
```

## RoleBinding and ClusterRoleBinding

- A role binding grants the permissions defined in a role to a user or set of users. It holds a list of subjects (users, groups, or service accounts), and a reference to the role being granted. A RoleBinding grants permissions within a specific namespace whereas a ClusterRoleBinding grants that access cluster-wide.

- A RoleBinding may reference any Role in the same namespace. Alternatively, a RoleBinding can reference a ClusterRole and bind that ClusterRole to the namespace of the RoleBinding. If you want to bind a ClusterRole to all the namespaces in your cluster, you use a ClusterRoleBinding.

### RoleBinding examples

- Before the create the `rolebinding`, create a pod and see that it doesn't have any authority to reach kubernetes cluster.

- Create deafult-sa-pod.yaml and copy below code

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: kubectl-pod
spec:
  containers:
  - name: kubectl-container
    image: ersinsari/kubectl
    command: ["bash", "-c", "while true; do sleep 3600; done"]
```

```bash
kubectl apply -f default-sa-pod.yaml
kubectl exec -it kubectl-pod -- sh
kubectl get po
# Error from server (Forbidden): pods is forbidden: User "system:serviceaccount:default:default" cannot list resource "pods" in API group "" in the namespace "default"
exit
kubectl get po kubectl-pod -o yaml | grep -i serviceaccount # see default service account
```

- Create the rolebinding.yaml file and copy below code

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: read-pods
  namespace: default
subjects:
- kind: ServiceAccount
  name: my-serviceaccount # "name" is case sensitive
  namespace: default
roleRef:
  # "roleRef" specifies the binding to a Role / ClusterRole
  kind: Role #this must be Role or ClusterRole
  name: pod-reader # this must match the name of the Role or ClusterRole you wish to bind to
  apiGroup: rbac.authorization.k8s.io
```

```bash
kubectl apply -f rolebinding.yaml
kubectl get rolebinding
```

### ClusterRoleBinding example

- To grant permissions across a whole cluster, you can use a ClusterRoleBinding. The following ClusterRoleBinding allows any user in the group "manager" to read secrets in any namespace. Create a yaml file and name it as `clusterrolebinding.yaml`.

- Create clusterrolebinding.yaml file copy below code

```yaml
apiVersion: rbac.authorization.k8s.io/v1
# This cluster role binding allows anyone in the "manager" group to read secrets in any namespace.
kind: ClusterRoleBinding
metadata:
  name: read-secrets-global
subjects:
- kind: ServiceAccount
  name:  my-serviceaccount # Name is case sensitive
  namespace: default
roleRef:
  kind: ClusterRole
  name: secret-reader
  apiGroup: rbac.authorization.k8s.io
```

```bash
kubectl apply -f clusterrolebinding.yaml
kubectl get clusterrolebinding
```

- To check serviceaccount,role-clusterrole and rolebinding-clusterrolebinding create kubectl-pod.yaml file and copy below code

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: kubectl-pod-with-sa
spec:
  serviceAccountName: my-serviceaccount
  containers:
  - name: kubectl-container
    image: ersinsari/kubectl
    command: ["bash", "-c", "while true; do sleep 3600; done"]
```

-To try to see list pod and secrets from pod that was attached my-serviceaccount serviceaccount 

```bash
kubectl apply -f kubectl-pod.yaml
kubectl get po
kubectl exec -it kubectl-pod-with-sa -- sh
kubectl get pod
kubectl get pod -A
kubectl get secret
kubectl get secret -A
```

We were able to view only the resources in the default namespace because we granted pod permissions to the service account using a Role and RoleBinding. However, we could view all secrets across the entire cluster because we granted secret permissions using a ClusterRole and ClusterRoleBinding.


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
kubectl config set-context elalem --cluster=minikube --namespace=test --user=user
```

### Test new user
- Change context 

```bash
kubectl config use-context elalem
kubectl get po
```
You can not access any resources in kubernetes cluster yes you There is a user but he/she doesn't have any permission


### Create Role and RoleBinding

```bash
kubectl config use-context minikube
kubectl get po
```

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
kubectl create ns test
kubectl apply -f role.yaml
kubectl apply -f rolebindings.yaml
```

- Test again 

```bash
kubectl config use-context elalem
kubectl get po # see all pod
kubectl get secret # can not secret
```