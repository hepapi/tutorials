# Hands-on Kubernetes Lesson 8: Rancher

## Pre-requisites

- **Install Multipass**
  Follow the instructions for your OS here: [https://multipass.run/docs/install-multipass#install]
  - For Mac: `brew install multipass`
  - For Windows: [https://multipass.run/download/windows]
  
## Environments to be created

- **Multipass Setup**: Create four virtual machines using Multipass.
  - Upstream(Rancher) Cluster:
    - Master Node:
      ```bash
      multipass launch 20.04 --name upstream-master --cpus 3 --memory 5G --disk 20G
      ```
    - Worker Node:
      ```bash
      multipass launch 20.04 --name upstream-worker --cpus 2 --memory 2G --disk 10G
      ```
  - Downstream Cluster
    - Master Node:
    ```bash
      multipass launch 20.04 --name downstream-1-master --cpus 2 --memory 4G --disk 15G
    ```
    - Worker Node:
      ```bash
      multipass launch 20.04 --name downstream-1-worker --cpus 2 --memory 2G --disk 10G
      ```

## Part 1: Creating the Upstream Cluster

### Install RKE2 to the upstream-master Node

1. **SSH into the master node**:

```bash
multipass shell upstream-master
sudo -i
```
2. **RKE2 prerequests**:

```bash
systemctl stop ufw
systemctl disable ufw
swapoff -a

```
3. **RKE2 Installation**:

```bash
# Documentation for installing RKE2: https://docs.rke2.io/install/quickstart
curl -sfL https://get.rke2.io | INSTALL_RKE2_TYPE=server sh -
# start and enable server service
systemctl enable rke2-server.service
systemctl start rke2-server.service

```
4. **After Installation**:

```bash
# Create a symbolic link for kubectl
ln -s $(find /var/lib/rancher/rke2/data/ -name kubectl) /usr/local/bin/kubectl
# add kubectl conf
export KUBECONFIG=/etc/rancher/rke2/rke2.yaml
# check node status
kubectl  get node
alias k=kubectl
echo "export KUBECONFIG=/etc/rancher/rke2/rke2.yaml" >> .bashrc
echo " alias k=kubectl" >> .bashrc
kubectl get no -w
systemctl status rke2-server

```

### Install RKE2 to upstream-worker Node

1. **SSH into the worker node**:

```bash
multipass shell upstream-worker
sudo -i
```

2. **RKE2 prerequests**:

```bash
systemctl stop ufw
systemctl disable ufw
swapoff -a

```

3. **RKE2 Installation**:

```bash
# Create a configuration file
mkdir -p /etc/rancher/rke2/
# Replace <upstream-master-ip> with the actual IP of the master node (use 'multipass list' to find it)
echo "server: https://<upstream-worker>:9345" > /etc/rancher/rke2/config.yaml
# Retrieve the <token> from the master node using 'cat /var/lib/rancher/rke2/server/node-token'
echo "token: <token>" >> /etc/rancher/rke2/config.yaml
curl -sfL https://get.rke2.io | INSTALL_RKE2_TYPE=agent sh -
# start and enable agent service
systemctl enable rke2-agent.service
systemctl start rke2-agent.service
systemctl status rke2-agent

```

4. **Check Upstream Cluster Nodes**:

```bash
multipass shell upstream-master
sudo -i
kubectl get nodes # Verify that both nodes are ready
```

5. **Delete Upstream Worker Node**:

- The worker node was added for demonstration purposes. Now, we will delete it to free up resources.
```bash
multipass delete upstream-worker
multipass purge
multipass shell upstream-master
sudo -i
kubectl get nodes # See that the worker node is not ready and delete it
kubectl delete no upstream-worker
kubectl get nodes
```

### Install Rancher to Upstream Cluster
1. **SSH into the master node**:

```bash
multipass shell upstream-master
sudo -i
```

2. **Install Helm and Add Helm Charts**:

```bash
curl -#L https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
# Add necessary Helm charts
helm repo add rancher-stable https://releases.rancher.com/server-charts/stable
helm repo add jetstack https://charts.jetstack.io
```

3. **Install CertManger**:

```bash
# Apply the Cert-Manager CRD
kubectl apply -f https://github.com/jetstack/cert-manager/releases/download/v1.6.1/cert-manager.crds.yaml
# Install Cert-Manager
helm upgrade -i cert-manager jetstack/cert-manager --namespace cert-manager --create-namespace
# Verify Cert-Manager pods
kubectl get pods -n cert-manager
```
4. **Install Rancher**:

```bash
helm upgrade -i rancher rancher-stable/rancher --create-namespace --namespace cattle-system --set hostname=<upstream-master-ip>.nip.io --set bootstrapPassword=admin --set replicas=1
# Verify Rancher pods
kubectl get pods -n cattle-system -w
kubectl get ingress -n cattle-system
```
5. **Access Rancher UI**:

- Update the /etc/hosts file: <upstream-master-ip> <upstream-master-ip>.nip.io
- Go to: https://<upstream-master-ip>.nip.io
- Change the password

## Part 2: Adding New Cluster in Rancher 

### Creating Downstream-1 Cluster(Dev) with Rancher

1. Go to **Cluster Management** Menu
2. Click the **Create** button and select **Custom**.
3. Name the downstream cluster dev, select the K3S version, and click **Create**.
4. Discuss the other options.
5. Copy the registration command to the downstream master node.

### Install K3S to Downstream-1 Cluster(Dev) Master

```bash
multipass shell downstream-1-master
sudo -i
# Update the /etc/hosts file: <upstream-master-ip> <upstream-master-ip>.nip.io
# Run the registration command with --insecure flag

# Create a symbolic link for kubectl
ln -s $(find /var/lib/rancher/rke2/data/ -name kubectl) /usr/local/bin/kubectl
# Add kubectl configuration
export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
# Check node status
kubectl  get node
alias k=kubectl
echo "export KUBECONFIG=/etc/rancher/k3s/k3s.yaml" >> .bashrc
echo " alias k=kubectl" >> .bashrc
```

###Install K3S on Downstream-1 Cluster (Dev) Worker

1. In the Rancher UI, select the **dev** cluster in the Cluster Management menu. Select the **Registration** tab, choose the **worker** option, and copy the registration command.
```bash
multipass shell downstream-1-worker
sudo -i

# Update the /etc/hosts file: <upstream-master-ip> <upstream-master-ip>.nip.io
# Run the registration command with --insecure flag
```
2. Verify the new worker node in the Rancher UI and on the master node.

```bash
multipass shell downstream-1-master
sudo -i
kubectl get nodes # Ensure the second node is recognized as a worker
```

### Delete Downstream Worker Node

- The worker node was added for demonstration purposes. Now, we will delete it to free up resources.
```bash
multipass delete downstream-1-worker
multipass purge
multipass shell downstream-1-master
sudo -i
kubectl get nodes  # See that the worker node is not ready and delete it
kubectl delete node downstream-1-worker
kubectl get nodes
```

### Import Downstream Cluster(test) with Rancher

1. Create a Minikube cluster:
```bash
minikube start --cpus 2 --memory 2048 --disk-size 5g
```
2. Go to **Cluster Management** Menu
3. Click **Import** and select **Generic**.
3. Name the downstream cluster **test**, and click Create.
4. Run the registration command in the Minikube cluster.

```bash
kubectl get node # check which cluster
# run registration command as insecure
kubectl get po -n cattle-system -w
```

## Part 3: Exploring the Rancher UI

### Cluster Management
- we have already covered Cluster Creation and Import.
- **Cloud Credentials:** Used to import and create managed clusters from public cloud providers.
- **Drivers**: Add other cloud providers for integration.

### Users & Authentication
- Add new user
  - Name: comez
    Password: Comez.123456
  - Global Permission: **User-Base**
  - Create a project in the **Dev** cluster: **demo** and give member role in the demo project.
  - Create a namespace: **apps** and add namespace to **demo** project.
  - Create a pod in **apps** namespace
  - Open a Kubectl Shell
```bash
kubectl create deployment demo-app --image nginx  -n apps
```
  - Log out and log in with the new user (comez). You should only see the dev cluster and the demo project.

- Groups can use only auth prividers
- Auth Providers
- Role Template
  - Global, Cluster and Project/Namespaces Roles
  - Create a custom role
- Select Comez User and look at the roles he has

### Continuous Delivery

- Add a new Git repository with Kubernetes objects.
  - Name the repository **gitops**.
  - Rpostiory URL: **https://github.com/hepapi/gitops.git** (public repo).
  - Branch: **main**.
  - Path: **dev** and **next**
  -Deploy to: Select the **dev** cluster and click **Create**.
  - Go to the dev cluster and the dev namespace to see all the resources (deployments, pods, config maps, services, etc.).

### Managing a Cluster

- Go to home page and select dev cluster
- Import YAML, Open a Kubectl shell, copy/download kubeconfig
- Preferences, Account and APIkeys
- Cluster:
  - Events, Alerts, and Certificates; verify the health of core components (etcd, scheduler, etc.).
  - Total Usage and counts(node, deployments, pods)

  - Projects/Namespace: create projects from namespaces and Manage authentication
  - Cluster and Project Members
  - Events

- Workloads:
  - Select All Namespaces to see all compute objects.
  - Scale any deployment.
  - Explore Cronjobs, DaemonSets, Deployments, Jobs, StatefulSets, and Pods.
  - Go to Deployment and create a deployment from UI.
  - View logs of any pod
  - Execute Shell of any pod
  - Edit a deployment from UI or as YAML
- Apps:
  - Deploy a monitoring App
- Service Discovery:
  - Explore Services, Ingress, and HPA.
- Storage:
  - View Storage Classes, Persistent Volumes, Persistent Volume Claims, ConfigMaps, and Secrets.





