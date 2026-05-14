# Helm

## Introduction

Helm is a package manager for Kubernetes that helps you manage Kubernetes applications.

Helm uses a templating engine to allow users to create reusable and customizable Kubernetes manifests.

## Table of Contents

- [Why Helm?](#why-helm)
- [Installing Helm](#installing-helm)
- [Helm Chart Requirements](#helm-chart-requirements)
- [Minimal ConfigMap Release Example](#minimal-configmap-release-example)
- [Useful First Commands](#useful-first-commands)
- [Helm Templates](#helm-templates)
- [Using the Existing Chart for Our Manual App](#using-the-existing-chart-for-our-manual-app)
- [Deploying the Helm Version of Our Manual App](#deploying-the-helm-version-of-our-manual-app)
- [Hosting a Private Helm Repository with Nexus](#hosting-a-private-helm-repository-with-nexus)
- [Artifact Hub](#artifact-hub)
- [Final Cleanup](#final-cleanup)

## Why Helm?

When working with Kubernetes, it's common to create multiple applications that share similar configurations. However, writing the same Kubernetes manifests for each application can quickly become cumbersome and error-prone.

### Simple Kubernetes App (Manual Manifests)

```bash
minikube start
minikube addons enable ingress
```

```bash
mkdir manual-manifests
cd manual-manifests
touch deployment.yaml serviceaccount.yaml  service.yaml ingress.yaml configmap.yaml
```

Here are example YAML manifests for a basic deployment on Kubernetes. In this repository, the same files are also available under `manual-manifests/` so you can run them directly during the lesson:

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

- serviceaccount.yaml

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: nginx-serviceaccount
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
kubectl delete -f .
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

Before looking at the required files, it helps to map Helm concepts to something more familiar from the container world:

| Helm              | Docker            | Meaning |
| ------------------| ------------------| ------------------------------------------------------- |
| `Helm repository` | `Docker registry` | A place to store and distribute packages                |
| `Chart`           | `Image`           | A reusable package definition                           |
| `Release`         | `Container`       | A running or deployed instance created from that package|

In short: a **Helm chart** relates to a **Helm release** in a similar way that a **Docker image** relates to a **container**.

You can also summarize the Helm rendering model like this:

```text
Template folder + values.yaml = rendered Kubernetes manifests
```

## Helm Chart Requirements

A valid helm chart requires following files:

- `Chart.yaml`: Contains the metadata, an example file:
- `templates/*.yaml`: Actual templates to render
- `values.yaml`: variables for the templates
- `.helmignore`: Optional, but useful to keep unnecessary local files out of the packaged chart

Example `Chart.yaml`:

```yaml
apiVersion: v2
name: demo-chart
description: A Helm chart for nginx
type: application
version: 0.1.0
appVersion: "1.19.2"
```

## Minimal ConfigMap Release Example

This is a good first Helm exercise because it shows the core idea with the smallest possible chart: put Kubernetes manifests under `templates/`, then let Helm render and install them as a release.

First create a minimal chart:

```bash
cd ..
helm --help
helm create <chart_name>
helm create demo-chart
```

`helm create` generates several default files. For this first example, we want to keep the chart as small as possible, so remove the default template files and keep only one file under `templates/`:

```bash
rm -rf demo-chart/templates/*
touch demo-chart/templates/configmap.yaml
```

For this first example, we only care about `demo-chart/templates/configmap.yaml`.

Update `demo-chart/templates/configmap.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: demo-chart-config
data:
  myvalue: "Hello World"
```

At this point, if you check the cluster, the ConfigMap does not exist yet:

```bash
kubectl get cm
```

Now install the chart as a release:

```bash
helm install <release_name> ./demo-chart
helm install helm-demo ./demo-chart
```

- `helm-demo` is the release name.
- `./demo-chart` is the chart directory.
- `helm install` is the step that renders the files under `templates/` and creates the Kubernetes objects.

Verify the release and the created ConfigMap:

```bash
helm ls
kubectl get cm
kubectl get cm demo-chart-config -o yaml
```

Remove the release:

```bash
helm ls
helm uninstall helm-demo
helm ls
kubectl get cm
```

When the release is removed, the objects created by that release are removed as well.

### Moving Values Into `values.yaml`

This is the natural continuation of the same example: keep the chart generic and place user-editable values in `values.yaml`.

Now update `demo-chart/values.yaml`:

```yaml
course: Kubernetes
```

Then update `demo-chart/templates/configmap.yaml` again:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: demo-chart-config
data:
  myvalue: "Hello World"
  course: {{ .Values.course }}
```

This means:

- `.` refers to the current template context.
- `.Values` reads from `values.yaml`.
- `.Values.course` gets the `course` value from `values.yaml`.

Preview the rendered manifests without creating any objects:

```bash
helm install --debug --dry-run mydryrun ./demo-chart
```

This is useful because:

- `--dry-run` simulates the install without creating resources.
- `--debug` prints more detail, including the rendered manifests.

If the output looks correct, run a real install:

```bash
helm install myvalue ./demo-chart
kubectl get cm
kubectl get cm demo-chart-config -o yaml
helm ls
helm get manifest myvalue
```

- `helm get manifest myvalue` shows the final rendered Kubernetes manifests that belong to the `myvalue` release.
- This is useful when you want to see exactly what Helm installed into the cluster.

### Overriding Values From the Command Line

Helm values from the command line override the same key in `values.yaml`.

```bash
helm install --debug --dry-run setflag ./demo-chart --set course=observability
```

In this example, `course` becomes `observability` even if `values.yaml` says `Kubernetes`.

### Using Built-In Objects

Helm also provides built-in objects such as `.Release.Name` and `.Chart.Name`.

Update `demo-chart/templates/configmap.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-config
data:
  myvalue: "Hello World"
  chart: {{ .Chart.Name | quote }}
  course: {{ .Values.course }}
  release-name: {{ .Release.Name }}
```

Now the ConfigMap name changes based on both the release name and the chart name:

```bash
helm install --debug --dry-run builtin-object ./demo-chart
```

If the release name is `builtin-object` and the chart name is `demo-chart`, the ConfigMap name becomes `builtin-object-demo-chart-config`.

### Using Template Functions

You can also transform values with Helm template functions.

Set `demo-chart/values.yaml` to:

```yaml
course: Kubernetes
lesson:
  topic: templating
```

Then update `demo-chart/templates/configmap.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-config
data:
  myvalue: "Hello World"
  chart: {{ quote .Chart.Name }}
  course: {{ quote .Values.course }}
  release-name: {{ quote .Release.Name }}
  topic: {{ upper .Values.lesson.topic }}
  time: {{ now | date "2006.01.02" | quote }}
```

This example shows four useful functions and one built-in chart value:

- `.Chart.Name` gives the current chart name.
- `quote` wraps the value in double quotes.
- `upper` converts a string to uppercase.
- `now | date "2006.01.02"` prints the current date in a formatted way.

Preview it:

```bash
helm install --debug --dry-run morevalues ./demo-chart
```

If you used `time: {{ now }}` instead, Helm would print the full timestamp rather than the formatted date.

## Useful First Commands

Before installing a chart into the cluster, these commands are very useful:

Current directory: project root

```bash
# check chart structure and basic validity
helm lint ./demo-chart

# render templates locally without applying them
helm template nginx-app ./demo-chart -f demo-chart/values.yaml

# simulate an install and print the rendered manifests
helm install nginx-app ./demo-chart -f demo-chart/values.yaml --dry-run --debug
```

- `helm template` is the simplest way to render YAML locally.
- `helm install --dry-run --debug` is closer to a real install flow and also shows install-style debug output.
- In newer Helm versions, `--dry-run=client` keeps the preview local, while `--dry-run=server` performs a server-side simulation.

Tip: `.helmignore` works like `.gitignore`. It helps prevent files such as `.git/`, editor files, or local test files from being included in the chart package.

### Intentional Mistakes For Demo

If you want to show how Helm errors look, it is better to demonstrate mistakes inside `values.yaml` and `templates/configmap.yaml`.

#### Example 1: Broken `values.yaml`

Temporarily change `demo-chart/values.yaml` to an invalid YAML structure:

```yaml
course Kubernetes
lesson:
  topic: templating
```

Then run:

```bash
helm lint ./demo-chart  

helm template nginx-app ./demo-chart -f demo-chart/values.yaml
```

This is useful because you can see that Helm fails before rendering when the values file itself is invalid YAML.

#### Example 2: Broken `templates/configmap.yaml`

Temporarily change `demo-chart/templates/configmap.yaml` to an invalid template:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: demo-chart-config
data:
  myvalue: "demo chart configmap example"
  course: {{ .Values.course
```

Then run:

```bash
helm lint ./demo-chart

helm template nginx-app ./demo-chart -f demo-chart/values.yaml
```

This is useful because you can see the difference between:

- a YAML problem in `values.yaml`
- a template problem in `templates/configmap.yaml`

After the demo, restore both files to the working versions before continuing.

## Helm Templates

The examples in this section are for understanding Helm template syntax. You do not need to run them yet. Later, you can apply the same ideas inside `mychart`.

The `configmap.yaml` examples below are standalone syntax demos. They build on one another to explain `if`, `range`, and `with`. Later, when we create `mychart`, we keep its real `templates/configmap.yaml` simpler on purpose.

### Named Templates With `_helpers.tpl`

`_helpers.tpl` is optional. Our current `mychart/` does not use it because the chart is intentionally kept simple for learning.

If you later want to avoid repeating name logic like:

```yaml
{{ .Release.Name }}-{{ .Chart.Name }}
```

you can move that logic into `templates/_helpers.tpl` and call it with `include`. For this introductory chart, plain inline names are easier to follow.

Example:

`templates/_helpers.tpl`

```yaml
{{- define "mychart.fullname" -}}
{{- printf "%s-%s" .Release.Name .Chart.Name -}}
{{- end -}}
```

Then use it in another template:

```yaml
metadata:
  name: {{ include "mychart.fullname" . }}
```

### Control Statements

#### if Statements

- The if statement allows conditional rendering in templates.

- Create a `configmap.yaml` file in templates folder.

```yaml
{{- if .Values.configmap.enabled }}
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-configmap
data:
{{- toYaml .Values.configmap.data | nindent 2 }}
{{- end }}
```

Immediately after the `if` block:

- If `configmap.enabled` is `false`, Helm skips the whole file and no ConfigMap is rendered.

- Add the configmap section in `values.yaml`:

```yaml
# information for configmap
configmap:
  enabled: true
  data:
    port: "80"
    env: "dev"
```

```bash
helm template nginx-app ./demo-chart -f demo-chart/values.yaml
```

#### range Statements

The `range` statement is used to iterate over a list or a map.

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-config
data:
  myvalue: "Hello World"
  chart: {{ quote .Chart.Name }}
```

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

- Continue with the same `values.yaml` and add one more key:

```yaml
configmap:
  enabled: true
  data:
    port: "80"
    env: "dev"
    loglevel: "info"
```

```bash
helm template nginx-app ./demo-chart -f demo-chart/values.yaml
```

- Update `deployment.yaml` in the `templates/` folder:

```yaml
          {{- if .Values.configmap.enabled }}     
          envFrom:
          - configMapRef:
              name: {{ .Release.Name }}-configmap
          {{- end }}
```

- `kubectl rollout restart` is often needed in real deployments because changing the ConfigMap alone does not automatically refresh environment variables inside already running containers.

Example:

```bash
kubectl rollout restart deployment nginx-app-mychart
```

#### with Statements

The `with` statement helps you work with a nested object more cleanly.

Normally, you may need to repeat the full path of a nested value many times:

Example `values.yaml`:

```yaml
nodeSelector:
  disktype: ssd
  zone: east
```

```yaml
disktype: {{ .Values.nodeSelector.disktype }}
zone: {{ .Values.nodeSelector.zone }}
```

This works, but it becomes repetitive.

`with` says: "for this block, treat this nested object as the current `.` context."

Example:

```yaml
{{- with .Values.nodeSelector }}
disktype: {{ .disktype }}
zone: {{ .zone }}
{{- end }}
```

Inside the `with` block:

- before `with`, `.` is the whole chart context
- after `{{- with .Values.nodeSelector }}`, `.` becomes `.Values.nodeSelector`
- that is why `.Values.nodeSelector.disktype` becomes just `.disktype`
- and `.Values.nodeSelector.zone` becomes just `.zone`


Now look at the more practical form:

```yaml
{{- with .Values.nodeSelector }}
nodeSelector:
  {{- toYaml . | nindent 2 }}
{{- end }}
```

If `nodeSelector` is empty, this block is skipped. If it has values, they are rendered under `nodeSelector:`.


Here, inside the block, `.` is already the `nodeSelector` object itself.

So:

```yaml
toYaml .
```

renders this:

```yaml
disktype: ssd
zone: east
```

Rendered result:

```yaml
nodeSelector:
  disktype: ssd
  zone: east
```

`with` is also used later in our `mychart/templates/deployment.yaml`.

### Functions

#### Built-in Functions

Helm provides several built-in functions for string manipulation, type conversion, and more. Here are a few common functions:

- `quote`: Wraps a string in quotes.
  ```yaml
  image: {{ .Values.image.repository | quote }}
  ```
  
- `toYaml`: Converts an object to YAML format.

  ```yaml
  data:
{{ .Values.config | toYaml | nindent 4 }}
  ```

- `default`: Returns a default value if the provided value is empty.

  ```yaml
  replicas: {{ .Values.replicaCount | default 1 }}
  ```
You can find more functions in the [Helm function list documentation](https://helm.sh/docs/chart_template_guide/function_list/)

## Using the Existing Chart for Our Manual App

Now we will create a second chart for the same nginx application that we first deployed with plain Kubernetes YAML files.

Delete `myvalue` release:

```bash
helm uninstall myvalue
```

Start by creating the chart:

```bash
helm create mychart
```

`helm create` generates default files under `templates/`. For this lesson, remove them and create only the files we need:

```bash
rm -rf mychart/templates/*
touch mychart/templates/serviceaccount.yaml mychart/templates/deployment.yaml mychart/templates/service.yaml mychart/templates/ingress.yaml mychart/templates/configmap.yaml
```

Also update `mychart/values.yaml` with the values we will use for the nginx app:

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

# information for configmap
config:
  data:
    port: "80"

env:
  name: NGINX_PORT
  configMapKey: port

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

Now write the Helm versions of our Kubernetes files under `mychart/templates/`.

`mychart/templates/serviceaccount.yaml`

```yaml
{{- if .Values.serviceAccount.create }}
apiVersion: v1
kind: ServiceAccount
metadata:
  name: {{ .Values.serviceAccount.name }}
{{- end }}
```

`mychart/templates/configmap.yaml`

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-config
data:
{{- range $key, $value := .Values.config.data }}
  {{ $key }}: {{ $value | quote }}
{{- end }}
```

`mychart/templates/service.yaml`

```yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-service
spec:
  type: {{ .Values.service.type }}
  selector:
    app: {{ .Chart.Name }}
  ports:
    - protocol: TCP
      port: {{ .Values.service.port }}
      targetPort: {{ .Values.service.port }}
```

`mychart/templates/deployment.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}
spec:
  replicas: {{ .Values.replicaCount }}
  selector:
    matchLabels:
      app: {{ .Chart.Name }}
  template:
    metadata:
      labels:
        app: {{ .Chart.Name }}
    spec:
      serviceAccountName: {{ .Values.serviceAccount.name }}
      containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - containerPort: {{ .Values.service.port }}
          env:
            - name: {{ .Values.env.name }}
              valueFrom:
                configMapKeyRef:
                  name: {{ .Release.Name }}-{{ .Chart.Name }}-config
                  key: {{ .Values.env.configMapKey }}
          {{- with .Values.resources }}
          resources:
            {{- toYaml . | nindent 12 }}
          {{- end }}
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
```

- `nindent` means "start on a new line and indent by N spaces".
- For example, `nindent 12` is used here so the rendered YAML stays aligned correctly under the `resources:` block.
- This matters because YAML structure depends on whitespace.

`mychart/templates/ingress.yaml`

```yaml
{{- if .Values.ingress.enabled }}
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: {{ .Release.Name }}-{{ .Chart.Name }}-ingress
  annotations:
    {{- toYaml .Values.ingress.annotations | nindent 4 }}
spec:
  rules:
    {{- range .Values.ingress.hosts }}
    - host: {{ .host }}
      http:
        paths:
          {{- range .paths }}
          - path: {{ .path }}
            pathType: {{ .pathType }}
            backend:
              service:
                name: {{ $.Release.Name }}-{{ $.Chart.Name }}-service
                port:
                  number: {{ $.Values.service.port }}
          {{- end }}
    {{- end }}
{{- end }}
```

- Inside `range`, the `.` value changes and starts pointing to the current item in the loop.
- That is why the template uses `$` in places like `$.Release.Name` and `$.Chart.Name`.
- `$` lets you reach the root context even when you are inside a nested loop.

## Deploying the Helm Version of Our Manual App

Current directory: project root

```bash
helm lint ./mychart

helm template nginx-app ./mychart

helm template nginx-app ./mychart --show-only templates/deployment.yaml
```

- This command renders the final Kubernetes YAML locally so we can inspect it before creating anything in the cluster.

- Install the Helm chart:

```bash
helm install nginx-app ./mychart
helm list
# Verify all resource
kubectl get all
kubectl get sa
kubectl get cm
kubectl get ingress
```

- This chart renders these files under `mychart/templates/`:
  `serviceaccount.yaml`, `configmap.yaml`, `service.yaml`, `deployment.yaml`, `ingress.yaml`

- If you are using Minikube with the ingress addon, first check the ingress address:

```bash
kubectl get ingress
```

- Then map `nginx.local` to the `ADDRESS` value shown in that output:

```text
<INGRESS_ADDRESS> nginx.local
```

- In some local setups `127.0.0.1` may work, but the safest rule is:
  use the actual ingress `ADDRESS` shown by `kubectl get ingress`.

- If the ingress address stays pending in Minikube, open another terminal and run:

```bash
minikube tunnel
```

- Keep that terminal open while testing the ingress.
- On some systems, `minikube tunnel` may ask for your password.

- Then access the application through the ingress host.

- **Namespace hands-on:**

This is one of Helm's biggest advantages: you can deploy the same chart side by side for different environments.

Create small override files for each environment first.

```bash
touch mychart/staging-values.yaml
```

`mychart/staging-values.yaml`

```yaml
# information for serviceaccount
serviceAccount:
  create: true
  name: nginx-serviceaccount

# information for deployment
replicaCount: 2

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

config:
  data:
    port: "80"

env:
  name: NGINX_PORT
  configMapKey: port

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
    - host: staging.nginx.local
      paths:
        - path: /
          pathType: Prefix
```

```bash
touch mychart/prod-values.yaml
```

`mychart/prod-values.yaml`

```yaml
# information for serviceaccount
serviceAccount:
  create: true
  name: nginx-serviceaccount

# information for deployment
replicaCount: 4

image:
  repository: nginx
  pullPolicy: IfNotPresent
  tag: "1.19.2"

resources:
  requests:
    cpu: 200m
    memory: 256Mi
  limits:
    cpu: 400m
    memory: 512Mi

nodeSelector: {}

config:
  data:
    port: "80"

env:
  name: NGINX_PORT
  configMapKey: port

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
    - host: prod.nginx.local
      paths:
        - path: /
          pathType: Prefix
```

Now install the same chart into two namespaces with slightly different values:

```bash
kubectl create namespace staging
kubectl create namespace prod

# alternatively, Helm can create the namespace during install:
# helm install nginx-app-staging ./mychart -f mychart/staging-values.yaml -n staging --create-namespace
# helm install nginx-app-prod ./mychart -f mychart/prod-values.yaml -n prod --create-namespace

helm install nginx-app-staging ./mychart -f mychart/staging-values.yaml -n staging
helm install nginx-app-prod ./mychart -f mychart/prod-values.yaml -n prod

helm list -n staging
helm list -n prod
kubectl get all -n staging
kubectl get all -n prod
```

In this example:

- `staging` uses 2 replicas and smaller resource limits
- `prod` uses 4 replicas and larger resource limits
- each environment has its own ingress host

If you are testing with Minikube ingress locally, also add these hostnames to `/etc/hosts`:

```text
<INGRESS_ADDRESS> staging.nginx.local
<INGRESS_ADDRESS> prod.nginx.local
```

Get that address with:

```bash
kubectl get ingress -A
```

### Upgrading Helm Releases

After the first install, you can update the existing release with `helm upgrade`.

For example, keep the same chart and change only the replica count from the command line:

```bash
# alternatively, use upgrade --install if you want Helm to install the release when it does not exist
# helm upgrade --install nginx-app ./mychart -f mychart/values.yaml --set replicaCount=2

kubectl get deployment nginx-app-mychart
helm upgrade nginx-app ./mychart -f mychart/values.yaml --set replicaCount=2
kubectl get deployment nginx-app-mychart
```

This updates the current `nginx-app` release instead of creating a new one.


### Previewing Changes Before Upgrade With `helm diff`

Before upgrading a release, it is a good habit to preview what will change.

```bash
helm plugin install https://github.com/databus23/helm-diff
helm diff upgrade nginx-app ./mychart -f mychart/values.yaml --set replicaCount=5
```

If you get an error like `plugin source does not support verification`, install it like this instead:

```bash
helm plugin install https://github.com/databus23/helm-diff --verify=false
```

This is especially useful in production because you can review the changes before applying them.

- `+` green lines show what will be added.
- `-` red lines show what will be removed or changed.
- Note: `helm diff` is a plugin, so it must be installed first on each local machine or CI runner where you want to use it.

### Debugging Your Charts

You do not need to install a chart into the cluster to catch many template problems.

**Important:** `helm template` only renders YAML text. It does not fully validate Kubernetes field types.
So a bad value like `replicaCount=invalid_integer` may still render, because Helm can print that text even though Kubernetes will reject it later.

```bash
# render only the deployment template so the output is easier to read
helm template nginx-app ./mychart --show-only templates/deployment.yaml

# now inject a wrong value on purpose
helm template nginx-app ./mychart --show-only templates/deployment.yaml --set replicaCount=invalid_integer
```

In that output, this line is the problem:

```yaml
replicas: invalid_integer
```

Helm may still render it, but Kubernetes expects `replicas` to be an integer.

If you want Kubernetes to validate the rendered YAML before a real install, run:

```bash
helm template nginx-app ./mychart --show-only templates/deployment.yaml --set replicaCount=invalid_integer | kubectl create --dry-run=server -f -
```

That is the command that should show the real validation error.

Use this habit before real installs or upgrades so you can catch rendering problems early.

### Atomic Installs & Upgrades

If you want Helm to wait for resources and automatically roll back on failure, use `--wait` and `--rollback-on-failure`.

```bash
helm upgrade --install nginx-app ./mychart -f mychart/values.yaml \
  --set image.tag=not-found-image \
  --wait \
  --rollback-on-failure \
  --timeout 1m
```

This is a good production habit because a failed rollout will not leave the release half-finished.

After the failed test, Helm should roll back automatically. Check the release status and history:

```bash
helm status nginx-app
helm history nginx-app
```

### Inspecting a Release

```bash
helm list
helm status nginx-app
helm get values nginx-app # show values.yaml
helm get manifest nginx-app # show real render k8s values
```

### Rolling Back a Helm Release

```bash
helm ls # see all helm release in default namespace
helm history nginx-app # see all version of nginx-app
helm rollback nginx-app <version>
kubectl describe deployment nginx-app-mychart | grep -i image:
```

- Note: Every rollback is a separate release

## Hosting a Private Helm Repository with Nexus

### Creating Helm Repository with Nexus

1. Go to [artifacthub.io](https://artifacthub.io) and search for the nexus3 chart. Select the [nexus3 chart](https://artifacthub.io/packages/helm/stevehipwell/nexus3)

2. Customize the values.yaml for Nexus. Create a `nexus-values.yaml` file:

```bash
touch nexus-values.yaml
```

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
helm repo update stevehipwell
helm install my-nexus3 stevehipwell/nexus3 --version 5.21.0 --values nexus-values.yaml
```

4. Verify nexus pod is ready.

```bash
kubectl get pod
kubectl get ingress
```

5. Add `nexus.local` to your `/etc/hosts` file using the ingress `ADDRESS` shown by `kubectl get ingress`.
   Example: `<INGRESS_ADDRESS> nexus.local`
   Then visit `nexus.local` in your browser, change the default password, and create a Helm hosted repository named `helm-repo`.

6. Nexus username is `admin`. To get the initial password, run:

```bash
kubectl get pod
kubectl exec -it <nexus-pod-name> -- cat /nexus-data/admin.password
kubectl exec -it my-nexus3-0 -- cat /nexus-data/admin.password
```

- After you log in and change the admin password, Nexus removes this file for security reasons.

7. Log in with the `admin` user, change the initial password, and complete the first-login setup screens in the Nexus UI.

8. In the Nexus UI, create a hosted Helm repository named `helm-repo` using the `helm (hosted)` recipe.


### Create a Helm Chart and Push it to the Nexus Helm Repository

Return to the project root first:

```bash
cd ..
```

1. Package the Helm chart:
```bash
helm package ./mychart  # Generates a .tgz file
ls -l
```

2. Push the packaged chart to Nexus:
```bash
curl -u <username>:<password> http://nexus.local/repository/helm-repo/ --upload-file mychart-0.1.0.tgz -v
curl -u admin:admin1234 http://nexus.local/repository/helm-repo/ --upload-file mychart-0.1.0.tgz -v
```
- On macOS, you can inspect the local Helm repository cache under `~/Library/Caches/helm/repository`

```bash
ls ~/Library/Caches/helm/repository
```
### Deploy Own Helm Chart From Helm Repository

1. Add the Nexus Helm repository:
```bash
helm repo add helm-repo http://nexus.local/repository/helm-repo --username admin --password admin1234
helm repo ls
```

2. Install your Helm chart from the Nexus repository:
```bash
helm uninstall nginx-app # remove previous helm release

helm install <release_name> <repo_name>/<chart_name> -f <values_file>
helm install nginx helm-repo/mychart -f mychart/values.yaml
```

## Artifact Hub

[Artifact Hub](https://artifacthub.io/) is a platform for discovering, sharing, and managing Helm charts and other Kubernetes-related artifacts. It saves time by allowing developers to easily find high-quality, community-contributed charts that meet their deployment needs.

### Real World Usage

Let's use the **kube-prometheus-stack** for our example and explore the helm commands.

[kube-prometheus-stack on Artifact Hub](https://artifacthub.io/packages/helm/prometheus-community/kube-prometheus-stack)

#### Helm Repositories (`helm repo`)

```bash
# helm repo add [NAME] [URL]
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

- On macOS, you can inspect the local Helm repository cache under `~/Library/Caches/helm/repository`
  See the [Helm command reference](https://helm.sh/docs/helm/helm/).

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

Use `helm show readme` to quickly inspect chart-specific installation notes before deploying it.

```bash
# helm show values [CHART]
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

- Add `monitoring.local` to `/etc/hosts` using the ingress `ADDRESS` shown by `kubectl get ingress`.
  Example: `<INGRESS_ADDRESS> monitoring.local`

- Visit monitoring.local

```bash
# helm list
helm list -n default

helm list -A  # all namespaces
```

- Delete a release

```bash
helm uninstall <release-name>
```

#### NOTES.txt

When you run `helm install`, Helm may print extra usage text after the release is created. That output usually comes from `templates/NOTES.txt`.

You can inspect it in your chart:

```bash
cat templates/NOTES.txt
```

This is useful for printing URLs, usernames, or follow-up commands after installation.

#### Using the Dashboard (`helm dashboard`)


```bash
# helm dashboard 
helm plugin install https://github.com/komodorio/helm-dashboard.git --verify=false
helm plugin update dashboard
helm dashboard
```

This opens a local web UI to inspect releases, rendered manifests, and values.

## Final Cleanup

```bash
helm uninstall nginx-app --ignore-not-found
helm uninstall nginx --ignore-not-found
helm uninstall kube-prometheus-stack --ignore-not-found
helm uninstall my-nexus3 --ignore-not-found
helm repo remove prometheus-community helm-repo stevehipwell 2>/dev/null || true
kubectl delete ingress nginx-ingress --ignore-not-found
kubectl delete service nginx-service --ignore-not-found
kubectl delete serviceaccount nginx-serviceaccount --ignore-not-found
kubectl delete configmap nginx-config --ignore-not-found
kubectl delete namespace staging --ignore-not-found
kubectl delete namespace prod --ignore-not-found
rm -rf demo-chart kube-prometheus-stack-values.yaml mychart nexus-values.yaml manual-manifests mychart-0.1.0.tgz
```
