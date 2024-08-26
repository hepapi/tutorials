# Kubernetes Secrets and ConfigMaps Hands-On

In this session, we'll explore using environment variables directly, with ConfigMaps, and with Secrets.

- Part 1 - Deploying an Application Without Secrets/ConfigMaps

- Part 2 - Using ConfigMaps for Configuration

- Part 3 - Using Secrets for Sensitive Data

- Part 4 - ConfigMaps as Volumes

- Part 5 - Secrets as Volumes

- Part 6 - Secret Types

## Part 1 - Deploying an Application Without Secrets/ConfigMaps

Create a simple deployment with two environment variables defined directly as `simple-env-pod.yaml`:

```bash
  mkdir -p Kubernetes/examples/secret-configmap
  cd Kubernetes/examples/secret-configmap
  touch pod-with-simple-env.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: simple-env-pod
spec:
  containers:
    - name: simple-env-container
      image: nginx
      env:
        - name: DATABASE_NAME
          value: "name-of-db"
        - name: DATABASE_PASSWORD
          value: "pass123"
```

- Apply the `pod-with-simple-env.yaml` file

```bash
kubectl apply -f pod-with-simple-env.yaml
```

- Check the environment variables:

```bash
kubectl exec -it simple-env-pod -- env | grep DATABASE_NAME
kubectl exec -it simple-env-pod -- env | grep DATABASE_PASSWORD
```

## Part 2 - Using ConfigMaps for Configuration

### Create ConfigMap from Literal Values

```bash
    kubectl create configmap imperative-configmap --from-literal=LOGGING=info --from-literal=LANGUAGE=python
    kubectl get configmap imperative-configmap -o yaml 
```

### Create Configmap with Declerative Format

- Create a `configmap.yaml` file

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  ENV: "dev"
  ANOTHER_CONFIG_VAR: "another-config-value"
```

- Apply the `configmap.yaml` file

```bash
kubectl apply -f configmap.yaml
kubectl get configmap app-config -o yaml
```

### Create a pod using the ConfigMap

- Create a `pod-with-configmap.yaml` file

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: configmap-env-pod
spec:
  containers:
    - name: configmap-env-container
        image: nginx
        env:
        - name: ENVIRONMENT
            valueFrom:
            configMapKeyRef:
                name: app-config
                key: ENV
        - name: LOGGING
            valueFrom:
            configMapKeyRef:
                name: imperative-configmap
                key: LOGGING
```

- Apply `pod-with-configmap.yaml` file

```bash
  kubectl apply -f pod-with-configmap.yaml
```

- Check the environment variables:

```bash
    kubectl exec -it configmap-env-pod -- env | grep ENVIRONMENT
    kubectl exec -it configmap-env-pod -- env | grep LOGGING
```

## Part 3 - Using Secrets for Sensitive Data

### Create Secret from files

```bash
    echo -e "PASSWORD=passwd123" > secret.txt
    kubectl create secret generic app-secret-file --from-file=secret.txt
```

### Create Secret with Declerative Format

- create a `secret.yaml` file

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secret
type: Opaque
data:
  SECRET_ENV_VAR: c2VjcmV0LXZhbHVl # "secret-value" encoded in base64
```

- apply the `configmap.yaml` file

```bash
  kubectl apply -f configmap.yaml
```

### Create a Pod Using the Secret

- create a `pod-with-secret.yaml` file

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: secret-env-pod
spec:
  containers:
    - name: secret-env-container
      image: nginx
      env:
        - name: SECRET_ENV_VAR
          valueFrom:
            secretKeyRef:
              name: app-secret
              key: SECRET_ENV_VAR
        - name: PASSWORD
          valueFrom:
            secretKeyRef:
              name: app-secret-file
              key: PASSWORD
```

- Apply the `pod-with-secret.yaml` file

```bash
  kubeclt apply -f pod-with-secret.yaml
```

- Check the environment variables:

```bash
    kubectl exec -it secret-env-pod -- env | grep SECRET_ENV_VAR
    kubectl exec -it secret-env-pod -- env | grep PASSWORD
```

### Use All Secret or Configmap Values in Pod

- create a `pod-with-all-secret-cm.yaml` file

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: pod-with-all-secret-cm
spec:
  containers:
    - name: config-secret-container
      image: nginx
      envFrom:
        - configMapRef:
            name: app-config
        - secretRef:
            name: app-secret
```

- Apply the `pod-with-all-secret-cm.yaml` file

```bash
  kubeclt apply -f pod-with-all-secret-cm.yaml
```

- Check the environment variables:

```bash
    kubectl exec -it pod-with-all-secret-cm -- env | grep SECRET_ENV_VAR
    kubectl exec -it pod-with-all-secret-cm -- env | grep ENV
```

## Part 4 - ConfigMaps as Volumes

### Using ConfigMap as a Volume

Sometimes, configuration files are better suited as volumes. Let's use the ConfigMap to create a custom NGINX configuration file.

- Create a `configmap-nginx-conf.yaml` file

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: nginx-config
data:
  NGINX_ENV: "production"
  nginx.conf: |
    server {
      listen 80;
      server_name localhost;

      location / {
        root /usr/share/nginx/html;
        index index.html;
      }

      error_page 500 502 503 504 /50x.html;
      location = /50x.html {
        root /usr/share/nginx/html;
      }
    }
```

- Apply the `configmap-nginx-conf.yaml` file

```bash
  kubectl apply -f configmap-nginx-conf.yaml
```

### Create a Deployment Using the Configmap as Volume

- Create a `deployment-with-cm-volume.yaml` file

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
        env:
          - name: NGINX_ENV
            valueFrom:
              configMapKeyRef:
                name: nginx-config
                key: NGINX_ENV
        volumeMounts:
          - name: nginx-config-volume
            mountPath: /etc/nginx/nginx.conf
            subPath: nginx.conf
      volumes:
        - name: nginx-config-volume
          configMap:
            name: nginx-config
            items:
              - key: nginx.conf
                path: nginx.conf
```

- Apply the `deployment-with-cm-volume.yaml` file

```bash
  kubectl apply -f deployment-with-cm-volume.yaml
```

- Verify that the NGINX configuration is loaded:

```bash
    kubectl get pods
    kubectl exec -it <nginx-pod-name> -- cat /etc/nginx/nginx.conf
```

## Part 6 - Secret Types

### Docker Registry Secrets

#### Create a Docker Registry Secret

- Try to run pod with private docker image

- Create a file `private-registry-pod.yaml`

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: private-registry-pod
spec:
  containers:
    - name: private-registry-container
      image: sametustaoglu/sample-app:latest # use private docker image
```

- Apply the file `private-registry-pod.yaml`

```bash
  kubectl apply -f private-registry-pod.yaml
```

- Check if the pod is running. 

- Create a docker-registry secret

```bash
  kubectl create secret docker-registry docker-registry-secret --docker-username=<username> --docker-password=<password>  --docker-server=<server>
```

#### Use a Docker Registry Secret While Pulling Image From Private Image Registry

- Edit the file `private-registry-pod.yaml`

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: private-registry-pod
spec:
  containers:
    - name: private-registry-container
      image: sametustaoglu/sample-app:latest # use private docker image
  imagePullSecrets:
    - name: docker-registry-secret
```

- Apply the file `private-registry-pod.yaml`

```bash
  kubectl apply -f private-registry-pod.yaml
```

- Check if the pod is running. 

## TLS Secrets

- Create a TLS secret manifest `tls-secret.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: tls-secret
type: kubernetes.io/tls
data:
  tls.crt: <base64-encoded-cert>
  tls.key: <base64-encoded-key>
```

- Create a TLS secret with imperative way

```bash
  kubectl create secret tls tls-secret --cert=path/to/tls.crt --key=path/to/tls.key
```