# Hands-on Kubernetes Lesson 5 : Kubernetes Volumes 

Purpose of this hands-on training is to give participants the knowledge of Kubernetes Volumes.

# Pre-requisites

- **Install Multipass**
  Follow the instructions for your OS here: [https://multipass.run/docs/install-multipass#install]
  - For Mac: brew install multipass
  - For Windows: [https://multipass.run/download/windows]
- Install Helm CLI
  - For windows: 
    - choco install kubernetes-helm
    - scoop install helm
    - winget install Helm.Helm
  - For Mac:
    - brew install helm

## Kubernetes Volume Types

- Part 1 - Kubernetes Volume Ephemeral (Emptydir)

- Part 2 - Kubernetes Volume Persistence (HostPath - Static - Dinamic volume)


## Part 1 - Kubernetes Volume Ephemeral (Emptydir)

- Check if Kubernetes is running and nodes are ready.

```bash
minikube delete
minikube start # with single node 
kubectl get nodes
```
  
### Emptydir
- Create a `pod-emptydir.yaml` file that uses emptydir volume using the following content.

```bash
  mkdir -p Kubernetes/examples/volumes
  cd Kubernetes/examples/volumes
  touch pod-emptydir.yaml
```

- In the pod manifest below, an emptyDir volume is used. The emptyDir volume is for sharing data between containers within the same pod.

  The /cache directory in the frontend pod and the /tmp/log directory in the sidecar pod will share the same volume.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: emptydir
spec:
  containers:
  - name: frontend
    image: sametustaoglu/volume:v1
    ports:
    - containerPort: 80
    livenessProbe:
      httpGet:
        path: /healthcheck
        port: 80
      initialDelaySeconds: 5
      periodSeconds: 5
    volumeMounts:
    - name: cache-vol
      mountPath: /cache
  - name: sidecar
    image: busybox
    command: ["/bin/sh"]
    args: ["-c", "sleep 3600"]
    volumeMounts:
    - name: cache-vol
      mountPath: /tmp/log
  volumes:
  - name: cache-vol
    emptyDir: {}
```

- Create and exec into the emptyDir pod and create some files.

```bash
kubectl apply -f pod-emptydir.yaml
```
- Let's enter the frontend pod with the exec command, create some files, and try to see those files in the sidecar container.

```bash
  kubectl exec -it emptydir -c frontend -- bash
  cd /cache
  touch test{1..3}.txt # create some files
  ls
  cd ..
  ls
  mkdir deneme && cd deneme
  touch test4.pic
  ls
  exit

  kubectl exec -it emptydir -c sidecar -- sh
  cd /tmp/log
  ls # see all test files
  echo "message from sidecar containers" > newfile.txt
  exit
  kubectl exec -it emptydir -c frontend -- bash
  ls /cache
  cat /cache/newfile.txt
  exit
```

- Restart pod with delete healthcheck. Don't delete pod.

```bash
kubectl get pods -w # watch pod from another terminal before restart
kubectl exec emptydir -c frontend -- rm -rf healthcheck
```

- after restart the container, control the folders

```bash
  # control the folders in the sidecar container
  kubectl exec -it emptydir -c sidecar -- sh
  cd /tmp/log
  ls #show the files
  exit
```
- **Question**: If we created a deployment instead of a pod and deleted the pod, would we be able to see the old data in the new pod?

## Part 2 - Kubernetes Persistence Volumes

### Hostpath

- Create a `pod-hosthpath.yaml` file that uses hosthpath volume using the following content.
  In a hostPath volume type, the data is stored on the specified path on the node where the pod is running.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hostpath
spec:
  containers:
  - name: hostpathcontainer
    image: sametustaoglu/volume:v1
    ports:
    - containerPort: 80
    livenessProbe:
      httpGet:
        path: /healthcheck
        port: 80
      initialDelaySeconds: 5
      periodSeconds: 5
    volumeMounts:
    - name: firstvolume
      mountPath: /cache    
  volumes:
  - name: firstvolume
    hostPath:
      path: /tmp/data
      type: DirectoryOrCreate
```

```bash
  kubectl apply -f pod-hosthpath.yaml
  kubectl get pod -o wide # See which node the pod is running on
  minikube ssh --node <node-name> # run this command in side terminal
  cd /tmp/data
  ls # see there is no file
```

- create some files in pods.

```bash
kubectl exec -it hostpath -- bash
cd /cache
ls
touch test{1..3}.txt
ls
exit
```
- check that files are created from host
```bash
minikube ssh --node <node-name>
cd /tmp/data
ls # see new files come from pod
exit
```

- restart and running other node senario 
```bash
kubectl delete pod hostpath
kubectl get po -o wide
kubectl apply -f pod-hosthpath.yaml
kubectl exec -it hostpath -- bash
cd /cache
ls  # see old files come from node path
exit

kubectl delete pod hostpath
minikube node add

kubectl get node
kubectl apply -f pod-hosthpath.yaml
kubectl get po -o wide
kubectl exec -it hostpath -- ls /cache # Verify that you can see old data in the new pod

minikube node delete minikube-m02
kubectl get node
```

- In this part, we'll create an external volume and use it inside Kubernetes. To simulate an external volume, we'll use Multipass. We'll create a VM with Multipass and set up an NFS server on it.

### on nfs-server

- In this section, we will create a VM for NFS and configure it as an NFS server. We'll also allow write access from the 192.168.0.0/16 network block.

```bash
# Create a VM named NFS
multipass launch 20.04 --name nfs --cpus 2 --disk 20G --memory 2G # this command takes some time
multipass shell nfs
# Configure the created VM as nfs-server
sudo apt-get update
sudo apt-get install nfs-kernel-server -y
sudo mkdir -p /data
sudo chown -R nobody:nogroup /data 
sudo chmod 777 /data
sudo vi /etc/exports
  /data 192.168.0.0/16(rw,sync,no_subtree_check,insecure,no_root_squash)   
sudo exportfs -a
sudo systemctl restart nfs-kernel-server
sudo systemctl status nfs-kernel-server
sudo ufw disable
exit
```

- Create a `persistentvolume.yaml` file using the following content. This volume will be created on the NFS server we set up earlier.

```yaml
apiVersion: v1
kind: PersistentVolume
metadata:
  name: first-pv
  labels:
    type: local
spec:
  storageClassName: manual
  capacity:
    storage: 1Gi
  accessModes:
    - ReadWriteOnce
  persistentVolumeReclaimPolicy: Delete
  nfs:
    path: /data
    server: <nfs-ip> # see ip with "multipass list" command
```

- talk about **persistentVolumeReclaimPolicy**: delete, recycle, retain
  - **delete**: The PV is automatically deleted when the PVC is deleted, removing all associated data.
  - **retain**: The PV retains its data even after the PVC is deleted, requiring manual intervention to reuse or clean up the volume.
  - **recycle**: The PV is cleaned by deleting the files and made available for reuse. Note: This policy is deprecated.

- Create the PersistentVolume `first-pv`.

```bash
kubectl apply -f persistentvolume.yaml
```

- View information about the `PersistentVolume` and notice that the `PersistentVolume` has a `STATUS` of available which means it has not been bound yet to a `PersistentVolumeClaim`.

```bash
kubectl get pv
```

- Create a `persistentvolumeclaim.yaml` file using the following content to create a `PersistentVolumeClaim` and explain fields.

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: first-pvc
spec:
  storageClassName: manual
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 1Gi
  selector:
    matchLabels:
      type: local
```

- Create the PersistentVolumeClaim `persistent-volume-claim`.

```bash
kubectl apply -f persistentvolumeclaim.yaml
```

- View information about the `PersistentVolumeClaim` and show that the `PersistentVolumeClaim` is bound to your PersistentVolume `first-pv`.

```bash
kubectl get pv,pvc
```
- View information about the `PersistentVolume` and show that the PersistentVolume `STATUS` changed from Available to `Bound`.

- Create a `persistent-pod.yaml` file that uses your PersistentVolumeClaim as a volume using the following content.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: pod-persistent
  labels:
    app: web 
spec:
  volumes:
    - name: pv-storage
      persistentVolumeClaim:
        claimName: first-pvc
  containers:
    - name: pv-container
      image: nginx
      ports:
        - containerPort: 80
          name: "http-server"
      volumeMounts:
        - mountPath: "/usr/share/nginx/html"
          name: pv-storage
```

- Create the Pod `pod-persistent`.

```bash
kubectl apply -f persistent-pod.yaml
```

- Verify that the Pod is running.

```bash
kubectl get pod
```

- Open a shell to the container running in your Pod. And Change index html from pod in /usr/share/nginx/html path.

```bash
kubectl exec -it pod-persistent -- /bin/bash
curl http://localhost/
cd /usr/share/nginx/html
echo "Welcome to Kubernetes persistence volume lesson" > index.html
cat index.html
curl http://localhost/
```

- Check volume from NFS server

```bash
  multipass shell nfs
  cat /data/index.html
```

### test persistence senario

- delete running pod-persistent then testing reach again the same volume

```bash
kubectl delete pod pod-persistent
kubectl apply -f persistent-pod.yaml
kubectl port-forward pods/pod-persistent 8888:80
# go to localhost:8888 from browser
# change in nfs server path index.html and check again browser 
```

### cleaning

- Delete the `Pod`, the `PersistentVolumeClaim` and the `PersistentVolume`.

```bash
kubectl delete pod pod-persistent
kubectl delete pvc first-pvc
kubectl delete pv first-pv
#or 
kubectl delete -f .
```

### Dinamic Volume

- Nfs server setup is done before section

```bash
  multipass shell nfs
  mkdir -p /data/dynamic
  sudo chown -R nobody:nogroup /data 
```


### on kubernetes setup
 
```bash
minikube ssh
sudo apt update
sudo apt install nfs-common
exit
```

```bash
# instal helm sudo snap install helm 

helm repo add nfs-subdir-external-provisioner https://kubernetes-sigs.github.io/nfs-subdir-external-provisioner/
helm repo update

# change nfs-server-ip
kubectl create namespace nfs
helm install nfs-subdir-external-provisioner nfs-subdir-external-provisioner/nfs-subdir-external-provisioner \
    --set nfs.server=<nfs-server-ip> \
    --set nfs.path=/data/dynamic \
    -n nfs

# for control
- check the pod name nfs-subdir-external-provisioner up and running
kubectl get pod -n nfs
- check the storageclass name nfs-client
kubectl get storageclass
```

- Create a persistent volume claim `dynamic-volume-pvc.yaml` file that uses nfs-client storageclass using the following content.

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: dynamic-volume-pvc
spec:
  accessModes:
    - ReadWriteMany
  storageClassName: nfs-client
  resources:
    requests:
      storage: 1Gi
```

- Create the PersistentVolumeClaim `dynamic-volume-pvc.yaml`.

```bash
  kubectl apply -f dynamic-volume-pvc.yaml
```

- Verify that the dynamic volume has been created and that the dynamic-volume-pvc is in the Bound state.

```bash
  kubectl get pv,pvc
```

- Create a pod "pod-dynamic-volume.yaml" file that uses the dynamic-volume-pvc. 

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: pod-dynamic-volume
spec:
  containers:
  - name: app
    image: alpine
    command: ["/bin/sh"]
    args: ["-c", "while true; do date >> /mnt/data/date.txt; sleep 5; done"]
    volumeMounts:
    - name: nfs
      mountPath: "/mnt/data"
  volumes:
  - name: nfs
    persistentVolumeClaim:
      claimName: dynamic-volume-pvc
```

```bash
kubectl apply -f pod-dynamic-volume.yaml
kubectl get po
```

- check bound volume on nfs server

```bash
multipass shell nfs
cd /data/dynamic
ls
cd <dynamic-volume-folder-name>
cat date.txt or tail -f date.txt
```

- create new file in nfs server and check in the pod path

```bash
touch second.txt
```

- check new file from pod

```bash
kubectl exec -it pod-dynamic-volume -- sh
cd /mnt/data
ls
# check restart pod then verify files
kubectl delete pod pod-dynamic-volume
kubectl apply -f pod-dynamic-volume.yaml

kubectl exec -it pod-dynamic-volume -- sh
cd /mnt/data
ls
```
- cleaning

```bash
kubectl delete -f .
```