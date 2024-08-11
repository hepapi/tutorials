# Kubernetes Introduction Hands-On

## Installing Minikube

### Pre-requisites

1. **Install Docker Desktop**
   - Follow the instructions for your OS here: [Minikube Documentation](https://minikube.sigs.k8s.io/docs/start)
   - For Mac:
     ```bash
     brew install minikube
     ```

2. **Install kubectl**
   - Follow the instructions here: [Install kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl-macos/)

## Slide Content and Demonstrations

### Slide 15: Show a Kubernetes Cluster
- **As UI**: Use tools like Lens or Rancher to visually manage and monitor your cluster.
- **As CLI**: Use `kubectl` to interact with your cluster from the terminal.
  - To access the cluster from the terminal:
    ```bash
    kubectl get nodes
    ```

### Slide 16: Show Core Components in the Cluster
- Use the following command to list all pods in the `kube-system` namespace:
  ```bash
  kubectl get pods -n kube-system

## Slide 17: Kube-API Server
- **Show the kube-apiserver pod and logs**:
  ```bash
  kubectl -n kube-system get pods | grep kube-apiserver
  kubectl -n kube-system logs <kube-apiserver-pod-name>
- show static manifests
  - Typically located at /etc/kubernetes/manifests on the control plane node:
- increase log level - --v=3

## Slide 18: ETCD 
- store all cluster data
    ```bash
    ETCDCTL_API=3 etcdctl get / --prefix --keys-only
    ```

- backup of cluster
     ```bash
    ETCDCTL_API=3 etcdctl snapshot save snapshot.db
    ETCDCTL_API=3 etcdctl snapshot status snapshot.db
    ```
- Raft algorithm: Discuss the consensus algorithm used by etcd.

## Slide 19: kube-controller manager
- node controller
    - monitors node every 5 seconds
    - node monitor grace period 40 second
    - pod eviction timeout 5m
- replication controller
    - change replica count any deployment and see logs of controller-manager

## Slide 20: kube-schedular
- right container to right node
- select node options
    - nodeName
    - nodeSelector (key/value)
    - affinity s
    ```bash
        apiVersion: v1
        kind: Pod
        metadata:
        name: mypod
        spec:
        containers:
        - name: mycontainer
            image: myimage
        nodeSelector:
            disktype: ssd
    ```
    
## Slide 21: kubelet
- Manages each node in the cluster, ensuring that containers are running in a pod.

## Slide 22: kube-proxy
- Maintains network rules on nodes, allowing network communication to your pods.

## Slide 23: container runtime
- Software that runs containers, such as Docker, containerd, or CRI-O.
```bash
kubectl get nodes -o wide
```

## Setting Up Local Environment

1. Start Minikube:
    ```bash
        minikube --help # see all available parameter for minikube
        minikube start --nodes 3 --driver=docker # this will create a local Kubernetes cluster with 3 nodes using Docker

    ```

2. Check Minikube status:
    ```bash
    minikube status
    ```

3. Verify the nodes:
    ```bash
    kubectl get nodes 
    docker ps # # to see Minikube containers running on Docker. Special case for minikube
    ```

## Some kubectl commands

1. List the Kubernetes contexts:
    ```bash
    kubectl config get-contexts # shows all available contexts
    ```

2. Switch to the Minikube context:
    ```bash
    kubectl config use-context minikube
    kubectl get nodes
    ```
## kubeconfig File

- **Kubeconfig File**: Discuss the kubeconfig file's path and content
  - Default location: `~/.kube/config`
  - It contains configuration information for kubectl, including clusters, users, and contexts.


## Make a demo managed kubernetes cluster with EKS (optional)