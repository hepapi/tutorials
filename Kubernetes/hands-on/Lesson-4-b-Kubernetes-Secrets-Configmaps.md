# Kubernetes Secrets and ConfigMaps Hands-On

In this session, we'll explore using environment variables directly, with ConfigMaps, and with Secrets.

## Quick Comparison

| Feature | ConfigMap | Secret |
| --- | --- | --- |
| Main purpose | Non-sensitive configuration | Sensitive data such as passwords, tokens, and certificates |
| Data storage | Plain text | Base64-encoded values in manifests and API output |
| Volume mount | Yes | Yes |
| Typical examples | Port, environment, log level | Password, API token, TLS key |
| Size limit | 1 MiB | 1 MiB |

Important:

- Base64 is not encryption. It is only an encoding format.
- Both ConfigMaps and Secrets can be injected into Pods as environment variables or mounted files.

## Mental Model

- A `ConfigMap` or `Secret` stores data in Kubernetes.
- A Pod consumes that data in one of two common ways:
- As environment variables
- As files mounted into the container filesystem

## Prerequisites

- A running Kubernetes cluster
- `kubectl` configured to talk to that cluster
- `nginx` image pullable from your cluster

## Table of Contents

- [Part 1 - Deploying an Application Without Secrets/ConfigMaps](#part-1---deploying-an-application-without-secretsconfigmaps)
- [Part 2 - Using ConfigMaps for Configuration](#part-2---using-configmaps-for-configuration)
- [Part 3 - Using Secrets for Sensitive Data](#part-3---using-secrets-for-sensitive-data)
- [Part 4 - ConfigMaps as Volumes](#part-4---configmaps-as-volumes)
- [Part 5 - Secrets as Volumes](#part-5---secrets-as-volumes)
- [Part 6 - Secret Types](#part-6---secret-types)

## Part 1 - Deploying an Application Without Secrets/ConfigMaps

Create a simple Pod with two environment variables defined directly in `pod-with-simple-env.yaml`:

```bash
vi pod-with-simple-env.yaml
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

Cleanup:

```bash
kubectl delete pod simple-env-pod
```

## Part 2 - Using ConfigMaps for Configuration

### Create ConfigMap from Literal Values

```bash
kubectl create configmap --help
kubectl create configmap imperative-configmap --from-literal=LOGGING=info --from-literal=LANGUAGE=python
kubectl get configmap
kubectl describe configmap imperative-configmap
kubectl get configmap imperative-configmap -o yaml
```

### Create ConfigMap with Declarative Format

- Create a `configmap.yaml` file

```bash
vi configmap.yaml
```

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  ENV: "Dev"
  ANOTHER_CONFIG_VAR: "another-config-value"
```

- Apply the `configmap.yaml` file

```bash
kubectl apply -f configmap.yaml
kubectl get configmap
kubectl get configmap app-config -o yaml
```

### Create a pod using the ConfigMap

- Create a `pod-with-configmap.yaml` file

```bash
vi pod-with-configmap.yaml
```


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


- Create a `second-configmap-pod.yaml` file

```bash
vi second-configmap-pod.yaml
```


```yaml
apiVersion: v1
kind: Pod
metadata:
  name: second-configmap-pod
spec:
  containers:
  - name: second-configmap-env-container
    image: nginx
    env:
    - name: ENVIRONMENT
      valueFrom:
        configMapKeyRef:
          name: app-config
          key: ENV
```

- Apply `second-configmap-pod.yaml` file

```bash
kubectl apply -f second-configmap-pod.yaml
```

- Check the environment variables:

```bash
kubectl exec -it second-configmap-pod -- env | grep ENVIRONMENT
kubectl exec -it second-configmap-pod -- env | grep LOGGING
```


## Part 3 - Using Secrets for Sensitive Data

### Create Secret from files

```bash
echo -n "passwd123" > PASSWORD
kubectl create secret --help
kubectl create secret generic --help
kubectl create secret generic app-secret-file --from-file=PASSWORD # You will get the same result when you create it as below.
# kubectl create secret generic app-secret-file --from-literal=PASSWORD=passwd123
```

Important:

- With `--from-file=PASSWORD`, Kubernetes uses the filename `PASSWORD` as the secret key.
- That is why `key: PASSWORD` works in the Pod manifest below.

```bash
kubectl get secret
kubectl describe secret app-secret-file
kubectl get secret app-secret-file -o yaml
echo -n "cGFzc3dkMTIz" | base64 -d
```


### Create Secret with Declarative Format


If you want to generate the base64 value manually:

```bash
echo -n "secret-value" | base64
```

- Create a `secret.yaml` file

```bash
vi secret.yaml
```


```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secret
type: Opaque
data:
  SECRET_ENV_VAR: c2VjcmV0LXZhbHVl # "secret-value" encoded in base64
```

You can also use `stringData` instead of `data`. In that case, Kubernetes will do the base64 conversion for you:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secret
type: Opaque
stringData:
  SECRET_ENV_VAR: secret-value
```

- Apply the `secret.yaml` file

```bash
kubectl apply -f secret.yaml
kubectl describe secret app-secret
kubectl get secret app-secret -o yaml
```

### Create a Pod Using the Secret

- Create a `pod-with-secret.yaml` file

```bash
vi pod-with-secret.yaml
```

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
kubectl apply -f pod-with-secret.yaml
```

- Check the environment variables:

```bash
kubectl get pod
kubectl exec -it secret-env-pod -- env | grep SECRET_ENV_VAR
kubectl exec -it secret-env-pod -- env | grep PASSWORD
```

### Use All Secret or Configmap Values in Pod

- Create a `pod-with-all-secret-cm.yaml` file

```bash
vi pod-with-all-secret-cm.yaml
```


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
kubectl apply -f pod-with-all-secret-cm.yaml
```

- Check the environment variables:

```bash
kubectl get pod
kubectl exec -it pod-with-all-secret-cm -- env | grep SECRET_ENV_VAR
kubectl exec -it pod-with-all-secret-cm -- env | grep ENV
kubectl exec -it pod-with-all-secret-cm -- env | grep ANOTHER_CONFIG_VAR
```

Note:

- This example expects the secret named `app-secret` to contain the key `SECRET_ENV_VAR`.
- If you recreate the secret in a different way, keep the same secret name and key so the Pod manifest still matches.

## Part 4 - ConfigMaps as Volumes

### Using ConfigMap as a Volume

Sometimes, configuration files are better suited as volumes. Let's use the ConfigMap to create a custom NGINX configuration file.

- Create a `configmap-nginx-conf.yaml` file

```bash
vi configmap-nginx-conf.yaml
```

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: nginx-config
data:
  NGINX_ENV: "production"
  nginx.conf: |
    events {
      worker_connections 1024;
    }
    http {
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
    }
```

- Apply the `configmap-nginx-conf.yaml` file

```bash
kubectl apply -f configmap-nginx-conf.yaml
```

```bash
kubectl get configmap
kubectl get configmap nginx-config -o yaml
```

### Create a Deployment Using the Configmap as Volume

- Create a `deployment-with-cm-volume.yaml` file

```bash
vi deployment-with-cm-volume.yaml
```

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

Important note:

- This example uses `subPath`, which is useful because it mounts only the single file instead of replacing the whole `/etc/nginx` directory.
- But when `subPath` is used, updating the ConfigMap does not automatically refresh that file inside an already running Pod.
- To see the new content, restart the Pod or roll the Deployment.

```bash
kubectl rollout restart deployment nginx
```

## Part 5 - Secrets as Volumes

### Using Secret as a Volume

Sometimes secret values are better mounted as files instead of environment variables.

- Create a `secret-volume.yaml` file

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secret-volume
type: Opaque
stringData:
  username: admin
  password: passwd123
---
apiVersion: v1
kind: Pod
metadata:
  name: secret-volume-pod
spec:
  containers:
  - name: secret-volume-container
    image: nginx
    volumeMounts:
    - name: secret-volume
      mountPath: /etc/secret-data
      readOnly: true
  volumes:
  - name: secret-volume
    secret:
      secretName: app-secret-volume
```

- Apply the file

```bash
kubectl apply -f secret-volume.yaml
```

- Verify the mounted files

```bash
kubectl exec -it secret-volume-pod -- ls /etc/secret-data
kubectl exec -it secret-volume-pod -- cat /etc/secret-data/username
kubectl exec -it secret-volume-pod -- cat /etc/secret-data/password
```

## Part 6 - Secret Types

### Docker Registry Secrets

#### Create a Docker Registry Secret

- Try to run pod with private docker image

- Create a file `private-registry-pod.yaml`

```bash
vi private-registry-pod.yaml
```

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: private-registry-pod
spec:
  containers:
    - name: private-registry-container
      image: necipulusoyy9120/private-repo:demo
      imagePullPolicy: Always
```

- Apply the file `private-registry-pod.yaml`

```bash
kubectl apply -f private-registry-pod.yaml
kubectl get pod # Check if the pod is running.
kubectl describe pod private-registry-pod # see "pull access denied" message from events
```

- Create a docker-registry secret

For Docker Hub, use your Docker Hub username and password or access token:

```bash
kubectl create secret --help
kubectl create secret docker-registry --help
```

```bash
kubectl create secret docker-registry docker-registry-secret \
  --docker-username=<dockerhub-username> \
  --docker-password=<dockerhub-password-or-token> \
  --docker-server=https://index.docker.io/v1/

kubectl get secret # check secret types
```

Harbor private registry:

```bash
kubectl create secret docker-registry harbor-registry-secret \
  --docker-username=<harbor-username> \
  --docker-password=<harbor-password> \
  --docker-server=https://harbor.example.com
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
      image: necipulusoyy9120/private-repo:demo
      imagePullPolicy: Always
  imagePullSecrets:
    - name: docker-registry-secret
```

- Apply the file `private-registry-pod.yaml`

```bash
kubectl delete -f private-registry-pod.yaml
kubectl apply -f private-registry-pod.yaml
kubectl get pod # check pod is running
kubectl describe po private-registry-pod
```

- Expected result:

- In the first version, image pull should fail because the image is private.
- After adding `imagePullSecrets`, the Pod should start successfully if the credentials are correct.

Private image examples:

Docker Hub private image:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: private-nginx-pod
spec:
  containers:
    - name: private-nginx
      image: necipulusoyy9120/private-repo:demo
      imagePullPolicy: Always
  imagePullSecrets:
    - name: docker-registry-secret
```

## TLS Secrets

TLS secrets are commonly used by:

- `Ingress` resources to terminate HTTPS traffic
- applications that need a certificate and private key mounted into the container

The most common classroom example is an `Ingress` using a TLS secret.

- Create a TLS secret manifest `tls-secret.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: tls-secret-example
type: kubernetes.io/tls
data:
  tls.crt: <base64-encoded-cert>
  tls.key: <base64-encoded-key>
```

- Create a TLS secret with imperative way

```bash
kubectl create secret --help
kubectl create secret tls --help
kubectl create secret tls tls-secret --cert=path/to/tls.crt --key=path/to/tls.key
```

### Small TLS Hands-On

Use either the declarative example above or the imperative command below. If you use both, make sure the secret names are different.

If you want a minimal local example, create a self-signed certificate first:

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout tls.key -out tls.crt -subj "/CN=example.local"
kubectl create secret tls tls-secret --cert=tls.crt --key=tls.key
kubectl get secret tls-secret
kubectl describe secret tls-secret
```

### Common Use Case: Ingress TLS

This example assumes:

- you already have the `nginx` Deployment from Part 4
- your cluster has an Ingress controller installed

Create a Service for the `nginx` Deployment:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-svc
spec:
  selector:
    app: nginx
  ports:
  - port: 80
    targetPort: 80
```

Then create an Ingress using the TLS secret:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: nginx-ingress-tls
spec:
  tls:
  - hosts:
    - example.local
    secretName: tls-secret
  rules:
  - host: example.local
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: nginx-svc
            port:
              number: 80
```

Apply them:

```bash
kubectl apply -f nginx-svc.yaml
kubectl apply -f nginx-ingress-tls.yaml
kubectl get ingress
kubectl describe ingress nginx-ingress-tls
```

Main point:

- The `tls-secret` holds the certificate and private key.
- The `Ingress` references that secret with `secretName`.
- HTTPS traffic is terminated at the Ingress layer using that certificate.

Example reference:

- [Rancher TLS Secret Example](https://ranchermanager.docs.rancher.com/getting-started/installation-and-upgrade/resources/add-tls-secrets)

## Final Cleanup

```bash
kubectl delete -f .
kubectl delete secret app-secret-file docker-registry-secret
kubectl delete configmap imperative-configmap
rm -f PASSWORD tls.key tls.crt
```
