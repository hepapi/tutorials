# Helm

## Introduction

Helm is a package manager for Kubernetes that helps you manage Kubernetes applications.

Helm uses a templating engine to allow users to create reusable and customizable Kubernetes manifests.

## Installing Helm

Follow the [documentation to install helm](https://helm.sh/docs/intro/install/) on your system.

```bash
helm --help
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
myStringList:
    {{- range .Values.myList }}
    - {{ . | quote }}
    {{- end }}    
```

##### with Statements

The with statement allows you to **reduce the scope of the context**.

```yaml
{{- with .Values.service }}
apiVersion: v1
kind: Service
metadata:
  name: {{ .name }}
spec:
  ports:
    - port: {{ .port }}
{{- end }}
```

#### Functions

##### Built-in Functions

Helm provides several built-in functions for string manipulation, type conversion, and more. Here are a few common functions:

- `quote`: Wraps a string in quotes.
    ```yaml
    image: {{ .Values.image.repository | quote }}
    ```
- `toYaml`: Converts an object to YAML format.
    ```yaml
    data: {{ .Values.config | toYaml | nindent 4 }}
    ```
- `default`: Returns a default value if the provided value is empty.
    ```yaml
    replicas: {{ .Values.replicaCount | default 1 }}
    ```

##### Custom Functions

You can also define custom functions in your templates using the `define` keyword.




```yaml
{{- define "my.custom.function" -}}
Hello, {{ . }}!
{{- end -}}

{{ include "my.custom.function" "World" }}
```

















### Artifact Hub Helm Repos

https://artifacthub.io/

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update bitnami


helm search repo bitnami

helm install my-release bitnami/<chart>
```
