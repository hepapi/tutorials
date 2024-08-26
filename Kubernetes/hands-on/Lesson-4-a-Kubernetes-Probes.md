
## liveness probe

Monitor container health and restart containers as needed to maintain application availability.

Kubernetes offers HTTP GET, TCP socket, gRPC, and liveness command probe types, each catering to specific use cases and application requirements. If the liveness probe fails, Kubernetes will automatically restart the container, allowing the application to recover from a faulty state.

### HTTP GET liveness probe

An HTTP GET liveness probe is a common choice to determine container health when your container exposes a web service or an HTTP endpoint. The probe uses an HTTP GET request to the specified endpoint, where the container is considered healthy if the response has a successful HTTP status code (between 200 and 399)

We have created a Docker image to test the liveness probe. The pod running this image performs checks on the specified path every 3 seconds. The pod's behavior is configured to return a success status for the first 30 seconds, followed by a failure status for the next 30 seconds. During the failure period, the pod will be restarted as the liveness probe detects the failure
```bash
  mkdir -p Kubernetes/examples/kubernetes-network/probes
  cd Kubernetes/examples/kubernetes-network/probes
```

- Create liveness-pod-http.yaml and copy below code.

```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    test: liveness
  name: liveness-http
spec:
  containers:
  - name: liveness
    image: ersinsari/liveness-http
    livenessProbe:
      httpGet:
        path: /health
        port: 80
      initialDelaySeconds: 10 # default 0
      failureThreshold: 1 # default 3
      periodSeconds: 3 # default 10
---
apiVersion: v1
kind: Service   
metadata:
  name: liveness-svc
spec:  
  ports:
  - port: 80
    targetPort: 80
  selector:
    test: liveness
```

- In the configuration file, you can see that the Pod has a single container. 

- The `periodSeconds` field specifies that the kubelet should perform a `liveness probe every 3 seconds`. 

- The `initialDelaySeconds` field tells the kubelet that it should `wait 10 seconds before performing the first probe`. 

- To perform a probe, the kubelet sends an `HTTP GET request` to the server that is running in the container and listening on port 80. If the handler for the server's `/health` path returns a success code, the kubelet considers the container to be alive and healthy. If the handler returns a failure code, the kubelet kills the container and restarts it.

- Any code `greater than or equal to 200 and less than 400` indicates `success`. Any other code indicates `failure`.

- Check liveness-http state is running and enter below code your terminal for accessing app ui.


```bash
kubectl apply -f liveness-pod-http.yaml
kubectl port-forward svc/liveness-svc 8080:80
```

- open another terminal and inspect liveness-http state 

```bash
kubectl get pod -w
```

- The liveness probe is configured such that every failure will cause the pod to be restarted.

### exec liveness probe

- Another kind of liveness probe that executes the command in the contianer.

- Create a `liveness-exec.yaml` and input text below.

```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    test: liveness
  name: liveness-exec
spec:
  containers:
  - name: liveness
    image: busybox
    args:
    - /bin/sh
    - -c
    - touch /tmp/healthy; sleep 30; rm -f /tmp/healthy; sleep 600
    livenessProbe:
      exec:
        command:
        - cat
        - /tmp/healthy
      initialDelaySeconds: 5
      periodSeconds: 5
```

- Create the pod with `liveness-exec.yaml` command.

```bash
kubectl apply -f liveness-exec.yaml
```

- For the first 30 seconds of the container's life, there is a /tmp/healthy file. So during the first 30 seconds, the command cat /tmp/healthy returns a success code. After 30 seconds, cat /tmp/healthy returns a failure code.

- view the Pod events.

```bash
kubectl get po
kubectl describe pod liveness-exec
```

## startupProbe

- The kubelet uses startup probes to know when a container application has started. If such a probe is configured, it disables liveness and readiness checks until it succeeds, making sure those probes don't interfere with the application startup. This can be used to adopt liveness checks on slow starting containers, avoiding them getting killed by the kubelet before they are up and running.

- Sometimes, you have to deal with legacy applications that might require an additional startup time on their first initialization. In such cases, it can be tricky to set up liveness probe parameters without compromising the fast response to deadlocks that motivated such a probe. The trick is to set up a `startup probe` with the same command, HTTP or TCP check, with a `failureThreshold * periodSeconds` long enough to cover the worse case startup time.

- Create a `startup-pod.yaml` and input text below.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: flask-app
spec:
  containers:
  - name: flask-container
    image: ersinsari/startup-probe
    ports:
    - containerPort: 5000
    startupProbe:
      httpGet:
        path: /healthz
        port: 5000
      failureThreshold: 30
      periodSeconds: 3
    livenessProbe:
      httpGet:
        path: /healthz
        port: 5000
      initialDelaySeconds: 60
      periodSeconds: 10
```

- In this image (ersinsari/startup-probe), for the `first 60 seconds` the container returns a status of 500. Than the container will return a status of `200`. 

```bash
kubectl apply -f startup-pod.yaml
kubectl get po
kubectl describe pod flask-app
```

- Thanks to the startup probe, the application will have a maximum of 90 seconds (3 * 30 = 90s) to finish its startup. Once the startup probe has succeeded once, the liveness probe takes over to provide a fast response to container deadlocks. If the startup probe never succeeds, the container is killed after 75s and subject to the pod's restartPolicy.

## readinessProbe

- The kubelet uses readiness probes to know when a container is ready to start accepting traffic. A Pod is considered ready when all of its containers are ready. One use of this signal is to control which Pods are used as backends for Services. When a Pod is not ready, it is removed from Service load balancers.

- Sometimes, applications are temporarily unable to serve traffic. For example, an application might need to load large data or configuration files during startup, or depend on external services after startup. In such cases, you don't want to kill the application, but you don't want to send it requests either. Kubernetes provides readiness probes to detect and mitigate these situations. A pod with containers reporting that they are not ready does not receive traffic through Kubernetes Services.

> **Note:** Readiness probes runs on the container during its whole lifecycle.

> **Caution:** Liveness probes do not wait for readiness probes to succeed. If you want to wait before executing a liveness probe you should use initialDelaySeconds or a startupProbe.

- Readiness probes are configured similarly to liveness probes. The only difference is that you use the readinessProbe field instead of the livenessProbe field.

- Configuration for HTTP and TCP readiness probes also remains identical to liveness probes.

- Readiness and liveness probes can be used in parallel for the same container. Using both can ensure that traffic does not reach a container that is not ready for it, and that containers are restarted when they fail.