# Kubernetes Resources and Limits

In Kubernetes, resources and limits are mechanisms used to manage the amount of CPU and memory that containers can consume in a cluster. These settings help ensure that no single container monopolizes the available resources, which could negatively impact other containers running on the same node.

## Resource Requests

- Resource Requests specify the amount of CPU and memory a container is guaranteed to have. The scheduler uses these requests to decide which node to place the pod on.
- If a node has enough resources to meet the request, the pod will be scheduled on that node. Otherwise, it will not be scheduled until resources become available.

## Resource Limits

- Resource Limits define the maximum amount of CPU and memory that a container can use. If a container tries to use more than its limit, it may be throttled (for CPU) or killed (for memory) by Kubernetes.

Here’s an example of a Kubernetes pod specification that defines resource requests and limits:
```bash
apiVersion: v1
kind: Pod
metadata:
  name: my-pod
spec:
  containers:
  - name: my-container
    image: nginx
    resources:
      requests:
        memory: "64Mi"
        cpu: "250m"
      limits:
        memory: "128Mi"
        cpu: "500m"
```

Requests:
- memory: "64Mi": The container is guaranteed 64 MiB of memory.
- cpu: "250m": The container is guaranteed 250 millicores (0.25 cores) of CPU.

Limits:
- memory: "128Mi": The container can use up to 128 MiB of memory. If it exceeds this limit, the container may be terminated.
- cpu: "500m": The container can use up to 500 millicores (0.5 cores) of CPU. If it tries to use more, it will be throttled.

## Why Use Resource Requests and Limits?
Efficient Resource Allocation: Ensures that resources are allocated efficiently across the cluster.
Avoid Overcommitment: Prevents containers from overusing resources, which could cause other containers to be starved of CPU or memory.
Stability: Helps maintain the stability and performance of applications by preventing any single container from degrading the performance of others.