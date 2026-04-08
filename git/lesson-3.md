# Git Workflow Exercises

## Remote Git Repository Usage

### Creating an Account

1. Go to one of the following:
   - Bitbucket: [bitbucket.org](https://bitbucket.org/)
   - Azure DevOps: [dev.azure.com](https://dev.azure.com/)
2. Sign up for a free account and log in.

---

### Creating a Project and Repository

1. Create a project named `automation`.

2. Create a repository named `egitim_demo` under the `automation` project.

3. Create the repository with a `README.md` file from the start:
   - While creating the repository, enable **Initialize repository with a README**
   - This creates the repository on the default branch (`main`) with an initial `README.md`

4. Update `README.md` and create your first manual commit:
   - Go to **Source** (or **Files**) and open `README.md`
   - Click **Edit** and update the content:
     ```md
     # egitim_demo

     Initial README update
     ```
   - Commit message: `update README`
   - Commit directly to the default branch (`main`)

5. Explore the repository UI:
   - Navigate to **Branches** via the left menu and notice that the default branch now exists.
   - Navigate to **Commits** via the left menu and confirm that both the automatic initial commit and your README update commit are visible.

6. Create a new branch named `dev` via the UI:
   - Click **Branches** -> **Create branch**
   - Branch name: `dev`
   - Branch from: the default branch (`main` or `master`)

7. Add a file named `deployment.yaml` to the `dev` branch via the UI:
   - Go to **Source** -> select the `dev` branch -> click **Add file**
   - File name: `deployment.yaml`
   - Content:
     ```yaml
     apiVersion: apps/v1
     kind: Deployment
     metadata:
       name: demo-deployment
       labels:
         app: demo
     spec:
       replicas: 1
       selector:
         matchLabels:
           app: demo
       template:
         metadata:
           labels:
             app: demo
         spec:
           containers:
           - name: demo
             image: nginx:latest
             ports:
             - containerPort: 80
     ```
   - Commit message: `add initial deployment.yaml`
   - Commit directly to `dev` branch.

8. Verify the commit appeared under **Commits**.

9. Compare branches:
   - In `egitim_demo`, go to **Branches** -> click the three dots (`...`) next to a branch -> **More options** -> **Compare branches**
   - Select `dev` vs the default branch to see the diff

10. Manage notifications:
   - **Bitbucket:** Go to **Source** -> three dots menu -> **Manage notifications**
   - **Azure DevOps:** Go to **Project Settings** -> **Notifications** -> **New subscription**
     - Create a subscription to get notified only when the default branch is updated:

       | Field | Value |
       |---|---|
       | Description | `A commit is pushed` |
       | Subscriber | `automation Team` |
       | Deliver to | `Custom email address` |
       | Address | `your@email.com` |
       | Filter | `A specific team project` -> `automation` |
       | Filter criteria - Field | `Branches updated` |
       | Filter criteria - Operator | `Contains` |
       | Filter criteria - Value | your default branch name (for example `main`) |

     - Click **Finish** to save.
     - Make a commit to the `dev` branch and verify that no email arrives.
     - Make a commit to the default branch (for example, edit `README.md` via the UI) and verify that the notification email arrives.

11. Explore the `demo_app` repository (pre-created for demo purposes):
   - Show commit history under **Commits**.
   - Add a new file named `deployment.yaml` via the UI with the same content as step 7.
   - Show all commits across all branches via **Commits** -> **All branches**.

---

## Pull Request Workflow

> Goal: clone the repo, make a change on a feature branch, push it, and open a pull request.

1. Copy the clone URL from the repository UI (`Clone` button).

2. Clone the repository:
   ```bash
   git clone <url>
   cd egitim_demo
   ```

3. List branches and switch to `dev`:
   ```bash
   git branch          # show local
   git branch -r       # show remote
   git branch -a       # show all local and remote branches
   git checkout dev
   ```

4. Create a feature branch from `dev`:
   ```bash
   git checkout -b feature/deployment-task
   ```

5. Edit `deployment.yaml` and change `replicas` from `1` to `5`:
   ```yaml
   spec:
     replicas: 5   # was 1
   ```

6. Stage and commit:
   ```bash
   git add deployment.yaml
   git commit -m "increase replica count to 5"
   ```

7. Try pushing without setting upstream to show the error:
   ```bash
   git push
   # Error: no upstream branch set
   git remote -v   # show where origin points
   ```

8. Push with upstream:
   ```bash
   git push --set-upstream origin feature/deployment-task
   ```

9. In the repository UI:
   - Go to **Branches** and confirm `feature/deployment-task` is visible.
   - Click **Create pull request** or go to **Pull requests** -> **New**.
   - Source branch: `feature/deployment-task`
   - Target branch: `dev`
   - Title: `Increase replica count to 5`
   - Click **Create**.

10. Review the diff in the PR, then click **Merge**.

---

## Team Workflow and Conflict Resolution

> Scenario: Ali and Veli both branch from the same `dev` commit. Ali merges first. Veli continues on the older base, so his pull request later conflicts with `Dockerfile`.

### Setup (Before Ali and Veli Start)

1. In the remote repository UI, create a `Dockerfile` in the `dev` branch:
   - Go to **Source** -> select `dev` branch -> **Add file**
   - File name: `Dockerfile`
   - Content:
     ```dockerfile
     FROM alpine:latest
     COPY . /app
     WORKDIR /app
     CMD ["echo", "Hello, World!"]
     ```
   - Commit message: `add initial Dockerfile`

2. If you are doing this alone, use two separate folders or terminals before merging anything:
   - one folder for Ali
   - one folder for Veli
   - create Veli's branch before merging Ali's pull request

### Ali's Steps

3. Clone the repository:
   ```bash
   git clone <repo-url>
   cd egitim_demo
   ```

4. Switch to `dev` and create `Ali's feature branch`:
   ```bash
   git checkout dev
   git pull
   git checkout -b feature/ali
   ```

5. Create `deployment-ali.yaml`:
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: ali-deployment
     labels:
       app: ali
   spec:
     replicas: 3
     selector:
       matchLabels:
         app: ali
     template:
       metadata:
         labels:
           app: ali
       spec:
         containers:
         - name: ali-app
           image: nginx:latest
           ports:
           - containerPort: 80
   ```

6. Edit `Dockerfile` and change the last line:
   ```dockerfile
   FROM alpine:latest
   COPY . /app
   WORKDIR /app
   CMD ["echo", "Hello, Ali!"]
   ```

7. Commit and push:
   ```bash
   git add deployment-ali.yaml Dockerfile
   git commit -m "add ali deployment and update Dockerfile"
   git push --set-upstream origin feature/ali
   ```

- Do not merge Ali's pull request yet. First create `Veli's branch` from the old `dev` state.

---

### Veli's Steps

> Veli starts from the same original `dev` commit as Ali. He does **not** pull again after Ali merges, so he is unaware of Ali's `Dockerfile` change.

8. Before Ali's pull request is merged, clone the repository or use a separate folder:
   ```bash
   git clone <repo-url>
   cd egitim_demo
   git checkout dev
   git checkout -b feature/veli
   ```

9. Create `deployment-veli.yaml`:
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: veli-deployment
     labels:
       app: veli
   spec:
     replicas: 3
     selector:
       matchLabels:
         app: veli
     template:
       metadata:
         labels:
           app: veli
       spec:
         containers:
         - name: veli-app
           image: nginx:latest
           ports:
           - containerPort: 80
   ```

10. Edit `Dockerfile` and change the last line:
    ```dockerfile
    FROM alpine:latest
    COPY . /app
    WORKDIR /app
    CMD ["echo", "Hello, Veli!"]
    ```

11. Commit and push:
    ```bash
    git add deployment-veli.yaml Dockerfile
    git commit -m "add veli deployment and update Dockerfile"
    git push --set-upstream origin feature/veli
    ```

12. Now merge Ali's pull request first:
    - In the UI, create a pull request from `feature/ali` -> `dev`, then merge it.

13. Create or refresh Veli's pull request from `feature/veli` -> `dev`.
    - You will see: **`This pull request can't be merged`** because `Dockerfile` was already changed by Ali.

### Resolve the Conflict (Veli)

14. Pull the latest `dev` into Veli's branch and merge:
    ```bash
    git checkout dev
    git pull
    git checkout feature/veli
    git merge dev
    ```
    - Git will report a conflict in `Dockerfile`.

15. Open `Dockerfile` and you will see conflict markers:
    ```text
    <<<<<<< HEAD
    CMD ["echo", "Hello, Veli!"]
    =======
    CMD ["echo", "Hello, Ali!"]
    >>>>>>> dev
    ```
    - Decide which line to keep or combine, then remove the conflict markers.
    - Final version keeping Veli's line:
      ```dockerfile
      FROM alpine:latest
      COPY . /app
      WORKDIR /app
      CMD ["echo", "Hello, Veli!"]
      ```

16. Stage the resolved file and commit:
    ```bash
    git add Dockerfile
    git commit -m "resolve merge conflict in Dockerfile"
    git push
    ```

17. In the UI, the PR is now mergeable. Click **Merge**.

---

## Pushing a Local Repository to Remote

> Goal: initialize a repository locally and push it to Bitbucket or Azure DevOps for the first time without cloning an existing remote repository.

### Prerequisites

- You have a Bitbucket or Azure DevOps account.
- You have created an empty repository on Bitbucket or Azure DevOps named `from_local_demo`.
- Do not add a `README`, `.gitignore`, or any starter files when creating the remote repository.

### Step 1: Configure Your Identity

```bash
git config --global user.name "Your Name"
git config --global user.email "your_email@example.com"
```

### Step 2: Create a Local Repository

```bash
mkdir from_local_demo
cd from_local_demo
git init
touch README.md
git add README.md
git commit -m "Added README.md"
git checkout -b dev
git branch
```

#### `README.md` Markdown Basics

Open `README.md` and paste the following content:

```md
# Project Title

## Description
This is **bold**, this is *italic*, this is ***bold and italic***.

## Features
- Feature one
- Feature two
  - Sub-feature

# ========== Ansible Galaxy Packages ==========

## Numbered List
1. First step
2. Second step

## Inline Code
Inline: `print('hello world')`

## Link and Image
[Hepapi](https://hepapi.com/)

![Git](./download.png)

## Table
| Name  | Role |
|-------|------|
| Ali   | Dev  |
| Veli  | QA   |

## Blockquote
> This is a note or a warning.

## Horizontal Rule
---
```

Commit the updated `README.md`:

```bash
git add README.md
git commit -m "add markdown examples to README"
```

### Step 3: Add a File and Commit

Create a file named `test.py`:

```python
print('hello world')
```

```bash
git add test.py
git commit -m "initial commit: add test.py"
git log
```

### Step 3b: Make Additional Commits

Add a second line to `test.py`:

```python
print('hello world')
print('line-2')
```

```bash
git add test.py
git commit -m "added line-2"
git log
```

Add a third line to `test.py`:

```python
print('hello world')
print('line-2')
print('line-3')
```

```bash
git add test.py
git commit -m "added line-3"
git log
```

### Step 4: Connect to the Remote Repository

```bash
git remote add origin https://<username>@bitbucket.org/<username>/from_local_demo.git
```

Azure DevOps example:

```bash
git remote add origin https://dev.azure.com/<org>/<project>/_git/from_local_demo
```

Verify the remote configuration:

```bash
git branch
git branch -r
git remote -v
```

### Step 5: Push to Remote

```bash
git push -u origin dev
git log
```

Go to the Bitbucket or Azure DevOps repository UI and confirm:

- The `dev` branch exists.
- The full commit history is visible.
- The `README.md` and `test.py` commits are present.

### Hands-on: Using Multiple Remotes (`origin` vs `other`)

Goal: demonstrate that `origin` is just an alias and a single local repository can push to multiple remotes.

#### Step 1: Verify Existing Remote (`origin`)

```bash
git remote -v
```

Expected output:

```text
origin  https://...from_local_demo.git (fetch)
origin  https://...from_local_demo.git (push)
```

The repository already has a remote named `origin`.

#### Step 2: Create a Second Remote Repository in the UI

Create a new empty repository in Azure DevOps or Bitbucket with this configuration:

- Repository name: `other`
- Do not add `README` or any files

#### Step 3: Add a New Remote (`other`)

```bash
git remote add other <repo-url>
```

Example:

```bash
git remote add other https://dev.azure.com/<org>/<project>/_git/other
```

#### Step 4: Verify Both Remotes

```bash
git remote -v
```

Expected output:

```text
origin  https://...from_local_demo.git
other   https://...other.git
```

Now we have two remotes:

- `origin` -> first repository
- `other` -> second repository

#### Step 5: Push to the New Remote

```bash
git push other dev
```

This pushes the same local branch to a different remote repository.

#### Step 6: Verify in the UI

Check the `other` repository:

- The `dev` branch exists
- The commit history is visible
- Files such as `README.md` and `test.py` are present

#### Step 7: Set Upstream to `other`

```bash
git push -u other dev
```

Now this branch is linked to the `other` remote.

#### Step 8: Make a New Commit

Update `test.py`:

```python
print('hello world')
print('line-2')
print('line-3')
print('line-4')
```

Commit and push:

```bash
git add test.py
git commit -m "added line-4"
git push
```

This push goes to `other` because the upstream is set.

#### Step 9: Push to `origin` Again

```bash
git push origin dev
```

This sends the same changes to the original repository.

#### Step 10: Push to Both Remotes

```bash
git push other dev
git push origin dev
```

Same branch -> multiple remotes.

#### Key Takeaways

- `origin` is not special -> it is just a default alias
- You can define multiple remotes
- One branch can be pushed to multiple repositories

#### One-line Summary

`origin` is just a name - you can use any alias and push to multiple remotes.

### Mirror Push (One Command to Push to Two Remotes)

#### 1. Problem

Normally, if you have two remotes, you push twice:

```bash
git push origin dev
git push other dev
```

If you want, you can configure Git so that one push command sends to multiple repositories.

#### 2. Solution: Add Extra Push URLs

Git allows a single remote to have multiple push targets.

With your current setup:

```bash
git remote -v
```

```text
origin  https://dev.azure.com/<org>/<project>/_git/from_local_demo (fetch)
origin  https://dev.azure.com/<org>/<project>/_git/from_local_demo (push)
other   https://dev.azure.com/<org>/<project>/_git/other (fetch)
other   https://dev.azure.com/<org>/<project>/_git/other (push)
```

At this point, a single `git push` does not automatically go to both repositories.

To make that happen, define two push URLs under `origin`.

#### 3. Setup

Run these commands:

```bash
git remote set-url --add --push origin https://dev.azure.com/<org>/<project>/_git/from_local_demo
git remote set-url --add --push origin https://dev.azure.com/<org>/<project>/_git/other
```

Now `origin` has two push targets:

- `from_local_demo`
- `other`

#### 4. What Changed?

Now:

- `origin` fetches from `from_local_demo`
- `origin` pushes to `from_local_demo`
- `origin` also pushes to `other`

That means `origin` can push to two repositories.

#### 5. Test It

Make a small change first.

For example, update `test.py`:

```python
print('hello world')
print('line-2')
print('line-3')
print('line-4')
print('line-5')
```

Then commit and push:

```bash
git add test.py
git commit -m "added line-5"
git push origin dev
```

This push will go to both repositories:

- `from_local_demo`
- `other`

If your current branch upstream is still set to `other`, then plain `git push` will keep using `other`.

To make plain `git push` use `origin` again, run:

```bash
git push -u origin dev
```

#### 6. Verify

```bash
git remote -v
```

Expected output:

```text
origin  https://dev.azure.com/<org>/<project>/_git/from_local_demo (fetch)
origin  https://dev.azure.com/<org>/<project>/_git/from_local_demo (push)
origin  https://dev.azure.com/<org>/<project>/_git/other (push)
other   https://dev.azure.com/<org>/<project>/_git/other (fetch)
other   https://dev.azure.com/<org>/<project>/_git/other (push)
```

Seeing three `push` lines is normal:

- two of them belong to `origin`
- one belongs to the separate `other` remote

#### 7. Logic

Fetch comes from one place, while push can go to multiple places.

#### 8. When Is This Useful?

- Backup repository
- Disaster recovery
- Multi-environment deployment
- Git mirror setup

#### 9. Things to Watch Out For

Push now goes to both repositories, so be careful:

- A wrong commit goes to both repositories
- A force push affects both repositories

#### 10. One-line Lesson

If we define multiple push URLs for one remote, a single push command can send to multiple repositories.

### Undo / Cancel Operations

If you want to go back to the old setup, you can remove the extra configuration.

#### 1. Remove Mirror Push

If you want `origin` to stop pushing to `other`, remove the second push URL:

```bash
git remote set-url --delete --push origin https://dev.azure.com/<org>/<project>/_git/other
```

After this, `origin` will push only to `from_local_demo`.

If you want to fully return to the original default setup, also remove the explicit `origin` push URL:

```bash
git remote set-url --delete --push origin https://dev.azure.com/<org>/<project>/_git/from_local_demo
```

#### 2. Remove the `other` Remote Completely

If you no longer want the second remote:

```bash
git remote remove other
```

#### 3. Verify Final State

```bash
git remote -v
git branch -a
```

---

## git fetch vs git pull

> Key concept from the diagram:
> - `git fetch` downloads changes from the remote Git repository into your **local Git repository** only. Your working directory is untouched. You decide when and whether to merge.
> - `git pull` does `git fetch` + `git merge` in one step. It downloads and immediately applies changes to your working directory.

### Setup: Simulate a Remote Change

Before starting, make a change directly in the remote repository UI so there is something to fetch:

1. Open the `from_local_demo` repository in the UI.
2. Find `test.py` on the `dev` branch and click **Edit**.
3. Add a new line at the bottom:
   ```python
   print('added from remote UI')
   ```
4. Commit with message: `add line from remote UI`

Now the remote is 1 commit ahead of your local repo.

---

### Part 1: `git fetch` (Download Only, Do Not Merge)

```bash
# Check current state - local is behind
git log --oneline

# Fetch: download the remote commit into your local repo
git fetch origin

# The remote commit is now stored locally under origin/dev
# BUT your working directory has NOT changed yet
git log --oneline
git log --oneline origin/dev
```

Compare what changed:

```bash
git diff dev origin/dev
```

Manually merge when you are ready:

```bash
git merge origin/dev
git log --oneline
```

---

### Part 2: `git pull` (`fetch` + `merge` in one step)

First, simulate another remote change:

1. Edit `test.py` in the UI again and add `print('second remote line')`
2. Commit with message: `add second line from remote UI`

Now pull:

```bash
git pull
git log --oneline
```

---

### Summary

| Command | Downloads to local repo | Merges into working directory |
|---|---|---|
| `git fetch` | Yes | No - you merge manually |
| `git pull` | Yes | Yes - automatic |

**When to use `git fetch`:** You want to inspect what changed before merging.

**When to use `git pull`:** You trust the remote changes and want to update immediately.

---

## Repository Settings for Bitbucket

1. Go to **Repository settings** in the left sidebar.
2. Under **User and group access**, add a user and assign `read`, `write`, or `admin` permission.
3. Under **Access keys**, add a deploy key for read-only CI access.
4. Under **Branch restrictions**, set rules for the `main` branch or a branch pattern:
   - Restrict direct pushes
   - Control who can merge
   - Add merge checks such as minimum approvals, resolved tasks, or passing builds
5. Under **Pull request and merge settings**, choose the merge strategies users can select in pull requests:

   - **Merge commit** preserves all commits and creates a merge commit:
     ```text
     A---B---C---D---E (main)
         \           /
          F---G---H (feature)
     ```
   - **Squash** squashes all PR commits into one commit on `main`:
     ```text
     A---B---C---D---E---S (main)
                         |
                         S = squashed from F+G+H
     ```
   - **Fast Forward** is allowed only when there are no new commits on `main`:
     ```text
     A---B---C---F---G---H (main, after fast-forward)
     ```

6. Under **Project settings**, configure project-level permissions and defaults.
7. Under **Webhooks**, add a webhook URL to trigger external services on push or PR events.
8. Under **Downloads**, attach release artifacts or binaries to the repository.

---

## Repository Settings for Azure DevOps

1. Go to **Project Settings** -> **Repositories**.
2. Under **Repositories**, select a repository and configure permissions such as `Read`, `Contribute`, `Create branch`, and `Create tag`.
3. Protect the default branch with **Policies** or **Branch policies**:
   - Require a minimum number of reviewers
   - Check for linked work items
   - Check for comment resolution
   - Limit merge types
   - Add build validation or status checks when needed
4. Review repository-level settings such as the default branch and inherited policies.
5. Under **Project Settings** -> **Service hooks** -> **Web Hooks**, send push or pull request events to external systems.

---

## .gitignore

### Introduction

`.gitignore` tells Git which files to never track. These are typically build artifacts, secrets, logs, and OS-generated files.

### Step 1: Create and Commit `.gitignore`

```bash
touch .gitignore
git add .gitignore
git commit -m "add .gitignore"
```

### Step 2: Basic Rules and Pattern Types

Open `.gitignore` and add the following rules one by one to see each in action.

#### Pattern 1: `*.log` Ignore by Extension

Ignores any file ending with `.log`, regardless of name.

```text
*.log
```

```bash
touch app.log
touch error.log
git status
git status --ignored
```

---

#### Pattern 2: `logs/` Ignore a Directory

Ignores the entire `logs/` folder and everything inside it.

```text
logs/
```

```bash
mkdir logs
touch logs/server.log
git status
git status --ignored
```

---

#### Pattern 3: `**/debug.log` Ignore Across All Subdirectories

`**` matches any number of nested folders. Useful when the same filename can appear anywhere in the project.

```text
**/debug.log
```

```bash
mkdir -p src/utils
touch debug.log
touch src/debug.log
touch src/utils/debug.log
git status
git status --ignored
```

---

#### Pattern 4: `!` Exception Rule

If you want to keep one file inside a folder trackable, ignore the folder contents, not the folder itself:

```text
*.log
logs/*
!logs/important.log
```

```bash
mkdir -p logs
touch logs/server.log
touch logs/important.log

git status
git status --untracked-files=all
git status --ignored

git add .
git commit -m "update gitignore demo"
```

---

Go to your Git platform:

Azure DevOps
Bitbucket

Check the following: Is the all new files visible?

---

## SSH Key Setup

> Use SSH instead of HTTPS to avoid entering your password on every push and pull.

### Why SSH? See the Error First

Try cloning with SSH before setting up the key. You will get this error:

```bash
git clone git@ssh.dev.azure.com:v3/<org>/<project>/<repo>
# fatal: Could not read from remote repository.
# remote: Public key authentication failed.
```

This means the remote does not recognize your machine yet. The fix is to generate an SSH key, add the public key to your account, then clone again.

---

### Step 1: Generate a New SSH Key Pair

If you already have an `id_rsa` key, give this one a different name to avoid overwriting it:

```bash
ssh-keygen -t rsa -b 2048
# When prompted for a file name, type a custom path:
# /Users/<username>/.ssh/id_rsa_azure
# Optionally set a passphrase
```

### Step 2: Copy the Public Key

```bash
cat ~/.ssh/id_rsa_azure.pub
```

Copy the output. This is the key you will paste into Bitbucket or Azure DevOps.

### Step 3: Add the Public Key to Your Account

**Bitbucket:** `Avatar` -> `Personal settings` -> `SSH keys` -> `Add key`

**Azure DevOps:** `User settings` -> `SSH public keys` -> `New Key`

You will see fields like these:

| Field | Value |
|---|---|
| Name | `id_rsa_azure.pub` |
| Public Key Data | paste the full output of `cat ~/.ssh/id_rsa_azure.pub` |

Click **Add** to save.

### Step 4: Add the Key to Your SSH Agent

```bash
ssh-add ~/.ssh/id_rsa_azure
```

### Step 5: Configure `~/.ssh/config`

For Bitbucket:

```text
Host bitbucket.org
    AddKeysToAgent yes
    IdentityFile ~/.ssh/id_rsa_azure
```

For Azure DevOps:

```text
Host ssh.dev.azure.com
    IdentityFile ~/.ssh/id_rsa_azure
    IdentitiesOnly yes
```

### Step 6: Test the Connection

```bash
ssh -T git@bitbucket.org
ssh -T git@ssh.dev.azure.com
```

- Try again with ssh
  
```bash
rm -rf from_local_demo  from_local_demo
```
git clone git@ssh.dev.azure.com:v3/<organization>/<project>/<repo_name>


---

## Forking a Repository

> A fork is a copy of someone else's repository under your own account. It allows you to make changes and propose them back to the original project via a pull request without needing direct access to the original repo.

1. Go to [github.com/rancher/rancher](https://github.com/rancher/rancher)
2. Click **Fork** and choose your account.
3. In your forked repo, open `README.md` via the UI and click the edit icon.
4. Add a line at the top:
   ```text
   # My Fork Notes
   ```
5. Commit message: `update README with fork notes`
6. Commit directly to `main`.
7. Go to **Pull requests** -> **New pull request**.
   - Base repository: `rancher/rancher`
   - Head repository: `<your-account>/rancher`
   - Observe the diff. This is how open-source contributions work.
