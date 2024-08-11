# Kubernetes Objects Hands-On

## Object Model
- Examine what parts a kubernetes object consists of
```bash
  kubectl api-resources # This command shows the apiversion of objects, their abbreviation usage and whether they are namespace objects.
```

## PODs
### Basic Pods
1. Create a YAML file for a simple Nginx Pod with IMPERATIVE way

```bash
    kubectl run nginx-imperative-pod --image nginx 
```
2. Create a YAML file for a simple Nginx Pod with DECLERATIVE way

- basic-pod.yaml
```yaml
    apiVersion: v1
    kind: Pod
    metadata:
    name: nginx-declerative-pod
    spec:
    containers:
    - name: main-app
        image: nginx
```

```bash
    cd Kubernetes/pod
    kubectl apply -f basic-pod.yaml
```

3. Delete pods

```bash
    kubectl delete pods nginx-imperative-pod 
```

```bash
    kubectl delete -f basic-pod.yaml
```

### Pods with Init Container
- initcontainer usage scenario
    - change configuration
    - modified folder permissions
    - db migrations
- Create yaml file named pod-with-init-container.yaml

Ignore the volume part, we will talk about it later.
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: init-container-pod
spec:
  initContainers:
  - name: init-myservice
    image: busybox:1.31
    command:
    - wget
    - "-O"
    - "/work-dir/index.html"
    - "https://www.example.com"
    volumeMounts:
    - name: workdir
      mountPath: "/work-dir"
  containers:
  - name: nginx
    image: nginx:latest
    ports:
    - containerPort: 80
    volumeMounts:
    - name: workdir
      mountPath: "/usr/share/nginx/html"
  volumes:
  - name: workdir
    emptyDir: {}
```

```bash
    kubectl apply -f pod-with-init-container.yaml
    kubectl get pod # see init pod as  Init:0/1
```
- see changes of index.html from browser
```bash
    kubectl port-forward pods/init-container-pod 8888:80 
```

- Init container terminates  when job is finished.

### Pods with Sidecar Container
- Sidecar usage scenario
    - logging
    - Data Synchronization
    - Configuration Management
- Create yaml file named pod-with-sidecar-container.yaml

Ignore the volume part, we will talk about it later.
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: sidecar-container-pod
spec:
  containers:
  - name: main-app
    image: nginx:latest
    volumeMounts:
    - name: log-volume
      mountPath: /var/log/nginx
  - name: logger-sidecar
    image: fluentd
    env:
    - name: FLUENTD_ARGS
      value: "--no-supervisor -q"
    volumeMounts:
    - name: log-volume
      mountPath: /fluentd/log
  volumes:
  - name: log-volume
    emptyDir: {}
```

```bash
    kubectl apply -f pod-with-sidecar-container.yaml
    kubectl get pod # see sidecar pod as  2/2     Running
    kubectl exec -it sidecar-container-pod -c logger-sidecar -- sh 
    # run below command inside the container
    cd /fluentd/log
    ls
    cat access.log
```
- review prometheus-rancher-monitoring-prometheus-0 pod in cluster.

## ReplicaSet
- talk about replicaset. The Deployment object will include replicaset.

## Deploymnet
1. Create a basic Nginx Deployment with imprative command:
```bash
  kubectl create deployment imperative-deployment --image nginx:alpine --replicas 3
  kubectl get deploy # see replicas 3/3 
  kubectl get po # see random characters in name
  kubectl delete pod imperative-deployment-xxxx-xxxx
  kubectl get deploy # see number of pod 
  kubectl get pods # see new pod
  kubectl scale deployment imperative-deployment --replicas 10 # change replica count of deployment
  kubectl get po # see 10 pod 
  kubectl delete deployment imperative-deployment 
```

2. Create a basic Nginx Deployment YAML file:

- create a folder named deployment and create a file named basic-deployment.yaml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-deployment
  labels:
    app: nginx
spec:
  replicas: 3
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
        image: nginx:1.21.6
        ports:
        - containerPort: 80
```
- talk about selectors
- see replicaset for basic-deployment
```bash
  kubectl get all
  kubectl get replicaset # or rs
```
- see first random character group in replicaset
- change image from nginx:1.21.6 to nginx:latest in basic-deployment.yaml 
```bash
  kubectl apply -f basic-deployment.yaml  # apply same file again without delete
  kubectl get replicaset # see 2 replicaset old one's count is 0 and new replica's count is 3
```
- see rollout status
```bash
  kubectl rollout status deployment nginx-deployment
```
- rollback old versions
```bash
  kubectl rollout history deployment/nginx-deployment # see history
  kubectl rollout undo deployment/nginx-deployment --to-revision=1 # the lowest number is the oldest version
  kubectl describe deployments nginx-deployment # review  describe of deployment
  kubectl rollout history deployment/nginx-deployment # increased version
```
- talk about creating pod or deployment even if 1 replica. Why should we choose deployment instead of pod?

- create a file named complex-deployment.yaml 
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: complex-deployment
  labels:
    app: complex-app
spec:
  replicas: 3
  selector:
    matchLabels:
      app: complex-app
  template:
    metadata:
      labels:
        app: complex-app
    spec:
      containers:
        - name: app-container
          image: sametustaoglu/simple-node-app:small-size
          ports:
            - containerPort: 3000
          env:
            - name: LOG_LEVEL
              value: "warn"
          resources:
            requests:
              memory: "128Mi"
              cpu: "500m"
            limits:
              memory: "256Mi"
              cpu: "1"
          readinessProbe:
            httpGet:
              path: /
              port: 3000
            initialDelaySeconds: 5
            periodSeconds: 5
          livenessProbe:
            httpGet:
              path: /
              port: 3000
            initialDelaySeconds: 15
            periodSeconds: 20
  strategy:
   type: RollingUpdate
   rollingUpdate:
     maxUnavailable: 1
```
```bash
  kubectl apply -f complex-deployment.yaml 
  kubectl get pods # see that the pods are not ready right away
  # change image from sametustaoglu/simple-node-app:small-size to sametustaoglu/simple-node-app:v1
  kubectl apply -f complex-deployment.yaml # run this command again
  watch kubectl get po # see max 1 pod unavailable. because of probes
```
- for details go to https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#strategy

- change strategy type from RollingUpdate to Recreate and delete lines releated with rollingUpdate.
```bash
  kubectl apply -f complex-deployment.yaml 
  # change image from sametustaoglu/simple-node-app:v1 to sametustaoglu/simple-node-app:small-size
  kubectl apply -f complex-deployment.yaml # run this command again
  watch kubectl get po # see new node will create after old pods terminated
```

## Namespace

- Why we use namespaces?
  - izolation of resources
  - manage permissions
  - specify qoutas 

```bash
  cd ..
  mkdir namespace && cd namespace
  touch basic-namespace.yaml
```

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: demo-namespace
```

```bash
  kubectl get namespace # see all namespaces 
  kubectl create namespace imperative-namespace
  kubectl get namespace
  kuebctl apply -f basic-namespace.yaml
  kubectl get ns
```
- create a pod in basic-namespace
```bash
  touch pod-with-namespace.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: my-pod
  namespace: basic-namespace
spec:
  containers:
  - name: nginx
    image: nginx
```
- try to see new pod named pod-in-basic-namespace
```bash
kubectl get pods # you can not new pod in default namespaces
kubectl get pods --namespace basic-namespace # now you can see new pod
```
- change default namespace (changing context)
```bash
  kubectl config set-context --current --namespace=basic-namespace
  kubectl get pods # default namesapce is basic-namespace
  kubectl config set-context --current --namespace=default # revert to default namespace
```

## Daemonset

- Why we use Daemonset?
  - Log Collection
  - Monitoring
  - Networking

```bash
  cd ..
  mkdir daemonset && cd daemonset
  touch daemonset.yaml
```

```daemonset.yaml
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: fluentd-daemonset
  namespace: logging
spec:
  selector:
    matchLabels:
      name: fluentd
  template:
    metadata:
      labels:
        name: fluentd
    spec:
      containers:
      - name: fluentd
        image: fluent/fluentd:v1.11
        resources:
          limits:
            memory: 200Mi
            cpu: 100m
          requests:
            memory: 200Mi
            cpu: 100m
```
```bash
  kubectl create ns logging
  kubectl apply -f daemonset.yaml
  kubectl get daemonsets -n logging
  kubectl get nodes # see how many nodes there are
  kubectl get pods -n logging 
```
- you can rollout if you need like deployment
  - change image tah v1.12
  ```bash
    kubectl rollout status daemonset/fluentd-daemonset -n logging
    kubectl rollout history daemonset/fluentd-daemonset -n logging
    kubectl rollout undo  daemonset/fluentd-daemonset -n logging --to-revision=1
  ```
- adding toleration to Daemonset

```bash
  kubectl delete daemonset fluentd-daemonset -n logging
  kubectl get node
  kubectl taint nodes minikube node-role.kubernetes.io/master:NoSchedule # we will discuss later taints and tolaration
  kubectl apply -f daemonset.yaml
  kubectl get pods -n logging  -o wide # see minikube node doesnt have fluentd pod 
```

- change daemonset.yaml as below

```daemonset.yaml
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: fluentd-daemonset
  namespace: kube-system
spec:
  selector:
    matchLabels:
      name: fluentd
  template:
    metadata:
      labels:
        name: fluentd
    spec:
      tolerations:
      - key: node-role.kubernetes.io/master
        operator: Exists
        effect: NoSchedule
      containers:
      - name: fluentd
        image: fluent/fluentd:v1.12
        resources:
          limits:
            memory: 200Mi
            cpu: 100m
          requests:
            memory: 200Mi
            cpu: 100m
```

```bash
  kubectl apply -f daemonset.yaml # apply again with toleration
  kubectl get pods -n logging  -o wide # see 3 pod. all nodes have pod
```

## Jobs
- Kubernetes Jobs are used to run tasks that need to be completed once or a specific number of times. They are essential for:
  - Batch Processing
  - One-Time Tasks
  - Retry Mechanism

- If we tried to do what job does with pod
```bash
  cd ..
  mkdir jobs && cd jobs
  touch pod.yaml

```
```pod.yaml
apiVersion: v1
kind: Pod
metadata:
  name: pi-pod
spec:
  containers:
    - name: pi
      image: perl
      command: ["perl",  "-Mbignum=bpi", "-wle", "print bpi(2000)"]
```

```bash
  kubectl apply -f pod.yaml
  watch kubectl get po # will want to continue forever
  kubeclt logs -f pi-pod
```
### Createing Simple job

```bash
  touch simple-job.yaml
```

```simple-job.yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: simple-job
  namespace: default
spec:
  template:
    spec:
      containers:
      - name: pi
        image: perl
        command: ["perl",  "-Mbignum=bpi", "-wle", "print bpi(2000)"]
      restartPolicy: Never
  backoffLimit: 4
```

```bash
  kubectl apply -f simple-job.yaml
  watch kubectl get po # will want to continue forever
  kubeclt logs -f simple-job-<xxxx>
  kubectl get jobs
  kubectl describe job simple-job
```

- backoffLimit: Specifies the number of times the operation should be retried after failure.












minikube addons enable ingress

127.0.0.1 example.com 

logs3