# Kubernetes Probes Hands-On

This lab covers the lifecycle of Kubernetes probes:

- `livenessProbe`
- `startupProbe`
- `readinessProbe`

## Prerequisites

- A running Kubernetes cluster such as Minikube, Kind, Docker Desktop, or OrbStack
- `kubectl` configured to talk to that cluster
- `curl` installed for direct endpoint checks

Notes:

- Some sections use `kubectl port-forward ...`. Before starting the next section, stop any previous port-forward process, or you may get `address already in use`.

## Table of Contents

- [Liveness Probe](#liveness-probe)
- [Exec Liveness Probe](#exec-liveness-probe)
- [TCP Socket Liveness Probe](#tcp-socket-liveness-probe)
- [Startup Probe](#startup-probe)
- [Readiness Probe](#readiness-probe)
- [Final Cleanup](#final-cleanup)

## Probe Summary

| Probe Type | Purpose | If It Fails |
| --- | --- | --- |
| `livenessProbe` | Checks whether the application is still alive | Kubernetes restarts the container |
| `readinessProbe` | Checks whether the Pod is ready to receive traffic | The Pod is removed from Service traffic |
| `startupProbe` | Checks whether the application has finished startup | Until it succeeds, liveness and readiness stay blocked |

## Liveness Probe

This hands-on shows two different behaviors:

- A `Deployment` notices when a Pod is deleted and creates a new one.
- But if the application is broken and the container process is still running, Kubernetes cannot detect that without a `livenessProbe`.

That is why this example uses a `Deployment` instead of a standalone `Pod`.

### Image Used

This lab uses the following image:

- For the first 60 seconds, the `/health` endpoint returns `200`.
- After 60 seconds, the `/health` endpoint returns `500`.
- The application state lives only as long as the process lives.
- If the container restarts, the application starts again and becomes healthy for the first 60 seconds.
- The home page shows a visual status screen.

The default transition time is `60` seconds. You can change it later by adding the `FAIL_AFTER_SECONDS` environment variable.

In short, we first observe the application fail, then we add a `livenessProbe` and watch the kubelet restart the container.

## Hands-on 1: Deployment Without Liveness Probe

Goal of this section:

- Show that a Deployment notices a deleted Pod.
- Show that it does not understand application failure without a probe.

This manifest uses the same `Deployment` and `Service` names as the next example.
The only difference is that this version does not include a `livenessProbe`.

Create the file:

```bash
vi liveness-deployment-http-no-probe.yaml
```

Paste the following YAML:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: liveness-http
spec:
  replicas: 1
  selector:
    matchLabels:
      app: liveness-http
  template:
    metadata:
      labels:
        app: liveness-http
    spec:
      containers:
      - name: liveness-http
        image: necipulusoyy9120/custom-liveness-http:1.3
        ports:
        - containerPort: 8080
---
apiVersion: v1
kind: Service
metadata:
  name: liveness-svc
spec:
  selector:
    app: liveness-http
  ports:
  - name: http
    port: 80
    targetPort: 8080
```

Apply the file:

```bash
kubectl apply -f liveness-deployment-http-no-probe.yaml
watch kubectl get pod  
```

In another terminal, expose the Service:

```bash
kubectl port-forward svc/liveness-svc 8080:http
```

Open it in the browser:

```bash
open http://127.0.0.1:8080/
```

If you want to check the API directly:

```bash
curl -i http://127.0.0.1:8080/health
```

After 60 seconds, the page turns red. At the same time, the `/health` endpoint starts returning `500`.

Expected result:

- First `200`
- Then `500`
- The Pod is still `Running`
- `RESTARTS` does not increase
- The Deployment still shows `1/1`

Main point:

- The application is now broken.
- But the container process is still alive, so Kubernetes does not treat it as a failure.
- Because there is no `livenessProbe`, the kubelet does not restart it.

### How Does the Deployment Detect Pod Deletion?

Get the Pod name:

```bash
kubectl get pod
```

Delete the Pod manually:

```bash
kubectl delete pod <pod-name>
```

Then watch again:

```bash
watch kubectl get pod  
```

Expected result:

- The old Pod is deleted.
- The Deployment notices it.
- A new Pod is created.

So in this case, Kubernetes detects that the Pod object is gone.

But it still cannot understand this case:

- The application returns `500`.
- The process is still running.
- The Pod object still exists.

Without a probe, Kubernetes cannot say "the application is broken".

Cleanup:

```bash
kubectl delete -f liveness-deployment-http-no-probe.yaml
```

## Hands-on 2: Same Deployment With Liveness Probe

Now we add a `livenessProbe` to the same application.

Create the file:

```bash
vi liveness-deployment-http.yaml
```

Paste the following YAML:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: liveness-http
spec:
  replicas: 1
  selector:
    matchLabels:
      app: liveness-http
  template:
    metadata:
      labels:
        app: liveness-http
    spec:
      containers:
      - name: liveness-http
        image: necipulusoyy9120/custom-liveness-http:1.3
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 3
          failureThreshold: 1
---
apiVersion: v1
kind: Service
metadata:
  name: liveness-svc
spec:
  selector:
    app: liveness-http
  ports:
  - name: http
    port: 80
    targetPort: 8080
```

Apply the file:

```bash
kubectl apply -f liveness-deployment-http.yaml
watch kubectl get pod
```

Demo note:

- `failureThreshold: 1` is used here to make the restart happen quickly during the lesson.
- In production, a higher value such as `3` is usually safer to avoid restarts caused by short network or response-time spikes.

If you want to watch the visual status in another terminal:

```bash
kubectl port-forward svc/liveness-svc 8080:http
open http://127.0.0.1:8080/
```

You can also check the API directly:

```bash
curl -i http://127.0.0.1:8080/health
```

Expected result:

- For the first 60 seconds, `/health` returns `200`.
- After 60 seconds, `/health` returns `500`.
- The kubelet sees the failed probe.
- The container is restarted.
- After the restart, the application starts from the beginning.
- You see `200` again for a while, then `500` again.
- The `RESTARTS` count increases.

Inspect in detail:

```bash
kubectl describe pod -l app=liveness-http
```

Typical event:

```text
Liveness probe failed: HTTP probe failed with statuscode: 500
```

Key difference:

- The Pod object was not deleted.
- The Deployment did not need to do anything extra.
- The kubelet detected the unhealthy container through the probe.
- That is why the restart happened.

Cleanup before moving to the next example:

```bash
kubectl delete -f liveness-deployment-http.yaml
```

## Exec Liveness Probe

Another type of liveness probe runs a command inside the container.

Create the file:

```bash
vi liveness-exec.yaml
```

Paste the following YAML:

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
      failureThreshold: 3
```

Apply the file:

```bash
kubectl apply -f liveness-exec.yaml
```

Probe timing in this example:

- `initialDelaySeconds: 5`
- `periodSeconds: 5`
- `failureThreshold: 3`
- `timeoutSeconds` is not set, so the default is `1`

This means:

- The first liveness check starts after `5 seconds`
- Then Kubernetes checks every `5 seconds`
- If the probe fails `3` times in a row, the container is restarted

In this scenario:

- For the first 30 seconds, `/tmp/healthy` exists.
- Then the file is removed.
- The probe command fails.
- The kubelet restarts the container.

Check:

```bash
kubectl get pod
kubectl describe pod liveness-exec
kubectl exec -it liveness-exec -- sh
cd /tmp
watch /tmp
```

Cleanup before moving to the next example:

```bash
kubectl delete -f liveness-exec.yaml --force --grace-period=0
```

## TCP Socket Liveness Probe

This probe checks a TCP port directly.

- Kubernetes sends a TCP connection attempt to the target port.
- If the port is open, the probe succeeds.
- If the port is closed, the probe fails.

Create the file:

```bash
vi tcp-liveness.yaml
```

Paste the following YAML:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: liveness-tcp
spec:
  containers:
  - name: liveness-tcp
    image: mysql
    ports:
    - containerPort: 3306
    env:
    - name: MYSQL_ROOT_PASSWORD
      value: "123456"
    livenessProbe:
      tcpSocket:
        port: 8080
      initialDelaySeconds: 10
      periodSeconds: 15
      failureThreshold: 2
```

In this example:

- The probe checks port `8080` every 15 seconds.
- The MySQL container listens on port `3306`, not `8080`.
- Because port `8080` is closed, the probe fails.
- The kubelet restarts the Pod.

Apply the file:

```bash
kubectl apply -f tcp-liveness.yaml
kubectl get pod
kubectl describe pod liveness-tcp
```

Probe timing in this example:

- `initialDelaySeconds: 10`
- `periodSeconds: 15`
- `failureThreshold: 2`

This means:

- The first liveness check starts after `10 seconds`
- Then Kubernetes checks every `15 seconds`
- If the probe fails `2` times in a row, the container is restarted

Watch the restart:

```bash
watch kubectl get pod
```

Delete the Pod before testing the working version:

```bash
kubectl delete -f tcp-liveness.yaml
vi tcp-liveness.yaml
```

Now update the probe port in the same file:

```yaml
    livenessProbe:
      tcpSocket:
        port: 3306
      initialDelaySeconds: 10
      periodSeconds: 15
      failureThreshold: 2
```

In this version:

- The probe checks port `3306`.
- That port is open in the MySQL container.
- The probe succeeds.
- The Pod keeps running without restarts.

Apply the file again:

```bash
kubectl apply -f tcp-liveness.yaml
kubectl describe pod liveness-tcp
watch kubectl get pod
```

You should see that the Pod stays healthy and does not restart.

Important:

- A container can have only one `livenessProbe`.
- If you try to define `livenessProbe` twice in the same container spec, only one definition becomes effective.
- Do not rely on duplicate probe keys in the YAML.

Cleanup before moving to the next example:

```bash
kubectl delete -f tcp-liveness.yaml 
```

## Startup Probe

Startup probes are usually used together with `livenessProbe` and `readinessProbe`.

They answer this question:

- Has the application started yet?

Until the startup probe succeeds:

- Kubernetes does not run liveness checks
- Kubernetes does not run readiness checks

This is useful for slow-starting applications.

### Why not only use `initialDelaySeconds`?

`initialDelaySeconds` only says "wait this long before the first check".

That is not always enough:

- If the delay is too short, liveness checks start too early.
- The container may be restarted before the application is ready.
- If the delay is too long, Kubernetes waits too long to detect real failures.

`startupProbe` solves this problem by separating startup detection from normal health checks.

Once the startup probe succeeds once:

- liveness probe starts running
- readiness probe can start running

If the startup probe fails enough times:

- the container is killed
- the Pod follows its restart policy

Without a startup probe, a slow application may restart again and again and eventually fall into `CrashLoopBackOff`.

For the next three examples, use this behavior:

- The application returns `500` for about the first `60 seconds`.
- After that, it starts returning `200`.
- The home page shows `FAILED` while `/healthz` returns `500`.
- After startup is complete, the home page shows `SUCCESS` and `/healthz` returns `200`.

### Hands-on 1: Liveness Starts Too Early

Create the file:

```bash
vi startup-without-startupprobe.yaml
```

Paste the following YAML:

```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: startup-without-probe
  name: startup-without-probe
spec:
  containers:
  - name: app
    image: necipulusoyy9120/custom-startup-probe:1.3
    ports:
    - containerPort: 5000
    livenessProbe:
      httpGet:
        path: /healthz
        port: 5000
      initialDelaySeconds: 3
      periodSeconds: 3
      failureThreshold: 3
---
apiVersion: v1
kind: Service
metadata:
  name: startup-without-probe-svc
spec:
  selector:
    app: startup-without-probe
  ports:
  - name: http
    port: 80
    targetPort: 5000
```

Apply the file:

```bash
kubectl apply -f startup-without-startupprobe.yaml
watch kubectl get pod
kubectl describe pod startup-without-probe
```

In another terminal, expose the Service:

```bash
kubectl port-forward svc/startup-without-probe-svc 8080:http
```

Check the application:

```bash
curl -i http://127.0.0.1:8080/
curl -i http://127.0.0.1:8080/healthz
```

Probe timing in this example:

- `initialDelaySeconds: 3`
- `periodSeconds: 3`
- `failureThreshold: 3`

This means:

- The first liveness check starts after `3 seconds`
- Then Kubernetes checks every `3 seconds`
- If the probe fails `3` times in a row, the container is restarted
- In practice, the Pod can be marked unhealthy very quickly during startup


Expected result:

- The application is still starting.
- Liveness starts checking after only `3 seconds`.
- The checks fail too early.
- The Pod restarts again and again.
- You may eventually see `CrashLoopBackOff`.

Main point:

- The application might have become healthy if it had more time.
- But liveness starts too early and keeps killing it.

Cleanup:

```bash
kubectl delete -f startup-without-startupprobe.yaml --force --grace-period=0
```

### Hands-on 2: Initial Delay Is Too Long

Create the file:

```bash
vi startup-with-long-initialdelay.yaml
```

Paste the following YAML:

```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: startup-long-delay
  name: startup-long-delay
spec:
  containers:
  - name: app
    image: necipulusoyy9120/custom-startup-probe:1.3
    ports:
    - containerPort: 5000
    livenessProbe:
      httpGet:
        path: /healthz
        port: 5000
      initialDelaySeconds: 75
      periodSeconds: 3
      failureThreshold: 3
---
apiVersion: v1
kind: Service
metadata:
  name: startup-long-delay-svc
spec:
  selector:
    app: startup-long-delay
  ports:
  - name: http
    port: 80
    targetPort: 5000
```

Apply the file:

```bash
kubectl apply -f startup-with-long-initialdelay.yaml
watch kubectl get pod 
kubectl describe pod startup-long-delay
```

In another terminal, expose the Service:

```bash
kubectl port-forward svc/startup-long-delay-svc 8080:http
```

Check the application:

```bash
curl -i http://127.0.0.1:8080/
curl -i http://127.0.0.1:8080/healthz
```

Expected result:

- At first, the home page shows `FAILED`.
- After about `60 seconds`, the home page changes to `SUCCESS`.
- The Pod eventually becomes healthy and stays running.
- There is no early restart problem.
- Kubernetes waits `75 seconds` before starting liveness checks.

Main point:

- This works, but it is too conservative.
- You are delaying health checks longer than necessary.
- The application becomes healthy earlier, but liveness still waits longer than necessary.
- If the application becomes unhealthy during that long wait, liveness still does nothing.

Cleanup:

```bash
kubectl delete -f startup-with-long-initialdelay.yaml --force --grace-period=0
```

### Hands-on 3: Startup Probe Solves Both Problems

Create the file:

```bash
vi startup-with-startupprobe.yaml
```

Paste the following YAML:

```yaml
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: startup-with-probe
  name: startup-with-probe
spec:
  containers:
  - name: app
    image: necipulusoyy9120/custom-startup-probe:1.3
    ports:
    - containerPort: 5000
    livenessProbe:
      httpGet:
        path: /healthz
        port: 5000
      periodSeconds: 3
      failureThreshold: 3
    startupProbe:
      httpGet:
        path: /healthz
        port: 5000
      initialDelaySeconds: 0
      failureThreshold: 7
      periodSeconds: 15
---
apiVersion: v1
kind: Service
metadata:
  name: startup-with-probe-svc
spec:
  selector:
    app: startup-with-probe
  ports:
  - name: http
    port: 80
    targetPort: 5000
```

Apply the file:

```bash
kubectl apply -f startup-with-startupprobe.yaml
kubectl get pod -w
kubectl describe pod startup-with-probe
```

In another terminal, expose the Service:

```bash
kubectl port-forward svc/startup-with-probe-svc 8080:http
```

Check the application:

```bash
curl -i http://127.0.0.1:8080/healthz
```

How this works:

- `startupProbe.periodSeconds: 15`
- `startupProbe.failureThreshold: 7`

Kubernetes can make up to `7` startup checks here, one immediately and then every `15 seconds`.

That means the last allowed failed check can happen at about `90 seconds`, and the next check at about `105 seconds` is the one that must succeed to avoid restart.

Expected result:

- During startup, Kubernetes checks the startup probe.
- Liveness does not take control yet.
- The app gets enough time to become healthy.
- Once startup succeeds, liveness begins.
- You avoid early restarts without using an unnecessarily long liveness delay.

Main point:

- `startupProbe` protects slow startup.
- `livenessProbe` can stay aggressive for real runtime failures.
- This is usually better than guessing a large `initialDelaySeconds`.

Cleanup:

```bash
kubectl delete -f startup-with-startupprobe.yaml --force --grace-period=0
```

## Readiness Probe

Readiness probes tell Kubernetes whether a Pod is ready to receive traffic.

If a Pod is not ready, the Service does not add it as a backend.

This probe does not restart the application. It only controls traffic flow.

Note:

- Readiness probes run for the whole container lifecycle.

Important:

- Liveness probes do not wait for readiness probes.
- If you need a delay, use `initialDelaySeconds` or `startupProbe`.

Readiness and liveness can be used together:

- Readiness controls traffic.
- Liveness controls restarts.

### Hands-on 1: Rolling Update With Readiness Probe

In this example:

- We start with one application running as `3` Pods.
- The image version is `v1`.
- Users send requests through one `Service`.
- Then we update the Deployment image to `v2`.
- The Deployment performs a rolling update automatically.
- `readinessProbe` decides when each new Pod is safe to receive traffic.

Main idea:

- If a new Pod starts but is not fully ready yet, the Service must not send requests to it.
- Otherwise, a user request may land on that half-started Pod and the page may fail.
- `readinessProbe` prevents that by keeping the Pod out of the Service until the check passes.

For this demo, the application behavior is built into the image:

- `/healthz` returns `503` for the first `45 seconds`.
- `/version` also returns `503` during that same startup window.
- After `45 seconds`, `/healthz` returns `200`.
- We use two image tags: `v1.1` and `v2.1`.


Create the file:

```bash
vi http-readiness.yaml
```

Paste the following YAML:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: readiness
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
  selector:
    matchLabels:
      app: readiness
  template:
    metadata:
      labels:
        app: readiness
    spec:
      containers:
      - name: readiness
        image: necipulusoyy9120/custom-readiness-http:v1.1
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 8080
        readinessProbe:
          httpGet:
            path: /healthz
            port: 8080
          initialDelaySeconds: 3
          failureThreshold: 3
          periodSeconds: 3
---
apiVersion: v1
kind: Service
metadata:
  name: readiness-http
spec:
  selector:
    app: readiness
  ports:
  - name: http
    port: 80
    targetPort: 8080
```

Apply the file:

```bash
kubectl apply -f http-readiness.yaml
watch kubectl get pod
```

Check the endpoints:

```bash
kubectl get ep readiness-http
kubectl describe ep readiness-http
```

If your cluster prints a deprecation warning for `Endpoints`, you can inspect `EndpointSlice` instead:

```bash
kubectl get endpointslice
```

Important:

- `kubectl port-forward svc/readiness-http ...` is useful for quick inspection, but it is not the best way to prove readiness-based Service routing.
- For this hands-on, use `Endpoints` or `EndpointSlice` as the source of truth for whether the Service can send traffic to Pods.


What you should see during the first `45 seconds`:

- The Pods are `Running`, but not `Ready`.
- The readiness check is still failing.
- The Service does not add those Pod IPs as ready endpoints.
- A request through the Service should fail because there is no ready backend yet.

Check it from inside the cluster:

```bash
kubectl run curlbox --rm -it --restart=Never --image=curlimages/curl -- sh
curl -i http://readiness-http
```

After about `45 seconds`:

- The Pods become `Ready`.
- The endpoints become ready.
- The Service starts sending traffic to those Pods.

```bash
curl -i http://readiness-http/version
```

- The Service now has ready backends.
- Requests succeed.

Now update the application from `v1` to `v2`:

```bash
kubectl set image deployment/readiness readiness=necipulusoyy9120/custom-readiness-http:v2.1
kubectl rollout status deployment/readiness
kubectl get pod
```

During the rolling update:

- New `v2` Pods start, but they are not added to the Service for about `45 seconds`.
- During that time, traffic keeps going to the old ready `v1` Pods.
- Once a `v2` Pod becomes ready, you may see a mix of `v1` and `v2`.
- After the rollout finishes, all responses become `v2`.

Main point:

- `readinessProbe` answers this question:
  Is this Pod ready to join the Service?
- If the readiness check fails, the Pod stays out of the Service.
- If the readiness check passes, the Pod becomes a Service backend.
- This protects users during startup and during rolling updates.

Best practice:

- In real applications, `livenessProbe`, `readinessProbe`, and `startupProbe` are often used together.
- `startupProbe` protects slow startup.
- `readinessProbe` controls traffic.
- `livenessProbe` detects runtime failure and triggers restart.

Cleanup:

```bash
kubectl delete -f http-readiness.yaml --force --grace-period=0
```

## Final Cleanup

When the hands-on is complete, delete the created resources:

```bash
kubectl delete -f .
```
