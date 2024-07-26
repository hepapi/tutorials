- coredns
- auto complition
- resource kota
- drain cordon node



# kubernetes-egitim
kubectl create secret docker-registry regcred --docker-server=<your-registry-server> --docker-username=<your-name> --docker-password=<your-pword> 


# NFS
helm repo add nfs-subdir-external-provisioner https://kubernetes-sigs.github.io/nfs-subdir-external-provisioner

helm install nfs nfs-subdir-external-provisioner/nfs-subdir-external-provisioner \
--set nfs.server=172.31.21.127 --set nfs.path=/mnt/nfs/shared_data --set storageClass.name=nfs-sc \
--set storageClass.provisionerName=k8s-sigs.io/second-nfs-subdir-external-provisioner