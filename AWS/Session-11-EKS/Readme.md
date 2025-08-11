# Session-12: EKS and ECS

After completing this hands-on, you should know how to do the following:

- Provision and manage Kubernetes clusters on Amazon EKS using both the Console and eksctl CLI
- Deploy and scale containerized workloads using EKS with Managed Node Groups, Auto Mode, and Fargate
- Implement autoscaling mechanisms using; Horizontal Pod Autoscaler (HPA), Cluster Autoscaler and Vertical Pod Autoscaler (VPA)
- Configure persistent storage for Kubernetes using Amazon EBS and test data retention
- Deploy workloads to Amazon ECS using Task Definitions and Fargate launch types
---

##  Part 1: Create EKS Cluster (Custom Configuration)

- In this part, we will create an Amazon EKS cluster using the **Custom Configuration** method via AWS Console. This provides fine-grained control over networking, IAM roles, Kubernetes versions, and access settings.

### Task 1: Navigate to EKS Console

- Go to the AWS Management Console
- In the search bar, type and select **EKS (Elastic Kubernetes Service)**
- Click on **Create Cluster**
- Select **Custom Configuration**


### Task 2: Configure Basic Cluster Settings

- close **Use EKS Auto Mode**

```text
Cluster name                            : Lab-Cluster-<YourName>
Cluster IAM role                        : AmazonEKSClusterRole-<YourName> (recommended)
Kubernetes version                      : 1.33
Upgrade policy                          : Standard
Cluster access                          : Allow cluster administrator access
Cluster authentication mode             : EKS API
ARC Zonal shift                         : Disabled
Tags
    Key                                 : Name
    Value                               : YourName
```

- click **Next**

```text
Networking 
    VPC                                 : Default
    Subnets                             : All Subnets
    Additional security groups          : 
    Choose cluster IP address family    : IPv4
Cluster endpoint access                 : Public and private   
```

- Keep clicking **Next** (three times) to continue.
- Click **Create** to launch the EKS Cluster



##  Part 2: Installing kubectl and eksctl on Amazon Linux 2

- At the top of the AWS Management Console, in the search bar, search for and choose `EC2`
- Click on `Launch Instance`

### Task 1: Launch an EC2 Instance

- At the top of the AWS Management Console, in the search bar, search for and choose `EC2`
- Click on `Launch Instance`

```text
Name                            : Lab-kubectl-<YourName>
AMI                             : Amazon Linux 2023 AMI
Instance Type                   : t3.medium
Key pair name                   : Lab-Key-<YourName>
Network settings 
    VPC                         : default
    Subnet                      : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name              : default
    Rules                       : TCP --- > 22 ---> Anywhere
                                  All ICMP IPv4  ---> Anywhere
Configure storage               : 20 gp3
```

### Task 2: Configure AWS CLI

- Connect to the instance with SSH.

- Update the installed packages and package cache on your instance.

```bash
sudo yum update -y
```

- Configure AWS credentials. Or you can attach `AWS IAM Role` to your EC2 instance.

**Not**: In a test or lab environment, you may assign the `AdministratorAccess` IAM policy to simplify setup and ensure full permissions for all AWS services.
However, this approach is not recommended in production environments due to security best practices. In production, you should follow the principle of least privilege, granting only the specific permissions required for the tasks at hand.

```bash
aws configure
```

- aws configuration

```bash
  aws configure
  AWS Access Key ID [None]: XXXXXXXXXXXXXXX
  AWS Secret Access Key [None]: XXXXXXXXXXXXXXX
  Default region name [None]: eu-west-3
  Default output format [None]: json
```

- Verify that you can see your cluster listed, when authenticated

```bash
aws eks list-clusters
{
  "clusters": Lab-Cluster-<YourName>
}
```
### Task 3: Install kubectl

- Download the Amazon EKS vended kubectl binary.  [📄 Install kubectl  ](https://docs.aws.amazon.com/eks/latest/userguide/install-kubectl.html)

```bash
curl -O https://s3.us-west-2.amazonaws.com/amazon-eks/1.33.0/2025-05-01/bin/linux/amd64/kubectl
```

- Apply execute permissions to the binary.

```bash
chmod +x ./kubectl
```

- Copy the binary to a folder in your PATH. If you have already installed a version of kubectl, then we recommend creating a $HOME/bin/kubectl and ensuring that $HOME/bin comes first in your $PATH.

```bash
mkdir -p $HOME/bin && cp ./kubectl $HOME/bin/kubectl && export PATH=$HOME/bin:$PATH
```

- (Optional) Add the $HOME/bin path to your shell initialization file so that it is configured when you open a shell.

```bash
echo 'export PATH=$PATH:$HOME/bin' >> ~/.bashrc
```

- After you install kubectl , you can verify its version with the following command:

```bash
kubectl version

echo "alias k=kubectl" >> ~/.bashrc
source ~/.bashrc
```

### Task 4: Update kubeconfig

- Show the content of the $HOME directory including hidden files and folders. If there is a ```.kube``` directory, show what it has inside.  

- Run the command

```bash
aws eks --region <region_name> update-kubeconfig --name <cluster_name>

aws eks --region eu-west-3 update-kubeconfig --name Lab-Cluster-necip
``` 

- Run the command on your terminal

```bash
kubectl get svc
```

- You should see the output below

```bash
E0402 20:38:53.976498    2836 memcache.go:265] "Unhandled Error" err="couldn't get current server API group list: Get \"https://8EA963C5094FF9EF530E839618E4451B.yl4.eu-west-1.eks.amazonaws.com/api?timeout=32s\": dial tcp 172.31.12.83:443: i/o timeout"
```

- Open port **443** of security group belonging to eks cluster

- Run the command on your terminal again

```bash
kubectl get svc
```

```bash
E0807 11:07:41.464099    3679 memcache.go:265] "Unhandled Error" err="couldn't get current server API group list: the server has asked for the client to provide credentials"
```
- We are creating an access entry in the Access tab of the EKS cluster for the IAM role. This step is necessary to allow the IAM principal to authenticate and authorize with the Kubernetes API server.
- **Lab-Cluster-necip** --> **Access** ---> **Create access entry**

```text
IAM principal ARN: admin-<YourName>
Type             : Standart
```

- Click `Next`

```text
Policy name     : AmazonEKSClusterAdminPolicy
Access scope    : Cluster
```

- Click `Add policy`,`Next` and `Create`

- Run the command below again

```bash
kubectl get svc
```

```bash
NAME             TYPE        CLUSTER-IP   EXTERNAL-IP   PORT(S)   AGE
svc/kubernetes   ClusterIP   10.100.0.1   <none>        443/TCP   1m
```


### Task 5: Install eksctl

- Download and extract the latest release of eksctl with the following command.

```bash
curl --silent --location "https://github.com/eksctl-io/eksctl/releases/latest/download/eksctl_$(uname -s)_amd64.tar.gz" -o eksctl.tar.gz
```

- Move the extracted binary to /usr/local/bin.

```bash
tar -xzf eksctl.tar.gz
sudo mv eksctl /usr/local/bin
```

- Test that your installation was successful with the following command

```bash
eksctl version
```


##  Part 3 : Adding Worker Nodes to the Cluster

- In this section, we will add EC2 instances to our EKS cluster using a Managed Node Group via AWS

### Task 1: Navigate to the Cluster and Add Node Group

- Go to the EKS service from the AWS Management Console

- Click on your cluster name (e.g., Lab-Cluster-<YourName>)

- From the middle-hand menu, click on Compute --> **Add node group**

```text
Node group name                     : MyNodeGroup-<YourName>
Node IAM role                       : AmazonEKSNodeRole-<YourName> (recommended)
```

- The Node IAM role gives your worker nodes permissions to join the cluster and communicate with AWS services.

- Click **Next**

```text
Node group compute configuration
  AMI type                            : Amazon Linux 2023 (x86_64) Standard
  Capacity type                       : Spot
  Instance types                      : t3.medium
  Disk size                           : 20
Node group scaling configuration
  Desired size                        : 1
  Minimum size                        : 1
  Maximum size                        : 4
Node group update configuration 
  Maximum unavailable                 : Number 
  Value                               : 1 
  Update strategy                     : Default          
```

- These settings define the EC2 instance type and scaling behavior of your worker node group.

- Click **Next**

```text
Specify networking
  Node group network configuration
    Subnets                         : Choose All Subnets
```

- Open `Configure remote access to nodes` button

- Select your EC2 Key Pair

- Select Additional security groups (Open TCP 22 port)

- Click **Next**

- Review all settings and click **Create** to launch the node group

- After the node group is created and nodes join the cluster, go to terminal and verify them using 

```bash
kubectl get nodes -w
```

##  Part 4: Horizontal Pod Autoscaler (HPA) on EKS

- We’ll apply what we learned about HPA (Horizontal Pod Autoscaler) by deploying a sample application, applying CPU load, and watching Kubernetes automatically scale the number of pods.

- [📄 HPA Algorithm details ](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#algorithm-details)


### Task 1: Deploy a Simple CPU-Bound Application

- We'll use a simple PHP-Apache deployment with resource limits.

- Create a file named **php-apache.yaml**:

```bash
vi php-apache.yaml
```

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: php-apache
spec:
  selector:
    matchLabels:
      run: php-apache
  replicas: 1
  template:
    metadata:
      labels:
        run: php-apache
    spec:
      containers:
      - name: php-apache
        image: k8s.gcr.io/hpa-example
        ports:
        - containerPort: 80
        resources:
          limits:
            memory: 500Mi
            cpu: 150m
          requests:
            memory: 250Mi
            cpu: 100m
---
apiVersion: v1
kind: Service
metadata:
  name: php-apache-service
  labels:
    run: php-apache
spec:
  ports:
  - port: 80
    nodePort: 30001
  selector:
    run: php-apache 
  type: NodePort
```

- Apply the manifest:

```bash
kubectl apply -f php-apache.yaml

kubectl get deploy
```

- On opening browser (http://<public-node-ip>:<node-port>) we see

```text
OK!
```

- Alternatively, you can use;

```text
curl <public-worker node-ip>:<node-port>
OK!
```

- **Not**: Do not forget to open the **Port 30001** and **Port 80** in the security group of your node instance. 

### Task 2: Enable HPA

- Now create a file named **hpa.yaml**:

```bash
vi hpa.yaml
```

```yaml
apiVersion: autoscaling/v1
kind: HorizontalPodAutoscaler
metadata:
  name: php-apache
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: php-apache
  minReplicas: 3
  maxReplicas: 30
  targetCPUUtilizationPercentage: 50
```

- Apply the HPA: 

```bash
kubectl apply -f hpa.yaml
```

- Now monitor:

```bash
watch kubectl get pod

kubectl get deploy
```
- You should see that the deployment's replica count increases to 3 automatically.

```bash
kubectl get hpa

kubectl top pods
```

- The `metrics` can't be calculated. So, the `metrics server` should be uploaded to the cluster.



### Task 3: Install Metrics Server

- [📄 Metric Server ](https://docs.aws.amazon.com/eks/latest/userguide/metrics-server.html)

-  Deploy it using the official YAML:

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

- Then verify:

```bash
kubectl get deployment metrics-server -n kube-system
```

- After a few moments, try again:

```bash
kubectl top pods
```

### Task 4: Simulate CPU Load Inside the Pod

 - Check the pod name:

 ```bash
kubectl get pods
```

- Then watch the CPU usage 

```bash
kubectl top pods

NAME                          CPU(cores)   MEMORY(bytes)
php-apache-578548c5f7-5rfp7   1m           8Mi
php-apache-578548c5f7-6kl49   1m           10Mi
php-apache-578548c5f7-zdks9   1m           8Mi
```

- Download the hey binary

```bash
wget https://hey-release.s3.us-east-2.amazonaws.com/hey_linux_amd64
```

- Make the hey binary executable

```bash
mv hey_linux_amd64 hey

chmod +x hey

sudo mv hey /usr/local/bin/
```

- Verify the installation

```bash
hey -v
```

- To run a load test on your web application, you can use the following command:

```bash
hey -n 100000 http://<public-worker node-ip>:30001/

hey -n 100000 http://51.44.209.221:30001/
```

- This will send load requests to the specified URL and provide load testing metrics such as the number of successful requests, response times, and more.

- You’ll observe the CPU usage increasing 
- At this point, **HPA is not installed**, so even if CPU usage is high, Kubernetes won't scale pods automatically.


### Task 5: Notice Pending Pods

- Once the number of pods exceeds the cluster's capacity:

```bash
kubectl get pods

php-apache-xyz   Pending   0/1     ...     0s
```

### Task 6: Stop the process 

- While the hey command is running in the terminal,  press **CTRL + C** on your keyboard to interrupt and stop the process. 

- Terminate the background hey process, Find the Process ID (PID)

```bash
ps aux | grep hey
```

- Example output:

```bash
ec2-user   37115  0.0  1.2 710720 12368 pts/3    Sl+  14:04   0:00 hey -n 100000 http://3.254.114.47:30002/
```

- Terminate the process:

```bash
kill 37115
```

- If the process doesn’t stop, send a SIGKILL signal

```bash
kill -9 37115

```
##  Part 5: Configure Cluster Autoscaler

- In this part, we will configure the Cluster Autoscaler on Amazon EKS to automatically scale the worker nodes in your cluster based on resource usage 

### Task 1: Create IAM Policy

- Create a policy with following content. You can name it as `ClusterAutoscalerPolicy-<YourName>`.

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Action": [
                "autoscaling:DescribeAutoScalingGroups",
                "autoscaling:DescribeAutoScalingInstances",
                "autoscaling:DescribeLaunchConfigurations",
                "autoscaling:DescribeTags",
                "autoscaling:SetDesiredCapacity",
                "autoscaling:TerminateInstanceInAutoScalingGroup",
                "ec2:DescribeLaunchTemplateVersions"
            ],
            "Resource": "*",
            "Effect": "Allow"
        }
    ]
}
```

- Attach this policy to the IAM Worker Node Role `AmazonEKSNodeRole-<YourName>` which is already in use. 

### Task 2: Deploy Cluster Autoscaler

- First, download the Cluster Autoscaler YAML file:

```bash
cd

curl -o cluster-autoscaler-autodiscover.yaml https://raw.githubusercontent.com/kubernetes/autoscaler/master/cluster-autoscaler/cloudprovider/aws/examples/cluster-autoscaler-autodiscover.yaml
```

- Open the YAML file and replace <YOUR CLUSTER NAME> with **Lab-Cluster-YourName**

- Find an appropriate version of your cluster autoscaler in the [link](https://github.com/kubernetes/autoscaler/releases). The version number should start with version number of the cluster Kubernetes version. For example, if you have selected the Kubernetes version 1.31, you should find something like ```1.32.1```.

- Under the containers.command section of the Cluster Autoscaler deployment, we added the following flag:

```yaml
--skip-nodes-with-system-pods=false
```
- **Not**: Setting --skip-nodes-with-system-pods=false gives the Cluster Autoscaler more flexibility to scale down nodes, improving cost efficiency — but it requires careful testing to avoid evicting critical components.

- Add the following block inside the `ClusterRole` definition

```yaml
- apiGroups: ["storage.k8s.io"]
  resources: ["volumeattachments"]
  verbs: ["watch", "list", "get"]
```

- In the Deployment manifest of the Cluster Autoscaler, the following lines were added under the spec section of the Pod template

```yaml
spec:
  hostNetwork: true                     
  dnsPolicy: ClusterFirstWithHostNet
```

- These settings help avoid connectivity issues between the Cluster Autoscaler and the Kubernetes API server when using custom networking, firewall rules, or when running in restricted VPC environments

- Key modifications in this Cluster Autoscaler manifest:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: cluster-autoscaler
  labels:
    k8s-addon: cluster-autoscaler.addons.k8s.io
    k8s-app: cluster-autoscaler
rules:
  - apiGroups: ["storage.k8s.io"]                       # added  
    resources: ["volumeattachments"]                    # added
    verbs: ["watch", "list", "get"]                     # added
  
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cluster-autoscaler
  namespace: kube-system
  labels:
    app: cluster-autoscaler
spec:
  replicas: 1
  selector:
    matchLabels:
      app: cluster-autoscaler
  template:
    metadata:
      labels:
        app: cluster-autoscaler
      annotations:
        prometheus.io/scrape: 'true'
        prometheus.io/port: '8085'
    spec:
      hostNetwork: true                                   # added
      dnsPolicy: ClusterFirstWithHostNet                  # added
      priorityClassName: system-cluster-critical
      securityContext:
        runAsNonRoot: true
        runAsUser: 65534
        fsGroup: 65534
        seccompProfile:
          type: RuntimeDefault
      serviceAccountName: cluster-autoscaler
      containers:
        - image: registry.k8s.io/autoscaling/cluster-autoscaler:v1.32.1   # changed
          name: cluster-autoscaler
          resources:
            limits:
              cpu: 100m
              memory: 600Mi
            requests:
              cpu: 100m
              memory: 600Mi
          command:
            - ./cluster-autoscaler
            - --v=4
            - --stderrthreshold=info
            - --cloud-provider=aws
            - --skip-nodes-with-local-storage=false
            - --skip-nodes-with-system-pods=false         # added
            - --expander=least-waste
            - --node-group-auto-discovery=asg:tag=k8s.io/cluster-autoscaler/enabled,k8s.io/cluster-autoscaler/Lab-Cluster-necip   # changed
```

- Deploy the ``Cluster Autoscaler`` with the following command.

```bash
kubectl apply -f cluster-autoscaler-autodiscover.yaml
```

- After deploying the Cluster Autoscaler, check the logs to ensure it’s working correctly:

```bash
kubectl -n kube-system logs -f deployment.apps/cluster-autoscaler
```

- Add an annotation to the deployment with the following command.

```bash
kubectl -n kube-system annotate deployment.apps/cluster-autoscaler cluster-autoscaler.kubernetes.io/safe-to-evict="false"
```

- This command adds an annotation to the Cluster Autoscaler deployment in the kube-system namespace, telling the Kubernetes Cluster Autoscaler not to evict (terminate or reschedule) the pods managed by this deployment during scale-down operations


### Task 3: Simulate CPU Load Inside the Pod Again

- To run a load test on your web application, you can use the following command:

```bash
hey -n 100000 http://<public-worker node-ip>:30001/

hey -n 100000 http://51.44.209.221:30001/
```

- Watch the pods while creating. Show that some pods are pending state

```bash
kubectl get pod -w
```

- Cluster-autoscaler scales out and create one more node

```bash
kubectl get nodes
```

### Task 4: Stop the Load Test and Observe Scale-Down

- While the hey command is running in the terminal,  press **CTRL + C** on your keyboard to interrupt and stop the process. 

- Terminate the background hey process, Find the Process ID (PID)

```bash
ps aux | grep hey
```

- Example output:

```bash
ec2-user   37115  0.0  1.2 710720 12368 pts/3    Sl+  14:04   0:00 hey -n 100000 http://3.254.114.47:30002/
```

- Terminate the process:

```bash
kill 37115
```

- If the process doesn’t stop, send a SIGKILL signal

```bash
kill -9 37115
```

- After stopping the load, use the following commands to watch the changes:

```bash
kubectl get hpa 

kubectl get nodes 
```

##  Part 6: Vertical Pod Autoscaler (VPA) on EKS

- We’ll apply what we learned about VPA (Vertical Pod Autoscaler) by deploying a sample application, applying CPU load, and watching Kubernetes automatically adjust the resource requests of the pods based on actual usage over time.


### Task 1: Install the VPA components

- Delete HPA

```bash
k delete -f hpa.yaml
```

- Clone the official Kubernetes VPA repo:

```bash
sudo yum install git -y
git --version

git clone https://github.com/kubernetes/autoscaler.git
cd autoscaler/vertical-pod-autoscaler/
```

- Run the install script:

```bash
./hack/vpa-up.sh
```

- This deploys; `vpa-recommender`, `vpa-updater`, `vpa-admission-controller`

- Check that the pods are running:

```bash
kubectl get pods -n kube-system | grep vpa
```

### Task 2:  Deploy a test application (Hamster app)

```bash
kubectl apply -f examples/hamster.yaml
kubectl get pods -l app=hamster
```

- View one of the pods to check its initial CPU/Memory requests:

```bash
kubectl describe pod <hamster-pod-name>
```

- Expected output:

```bash
Requests:
  cpu:    100m
  memory: 50Mi
```
### Task 3: Observe VPA in action

- Wait for 1–2 minutes. VPA will analyze resource usage and, if necessary, evict the existing pods and restart them with adjusted resource values

```bash
kubectl get pods -l app=hamster --watch
```

- Then describe the new pod to see updated requests:

```bash
kubectl describe pod <new-hamster-pod-name>
```

- You may see increased values like:

```bash
Requests:
  cpu:    511m
  memory: 250Mi
```

### Task 4: View the VPA Recommendation

```bash
kubectl describe vpa hamster-vpa
```

- Example:

```bash
Recommendation:
  Container Name: hamster
  Target:
    CPU: 587m
    Memory: 250Mi
```

- **Not**: To exclude a container from VPA recommendations:

```yaml
resourcePolicy:
  containerPolicies:
  - containerName: my-logger
    mode: "Off"
```
- This allows full control over which containers are adjusted and which are ignored


##  Part 7: Goldilocks – Kubernetes Resource Optimization Tool

- Goldilocks is an open-source controller that runs in your Kubernetes cluster and provides resource recommendations for deployments. It uses data collected by the Vertical Pod Autoscaler (VPA) to suggest CPU and memory settings that are "just right" — helping you avoid both over-provisioning and under-provisioning.

### Task 1: Install Goldilocks

- Install helm

```bash
curl -fsSL https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
```

- Add Helm repository

```bash
helm repo add fairwinds-stable https://charts.fairwinds.com/stable
helm repo update
```

- Install Goldilocks in its own namespace

```bash
kubectl create ns goldilocks

helm install goldilocks fairwinds-stable/goldilocks --namespace goldilocks
```

- Open a new local terminal

```bash
ssh -i my-key.pem -L 9090:localhost:9090  ec2-user@<Kubectl_EC2_PUBLIC_IP>

ssh -i "Lab-paris-necip.pem" -L 9090:localhost:9090 ec2-user@15.237.109.25
```

- While the SSH connection is active, run the following command inside the EC2 instance:

```bash
kubectl port-forward --namespace goldilocks svc/goldilocks-dashboard 9090:80
```

- Enable the Goldilocks dashboard (If you are working in your local terminal, run this command)

```bash
kubectl port-forward --namespace goldilocks svc/goldilocks-dashboard 9090:80
```

- Then open your browser at: **http://localhost:9090**

### Task 2: Use Goldilocks

- Enable Goldilocks for specific namespaces by adding an label:

```bash
kubectl label namespace <your-namespace> goldilocks.fairwinds.com/enabled=true


kubectl label namespace default goldilocks.fairwinds.com/enabled=true
```

- Replace <your-namespace> with your actual namespace (e.g., default)

- Access http://localhost:9090 after port-forwarding

- Update your deployments with the suggested requests/limits:

```yaml
resources:
  requests:
    cpu: 511m
    memory: 263M
  limits:
    cpu: 511m
    memory: 263M
```

### Task 3: Clean Up

- List Existing VPA Resources

```bash
kubectl get verticalpodautoscaler -A
```

- Remove all existing VPA objects across all namespaces

```bash
kubectl delete verticalpodautoscaler --all --all-namespaces
```

- Use the following command to uninstall all its components:

```bash
cd autoscaler/vertical-pod-autoscaler
./hack/vpa-down.sh
```

- Verify Deletion

```bash
kubectl get pods -n kube-system | grep vpa
kubectl get crd | grep verticalpodautoscalers
```

- Delete hamster deployment

```bash
kubectl delete -f examples/hamster.yaml
```

##  Part 8: Configure and Test Amazon EBS Storage Integration in EKS

- In this part, we will configure and test persistent storage using Amazon EBS with Amazon EKS. 

### Task 1: Install the Amazon EBS CSI Driver Add-on

- Go to EKS --> Your Cluster --> Add-ons

- Click **Get more add-ons**

- Find and install **Amazon EBS CSI Driver**

- Attach an IAM Role with **AmazonEBSCSIDriverPolicy**


### Task 2: Create a gp3 StorageClass 

- Go to terminal and create a file named **storageclass.yaml**:

```yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: gp3
provisioner: ebs.csi.aws.com
volumeBindingMode: WaitForFirstConsumer
reclaimPolicy: Delete
allowVolumeExpansion: true
parameters:
  type: gp3
```

- Apply the manifest:

```bash
kubectl apply -f storageclass.yaml

kubectl get storageclass

NAME   PROVISIONER             RECLAIMPOLICY   VOLUMEBINDINGMODE      ALLOWVOLUMEEXPANSION   AGE
gp2    kubernetes.io/aws-ebs   Delete          WaitForFirstConsumer   false                  60m
gp3    ebs.csi.aws.com         Delete          WaitForFirstConsumer   true                   42m
```

### Task 3: Deploy Pod with PVC 

- Create a file named **ebs-deployment.yaml**:

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: my-ebs-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 5Gi
  storageClassName: gp3
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: my-ebs-deployment
spec:
  replicas: 1
  selector:
    matchLabels:
      app: nginx-ebs
  template:
    metadata:
      labels:
        app: nginx-ebs
    spec:
      containers:
      - name: app
        image: nginx
        volumeMounts:
        - mountPath: "/data"
          name: ebs-volume
      volumes:
      - name: ebs-volume
        persistentVolumeClaim:
          claimName: my-ebs-pvc
```

- Apply the manifest:

```bash
kubectl apply -f ebs-deployment.yaml
```
- This will create the PVC and a Deployment with 1 replica using the EBS volume

- Connect to the Pod and write a file

```bash
kubectl get pod

kubectl exec -it <pod-name> -- sh

echo "hello persistent" > /data/test.txt
```

- Delete the pod

```bash
kubectl delete pod <pod-name>
```
- Kubernetes will automatically recreate the pod due to the Deployment controller

- Connect to the new pod and check the file

```bash
kubectl get pod

kubectl exec -it <pod-name> -- sh

cat /data/test.txt
```

### Task 4: Access the EBS Volume from the Worker Node

- Connect to the Worker Node with SSH

```bash
ssh -i <YourKey.pem> ec2-user@<node-public-ip>

ssh -i "necip-ireland.pem" ec2-user@<node-public-ip>
```

- Kubernetes mounts EBS volumes using the CSI driver under the following path:

```yaml
/var/lib/kubelet/plugins/kubernetes.io/csi/pv/<pvc-id>/mount/
```

- To get the <pvc-id>, run:

```bash
kubectl get pvc my-ebs-pvc

NAME          STATUS   VOLUME                                     ...
my-ebs-pvc    Bound    pvc-1500bd50-a4f7-4f8c-86c3-0aaa607ada8a   ...
```

- Use this value as the folder name in the path above

```bash
cd /var/lib/kubelet/plugins/kubernetes.io/csi/pv/pvc-1500bd50-a4f7-4f8c-86c3-0aaa607ada8a/mount
ls
cat test.txt
```

- You can also update the file directly from the node:

```bash
echo "modified from node" > test.txt
```

- Then, go back to the pod and verify:

```bash
kubectl get pod

kubectl exec -it <pod-name> -- cat /data/test.txt
```

##  Part 9: EKS with Fargate 

- This guide explains how to extend an existing EKS Cluster with Fargate support, deploy a sample application on Fargate, and configure Horizontal Pod Autoscaler (HPA) to scale the pods automatically based on CPU utilization.

### Task 1: Add Fargate Profile to Existing EKS Cluster

- Go to the **EKS** console.
- Click on your existing cluster (e.g., `Lab-Cluster-necip`)
- From the left menu, click **Compute --> Add Fargate Profile**

```text
Fargate profile name            : fargate-profile-<YourName>
Pod execution role              : AmazonEKSFargatePodExecutionRole-<YourName> (create if it doesn't exist)
Subnets                         : Select Private Subnet (CIDR: 172.31.48.0/20)
```

- Click Next to configure namespace and label selectors

```text
Namespace                       : fargate-app
(Optional) Label key            : run
(Optional) Label value          : fargate-demo
```

- Choose subnets (use the same subnets used by your cluster)

- Click **Next**, then **Create**

- **Note**: The profile will take a few minutes to become `ACTIVE`

**Note**: When assigning a private subnet to a Fargate profile, the pods need internet access to pull container images (e.g., nginx, busybox, amazonlinux).

- Therefore:

1. The private subnet must be associated with a route table

2. The route table must have a 0.0.0.0/0 route pointing to a NAT Gateway

3. The NAT Gateway must reside in a public subnet and be associated with an Elastic IP

- Without this setup, Fargate pods in private subnets won't be able to pull images from public registries.


### Task 2: Deploy Application to Fargate Namespace

- Create Namespace

```bash
kubectl create namespace fargate-app
```

- Deploy a sample App to Fargate `fargate-deployment.yaml`

```bash
vi fargate-deployment.yaml
```

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: fargate-demo
  namespace: fargate-app
  labels:
    app: fargate-demo
spec:
  replicas: 1
  selector:
    matchLabels:
      app: fargate-demo
  template:
    metadata:
      labels:
        app: fargate-demo
        run: fargate-demo
    spec:
      containers:
        - name: web
          image: nginx
          ports:
            - containerPort: 80
          resources:
            requests:
              cpu: "100m"
              memory: "128Mi"
            limits:
              cpu: "500m"
              memory: "256Mi"
---
apiVersion: v1
kind: Service
metadata:
  name: fargate-demo-svc
  namespace: fargate-app
spec:
  selector:
    app: fargate-demo
  ports:
    - port: 80
      targetPort: 80
  type: LoadBalancer
```

```bash
kubectl apply -f fargate-deployment.yaml
```

```bash
kubectl get pod -n fargate-app -o wide
```

### Task 3: Enable HPA for the Fargate Deployment

- Create HPA object

```bash
kubectl autoscale deployment fargate-demo \
  --cpu-percent=50 \
  --min=3 \
  --max=5 \
  -n fargate-app
```

- Verify HPA behavior

```bash
kubectl get hpa -n fargate-app
```

- Monitor pods on fargate-demo deployment

```bash
kubectl get pods -n fargate-app -w
```

### Task 4: Clean Up

- Delete deployment, hpa and service

```bash
kubectl delete -f fargate-deployment.yaml

kubectl delete hpa fargate-demo -n fargate-app
```

- Delete **Fargate profiles**
- Delete **NAT gateways**
- Release **Elastic IP addresses**

##  Part 10: Create an Amazon EKS Cluster Using eksctl CLI

- In this part, we will create an Amazon EKS cluster using a single eksctl command, instead of manually configuring the cluster through the AWS Console. This is the CLI equivalent of the custom configuration cluster you previously created via the UI

### Task 1: Create the EKS Cluster

- Run the following eksctl command to create your cluster

**Not**: Replace values like `Lab-EKSCTLCluster-necip`, `my-nodegroup-necip`, `Lab-paris-necip`, and `eu-west-3` with your own cluster name, node group name, SSH key pair, and region as needed


```bash
eksctl -h
```

```bash
eksctl create cluster \
  --name Lab-EKSCTLCluster-necip \
  --version 1.33 \
  --region eu-west-3 \
  --nodegroup-name my-nodegroup-necip \
  --node-type t3.medium \
  --nodes 1 \
  --nodes-min 1 \
  --nodes-max 4 \
  --node-volume-size 20 \
  --ssh-access \
  --ssh-public-key Lab-paris-necip \
  --managed
```

- This process takes approximately 10–15 minutes

- Update your kubeconfig

```bash
aws eks list-clusters

aws eks --region eu-west-3 update-kubeconfig --name Lab-EKSCTLCluster-necip
```

- Check that nodes are ready

```bash
kubectl get nodes

NAME                                           STATUS   ROLES    AGE   VERSION
ip-192-168-xx-xx.eu-west-3.compute.internal    Ready    <none>   2m    v1.31.x
```

- View all kubeconfig contexts on your machine

```bash
kubectl config get-contexts


CURRENT   NAME                                                                 CLUSTER                                                              
          arn:aws:eks:eu-west-3:995194808144:cluster/Lab-Cluster-necip         arn:aws:eks:eu-west-3:995194808144:cluster/Lab-Cluster-necip                 
*         arn:aws:eks:eu-west-3:995194808144:cluster/Lab-EKSCTLCluster-necip   arn:aws:eks:eu-west-3:995194808144:cluster/Lab-EKSCTLCluster-necip 
```

-  Switch to a different context

```bash
kubectl config use-context <ContextName>
```

### Task 2: Delete the Cluster

- To remove the cluster and associated resources:

```bash
eksctl delete cluster --name Lab-EKSCTLCluster-necip --region eu-west-3
```

##  Part 11: Create an Amazon EKS Auto Mode Cluster

- In this part, we will create an Amazon EKS cluster using the Auto Mode option from the AWS Console. Auto Mode enables AWS to automatically manage your cluster’s compute, storage, networking, scaling, and security. We will then deploy a workload and observe how AWS automatically scales nodes in response to pod demand

### Task 1: Create EKS Auto Mode Cluster

- In the search bar, type and select **EKS (Elastic Kubernetes Service)**
- Click on **Create Cluster**
- Select **Quick configuration (with EKS Auto Mode) - new**

```text
Name                                : Lab-AutoModeCluster-<YourName>
Kubernetes version                  : 1.33
Cluster IAM role                    : AmazonEKSAutoClusterRole-<YourName>
VPC                                 : Default
Subnets                             : Select All Subnets
```

- Click `Create`
- The cluster will be ready in a few minutes


### Task 2: Deploy a Sample Application

- Run the command

```bash
aws eks list-clusters
```

```bash
aws eks --region <region_name> update-kubeconfig --name <cluster_name>

aws eks --region eu-west-3 update-kubeconfig --name Lab-AutoModeCluster-necip
``` 

**Not**: The EKS Control Plane ENI’s Security Group must allow inbound traffic on port **443** from your EC2's security group

- We are creating an access entry in the Access tab of the EKS cluster for the IAM role. This step is necessary to allow the IAM principal to authenticate and authorize with the Kubernetes API server.
- **Lab-Cluster-necip** --> **Access** ---> **Create access entry**

```text
IAM principal ARN: admin-<YourName>
Type             : Standart
```

- Click `Next`

```text
Policy name     : AmazonEKSClusterAdminPolicy
Access scope    : Cluster
```

- Click `Add policy`,`Next` and `Create`

- Test connection

```bash
kubectl get nodes
```

- Create and apply a deployment

```bash
vi nginx-auto.yaml
```

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-auto
spec:
  replicas: 1
  selector:
    matchLabels:
      app: nginx-auto
  template:
    metadata:
      labels:
        app: nginx-auto
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
  name: nginx-auto-svc
spec:
  type: NodePort
  selector:
    app: nginx-auto
  ports:
  - port: 80           
    targetPort: 80      
    nodePort: 30001
```

- Apply nginx-auto deployment

```bash
kubectl apply -f nginx-auto.yaml
```

- Watch for node provisioning

```bash
kubectl get pods

kubectl get nodes -w
```

- Increase the replica count to trigger autoscaling

```bash
kubectl scale deployment nginx-auto --replicas=30
```

- Watch pod and node scaling

```bash
watch kubectl get pods
watch kubectl get nodes
```

- You’ll observe; new nodes being created, pending pods becoming running and scaling handled without Cluster Autoscaler or Karpenter

### Task 3:  Clean Up

- Go to EKS --> Clusters --> Lab-AutoModeCluster-<YourName>
- Click Delete cluster



##  Part 12: Deploy the Application Using ECS

- In this part, we will deploy a simple web application using Amazon ECS on AWS Fargate, a serverless container orchestration service

### Task 1: Create an Amazon ECS cluster

- At the top of the AWS Management Console, in the search bar, search for and choose `ECS`

-  From the Amazon ECS left navigation menu, select `Clusters`

- Select `Create cluster`


```text
Cluster name                            : Lab-ECSCluster-<YourName>
Infrastructure - optional               : AWS Fargate (serverless)
```
- Click `Create`


### Task 2: Create a Task Definition

-  From the Amazon ECS left navigation menu, select `Task definitions`

- Select `Create a new task definition`

```text
Task definition family                : Lab-TaskDefinition-<YourName>
Launch type                           : AWS Fargate
Task size
  CPU                                 : .5 CPU
  Memory                              : 1 GB
Container - 1 
  Name                                : nginx
  Image URI                           : nginx
  Essential container                 : Yes
Port mappings
  Container port                      : 80
  Protocol                            : TCP
  Port name                           : lab-tcp-80
  App protocol                        : HTTP
```
- Uncheck the `Use log collection` checkbox

- Click `Create`

### Task 3: Create a Service

- Navigate to the Amazon ECS console and select `Clusters` from the left menu bar

- Select the `Lab-ECSCluster-<YourName>` cluster, select the Services tab then select `Create`

```text
Compute options                     : Capacity provider strategy
Deployment configuration
  Task definition family            : nginx
  Task definition revision          : 1
  Service name                      : Lab-Service-<YourName>
  Service type                      : Replica
  Desired tasks                     : 1
Networking
  VPC                               : Default VPC
  Subnets                           : All Subnets
  Security group                    : Lab-SecGroup-<YourName>
Load balancing 
  Load balancer type                : Application Load Balancer
  Container                         : nginx 80:80
  Application Load Balancer         : Create a new load balancer
  Load balancer name                : Lab-LoadBalancer-<YourName>
```

- Click `Create`

- To view our application from the browser `Clusters` --> `Lab-ECSCluster-<YourName>`--> `Lab-Service-<YourName>` --> `Configuraiton and networking` --> `DNS names`

- Or `Clusters` --> `Lab-ECSCluster-<YourName>`--> `Tasks` --> `Networking` --> `Public IP`

### Task 4: Clean Up

- Delete Clusters
- Deregister Task Definition
- Check Load Balancer



