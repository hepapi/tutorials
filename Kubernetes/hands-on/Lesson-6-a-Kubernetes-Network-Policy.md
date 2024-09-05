
## Network Policy
- If you want to control traffic flow at the IP address or port level (OSI layer 3 or 4), then you might consider using Kubernetes NetworkPolicies for particular applications in your cluster.

- Create minikube cluster # flannel CNI does not support network policy

```bash
minikube delete
minikube start --network-plugin=cni --cni=calico
```

- Create 3 namespace.

```bash
kubectl create ns busybox-ns
kubectl create ns nginx-ns
kubectl create ns alpine-ns
```

```bash
  mkdir -p Kubernetes/examples/netpol
  cd Kubernetes/examples/netpol
  touch busybox.yaml
```

- Create a `busybox.yaml` file for busybox deployment under `busybox-ns` namespace.

```yaml
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

- Check pod and service in busybox-ns

```bash
kubectl get po,svc -n busybox-ns
```

- Create a `nginx.yaml` for nginx deployment under `nginx-ns` namespace.

```yaml
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

```bash
kubectl get po,svc -n nginx-ns
```

- Create a `alpine.yaml` file for alpine deployment under `alpine-ns` namespace.

```yaml
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

- Check pod and service in alpine-ns

```bash
kubectl get po,svc -n alpine-ns
```

- Try to connect the pods for testing.

```bash
kubectl get pod -A -o wide
kubectl exec -it -n busybox-ns <busybox-deployment-pod> -- sh
#ping nginx-pod and alpine-pod from busybox-pod
ping <nginx-pod-ip> # see what you can reach
ping <alpine-pod>
```

By default, since all pods within a Kubernetes cluster can communicate with each other, you should be able to see a successful ping result.

### Create NetworkPolicy

- Let's create file as name network-policy.yaml and copy below code

```yaml
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
kubectl apply -f network-policy.yaml
```

```bash
kubectl get pod -A -o wide
kubectl exec -it -n busybox-ns <busybox-deployment-pod> -- sh
#ping nginx-pod and alpine-pod from busybox-pod
ping <nginx-pod-ip> # see what you can not reach
ping <alpine-pod> # see what you can reach
```

The ping to the Alpine pod should be successful, while the ping to the Nginx pod should fail because we have completely blocked both ingress and egress traffic to the Nginx pod using a network policy.

### Adding an Allow Rule

- to get busybox-ns namespace label run below command

```bash
kubectl get ns busybox-ns -o yaml
kubectl get ns busybox-ns -o jsonpath='{.metadata.labels}' 

```

- Update your networkpolicy below code

```yaml
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
kubectl apply -f network-policy.yaml # see configured message
```

- Try to connect the pods for testing.

```bash
kubectl get po -A -o wide
kubectl exec -it -n busybox-ns busybox-deployment-<XXXXXXX> -- sh
ping <nginx-pod-ip> # see what you can not reach
kubectl exec -it -n alpine-ns alpine-deployment-<XXXXXXX> -- sh
ping <busybox-pod-ip> # see what you can reach
ping <nginx-pod-ip> # see what you can not reach
```
- ping nginx-pod and from alpine and busybox.Your busybox pod can access to nginx pod but alpine-pod can not access nginx because we give permisssion only busybox pod ingress and egress.

### Understanding Kubernetes Network Policy Ingress/Egress selectors

Network Policy Ingress and Egress rules can use a few different selector types to identify the Pods that are allowed to communicate with the policy’s target

#### podSelector

```yaml
podSelector:
  matchLabels:
    app: demo
```
#### namespaceSelector

namespaceSelector is similar to podSelector but it selects an entire namespace using labels. All the Pods in the namespace will be included.

```yaml
namespaceSelector:
  matchLabels:
    app: demo
```
#### ipBlock

ipBlock selectors are used to allow traffic to or from specific IP address CIDR ranges. This is intended to be used to filter traffic from IP addresses that are outside the cluster. It’s not suitable for controlling Pod-to-Pod traffic because Pod IP addresses are ephemeral—they will change when a Pod is replaced.

```yaml
ipBlock:
  cidr: 10.0.0.0/24
  except:
    - 192.168.1.0/24
```
#### Combining selectors

selects the Pods 'or' expression

```yaml
ingress:
  - from:
      - namespaceSelector:
          matchLabels:
            app: demo
      - podSelector:
          matchLabels:
            app: demo-api
```
```yaml
ingress:
  - from:
      - namespaceSelector:
          matchLabels:
            app: demo
        podSelector:
          matchLabels:
            app: demo-api
```
Select to Pods 'and' expression

### affects all pods in a namespace

To affect all pods in a namespace, not just one pod

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: network-policy
  namespace: nginx-ns
spec:
  podSelector: {}
```
 
- To deny spesific namespace/pod/IP
 
 ```yaml
  ingress:
  - from:
    - namespaceSelector:
        matchExpressions:
        - key: kubernetes.io/metadata.name
          operator: NotIn
          values: ["demo"]
```
