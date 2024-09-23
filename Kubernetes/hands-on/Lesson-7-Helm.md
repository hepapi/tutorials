# Helm

## Introduction

Helm is a package manager for Kubernetes that helps you manage Kubernetes applications.

Helm uses a templating engine to allow users to create reusable and customizable Kubernetes manifests.

## Why Helm?

When working with Kubernetes, it's common to create multiple applications that share similar configurations. However, writing the same Kubernetes manifests for each application can quickly become cumbersome and error-prone.

### Simple Kubernetes App (Manual Manifests)

```bash
  minikube start
  minikube addons enable ingress
```

```bash
  mkdir -p Kubernetes/examples/manifests
  cd Kubernetes/examples/manifests
  touch serviceaccount.yaml deployment.yaml service.yaml ingress.yaml configmap.yaml
```

Here are example YAML manifests for a basic deployment on Kubernetes:

- servisaccount.yaml

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: nginx-serviceaccount
```

- deployment.yaml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-deployment
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
      serviceAccountName: nginx-serviceaccount
      containers:
      - name: nginx
        image: nginx:1.19.2
        ports:
        - containerPort: 80
        env:
        - name: NGINX_PORT
          valueFrom:
            configMapKeyRef:
              name: nginx-config
              key: port
```

- service.yaml

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-service
spec:
  selector:
    app: nginx
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
```

- ingress.yaml

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: nginx-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: nginx.local
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: nginx-service
            port:
              number: 80
```

- configmap.yaml

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: nginx-config
data:
  port: "80"
```

```bash
  kubectl apply -f .
```

- Without Helm, each Kubernetes resource must be manually defined and applied. This can be error-prone and time-consuming for large applications.

### The Drawbacks of Manual YAML Management

- **Repetitiveness**: Writing similar manifests for each application can lead to a lot of repetitive code.
- **Maintenance Overhead**: If you need to update a configuration (like changing the container image), you have to remember to update it in multiple places.
- **Complexity in Scaling**: As you scale your applications or add new ones, managing all these manifests manually becomes increasingly complex.

### How Helm Simplifies Management

Helm is a package manager for Kubernetes that helps address these challenges by allowing you to define, install, and manage Kubernetes applications using a templated approach.

- **Templating**: Helm uses templates to define Kubernetes resources. You can parameterize values such as the name, image, and replica count, making it easy to customize deployments without duplicating code.
- **Release Management**: Helm tracks the version of your deployments, making it easy to roll back to previous versions if needed.
- **Dependency Management**: Helm allows you to define dependencies between different charts (packages of Kubernetes resources).
- **Easier Upgrades and Rollbacks**: With Helm, if something goes wrong, you can roll back to a previous version with minimal hassle.
- **Community Charts**: Helm has a vast ecosystem of community-contributed charts for common applications, and you can leverage existing charts to deploy popular software quickly and easily, rather than starting from scratch.

---

## Installing Helm

Follow the [documentation to install helm](https://helm.sh/docs/intro/install/) on your system.

```bash
# ensure helm is installed
helm version
```

## Helm Chart Requirements

A valid helm chart requires following files:

- `Chart.yaml`: Contains the metadata, an example file:
- `templates/*.yaml`: Actual templates to render
- `values.yaml`: variables for the templates

## Creating our first chart

We can use a prepared template using `helm create <chart-name>` command.

```bash
cd ..
mkdir helm && cd helm

helm create mychart
cd mychart/
```

### Chart.yaml

`Chart.yaml` contains the metadata required.

```yaml
apiVersion: v2
name: mychart
description: A Helm chart for Kubernetes
type: application
version: 0.1.0 # helm chart version
appVersion: "1.16.0"
```

### values.yaml

`values.yaml` contains the variables we use in the template files.

```yaml
replicaCount: 1
image:
  repository: nginx
  pullPolicy: IfNotPresent
  tag: ""
```

`values.yaml` allow us to create one general `templates/` for all our Kubernetes applications. We only require to change the `values.yaml` **for each app**.

### templates/\*.yaml

Helm is written in golang and uses the `text/templates` library to render yaml files.

We use Doubly Curled Braces `{{ }}` to use template functionalities.

**Example of a Simple Deployment Template**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}
spec:
  replicas: {{ .Values.replicaCount }}
  template:
    metadata:
      labels:
        app: {{ .Chart.Name }}
    spec:
      containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
          ports:
            - containerPort: {{ .Values.service.port }}
```

## Deploying Our First Helm Chart

```bash
  cd ..
  touch values.yaml
```
- Create a custom `values.yaml` file:

```yaml
# information for serviceaccount
serviceAccount:
  create: true  
  name: nginx-serviceaccount

# information for deployment
replicaCount: 3

image:
  repository: nginx
  pullPolicy: IfNotPresent
  tag: "1.19.2"

resources:
  requests:
    cpu: 100m
    memory: 128Mi
  limits:
    cpu: 200m
    memory: 256Mi

nodeSelector: {}

# information for service
service:
  type: ClusterIP
  port: 80

# information for ingress
ingress:
  enabled: true  
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
  hosts:
    - host: nginx.local
      paths:
        - path: /
          pathType: Prefix
```

- Install the Helm chart:

```bash
helm install nginx-app ./mychart -f values.yaml
# Verify all resource
kubectl get all
kubectl get sa
kubectl get ingress
minikube tunnel
```

- Add `nginx.local` to /etc/hosts file. (127.0.0.1 nginx.local)

### Upgrading Helm Releases

- Change values.yaml and add "fullnameOverride: nginx"

```bash
helm upgrade --install nginx-app ./mychart -f values.yaml # Check version of release
kubectl get all # See changes of deployment/pod/service names
```

- Change values.yaml and update "tag: latest"

```bash
helm upgrade --install nginx-app ./mychart -f values.yaml
kubectl describe deployments.apps nginx # Verify changes of image tag
```

### Rolling Back a Helm Release

```bash
  helm ls # see all helm release in default namespace
  helm history nginx-app # see all version of nginx-app
  helm rollback nginx-app <version>
  kubectl describe deployments.apps nginx | grep -i image:
```

- Note: Every rollback is a seperate release

### Helm Templates

#### Control Statements

##### if Statements

- The if statement allows conditional rendering in templates.

- Create a `confimap.yaml` file in templates folder.

```yaml
{{- if .Values.configmap.enabled }}
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-configmap
data:
{{ .Values.configmap.data | toYaml | indent 2 }}
{{- end }}
```

- Add the configmap section in `values.yaml`:

```yaml
# information for configmap
configmap:
  enabled: true
  data:
    port: "80"
    env: "dev"
```

- Upgrade the Helm release and check the configmap:

```bash
helm upgrade --install nginx-app ./mychart -f values.yaml
kubectl describe cm nginx-app-configmap # Verify env variable in configmap
```

##### range Statements

The `range` statement is used to iterate over a list or a map.

- Modify the `configmap.yaml` file in template folder

```yaml
{{- if .Values.configmap.enabled }}
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-configmap
data:
{{- range $key, $value := .Values.configmap.data }}
  {{ $key }}: {{ $value | quote }}
{{- end }}
{{- end }}
```

- Add new variables in `values.yaml`:

```yaml
configmap:
  enabled: true
  data:
    port: "80"
    env: "dev"
    loglevel: "info"
```

- Update `deployment.yaml` in the `templates/` folder:

```yaml
          {{- if .Values.configmap.enabled }}     
          envFrom:
          - configMapRef:
              name: {{ .Release.Name }}-configmap
          {{- end }}
```

- Upgrade the Helm release and verify the ConfigMap and environment variables in the pod:

```bash
helm upgrade --install nginx-app ./mychart -f values.yaml
kubectl describe cm nginx-app-configmap # Verify environment variables in the ConfigMap
kubectl get pods
kubectl rollout restart deployment nginx-app-mychart
kubectl exec nginx-<xxxx>-<xxxx> -- printenv # Verify environment variables in the pod
```

#### Functions

##### Built-in Functions

Helm provides several built-in functions for string manipulation, type conversion, and more. Here are a few common functions:

- `quote`: Wraps a string in quotes.
  ```yaml
  image: { { .Values.image.repository | quote } }
  ```
  
- `toYaml`: Converts an object to YAML format.
  ```yaml
  data: { { .Values.config | toYaml | nindent 4 } }
  ```
- `default`: Returns a default value if the provided value is empty.
  ```yaml
  replicas: { { .Values.replicaCount | default 1 } }
  ```
You can find more functions in the [Helm function list documentation](https://helm.sh/docs/chart_template_guide/function_list/)

## Using own Helm repository

### Creating Helm Repository with Nexus

1. Go to [artifacthub.io](artifacthub.io) and search for the nexus3 chart. Select the [nexus3 chart](https://artifacthub.io/packages/helm/stevehipwell/nexus3)

2. Customize the values.yaml for Nexus. Create a `nexus-values.yaml` file:

```yaml
ingress:
  enabled: true
  ingressClassName: "nginx"
  hosts:
  - nexus.local
```

3. Deploy the Helm chart:

```bash
  helm repo add stevehipwell https://stevehipwell.github.io/helm-charts/
  helm install my-nexus3 stevehipwell/nexus3 --version 5.0.0 --values nexus-values.yaml
```

4. Verify nexus pod is ready.

```bash
  kubectl get pod
  kubectl get ingress
```

5. Add nexus.local to your /etc/hosts file (127.0.0.1 nexus.local), then visit nexus.local in your browser. Change the default password and create a Helm hosted repository named helm-repo.

6. Nexus username is admin and get initial password "cat /nexus-data/admin.password" in nexus pod.

7. Change password set admin.

8. Create a hosted helm repo(helm-repo)


### Create a Helm Chart and Push it to the Nexus Helm Repository

1. Package the Helm chart:
```bash
  helm package mychart ./mychart  # Generates a .tgz file
  ls -l
```

2. Push the packaged chart to Nexus:
```bash
  curl -u <username>:<password> http://nexus.local/repository/helm-repo/ --upload-file mychart-0.1.0.tgz -v
```

### Deploy Own Helm Chart From Helm Repository

1. Add the Nexus Helm repository:
```bash
   helm repo add helm-repo http://nexus.local/repository/helm-repo --username admin --password admin
```

2. Install your Helm chart from the Nexus repository:
```bash
  helm delete nginx-app # remove previous helm release
  helm install nginx helm-repo/mychart -f values.yaml
```

## Artifact Hub

[Artifact Hub](https://artifacthub.io/) is a platform for discovering, sharing, and managing Helm charts and other Kubernetes-related artifacts. It saves time by allowing developers to easily find high-quality, community-contributed charts that meet their deployment needs.

### Real World Usage

Let's use the **kube-prometheus-stack** for our example and explore the helm commands.

<https://artifacthub.io/packages/helm/prometheus-community/kube-prometheus-stack>

#### Helm Repositories (`helm repo`)

```bash
# helm repo add [NAME] [URL]
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

- To view Helm chart files, go to ~/Library/Caches/helm/repository (For Macbook)
  <https://helm.sh/docs/helm/helm/>

```bash
# helm repo update [REPO1 [REPO2 ...]]
# This command updates the local cache of chart information.
helm repo update prometheus-community


# helm search repo [keyword]
# This searches the local cache of charts in the added repositories for the kube-prometheus-stack
helm search repo prometheus-community
```

#### Getting helm default Values (`helm show`)

```bash
# helm show readme [CHART]
helm show readme prometheus-community/kube-prometheus-stack
```

```bash
# helm show readme [CHART]
helm show values prometheus-community/kube-prometheus-stack > kube-prometheus-stack-values.yaml

cat kube-prometheus-stack-values.yaml
```

#### Installing a Helm Chart  (`helm install`)

- Edit the `kube-prometheus-stack-values.yaml`. Delete all lines and add below lines.

```yaml
grafana:
  adminPassword: admin-password
  enabled: true
  ingress:
    annotations:
      kubernetes.io/ingress.class: nginx
    enabled: true
    hosts:
    - monitoring.local
    path: /
  persistence:
    accessModes:
    - ReadWriteOnce
    enabled: true
    size: 1Gi
    type: pvc
prometheus:
  prometheusSpec:
    retention: 15d
    storageSpec:
      volumeClaimTemplate:
        spec:
          accessModes:
          - ReadWriteOnce
          resources:
            requests:
              storage: 10Gi
```

Install the chart using the custom values file:

```bash
# helm install [NAME] [CHART]
helm install kube-prometheus-stack \
    -f kube-prometheus-stack-values.yaml \
    --set fullnameOverride=my-kube-prometheus-stack \
    prometheus-community/kube-prometheus-stack
```

#### After install (`helm list`, `helm status`)

```bash
minikube tunnel 
# go to monitoring.local and login with admin-password
# helm status [NAME]
helm status -n default kube-prometheus-stack
```

- Add `monitoring.local` to /etc/hosts file. (127.0.0.1 monitoring.local)

- Visit monitoring.local

```bash
# helm list
helm list -n default

helm list -A  # all namespaces
```

- Delete a release 

```bash
helm delete/uninstall <release-name>
```
#### Using the Dashboard (`helm dashboard`)


```bash
# helm dashboard 
helm plugin install https://github.com/komodorio/helm-dashboard.git 
helm plugin update dashboard
helm dashboard
```

Visit <http://localhost:8080> to use a web-based UI for managing Helm releases.