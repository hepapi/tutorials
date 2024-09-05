# Kubernetes Resources and Limits


Here’s an example of a Kubernetes pod specification that defines resource requests and limits:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hpa-deploy
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
        resources:
          requests:
            cpu: "25m"
            memory: "25Mi"
          limits:
            cpu: "50m"
            memory: "50Mi"
---
apiVersion: v1
kind: Service
metadata:
  name: nginx-svc
  labels:
    app: nginx
spec:
  ports:
  - port: 80
  selector:
    app: nginx 
```

```bash
kubectl apply -f deployment.yml
```

Requests:
- memory: "25Mi": The container is guaranteed 25 MiB of memory.
- cpu: "25m": The container is guaranteed 200 millicores (0.2 cores) of CPU.

Limits:
- memory: "128Mi": The container can use up to 128 MiB of memory. If it exceeds this limit, the container may be terminated.
- cpu: "200m": The container can use up to 500 millicores (0.5 cores) of CPU. If it tries to use more, it will be throttled.

## Why Use Resource Requests and Limits?
Efficient Resource Allocation: Ensures that resources are allocated efficiently across the cluster.
Avoid Overcommitment: Prevents containers from overusing resources, which could cause other containers to be starved of CPU or memory.
Stability: Helps maintain the stability and performance of applications by preventing any single container from degrading the performance of others.

## HPA

### Install Metric Server

```bash
  mkdir -p Kubernetes/examples/kubernetes-security/hpa
  cd Kubernetes/examples/kubernetes-security/hpa
```

- Install Metric Server

```bash
minikube addons enable metrics-server
```

- Check metric server pod

```bash
kubectl get po -n kube-system
```

Use kubectl to see CPU and memory metrics:

```bash
kubectl top pods -n kube-system
```

### Install the Horizontal Pod Autoscaler

We now have the sample application as part of our deployment, and the service is accessible on port 80. To scale our resources, we will use HPA to scale up when traffic increases and scale down the resources when traffic decreases.

- create hpa.yaml file and copy below code

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: nginx-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: hpa-deploy
  minReplicas: 1
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 30 
```

```bash
kubectl apply -f hpa.yaml
kubectl get hpa
```

### Testing
To test HPA in real-time, let’s increase the load on the cluster and check how HPA responds in managing the resources.

This command will continuously generate requests to the nginx-svc service, thereby increasing the CPU utilization of the nginx pods.

```bash
kubectl run -i --tty load-generator --rm --image=busybox:1.28 --restart=Never -- /bin/sh -c "while sleep 0.01; do wget -q -O- http://nginx-svc; done"
```

Once you triggered the load test, use the below command, which will show the status of the HPA every 30 seconds

```bash
watch kubectl get hpa
```

By issuing the command that increases CPU usage, the HPA (Horizontal Pod Autoscaler) will scale the number of pods based on the incoming load. As the load decreases, the HPA will reduce the number of pods back down to the minimum replica count.