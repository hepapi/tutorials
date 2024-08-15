
## Network Policy
- If you want to control traffic flow at the IP address or port level (OSI layer 3 or 4), then you might consider using Kubernetes NetworkPolicies for particular applications in your cluster.

- The entities that a Pod can communicate with are identified through a combination of the following 3 identifiers:

  1. Other pods that are allowed (exception: a pod cannot block access to itself)
  2. Namespaces that are allowed
  3. IP blocks (exception: traffic to and from the node where a Pod is running is always allowed, regardless of the IP address of the Pod or the node)

- When defining a pod or namespace based NetworkPolicy, you use a selector to specify what traffic is allowed to and from the Pod(s) that match the selector.

- Meanwhile, when IP based NetworkPolicies are created, we define policies based on IP blocks (CIDR ranges).

- Create minikube cluster

```bash
minikube start --network-plugin=cni --cni=calico
```

- Create 3 namespace.

```bash
kubectl create ns busybox-ns
kubectl create ns nginx-ns
kubectl create ns alpine-ns
```

- Create a `busybox.yaml` for busybox deployment under `busybox-ns` namespace.

```bash
apiVersion: apps/v1
kind: Deployment
metadata:
  name: busybox-deployment
  namespace: busybox-ns
spec:
  replicas: 1
  selector:
    matchLabels:
      app: busybox
  template:
    metadata:
      labels:
        app: busybox
    spec:
      containers:
      - name: busybox
        image: busybox
        command: 
        - /bin/sh
        - -c
        - sleep 3600
        ports:
        - containerPort: 80
---
apiVersion: v1
kind: Service   
metadata:
  name: busybox-svc
  namespace: busybox-ns
spec:
  type: ClusterIP  
  ports:
  - port: 80 
    targetPort: 80
  selector:
    app: busybox
```

```bash
kubectl apply -f busybox.yaml
```

- Create a `nginx.yaml` for nginx deployment under `nginx-ns` namespace.

```bash
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-deployment
  namespace: nginx-ns
spec:
  replicas: 1
  selector:
    matchLabels:
      app: nginx
  template:
    metadata:
      labels:
        app: nginx
    spec:
      containers:
      - name: nginx
        image: nginx
        ports:
        - containerPort: 80
---
apiVersion: v1
kind: Service   
metadata:
  name: nginx-svc
  namespace: nginx-ns
spec:
  type: ClusterIP  
  ports:
  - port: 80 
    targetPort: 80
  selector:
    app: nginx
```


```bash
kubectl apply -f nginx.yaml
```

- Create a `alpine.yaml` for alpine deployment under `alpine-ns` namespace.

```bash
apiVersion: apps/v1
kind: Deployment
metadata: 
  name: alpine-deployment
  namespace: alpine-ns 
spec:
  replicas: 1
  selector:
    matchLabels:
      db: alpine
  template:
    metadata:
      labels:
        db: alpine
    spec:
      containers:
      - name: alpine
        image: alpine
        command:
        - /bin/sh
        - "-c"
        - "sleep 60m"
        ports:
        - containerPort: 80
---
apiVersion: v1
kind: Service   
metadata:
  name: alpine-svc
  namespace: alpine-ns
spec:
  type: ClusterIP  
  ports:
  - port: 80 
    targetPort: 80
  selector:
    db: alpine
```


```bash
kubectl apply -f alpine.yaml
```

- Try to connect the pods for testing.

```bash
kubectl get po -A -o wide
k exec -it -n busybox-ns busybox-deployment-57c9cfc7d7-h9k9j -- sh
ping nginx-pod and from alpine and busybox
```

### Create NetworkPolicy

```bash
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: network-policy
  namespace: nginx-ns
spec:
  podSelector:
    matchLabels:
      app: nginx
  policyTypes:
    - Ingress
    - Egress
```

This is the Pod the Network Policy’s Ingress and Egress rules will apply to. Because the Ingress and Egress policy types are set but no further rules are added, the policy will block all network traffic to and from the Pod.

```bash
kubectl apply -f neywork-policy.yaml
```

### Adding an Allow Rule
Update your networkpolicy below code

```bash
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: network-policy
  namespace: nginx-ns
spec:
  podSelector:
    matchLabels:
      app: nginx
  policyTypes:
    - Ingress
    - Egress
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: busybox-ns
  egress:
    - to:
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: busybox-ns
```

```bash
kubectl apply -f network-policy.yaml
```

- Try to connect the pods for testing.

```bash
kubectl get po -A -o wide
k exec -it -n busybox-ns busybox-deployment-57c9cfc7d7-h9k9j -- sh
```
- ping nginx-pod and from alpine and busybox.Your busybox pod can access to nginx pod but alpine-pod can not access nginx because we give permisssion only busybox pod ingress and egress.

### Understanding Kubernetes Network Policy Ingress/Egress selectors

Network Policy Ingress and Egress rules can use a few different selector types to identify the Pods that are allowed to communicate with the policy’s target

#### podSelector

```bash
podSelector:
  matchLabels:
    app: demo
```
#### namespaceSelector

namespaceSelector is similar to podSelector but it selects an entire namespace using labels. All the Pods in the namespace will be included.

```bash
namespaceSelector:
  matchLabels:
    app: demo
```
#### ipBlock

ipBlock selectors are used to allow traffic to or from specific IP address CIDR ranges. This is intended to be used to filter traffic from IP addresses that are outside the cluster. It’s not suitable for controlling Pod-to-Pod traffic because Pod IP addresses are ephemeral—they will change when a Pod is replaced.

```bash
ipBlock:
  cidr: 10.0.0.0/24
```
#### Combining selectors

You can use multiple selectors to create complex conditions in your policies. The following policy selects all the Pods that are either labeled demo-api or belong to a namespace labeled app: demo:

```bash
ingress:
  - from:
      - namespaceSelector:
          matchLabels:
            app: demo
      - podSelector:
          matchLabels:
            app: demo-api
```
```bash
ingress:
  - from:
      - namespaceSelector:
          matchLabels:
            app: demo
        podSelector:
          matchLabels:
            app: demo-api
```

This policy only targets Pods that are both labeled app: demo-api and in a namespace labeled app: demo.