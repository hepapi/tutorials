## ServiceAccount
- A `service account` provides an identity for `processes` that run in a Pod, and maps to a `ServiceAccount` object. 
- Kubernetes by default creates a service account in each namespace of a cluster and call it a default service account. These default service accounts are mounted to every pod launched.

- Create serviceaccount.yaml and copy below code

```bash
apiVersion: v1
kind: ServiceAccount
metadata:
  annotations:
    kubernetes.io/enforce-mountable-secrets: "true"
  name: my-serviceaccount
```

```bash
kubectl get serviceaccount
kubectl get sa -A
kubectl apply -f serviceaccount.yaml
```
## RBAC
- Role-based access control (RBAC) is a method of regulating access to computer or network resources based on the roles of individual users within your organization.

- RBAC authorization uses the rbac.authorization.k8s.io API group to drive authorization decisions, allowing you to dynamically configure policies through the Kubernetes API.

### API objects

- The RBAC API declares four kinds of Kubernetes object: Role, ClusterRole, RoleBinding and ClusterRoleBinding

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

```bash
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

- Create clusterrole.yaml and copy below code.

```bash
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  # "namespace" omitted since ClusterRoles are not namespaced
  name: secret-reader
rules:
- apiGroups: [""]
  #
  # at the HTTP level, the name of the resource for accessing Secret
  # objects is "secrets"
  resources: ["secrets"]
  verbs: ["get", "watch", "list"]
```

```bash
kubectl apply -f clusterrole.yaml
kubectl get clusterrole
```

### RoleBinding and ClusterRoleBinding

- A role binding grants the permissions defined in a role to a user or set of users. It holds a list of subjects (users, groups, or service accounts), and a reference to the role being granted. A RoleBinding grants permissions within a specific namespace whereas a ClusterRoleBinding grants that access cluster-wide.

- A RoleBinding may reference any Role in the same namespace. Alternatively, a RoleBinding can reference a ClusterRole and bind that ClusterRole to the namespace of the RoleBinding. If you want to bind a ClusterRole to all the namespaces in your cluster, you use a ClusterRoleBinding.

### RoleBinding examples

- Before the create the `rolebinding`, create a pod and see that it doesn't have any authority to reach kubernetes cluster.

- Create non-sa-pod.yaml and copy below code

```bash
apiVersion: v1
kind: Pod
metadata:
  name: kubectl-pod-non-sa
spec:
  containers:
  - name: kubectl-container
    image: ersinsari/kubectl
    command: ["bash", "-c", "while true; do sleep 3600; done"]
```

```bash
kubectl apply -f non-sa-pod.yaml
kubectl exec -it kubectl-pod -- sh
/ # kubectl get po
Error from server (Forbidden): pods is forbidden: User "system:serviceaccount:default:default" cannot list resource "pods" in API group "" in the namespace "default"
/ # exit
```

- Create the rolebinding.

```bash
kubectl apply -f rolebinding.yaml
kubectl get rolebinding
```

- go to /var/run/secrets/kubernetes.io/serviceaccount path in pod and show token

### ClusterRoleBinding example

- To grant permissions across a whole cluster, you can use a ClusterRoleBinding. The following ClusterRoleBinding allows any user in the group "manager" to read secrets in any namespace. Create a yaml file and name it as `clusterrolebinding.yaml`.

- Create clusterrolebinding.yaml copy below code

```bash
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
kubectl apply -f clusterolebinding.yaml
kubectl get clusterrolebinding
```

```bash
kubectl exec -it kubectl-pod -- sh
kubectl get secret
kubectl get secret -A
```