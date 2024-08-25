# Kubernetes Objects Hands-On

## Object Model
- Examine the parts a Kubernetes object consists of:
```bash
  kubectl api-resources # This command shows the API version of objects, their abbreviations, and whether they are namespace-scoped.
```

## PODs
### Basic Pods
1. Create a YAML file for a simple Nginx Pod using the IMPERATIVE approach:

```bash
    kubectl run nginx-imperative-pod --image nginx 
```
2. Create a YAML file for a simple Nginx Pod using the DECLERATIVE approach:

```bash
    mkdir -p Kubernetes/examples/kubernetes-core-objects/pod
    cd Kubernetes/examples/kubernetes-core-objects/pod
```
- create basic-pod.yaml file.
```basic-pod.yaml
apiVersion: v1
kind: Pod
metadata:
  name: nginx-declerative-pod
spec:
  containers:
  - name: main-app
    image: nginx
```
- run basic-pod
```bash
    kubectl apply -f basic-pod.yaml
```

- Delete pods

```bash
    kubectl delete pods nginx-imperative-pod 
```

```bash
    kubectl delete -f basic-pod.yaml
```

### Pods with Init Container
- InitContainer Usage Scenarios:
    - Change configuration
    - Modify folder permissions
    - Perform database migrations
- Create a YAML file named pod-with-init-container.yaml. Ignore the volume part, we will talk about it later.

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
    kubectl get pod # see Init:0/1 for the init container status
```
- see changes of index.html from browser
```bash
    kubectl port-forward pods/init-container-pod 8888:80 
```

- Note: The Init Container terminates once its task is completed.

### Pods with Sidecar Container
- Sidecar Usage Scenarios:
    - logging
    - Data Synchronization
    - Configuration Management
- Create yaml file named pod-with-sidecar-container.yaml. Ignore the volume part, we will talk about it later.

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
    kubectl get pod # see both containers running (2/2)
    kubectl exec -it sidecar-container-pod -c logger-sidecar -- sh 
    # run below command inside the container
    cd /fluentd/log
    ls
    cat access.log
```
- Review the prometheus-rancher-monitoring-prometheus-0 pod in the cluster for a practical example.

## ReplicaSet
- ReplicaSet: Manages the number of pod replicas. The Deployment object automatically manages a ReplicaSet.

## Deploymnet
1. Create a basic Nginx Deployment with imprative command:
```bash
  kubectl create deployment imperative-deployment --image nginx:alpine --replicas 3
  kubectl get deploy # see replicas 3/3 
  kubectl get po # see random characters in pod names
  kubectl delete pod imperative-deployment-xxxx-xxxx
  kubectl get deploy # see number of pods
  kubectl get pods # see the new pod created
  kubectl scale deployment imperative-deployment --replicas 10 # change replica count
  kubectl get po # see 10 pods
  kubectl delete deployment imperative-deployment 
```

2. Create a basic Nginx Deployment YAML file:

- Create a folder named deployment and a file named basic-deployment.yaml

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
- Selectors: Match pods based on labels.
- See the ReplicaSet for the nginx-deployment
```bash
  kubectl get all
  kubectl get replicaset # or rs
```
- see first random character group in replicaset
- Change the image from nginx:1.21.6 to nginx:latest in basic-deployment.yaml
```bash
  kubectl apply -f basic-deployment.yaml  # reapply the updated file
  kubectl get replicaset #see 2 ReplicaSets, with the old one scaled down to 0 and the new one scaled up
```
- See rollout status:
```bash
  kubectl rollout status deployment nginx-deployment
```
- Rollback to an older version:
```bash
  kubectl rollout history deployment/nginx-deployment # see history
kubectl rollout undo deployment/nginx-deployment --to-revision=1 # rollback to revision 1
kubectl describe deployments nginx-deployment # review the deployment details
kubectl rollout history deployment/nginx-deployment # see the updated version
```
- Talk about creating pod or deployment even if 1 replica. Why use a Deployment instead of a Pod? 

- Create a file named complex-deployment.yaml
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
  # change the image to sametustaoglu/simple-node-app:v1
  kubectl apply -f complex-deployment.yaml # apply the updated file
  watch kubectl get po # see max 1 pod unavailable due to rolling updates with probes
```
- For more details, visit the Kubernetes Deployment Documentation. https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#strategy

- Change the strategy type from RollingUpdate to Recreate and delete lines related to rollingUpdate
```bash
  kubectl apply -f complex-deployment.yaml
  # change the image to sametustaoglu/simple-node-app:small-size
  kubectl apply -f complex-deployment.yaml # reapply the updated file
  watch kubectl get po # see new pods created after old ones are terminated
```

## Daemonset

- Why use DaemonSet?
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
- Rollout DaemonSet if needed, similar to Deployment:
  - Change the image to v1.12
  ```bash
    kubectl rollout status daemonset/fluentd-daemonset -n logging
    kubectl rollout history daemonset/fluentd-daemonset -n logging
    kubectl rollout undo  daemonset/fluentd-daemonset -n logging --to-revision=1
  ```
- Adding Toleration to DaemonSet:

```bash
  kubectl delete daemonset fluentd-daemonset -n logging
  kubectl get nodes
  kubectl taint nodes minikube node-role.kubernetes.io/master:NoSchedule # we will discuss taints and tolerations later
  kubectl apply -f daemonset.yaml
  kubectl get pods -n logging -o wide # see that the Minikube node doesn't have a Fluentd pod
```

- Update daemonset.yaml to include tolerations:

```yaml
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
  kubectl get pods -n logging  -o wide # see pods on all nodes
```

## Jobs
- Kubernetes Jobs: Used to run tasks that need to be completed once or a specific number of times. They are essential for:
  - Batch Processing
  - One-Time Tasks
  - Retry Mechanism

- If we try to do what a Job does with a Pod:
```bash
  cd ..
  mkdir jobs && cd jobs
  touch pod.yaml
```
- pod.yaml:
```yaml
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
  watch kubectl get po # pod continues running indefinitely
  kubeclt logs -f pi-pod
```
### Creating Simple job

```bash
  touch simple-job.yaml
```
- simple-job.yaml
```yaml
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
  watch kubectl get po # job continues running until completion
  kubeclt logs -f simple-job-<xxxx>
  kubectl get jobs
  kubectl describe job simple-job
```
- **Job Features:**
  - **backoffLimit:** Specifies the number of times the operation should be retried after failure.
  - **completions:** Specifies the number of successful completions required before the Job is considered complete.
    - delete job, add completions field, re-run job and monitor with `watch kubectl get po`
  - **parallelism:** Specifies the maximum number of pods that can run in parallel.
    - delete job, add parallelism field, re-run job and monitor with `watch kubectl get po`
  - **activeDeadlineSeconds:** Specifies the maximum time the Job can run, regardless of completions. If the Job exceeds this time, Kubernetes will terminate the Job.

## Cronjob
-Kubernetes CronJob: Used for running jobs on a schedule, similar to cron jobs in Unix/Linux systems. They are essential for:
  - Regular Backups
  - Cleanup Tasks
  - Data Processing

### Creating Cronjob

```bash
  touch cronjob.yaml
```
- cronjob.yaml
```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: history-cronjob
spec:
  schedule: "*/5 * * * *" # Runs every 5 minutes
  concurrencyPolicy: Forbid
  startingDeadlineSeconds: 100
  successfulJobsHistoryLimit: 3
  failedJobsHistoryLimit: 1
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: example
            image: busybox
            command: ["echo", "Keeping track of job history"]
          restartPolicy: OnFailure
```

```bash
  kubectl apply -f cronjob.yaml
  watch kubectl get po # see jobs running every 5 minutes
  kubeclt logs -f history-cronjob-<xxxx>
  kubectl get cronjobs
  kubectl describe cronjobs cron-job
```

- **CronJob Features:**
  - **schedule:** Defines the cron expression.
  - **concurrencyPolicy:** Controls how concurrent executions of a Job are handled.
    -  **Allow:** Allows CronJobs to run concurrently.
    -  **Forbid:** Prevents new Jobs from starting if a previous Job is still running.
    -  **Replace:** Stops the currently running Job and replaces it with a new one.
  - **startingDeadlineSeconds:** Specifies the deadline in seconds for starting the Job if it misses its scheduled time. After this time, the Job will not start.
  - **successfulJobsHistoryLimit:** Defines the number of successful Job executions to retain.
  - **failedJobsHistoryLimit:** Defines the number of failed Job executions to retain.

## Namespace

- Why use namespaces?
  - Isolation of resources
  - Manage permissions
  - Specify quotas

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
- Create a pod in basic-namespace
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
- Try to see the new pod in basic-namespace
```bash
kubectl get pods # you cannot see the new pod in the default namespace
kubectl get pods --namespace basic-namespace # now you can see the new pod
```
- Change the default namespace (changing context):
```bash
  kubectl config set-context --current --namespace=basic-namespace
  kubectl get pods # default namesapce is basic-namespace
  kubectl config set-context --current --namespace=default # revert to default namespace
```