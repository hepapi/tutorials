# Helm

## Introduction

Helm is a package manager for Kubernetes that helps you manage Kubernetes applications.

Helm uses a templating engine to allow users to create reusable and customizable Kubernetes manifests.

## Why Helm?

When working with Kubernetes, it's common to create multiple applications that share similar configurations. However, writing the same Kubernetes manifests for each application can quickly become cumbersome and error-prone.

### Simple Kubernetes App (Manual Manifests)

Following is a yaml Kubernetes Manifest list to create a very basic deployment on Kubernetes:

```yaml
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: myApplication
---
apiVersion: v1
kind: Service
metadata:
  name: myApplication
spec:
  type: ClusterIP
  ports:
    - port: 80
      targetPort: http
      protocol: TCP
      name: http
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myApplication
spec:
  replicas: 1
  selector:
    matchLabels:
      app.kubernetes.io/name: mychart
      app.kubernetes.io/instance: release-name
  template:
    metadata:
      labels:
        app.kubernetes.io/name: mychart
        app.kubernetes.io/instance: release-name
    spec:
      serviceAccountName: myApplication
      containers:
        - name: mychart
          securityContext: {}
          image: "nginx:1.16.0"
          imagePullPolicy: IfNotPresent
          ports:
            - name: http
              containerPort: 80
              protocol: TCP
```

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
helm
```

## Helm Chart Requirements

A valid helm chart requires following files:

- `Chart.yaml`: Contains the metadata, an example file:
- `templates/*.yaml`: Actual templates to render
- `values.yaml`: variables for the templates

## Creating our first chart

We can use a prepared template using `helm create <chart-name>` command.

```bash
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

### Helm Templates

#### Control Statements

##### if Statements

The if statement allows conditional rendering in templates.

```yaml
{{- if .Values.enabled }}
apiVersion: v1
kind: ConfigMap
metadata:
  name: my-config
data:
  key: "value"
{{- end }}
```

##### range Statements

The range statement is used to iterate over a list or a map.

```yaml
# values.yaml
myEnvironmentVars:
  - key: "envVar1"
    value: "value1"
  - key: "envVar2"
    value: "value2"
  - key: "envVar3"
    value: "value3"
```

```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: example-config
data:
  {{- range .Values.myEnvironmentVars }}
  {{ .key }}: {{ .value | quote }}
  {{- end }}
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

## Artifact Hub

<https://artifacthub.io/>

Artifact Hub is used for discovering, sharing, and managing Helm charts and other Kubernetes-related artifacts.

By using Artifact Hub, developers can easily find high-quality, community-contributed charts that meet their specific needs, saving time and effort in the deployment process.

### Real World Usage

Let's use the **kube-prometheus-stack** for our example and explore the helm commands.

<https://artifacthub.io/packages/helm/prometheus-community/kube-prometheus-stack>

#### Helm Repositories (`helm repo`)

```bash
# helm repo add [NAME] [URL]
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts

# helm repo update [REPO1 [REPO2 ...]]
helm repo update prometheus-community

# helm search repo [keyword]
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

#### Installing helm apps (`helm install`)

```bash
# helm install [NAME] [CHART]
helm install kube-prometheus-stack \
    -f kube-prometheus-stack-values.yaml \
    --set fullnameOverride=my-kube-prometheus-stack \
    prometheus-community/kube-prometheus-stack
```

#### After install (`helm list`, `helm status`)


```bash
# helm status [NAME]
helm status -n default kube-prometheus-stack
```

```bash
# helm list
helm list -n default

helm list -A  # all namespaces
```


#### Using the Dashboard (`helm dashboard`)


```bash
# helm dashboard 
helm dashboard
```

Go to <http://localhost:8080> and you can use all of the above commands as an UI.
