# Hands-on Kubernetes Lesson4 : Kubernetes Volumes 

Purpose of this hands-on training is to give participants the knowledge of Kubernetes Volumes.

## Learning Outcomes

At the end of the this hands-on training, students will be able to;

- Explain the need for persistent data management.

- Learn `Persistent Volumes` and `Persistent Volume Claims`.

## Outline

- Part 1 - Kubernetes Volume Ephemeral (Empthydir - Hostpath)

- Part 2 - Kubernetes Volume Persistence (Static - Dinamic volume)


## Part 1 - Kubernetes Volume Ephemeral (Empthydir-Hostpath)


- Launch minikube cluster

- Check if Kubernetes is running and nodes are ready.

```bash
kubectl cluster-info
kubectl get no
```

### Empthydir
- Create a `cmit-pod-empthydir.yaml` file that uses empthydir volume using the following content.

```bash
nano cmit-pod-empthydir.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: emptydir
spec:
  containers:
  - name: frontend
    image: ozgurozturknet/k8s:blue
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

- Create and Log into the emptydir pod and create some files

```bash
kubectl apply -f cmit-pod-empthydir.yaml

kubectl exec -it empthdir -c frontend -- bash
cd /
cd cache

touch test1.txt
touch test2.txt
touch test{1..3}.txt
cd ..
mkdir cmit && cd cmit
touch test3.txt
```

- for restart the container delete healthcheck

```bash
kubectl exec empthdir -c frontend -- rm -rf healthcheck
```

- after restart the container, control the folders

- control the folders in the sidecar container
```bash
kubectl exec -it empthdir -c frontend -- sh
cd /tmp/log
ls

show the files
```

### Hostpath


- Create a `cmit-pod-hosthpath.yaml` file that uses hosthpath volume using the following content.

- Log into the `Minikube-node` node, create a `cache` directory

```bash
minikube ssh
mkdir -p /cache
exit
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hostpath
spec:
  containers:
  - name: hostpathcontainer
    image: ozgurozturknet/k8s:blue
    ports:
    - containerPort: 80
    livenessProbe:
      httpGet:
        path: /healthcheck
        port: 80
      initialDelaySeconds: 5
      periodSeconds: 5
    volumeMounts:
    # - name: directory-vol
    #   mountPath: /dir1
    - name: dircreate-vol
      mountPath: /cache
    # - name: file-vol
    #   mountPath: /cache/config.json       
  volumes:
  # - name: directory-vol
  #   hostPath:
  #     path: /tmp
  #     type: Directory
  - name: dircreate-vol
    hostPath:
      path: /cache
      type: DirectoryOrCreate
  # - name: file-vol
  #   hostPath:
  #     path: /cache/config.json
  #     type: FileOrCreate
```

node üzerinde dizin oluşturulup deneme yapıalcak. pod restart-kontrol
```bash
k exec -it hostpath -- bash
cd /cache
ls

touch test{1..3}.pic

exit
```

```bash
minikube ssh
cd /cache
ls
exit
```
- restart and running other node senario 
```bash
minikube add node
kubectl get node - o wide
kubectl delete pod hostpath
kubectl get po -o wide
kubectl apply -f cmit-pod-hosthpath.yaml
kubectl get po -o wide
```

## Part 2 - Kubernetes Volume Persistence

### on nfs-server

```bash
multipass launch 20.04 --name nfs --cpus 2 --disk 20G --memory 2G 
multipass shell nfs

sudo apt-get update
sudo apt-get install nfs-kernel-server
sudo mkdir -p /mnt/cmit
sudo chown nobody:nogroup /mnt/cmit # ubuntu:ubuntu
sudo chmod 777 /mnt/cmit

sudo nano /etc/exports
  /mnt/cmit <ip-of-minikube-node> (rw,sync,no_subtree_check,insecure,no_root_squash)    # ip.0/24

sudo exportfs -a

sudo systemctl restart nfs-kernel-server
sudo systemctl status nfs-kernel-server
```

- there is a problem on firewall can try "sudo ufw disable"

- Get the documentation of `PersistentVolume` and its fields. Explain the volumes, types of volumes in Kubernetes and how it differs from the Docker volumes. [Volumes in Kubernetes](https://kubernetes.io/docs/concepts/storage/volumes/)

```bash
kubectl explain pv
```

- Log into the `nfs-server` node, create a `cmit` directory under home folder, also create an `index.html` file with `Welcome to Kubernetes persistence volume lesson` text and note down path of the `pv-data` folder.

```bash
multipass shell nfs
cd /mnt/cmit
echo "Welcome to Kubernetes persistence volume lesson" > index.html
ls
pwd
/home/cmit
```

- Log into `minikube-CP` node and create a folder named volume-lessons.

```bash
mkdir volume-lessons && cd volume-lessons
```

- Create a `cmit-pv.yaml` file using the following content with the volume type of `hostPath` to build a `PersistentVolume` and explain fields.

```yaml
apiVersion: v1
kind: PersistentVolume
metadata:
  name: cmit-pv-vol
  labels:
    type: local
spec:
  storageClassName: nfs
  capacity:
    storage: 1Gi
  accessModes:
    - ReadWriteOnce
  # hostPath:
  #   path: "/home/docker/pv-data"
  persistentVolumeReclaimPolicy: Delete
  nfs:
    path: /mnt/cmit
    server: 192.168.64.8
```

- talk about persistentVolumeReclaimPolicy: delete, recycle, retain

- Create the PersistentVolume `cmit-pv-vol`.

```bash
kubectl apply -f cmit-pv.yaml
```

- View information about the `PersistentVolume` and notice that the `PersistentVolume` has a `STATUS` of available which means it has not been bound yet to a `PersistentVolumeClaim`.

```bash
kubectl get pv cmit-pv-vol
```

- Get the documentation of `PersistentVolumeClaim` and its fields.

```bash
kubectl explain pvc
```

- Create a `cmit-pv-claim.yaml` file using the following content to create a `PersistentVolumeClaim` and explain fields.

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: cmit-pv-claim
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 1Gi
  storageClassName: "nfs"
  selector:
    matchLabels:
      type: local
```

- Create the PersistentVolumeClaim `cmit-pv-claim`.

```bash
kubectl apply -f cmit-pv-claim.yaml
```

> After we create the PersistentVolumeClaim, the Kubernetes control plane looks for a PersistentVolume that satisfies the claim's requirements. If the control plane finds a suitable `PersistentVolume` with the same `StorageClass`, it binds the claim to the volume. Look for details at [Persistent Volumes and Claims](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#introduction)

- View information about the `PersistentVolumeClaim` and show that the `PersistentVolumeClaim` is bound to your PersistentVolume `cmit-pv-vol`.

```bash
kubectl get pvc cmit-pv-claim
```

- View information about the `PersistentVolume` and show that the PersistentVolume `STATUS` changed from Available to `Bound`.

```bash
kubectl get pv cmit-pv-vol
```

- Create a `cmit-pod.yaml` file that uses your PersistentVolumeClaim as a volume using the following content.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: cmit-pod
  labels:
    app: cmit-web 
spec:
  volumes:
    - name: cmit-pv-storage
      persistentVolumeClaim:
        claimName: cmit-pv-claim
  containers:
    - name: cmit-pv-container
      image: nginx
      ports:
        - containerPort: 80
          name: "http-server"
      volumeMounts:
        - mountPath: "/usr/share/nginx/html"
          name: cmit-pv-storage
```

- Create the Pod `cmit-pod`.

```bash
kubectl apply -f cmit-pod.yaml
```

- Verify that the Pod is running.

```bash
kubectl get pod cmit-pod
```

- Open a shell to the container running in your Pod.

```bash
kubectl exec -it cmit-pod -- /bin/bash
```

- Verify that `nginx` is serving the `index.html` file from the `hostPath` volume.

```bash
curl http://localhost/
```

- Log into the `ControlPlane` node, change the `index.html`.

```bash
minikube ssh
cd pv-data
echo "Kubernetes Rocks!!!!" > index.html
```

- Log into the `minikube-master` node, check if the change is in effect.

```bash
kubectl exec -it cmit-pod -- /bin/bash
curl http://localhost/
```

- Expose the cmit-pod pod as a new Kubernetes service on master.

```bash
kubectl expose pod cmit-pod --port=80 --type=NodePort
```

- List the services.

```bash
kubectl get svc
for minikube tunnel
minikube service cmit-pod --url
```
- Check the browser (`http://127.0.0.1:TUNNEL_PORT`) that cmit-pod is running.

```bash
for down url 
ctrl + c 
```

### test persistence senario

- delete running cmit-pod then testing reach again the same volume

```bash
kubectl delete po cmit-pod
```

### cleaning

- Delete the `Pod`, the `PersistentVolumeClaim` and the `PersistentVolume`.

```bash
kubectl delete pod cmit-pod
kubectl delete pvc cmit-pv-claim
kubectl delete pv cmit-pv-vol
or 
kubectl delete -f .
```

### Dinamic Volume

- Nfs server setup is done before section

### on kubernetes setup
 
```bash
minikube ssh
sudo apt update
sudo apt install nfs-common
exit
```

```bash
# imstal helm sudo snap install helm 

helm repo add nfs-subdir-external-provisioner https://kubernetes-sigs.github.io/nfs-subdir-external-provisioner/
helm repo update

#change nfs-server-ip
helm install nfs-subdir-external-provisioner nfs-subdir-external-provisioner/nfs-subdir-external-provisioner\
    --set nfs.server=<nfs-server-ip> \
    --set nfs.path=/mnt/cmit

#for control
- check the pod name nfs-subdir-external-provisioner up and running
kubectl get po 
- check the storageclass name nfs-client
kubectl get sc
```

- Create a `cmit-pod-dinamic.yaml` file that uses nfs-client storageclass using the following content.


```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: nfs-pvc
spec:
  accessModes:
    - ReadWriteMany
  storageClassName: nfs-client
  resources:
    requests:
      storage: 1Gi

---

apiVersion: v1
kind: Pod
metadata:
  name: nfs-test-pod
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
      claimName: nfs-pvc
```

```bash
kubectl apply -f cmit-pod-dinamic.yaml

kubectl get pvc
kubectl get po
```

- check bound volume on nfs server

```bash
multipass shell nfs
cd /mntcmit
ls
```

- create new file and check in the pod path

```bash
touch second.txt

# cminikube-controlnode

kubectl exec -it nfs-test-pod -- sh
cd /mnt/data
ls
```