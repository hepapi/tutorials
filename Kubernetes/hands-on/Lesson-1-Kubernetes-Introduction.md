# Kubernetes Introduction Hands-On

## Installing Minikube

### Pre-requisites

1. **Install Docker Desktop**
   - Follow the instructions for your OS here: [Minikube Documentation](https://minikube.sigs.k8s.io/docs/start)
   - For Mac:
     ```bash
     brew install minikube
     ```
   - For Windows (PowerShell as Administrator):
     ```powershell
     New-Item -Path 'c:\' -Name 'minikube' -ItemType Directory -Force
     $ProgressPreference = 'SilentlyContinue'
     Invoke-WebRequest -OutFile 'c:\minikube\minikube.exe' `
       -Uri 'https://github.com/kubernetes/minikube/releases/latest/download/minikube-windows-amd64.exe' `
       -UseBasicParsing
     ```
     Then add `c:\minikube` to your PATH (run as Administrator):
     ```powershell
     $oldPath = [Environment]::GetEnvironmentVariable('Path', [EnvironmentVariableTarget]::Machine)
     if ($oldPath.Split(';') -inotcontains 'C:\minikube') {
       [Environment]::SetEnvironmentVariable('Path', $('{0};C:\minikube' -f $oldPath), [EnvironmentVariableTarget]::Machine)
     }
     ```
     > **Note:** After installation, close and reopen PowerShell before running `minikube`.

2. **Install kubectl**
   - For Mac (Homebrew):
     ```bash
     brew install kubectl
     ```
   - For Windows:
     - **Option 1 — Direct download:** Visit the [Kubernetes release page](https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/) and download the binary for your architecture (amd64, arm64, etc.).
     - **Option 2 — curl:**
       ```powershell
       curl.exe -LO "https://dl.k8s.io/release/v1.35.0/bin/windows/amd64/kubectl.exe"
       ```
     - **Validate the binary (optional):**
       ```powershell
       # Download checksum file
       curl.exe -LO "https://dl.k8s.io/v1.35.0/bin/windows/amd64/kubectl.exe.sha256"

       # Verify (PowerShell) — returns True if valid
       $(Get-FileHash -Algorithm SHA256 .\kubectl.exe).Hash -eq $(Get-Content .\kubectl.exe.sha256)
       ```
     - Add `kubectl.exe` folder to your `PATH`, then verify:
       ```powershell
       kubectl version --client --output=yaml
       ```

## Slide Content and Demonstrations

### How to Identify the Cluster Installation Type (kubeadm vs RKE2)

Before running any commands, connect to the **master / control plane node** first. The following checks should be run on the master node because control plane services, static pod manifests, and admin kubeconfig files are located there.

#### 1. SSH into the master node
```bash
ssh <user>@<master-node-ip>

# Example:
ssh ubuntu@192.168.1.10
```

After connecting to the master node, identify which distribution is installed on the cluster:

#### 2. Check the running service
```bash
# kubeadm — kubelet runs as a standalone service:
systemctl status kubelet

# RKE2 — kubelet is embedded inside rke2-server:
systemctl status rke2-server
```

#### 3. Check the static pod directory
```bash
# kubeadm:
ls /etc/kubernetes/manifests/

# RKE2:
ls /var/lib/rancher/rke2/agent/pod-manifests/
```
Whichever directory contains YAML files is the one in use.

#### 4. Check the kubeconfig file location
```bash
# kubeadm:
cat ~/.kube/config          # or /etc/kubernetes/admin.conf

# RKE2:
cat /etc/rancher/rke2/rke2.yaml
```

#### 5. Check whether the binary exists
```bash
which rke2      # output → RKE2
which kubeadm   # output → kubeadm
```

#### 6. Quick reference table

| Check | kubeadm | RKE2 |
|---|---|---|
| Running service | `kubelet` | `rke2-server` |
| Static pod directory | `/etc/kubernetes/manifests/` | `/var/lib/rancher/rke2/agent/pod-manifests/` |
| kubeconfig path | `~/.kube/config` | `/etc/rancher/rke2/rke2.yaml` |
| Binary | `kubeadm` | `rke2` |
| etcd snapshot directory | manual (via etcdctl) | `/var/lib/rancher/rke2/server/db/snapshots/` |

---

### Slide 15: Show a Kubernetes Cluster

A Kubernetes cluster consists of a **control plane** (master node) and one or more **worker nodes**. The control plane manages the cluster; worker nodes run the actual workloads (pods).

- **As UI**: Use tools like Lens or Rancher to visually manage and monitor your cluster.
- **As CLI**: Use `kubectl` to interact with your cluster from the terminal.

List all nodes in the cluster:
```bash
kubectl get nodes
```

Example output:
```
NAME       STATUS   ROLES           AGE   VERSION
master     Ready    control-plane   10d   v1.35.0
worker-1   Ready    <none>          10d   v1.35.0
worker-2   Ready    <none>          10d   v1.35.0
```
- **NAME** — the node's hostname
- **STATUS** — `Ready` means the node is healthy and can accept pods
- **ROLES** — `control-plane` is the master; worker nodes show `<none>`
- **VERSION** — the Kubernetes version running on that node

Set a short alias so you don't have to type `kubectl` every time:

- Linux / Mac (zsh):
  ```bash
  alias k=kubectl
  echo "alias k=kubectl" >> ~/.zshrc
  source ~/.zshrc
  ```
- Linux (bash):
  ```bash
  alias k=kubectl
  echo "alias k=kubectl" >> ~/.bashrc
  source ~/.bashrc
  ```

### Slide 16: Show Core Components in the Cluster

`kube-system` is a special namespace that Kubernetes creates automatically. All internal cluster components (API server, etcd, scheduler, controller manager, kube-proxy) run here as pods. These are not user workloads — they are the cluster itself.

List all system pods:
```bash
kubectl get pods --namespace kube-system
# shorthand:
kubectl get pods -n kube-system
```

You will see pods like:
- `kube-apiserver-*` — the API gateway
- `etcd-*` — the database
- `kube-scheduler-*` — assigns pods to nodes
- `kube-controller-manager-*` — watches and corrects cluster state
- `kube-proxy-*` — manages network rules on each node
- `coredns-*` — internal DNS for service discovery

## Slide 17: Kube-API Server

The kube-apiserver is the **front door of the cluster**. Every request — from `kubectl`, from the scheduler, from the controller manager — goes through it. It validates requests, authenticates users, and stores the result in etcd.

Nothing in Kubernetes talks to etcd directly. Everything goes through the API server.

**Find the kube-apiserver pod and read its logs:**
```bash
# Find the pod name
kubectl -n kube-system get pods | grep kube-apiserver

# Read its logs (shows every API request hitting the cluster)
kubectl -n kube-system logs <kube-apiserver-pod-name>
```

**Show the static manifest file** — this is how the API server is configured. It runs as a static pod, not a regular deployment:
```bash
# SSH into the master node first, then:
cat /etc/kubernetes/manifests/kube-apiserver.yaml
```

### Static Pod Demo

The kubelet watches the static pod manifest directory. On a kubeadm control
plane node, that directory is:

```bash
/etc/kubernetes/manifests/
```

If you put a pod manifest there, kubelet creates the pod automatically. If you
delete the mirror pod from the API server, kubelet recreates it.

Create a static pod manifest on the control-plane node:
```bash
sudo vi /etc/kubernetes/manifests/static-nginx.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: static-nginx
spec:
  containers:
  - name: nginx
    image: nginx
```

Wait for kubelet to create the pod:
```bash
kubectl get pods -o wide | grep static-nginx
```

The pod name will include the node name:
```text
static-nginx-<control-plane-node-name>
```

Delete the pod from the API server:
```bash
kubectl delete pod static-nginx-<control-plane-node-name>
```

Check again. Kubelet recreates the mirror pod because the manifest file still
exists under `/etc/kubernetes/manifests/`:
```bash
kubectl get pods -o wide | grep static-nginx
```

Now remove the manifest from the static pod directory:
```bash
sudo mv /etc/kubernetes/manifests/static-nginx.yaml /root/static-nginx.yaml
```

Wait and check again. The static pod should disappear because kubelet no longer
sees the manifest in the watched directory:
```bash
kubectl get pods -o wide | grep static-nginx
```
No output means the static pod is gone.

The same YAML file under `/root` does **not** create a pod by itself:
```bash
ls -l /root/static-nginx.yaml
kubectl get pods -o wide | grep static-nginx
```
Again, no output from the `kubectl` command is expected.

If you manually apply the file from `/root`, it becomes a normal API-created
pod, not a static pod:
```bash
kubectl apply -f /root/static-nginx.yaml
kubectl get pods -o wide | grep static-nginx
```

Delete it again:
```bash
kubectl delete -f /root/static-nginx.yaml
kubectl get pods -o wide | grep static-nginx
```

This time the pod should stay deleted, because `/root/static-nginx.yaml` is not
in the kubelet static pod manifest directory.

Clean up the demo file:
```bash
sudo rm /root/static-nginx.yaml
```

- increase log level with `--v` flag — edit the static manifest:
  ```bash
  sudo vi /etc/kubernetes/manifests/kube-apiserver.yaml
  ```
  Add `- --v=3` under the command section. kubelet will automatically restart the pod.

  | Level | What you see |
  |---|---|
  | `--v=0` | Critical errors only (default) |
  | `--v=1` | General info messages |
  | `--v=2` | Important operations, cluster changes |
  | `--v=3` | HTTP requests start appearing |
  | `--v=4` | Request details and headers |
  | `--v=6` | Full URL and response code for every request |
  | `--v=8` | Request and response bodies |
  | `--v=10` | Everything — too verbose for production |

  > Use `--v=3` for demos to show students what happens behind the scenes. Keep `--v=0` or `--v=2` in production.
  >
  > Full flag reference: [kube-apiserver CLI reference](https://kubernetes.io/docs/reference/command-line-tools-reference/kube-apiserver/)

## Slide 18: ETCD

### Where to Run etcdctl Commands

etcd only runs on the **control plane (master) node** — SSH in first, then run commands there:

```bash
# Step 1 — SSH into the master node
ssh ubuntu@<master-node-ip>

# Step 2 — Run the command
```

etcd requires TLS certificates for authentication. Certificate paths differ by distribution:

| | kubeadm | RKE2 | Minikube |
|---|---|---|---|
| CA cert | `/etc/kubernetes/pki/etcd/ca.crt` | `/var/lib/rancher/rke2/server/tls/etcd/server-ca.crt` | `/var/lib/minikube/certs/etcd/ca.crt` |
| Client cert | `/etc/kubernetes/pki/etcd/server.crt` | `/var/lib/rancher/rke2/server/tls/etcd/server-client.crt` | `/var/lib/minikube/certs/etcd/server.crt` |
| Client key | `/etc/kubernetes/pki/etcd/server.key` | `/var/lib/rancher/rke2/server/tls/etcd/server-client.key` | `/var/lib/minikube/certs/etcd/server.key` |

> **Minikube:** exec into the etcd pod instead of SSH:
> ```bash
> kubectl -n kube-system exec -it etcd-minikube -- sh
> ```

---

### List All Cluster Data

- store all cluster data
  ```bash
  etcdctl \
    --endpoints=https://127.0.0.1:2379 \
    --cacert=/etc/kubernetes/pki/etcd/ca.crt \
    --cert=/etc/kubernetes/pki/etcd/server.crt \
    --key=/etc/kubernetes/pki/etcd/server.key \
    get / --prefix --keys-only
  ```

- Create 2 pods to see them appear in etcd:
  ```bash
  kubectl run pod --image nginx
  kubectl run pod-2 --image nginx
  ```

### ETCD Snapshot Backup

This command creates an **etcd snapshot backup**. It backs up the Kubernetes
cluster state stored in etcd, such as Deployments, Services, ConfigMaps,
Secrets, RBAC objects, and custom resources.

> This does **not** back up Persistent Volume data, container images, or files
> stored on worker nodes.

Take an etcd snapshot backup:
```bash
etcdctl \
  --endpoints=https://127.0.0.1:2379 \
  --cacert=/etc/kubernetes/pki/etcd/ca.crt \
  --cert=/etc/kubernetes/pki/etcd/healthcheck-client.crt \
  --key=/etc/kubernetes/pki/etcd/healthcheck-client.key \
  snapshot save snapshot.db
```

Check that the backup file was created:
```bash
ls -lh snapshot.db
```

Check snapshot status:
```bash
etcdutl snapshot status snapshot.db --write-out=table
```

> **etcdctl vs etcdutl:** `etcdctl` talks to a running etcd cluster and is used
> for online operations such as `get`, `put`, `endpoint health`, and
> `snapshot save`. `etcdutl` works with offline etcd data or snapshot files and
> is used for operations such as `snapshot status` and `snapshot restore`.
>
> In newer etcd versions, `etcdctl snapshot` is mainly used to **save** a
> snapshot. Offline snapshot operations such as `status` and `restore` are done
> with `etcdutl`.

### ETCD Snapshot Restore Demo

This demo shows that an etcd snapshot restores Kubernetes API objects that
existed at the time the snapshot was taken.

> Run this only in a lab cluster. Restoring an etcd snapshot rolls back the
> entire Kubernetes cluster state to the snapshot time.

Create two test pods before taking the snapshot:
```bash
kubectl run pod --image nginx
kubectl run pod-2 --image nginx
kubectl get pods
```

Take and verify the snapshot by following the commands in
[ETCD Snapshot Backup](#etcd-snapshot-backup).

Delete the pods after the snapshot:
```bash
kubectl delete pod pod pod-2
kubectl get pods
```

Restore the snapshot on a single-control-plane kubeadm cluster:
```bash
# Stop the etcd static pod
mkdir -p /root/restore-backup
crictl ps 
mv /etc/kubernetes/manifests/etcd.yaml /root/restore-backup/etcd.yaml

# Wait until the etcd container is stopped

while crictl ps | grep -q etcd; do sleep 2; done

# Move the current etcd data directory out of the way
BACKUP_DIR=/var/lib/etcd.bak.$(date +%Y%m%d-%H%M%S)
mv /var/lib/etcd "$BACKUP_DIR"
echo "Original etcd data moved to: $BACKUP_DIR"

# Restore the snapshot into the original etcd data directory
etcdutl snapshot restore snapshot.db --data-dir /var/lib/etcd

# Start etcd again by moving the static pod manifest back
mv /root/restore-backup/etcd.yaml /etc/kubernetes/manifests/etcd.yaml
```

Wait for the API server to become healthy again, then verify that the pods are
back:
```bash
systemctl restart containerd
systemctl restart kubelet
crictl rm -f $(crictl ps -a -q)
crictl pull nginx

kubectl get pods
```

#### Roll Back to the Pre-Restore etcd State

During the restore step, the original etcd data directory was not deleted. It
was moved to a backup directory:

```bash
/var/lib/etcd.bak.<timestamp>
```

This means you still have both states:

```bash
/var/lib/etcd.bak.<timestamp>  # original state before snapshot restore
/var/lib/etcd                  # restored state from snapshot.db
```

If the snapshot restore causes a problem, or if you want to go back to the
state where the test pods were already deleted, move the backup directory back
into place.

> Use this only if you intentionally want to discard the restored etcd state and
> return to the cluster state that existed immediately before the restore.

Stop etcd:
```bash
mkdir -p /root/restore-backup
mv /etc/kubernetes/manifests/etcd.yaml /root/restore-backup/etcd.yaml

while crictl ps | grep -q etcd; do sleep 2; done
```

Find the backup directory if you did not save the name:
```bash
ls -ld /var/lib/etcd.bak.*
```

Remove the restored etcd data directory and move the original data back:
```bash
rm -rf /var/lib/etcd
mv /var/lib/etcd.bak.<timestamp> /var/lib/etcd
```

Start etcd again:
```bash
mv /root/restore-backup/etcd.yaml /etc/kubernetes/manifests/etcd.yaml
```

Restart the runtime and kubelet, then verify the cluster state:
```bash
systemctl restart containerd
systemctl restart kubelet

kubectl get pods
```

The deleted test pods should not be present anymore, because the cluster is
back to the state from before the snapshot restore.

### RKE2 etcd snapshots

- **RKE2 etcd snapshots**
  - Default snapshot directory (only on control-plane nodes, not workers):
    ```
    /var/lib/rancher/rke2/server/db/snapshots/
    ```
  - Snapshot filename format:
    ```
    etcd-snapshot-<node-name>-<unix-timestamp>
    # Example: etcd-snapshot-dr-ds-master-3-1775509200
    ```
  - Convert Unix timestamp to human-readable date:
    ```bash
    date -d @1775509200
    # Output: Tue Apr  6 12:00:00 UTC 2026
    ```

  - **RKE2 binary PATH issue** — `rke2` command not found by default because `/var/lib/rancher/rke2/bin/` is not in PATH:
    ```bash
    # Quick fix — full path
    /var/lib/rancher/rke2/bin/rke2 etcd-snapshot ls

    # Permanent fix — add to PATH
    echo 'export PATH=$PATH:/var/lib/rancher/rke2/bin' >> ~/.bashrc
    source ~/.bashrc
    ```

  - **Snapshot operations:**
    ```bash
    rke2 etcd-snapshot save                    # manual snapshot
    rke2 etcd-snapshot ls                      # list snapshots
    rke2 etcd-snapshot delete <snapshot-name>  # delete a snapshot
    ```

  - **Automatic snapshots** — add to `/etc/rancher/rke2/config.yaml`:
    ```yaml
    etcd-snapshot-schedule-cron: "0 */6 * * *"  # every 6 hours
    etcd-snapshot-retention: 10                  # keep last 10
    ```

  - **Restore (critical)** — restores entire cluster state, wrong snapshot = wrong cluster state
    - Step 1: Stop RKE2 on **all** control-plane nodes:
      ```bash
      systemctl stop rke2-server
      ```
    - Step 2: Run restore on **one** control-plane node only:
      ```bash
      rke2 server \
        --cluster-reset \
        --cluster-reset-restore-path=/var/lib/rancher/rke2/server/db/snapshots/<snapshot-name>
      ```
    - Step 3: Start RKE2 on the restored node:
      ```bash
      systemctl start rke2-server
      ```
    - Step 4: Start RKE2 on **other** control-plane nodes (they rejoin automatically, no reset needed):
      ```bash
      systemctl start rke2-server
      ```

  - **Restore from S3 (remote snapshot):**
    ```bash
    rke2 server \
      --cluster-reset \
      --etcd-s3=true \
      --etcd-s3-bucket=<bucket> \
      --etcd-s3-endpoint=<endpoint> \
      --cluster-reset-restore-path=<snapshot-name>
    ```

- Raft algorithm: Discuss the consensus algorithm used by etcd.

## Slide 19: kube-controller manager

The kube-controller-manager runs multiple control loops in a single process. Each controller watches the cluster state and takes action to move it toward the desired state. Think of it as the **self-healing brain** of the cluster.

**Node Controller** — watches the health of nodes:
- Checks each node every **5 seconds**
- If a node stops responding, waits **50 seconds** by default before marking it `NotReady`
- If the node is still unreachable after **5 minutes**, evicts all pods from that node and reschedules them elsewhere

### kubeadm Node Failure and Eviction Timing

In kubeadm clusters, the control plane components run as static pods. To tune
how quickly a failed node is detected and how quickly pods are evicted from it,
edit the static pod manifests on the control-plane node.

#### 1. Node `NotReady` Detection Time

This controls how long the node controller waits before marking a node
`NotReady`.

Parameter:
```text
--node-monitor-grace-period
```

Default:
```text
50s
```

Change it in the kube-controller-manager static pod manifest:
```bash
sudo vi /etc/kubernetes/manifests/kube-controller-manager.yaml
```

Add the flag under the `command` section:
```yaml
- --node-monitor-grace-period=20s
```

#### 2. Pod Eviction Time After `NotReady`

This controls how long pods tolerate a `NotReady` or `Unreachable` node before
they are evicted.

Parameters:
```text
--default-not-ready-toleration-seconds
--default-unreachable-toleration-seconds
```

Default:
```text
300s
```

Change them in the kube-apiserver static pod manifest:
```bash
sudo vi /etc/kubernetes/manifests/kube-apiserver.yaml
```

Add the flags under the `command` section:
```yaml
- --default-not-ready-toleration-seconds=60
- --default-unreachable-toleration-seconds=60
```

After saving the static pod manifest files, kubelet automatically restarts the
affected control plane pods. No manual `kubectl apply` is needed.

Short flow with the example values:
```text
Node down
↓
20 seconds → Node becomes NotReady
↓
60 seconds → Pods are evicted
```

> Use shorter values carefully in production. Aggressive eviction settings can
> move pods during short network interruptions or brief node pressure events.

**Replication Controller** — ensures the correct number of pod replicas are always running:

Demo — scale a deployment and watch the controller react:
```bash
# Create a deployment
kubectl create deployment demo --image=nginx --replicas=3

# Check pods
kubectl get pods

# Scale down manually
kubectl scale deployment demo --replicas=1

# Watch controller bring it back if you delete a pod
kubectl delete pod <pod-name>
kubectl get pods  # a new pod is created immediately
```

View controller manager logs to see it taking action:
```bash
kubectl -n kube-system logs <kube-controller-manager-pod-name>
```

Demo — make one node `NotReady` and watch the pod move to the other node:

This example uses a two-node lab cluster:
```bash
kubectl get nodes
```

Example:
```text
NAME           STATUS   ROLES           AGE   VERSION
controlplane   Ready    control-plane   15d   v1.35.1
node01         Ready    <none>          15d   v1.35.1
```

> In a two-node kubeadm lab, the `controlplane` node usually has a
> `NoSchedule` taint. That means normal application pods will not move there
> unless you temporarily remove the taint or add a toleration.

Create a demo Deployment. Because the control-plane node is tainted by default,
the pod should start on `node01`:
```bash
kubectl create deployment node-failover-demo --image=nginx --replicas=1
kubectl get pods -o wide -l app=node-failover-demo
```

Temporarily allow workloads to run on the control-plane node:
```bash
kubectl taint nodes controlplane node-role.kubernetes.io/control-plane:NoSchedule-
```

> If the command says the taint was not found, continue. It means your
> control-plane node was already schedulable.

Stop kubelet on `node01` to simulate a node failure:
```bash
ssh root@node01
systemctl stop kubelet
exit
```

Watch the node and pod status:
```bash
kubectl get nodes -w
```

In another terminal:
```bash
kubectl get pods -o wide -w -l app=node-failover-demo
```

Expected behavior:
- With default kubeadm values, after about **50 seconds**, `node01` becomes `NotReady`
- With default kubeadm values, after about **5 minutes**, pods on `node01` are evicted
- If you applied the tuning above, the flow becomes about **20 seconds** to
  `NotReady` and about **60 seconds** to eviction
- The Deployment controller creates a replacement pod
- The new pod is scheduled on `controlplane`

Start kubelet again on `node01`:
```bash
ssh root@node01
systemctl start kubelet
exit
```

Verify that both nodes are healthy:
```bash
kubectl get nodes
```

Clean up the demo and restore the default control-plane taint:
```bash
kubectl delete deployment node-failover-demo
kubectl taint nodes controlplane node-role.kubernetes.io/control-plane:NoSchedule
```

## Slide 20: kube-scheduler

The kube-scheduler decides **which node a pod runs on**. When a new pod is
created without a node assigned, the scheduler evaluates all available nodes,
filters out ones that don't meet requirements, ranks the rest, and picks the
best one.

It considers: available CPU/memory, node labels, taints/tolerations, affinity rules, and pod spread constraints.

> Important: the scheduler does **not** place pods based directly on
> `kubectl top nodes` live usage. It mainly uses node capacity, allocatable
> resources, and existing pod `resources.requests`. If a pod has no resource
> requests, the scheduler has very little information about how much CPU or
> memory that pod needs.

Example lab node usage:
```bash
kubectl top nodes
```

```text
NAME            CPU(cores)   CPU(%)   MEMORY(bytes)   MEMORY(%)
p-ds-master-1   484m         12%      5746Mi          72%
p-ds-worker-1   1253m        31%      4993Mi          62%
p-ds-worker-2   1198m        29%      5539Mi          69%
p-ds-worker-3   607m         15%      5220Mi          65%
p-ds-worker-4   430m         5%       9124Mi          57%
p-ds-worker-5   1681m        21%      9307Mi          58%
```

The master/control-plane node may have a `NoSchedule` taint, so the following
examples use worker nodes.

**Node selection options — from simplest to most flexible:**

All pod examples below include `resources.requests` and `resources.limits`, so
the scheduler has CPU and memory information while making placement decisions.

**0. No node selection** — let the scheduler choose any suitable node:
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: no-node-selection-demo
spec:
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

**1. nodeName** — hardcode the pod to a specific node (bypasses the scheduler entirely):
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: nodename-demo
spec:
  nodeName: p-ds-worker-1      # runs on this node only
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

```bash
kubectl describe nodes p-ds-worker-1 
```


**2. nodeSelector** — run the pod on nodes that have a specific label:
Add the label to a node first:
```bash
kubectl get nodes --show-labels

kubectl label node p-ds-worker-1 disktype=ssd
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: nodeselector-demo
spec:
  nodeSelector:
    disktype: ssd         # only schedule on nodes labeled disktype=ssd
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

Add the label to a node first:
```bash
kubectl label node p-ds-worker-4 disktype=ssd

kubectl label node p-ds-worker-4 disktype-
```

**3. affinity** — more expressive rules (required vs preferred, multiple conditions):
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: affinity-demo
spec:
  affinity:
    nodeAffinity:
      requiredDuringSchedulingIgnoredDuringExecution:
        nodeSelectorTerms:
        - matchExpressions:
          - key: disktype
            operator: In
            values:
            - ssd
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

To test any of these pod examples, save the YAML and apply it:
```bash
vi pod.yaml
kubectl apply -f pod.yaml
kubectl get pods -o wide
kubectl describe pod <pod-name>
```

Clean up before testing the next option:
```bash
kubectl delete pod <pod-name>
```

View scheduler logs to see scheduling decisions:
```bash
kubectl -n kube-system logs <kube-scheduler-pod-name>
```


**4. taints** — A pod can run on a tainted node only if it has a matching toleration:


Add a taint to `p-ds-worker-4`:
```bash
kubectl taint node p-ds-worker-4 dedicated=demo:NoSchedule
```

This means normal pods should not be scheduled on `p-ds-worker-4`.

Save this as `taint-demo-no-toleration.yaml`:
```bash
vi taint-demo-no-toleration.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: taint-demo-no-toleration
spec:
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

Apply it and check the pod status. Because the node is tainted and the pod has
no toleration, it should stay `Pending` if no other suitable node is available:
```bash
kubectl apply -f taint-demo-no-toleration.yaml
kubectl get pod taint-demo-no-toleration -o wide
kubectl describe pod taint-demo-no-toleration
```

Save this as `taint-demo-with-toleration.yaml`:
```bash
vi taint-demo-with-toleration.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: taint-demo-with-toleration
spec:
  tolerations:
  - key: "dedicated"
    operator: "Equal"
    value: "demo"
    effect: "NoSchedule"
  containers:
  - name: nginx
    image: nginx
    resources:
      requests:
        cpu: "250m"
        memory: "256Mi"
      limits:
        cpu: "500m"
        memory: "512Mi"
```

Apply it and check that it can run on the tainted node:
```bash
kubectl apply -f taint-demo-with-toleration.yaml
kubectl get pod taint-demo-with-toleration -o wide
```

Clean up:
```bash
kubectl delete pod taint-demo-no-toleration taint-demo-with-toleration --ignore-not-found
kubectl taint node p-ds-worker-4 dedicated=demo:NoSchedule-
kubectl label node p-ds-worker-1 disktype-
kubectl label node p-ds-worker-4 disktype-
```
    
## Slide 21: kubelet

kubelet is the **agent that runs on every node** — both master and worker. It receives pod specifications from the API server and makes sure the containers described in those specs are actually running. If a container crashes, kubelet restarts it.

kubelet is the only Kubernetes component that runs directly on the OS (not as a pod). It is what makes a machine a Kubernetes node.

Check kubelet status:
```bash
# kubeadm:
systemctl status kubelet

# RKE2 (kubelet is embedded — check rke2-server instead):
systemctl status rke2-server
```

### Accessing the Master Node via SSH

Before inspecting any control plane component, you need to SSH into the master node:
```bash
ssh <user>@<master-node-ip>
# Example:
ssh ubuntu@10.0.0.10
```

Once connected, you can explore the control plane files and check service status.

---

### Static Pod Support

kubelet watches a manifest directory and runs any YAML file it finds there **directly — without needing the API Server**. This is exactly how master components (apiserver, etcd, scheduler) bootstrap themselves.

| Setup | Static Pod Directory |
|---|---|
| kubeadm (standard) | `/etc/kubernetes/manifests/` |
| RKE2 | `/var/lib/rancher/rke2/agent/pod-manifests/` |

---

### Static Pods on the Control Plane Node (MOST CRITICAL)

Control plane components live in (RKE2):
```
/var/lib/rancher/rke2/agent/pod-manifests/
```

Manifest files in this directory:
- `kube-apiserver.yaml`
- `etcd.yaml`
- `kube-scheduler.yaml`
- `kube-controller-manager.yaml`
- `kube-proxy.yaml`

These static pods are launched directly by **kubelet** — they are the heart of the cluster.

### Why Does kubelet Work Differently in RKE2?

In standard Kubernetes, kubelet runs as a standalone service:
```bash
systemctl status kubelet
```

In RKE2, kubelet runs **embedded** inside the rke2-server process:
```bash
systemctl status rke2-server
```

The rke2-server binary bundles:
- **kubelet** 
- **containerd** 

---

## Slide 22: kube-proxy

kube-proxy runs on **every node** and maintains network rules (iptables or IPVS) that allow pods and services to communicate with each other across the cluster.

When you create a Kubernetes **Service**, kube-proxy is what makes it work: it watches for new services and updates the network rules on every node so that traffic sent to a service's virtual IP gets forwarded to the correct pod(s), even if those pods are on a different node.

```bash
# See the kube-proxy pod running on each node
kubectl get pods -n kube-system -o wide | grep kube-proxy

# View kube-proxy logs
kubectl -n kube-system logs <kube-proxy-pod-name>
```

> kube-proxy does **not** proxy traffic itself — it just programs the kernel's network rules. The actual packet forwarding is done by the OS network stack.

## Slide 23: container runtime

The container runtime is the software that **actually starts and stops containers** on a node. Kubernetes doesn't run containers itself — it delegates that to the runtime via the CRI (Container Runtime Interface).

Common runtimes:
- **containerd** — the most widely used, lightweight, production-grade
- **CRI-O** — built specifically for Kubernetes
- **Docker** — no longer supported directly in Kubernetes (uses containerd underneath anyway)

See which runtime each node is using:
```bash
kubectl get nodes -o wide
```

The `CONTAINER-RUNTIME` column shows the runtime and its version, for example:
```
NAME       STATUS   ROLES           AGE   VERSION   CONTAINER-RUNTIME
master     Ready    control-plane   10d   v1.35.0   containerd://1.7.0
worker-1   Ready    <none>          10d   v1.35.0   containerd://1.7.0
```

> All nodes in a cluster typically use the same runtime, but this is not required.

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
    kubectl config use-context orbstack
    kubectl get nodes
    ```
## kubeconfig File

The kubeconfig file tells `kubectl` **where the cluster is**, **who you are**, and **what context to use**. Without it, `kubectl` has no idea which cluster to talk to.

### File Location by Distribution

| Distribution | kubeconfig Path |
|---|---|
| **Standard (kubeadm)** | `~/.kube/config` (copied from `/etc/kubernetes/admin.conf`) |
| **RKE2 (as root)** | `/etc/rancher/rke2/rke2.yaml` |
| **RKE2 (kubectl shortcut)** | `/root/.kube/config` (symlinked or copied from above) |
| **K3s** | `/etc/rancher/k3s/k3s.yaml` |
| **Minikube** | `~/.kube/config` (auto-configured) |

> **Note for RKE2:** After bootstrapping, run this to make `kubectl` work without specifying `--kubeconfig`:
> ```bash
> mkdir -p ~/.kube
> cp /etc/rancher/rke2/rke2.yaml ~/.kube/config
> chmod 600 ~/.kube/config
> ```

### File Structure

A kubeconfig file has three main sections:

```yaml
apiVersion: v1
kind: Config
clusters:          # cluster API server addresses + CA certs
users:             # credentials (certs, tokens)
contexts:          # binds a cluster + user + namespace together
current-context:   # which context kubectl uses by default
```

### Useful Commands

View the full kubeconfig (sensitive fields masked):
```bash
kubectl config view
```

View raw with all secrets (certs, tokens):
```bash
kubectl config view --raw
```

List all contexts (a context = cluster + user + namespace):
```bash
kubectl config get-contexts
```

Show the currently active context:
```bash
kubectl config current-context
```

Switch to a different context:
```bash
kubectl config --help

kubectl config get-contexts

kubectl config use-context <context-name>
```

Merge multiple kubeconfig files (e.g. adding a new cluster):
```bash
export KUBECONFIG=~/.kube/config:~/new-cluster.yaml
kubectl config view --merge --flatten > ~/.kube/config.merged
mv ~/.kube/config.merged ~/.kube/config
```

Override kubeconfig for a single command (without changing the file):
```bash
kubectl --kubeconfig /etc/rancher/rke2/rke2.yaml get nodes
```

### Hands-on: Merge Two Fake kubeconfig Files

This demo does not require real clusters. The API server addresses and tokens
are fake. The goal is to understand how multiple kubeconfig files are merged
into one file with multiple contexts.

Create the first fake kubeconfig:
```bash
vi fake-dev.yaml
```

```yaml
apiVersion: v1
kind: Config
clusters:
- name: fake-dev-cluster
  cluster:
    server: https://fake-dev.example.com:6443
    insecure-skip-tls-verify: true
users:
- name: fake-dev-user
  user:
    token: fake-dev-token
contexts:
- name: fake-dev
  context:
    cluster: fake-dev-cluster
    user: fake-dev-user
    namespace: default
current-context: fake-dev
```

Create the second fake kubeconfig:
```bash
vi fake-prod.yaml
```

```yaml
apiVersion: v1
kind: Config
clusters:
- name: fake-prod-cluster
  cluster:
    server: https://fake-prod.example.com:6443
    insecure-skip-tls-verify: true
users:
- name: fake-prod-user
  user:
    token: fake-prod-token
contexts:
- name: fake-prod
  context:
    cluster: fake-prod-cluster
    user: fake-prod-user
    namespace: production
current-context: fake-prod
```

Merge both files into a new kubeconfig file:
```bash
export KUBECONFIG=$PWD/fake-dev.yaml:$PWD/fake-prod.yaml
kubectl config view --merge --flatten > merged-kubeconfig.yaml
```

View the merged result:
```bash
kubectl --kubeconfig ./merged-kubeconfig.yaml config get-contexts
```

Expected output:
```text
CURRENT   NAME        CLUSTER             AUTHINFO         NAMESPACE
*         fake-dev    fake-dev-cluster    fake-dev-user    default
          fake-prod   fake-prod-cluster   fake-prod-user   production
```

Switch context inside the merged file:
```bash
kubectl --kubeconfig ./merged-kubeconfig.yaml config use-context fake-prod
kubectl --kubeconfig ./merged-kubeconfig.yaml config current-context
```

Inspect the full merged kubeconfig:
```bash
kubectl --kubeconfig ./merged-kubeconfig.yaml config view
```

Try an API command to prove that the clusters are fake:
```bash
kubectl --kubeconfig ./merged-kubeconfig.yaml get nodes
```

Expected result: the command fails because `fake-dev.example.com` and
`fake-prod.example.com` are not real Kubernetes API servers.

Clean up the demo files:
```bash
rm fake-dev.yaml fake-prod.yaml merged-kubeconfig.yaml
unset KUBECONFIG
```


## Make a Demo Managed Kubernetes Cluster with EKS (Optional)

This section is reserved for an optional managed Kubernetes demo. Add the EKS
cluster creation steps here when the lab needs a cloud-based cluster instead of
Minikube, kubeadm, or RKE2.
