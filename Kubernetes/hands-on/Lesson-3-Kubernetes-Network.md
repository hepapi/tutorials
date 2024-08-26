# Kubernetes Network Hands-On

# ClusterIP

ClusterIP Services assign an IP address that can be used to reach the Service from within your cluster. This type doesn’t expose the Service externally.

## Hands-On

### Deploy the sample app

```bash
  mkdir -p Kubernetes/examples/kubernetes-network/service
  cd Kubernetes/examples/kubernetes-network/service
```

First, copy the following Deployment manifest and save it as `app.yaml` in your working directory:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx
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
        image: nginx:latest
        ports:
          - containerPort: 80
```
The manifest deploys three replicas of the nginx:latest container image. In the metadata.labels field, an app: nginx label is applied—this will be referenced by your Services in the following steps. The ports.containerPort field within the Pod spec template is used to indicate that the Pods will be exposing port 80, the default NGINX web server port.
 
- Use Kubectl to apply the Deployment manifest to your cluster:

```bash
kubectl apply -f app.yaml
```


### Create a ClusterIP Service

Now your NGINX deployment is running, but you don’t have a way to access it. Although you could directly connect to the Pods, this doesn’t load balance and will lead to errors if one of the Pods becomes unhealthy or is replaced. Creating a Service allows you to route traffic between the replicas so you can reliably access the Deployment.

- The following manifest defines a simple ClusterIP service. Save it as `clusterip.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-clusterip
spec:
  type: ClusterIP # default value is clusterip
  selector:
    app: nginx
  ports:
    - port: 8080
      targetPort: 80
```


There are a few points to note in the manifest:

- The spec.type field is set to ClusterIP as we’re creating a ClusterIP service.
- The spec.selector field selects the NGINX Pods using the app: nginx label applied in the Deployment’s manifest.
- The spec.ports field specifies that traffic to port 8080 on the Service’s Cluster IP address will be routed to port 80 at your Pods.

Save the manifest as clusterip.yaml, then add it to your cluster

```bash
kubectl apply -f clusterip.yaml
```

Next, use kubectl get services command to discover the cluster IP address that’s been assigned to the Service

```bash
kubectl get services
kubectl describe service/nginx-clusterip
```

In this example, the service has the IP address <service-ip-address> You can now connect to this IP from within your cluster in order to reach your NGINX Deployment, with automatic load balancing between your three Pod replicas.

#### Create New Pod For Testing
To check test create pod that is using nginx image

```bash
kubectl create namespace test
kubectl run test-pod --image=nginx -n test
kubectl exec -it -n test test-pod -- bash
curl http://<cluster-ip-service-ip>:8080
curl nginx-clusterip.default.svc.cluster.local:8080
# check dns resolution
apt update && apt install dnsutils -y
nslookup nginx-clusterip.default # see service ip address
```
#### Understanding Label and Selector Better

- Create a new pod with same label but different name. Copy the following Pod manifest and save it as `custom-pod.yaml` in your working directory:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: uvey-nginx
  labels:
    app: nginx #same label
spec:
  containers:
    - name: nginx
      image: nginx:latest
      ports:
        - containerPort: 80
```

```bash
  kubectl apply -f custom-pod.yaml
  kubectl describe service/nginx-clusterip
  # Endpoints is a Kubernetes object that represents the resources behind a service. Its abbreviation is ep.
  kubectl get endpoints 
  kubectl describe endpoints nginx-clusterip
```

### Create a NodePort Service

Let’s externally expose the Deployment using a NodePort Service. Specify type: NodePort instead of type: ClusterIP in the manifest and use the ports. A NodePort exposes an app to external traffic by opening a specific port on all nodes, with a port range of 30000 to 32767. If not specified, Kubernetes assigns a port within this range automatically.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-nodeport
spec:
  type: NodePort
  selector:
    app: nginx
  ports:
    - port: 80
      nodePort: 32000
```

Save the manifest as `nodeport.yaml` and use kubectl to apply it.

```bash
kubectl apply -f nodeport.yaml
```

- Check Node Port

```bash
kubectl get service
```

In this demo, we’re using a local Minikube cluster with no external IP address available. So to access the service externally as a NodePort, we'll use the tunnel feature provided by Minikube

- Run service tunnel

```bash
minikube service nginx-nodeport --url
```

- Try in your browser open in your browser

```bash
http://127.0.0.1:TUNNEL_PORT
```

### Create a LoadBalancer Service

The simplest LoadBalancer Service looks very similar to ClusterIP Services

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-lb
spec:
  type: LoadBalancer
  selector:
    app: nginx
  ports:
    - port: 9090
      targetPort: 80
```

Adding this Service to your cluster will attempt to use the configured load balancer integration to provision a new infrastructure component. If you created your cluster from a managed cloud service, this should result in a load balancer resource being added to your cloud account.

Save the manifest as `loadbalancer.yaml`, then apply it with kubectl

```bash
kubectl apply -f loadbalancer.yaml
```

- minikube tunnel runs as a process, creating a network route on the host to the service CIDR of the cluster using the cluster’s IP address as a gateway. The tunnel command exposes the external IP directly to any program running on the host operating system.

```bash
minikube tunnel
http://REPLACE_WITH_EXTERNAL_IP:9090
```

## Ingress

Now install ingress-controller by enabling it in minikube as follow

```bash
minikube addons enable ingress
```

Let’s check the newly created namespace ingress-nginx

```bash
kubectl get pods -n ingress-nginx
```

- Create `web-service.yaml` file and copy below code to inside

```bash
  cd ..
  mkdir ingress && cd ingress
```

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web-service
  namespace: default
spec:
  replicas: 3
  selector:
    matchLabels:
      app: web-service
  template:
    metadata:
      labels:
        app: web-service
    spec:
      containers:
        - name: web-service
          image: ersinsari/web-service
          ports:
            - containerPort: 80
---
apiVersion: v1
kind: Service
metadata:
  name: web-service
  namespace: default
spec:
  ports:
    - name: http
      port: 80
      targetPort: 80
      protocol: TCP
  type: NodePort
  selector:
    app: web-service
```

```bash
kubectl apply -f web-service.yaml
```

- Create `inventory-service.yaml` file and copy below code to inside

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: inventory-service
  namespace: default
spec:
  selector:
    matchLabels:
      app: inventory-service
  template:
    metadata:
      labels:
        app: inventory-service
    spec:
      containers:
        - name: inventory-service
          image: ersinsari/inventory-service
          ports:
            - containerPort: 80
---
apiVersion: v1
kind: Service
metadata:
  name: inventory-service
  namespace: default
spec:
  ports:
    - name: http
      port: 80
      targetPort: 80
      protocol: TCP
  type: NodePort
  selector:
    app: inventory-service
```

Now let’s deploy and expose a hello world application

```bash
kubectl apply -f inventory-service.yaml
```

- Create `ingress.yaml` file and copy below code.


```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: example-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  ingressClassName: nginx
  rules:
    - host: ingress-edu.test
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: web-service
                port:
                  number: 80
          - path: /inventory
            pathType: Prefix
            backend:
              service:
                name: inventory-service
                port:
                  number: 80
```

Now let’s create an ingress with single rule to publish the application in public

```bash
kubectl apply -f ingress.yaml
```

wait till the ingress is read and has an ddress assigned to it

```bash
kubectl get ingress
```

- Go to local host file and add dns name your host file

```bash
127.0.0.1 ingress-edu.test
```

```bash
minikube tunnel
```

- Go to web browser and enter  ingress-edu.test dns name see web-service ingress-edu.test/inventory and see inventory-service.

