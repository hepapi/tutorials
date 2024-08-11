
# Remote Git Repository Usage

## Creating an Account
1. Go to;
    - for bitbucket [Bitbucket](https://bitbucket.org/).
    - for azuredevops [AzureDevops](https://dev.azure.com/)

2. Sign up for a free account and log in.

## Creating a Project and Repository

1. Create a project named "automation".
2. Create a repository named "egitim_demo".
    - Navigate to branches via the left menu.
    - Navigate to commits via the left menu.
    - Create a new branch named "dev".
    - Add a file to the dev branch.
3. Fork a git repository
    - fork any public repo in github. fork rancher
    - add a change in own repo via UI
    - create a pull request
4. Compare branches. 
    - select compare branches Using the right three dots menu in Source menu
5. Manage notifications.
    - select Manage notifications Using the right three dots menu in Source menu # for bitbucket
    - Manage notification in project settings # for azuredevops
6. Select "demo_app" repository:
    - Show commit history.
    - Add and modify files. named deployment.yaml
    - Show all commits (across all branches).

## Pull Request Workflow
1. Clone the repository:
    ```bash
    git clone <url>
    ```
2. Check out to the "dev" branch:
    ```bash
    git branches
    git checkout dev
    ```
3. Make changes:
    ```bash
    # kubernetes deployment.yaml example
    git checkout -b feature/deployment-task
    git add deployment.yaml
    git commit -m "increase replica to 5"
    # try with "git push" command and talk about remote repository info(git remote -v)
    git push --set-upstream origin feature/deployment-task
    ```
4. See new branch in remote repository.
5. Create a pull request on remote repository via UI
6. Merge Pull Request

# Working with Team & Resolve Conflict
- Scenario: Two people named Ali and Veli are working on the same repo. Both have different tasks. There is a need to edit the common file within the task.
    1. Before starting, create a dockerfile from ui in dev branch
        - Dockerfile
        ```yaml
        FROM alpine:latest
        COPY . /app
        WORKDIR /app
        CMD ["echo", "Hello, World!"]
        ```
    2. Both clone the repo
        ```bash
            git clone <repo-url>
        ```
    3. Both create new feature branch from dev branch
        - ali
        ```bash
            git checkout dev
            git pull # if you dont clone for already cloned repos
            git checkout -b feature/ali
        ```
        - veli
        ```bash
            git checkout dev
            git checkout -b feature/veli
        ```
    4. 3 files will be created. deployment-ali.yaml, deployment-veli.yaml, dockerfile
        - ali creates deployment-ali.yaml and modify dockerfile
        - deployment-ali.yaml
            ```yaml
            apiVersion: apps/v1
            kind: Deployment
            metadata:
            name: ali-deployment
            labels:
                app: ali
            spec:
                replicas: 3
            ...
            ...

            ```
        - edit Dockerfile
            ```yaml
            FROM alpine:latest
            COPY . /app
            WORKDIR /app
            CMD ["echo", "Hello, Ali!"]
            ```
        - ali pushs his code and create a pull request
            ```bash
                git add .
                git commit -m "deployment created for ali's task and modified dockerfile"
                git push --set-upstream origin feature/ali
            ```
        - go to UI and create Pull request. then merge pull request to dev branch
        ----------------
        - veli creates deployment-veli.yaml and modify dockerfile
        - deployment-veli.yaml
            ```yaml
            apiVersion: apps/v1
            kind: Deployment
            metadata:
            name: veli-deployment
            labels:
                app: veli
            spec:
                replicas: 3
            ...
            ...

            ```
        - edit dockerfile
            ```yaml
            FROM alpine:latest
            COPY . /app
            WORKDIR /app
            CMD ["echo", "Hello, Veli!"]
            ```
        - veli pushs his code and create a pull request
            ```bash
                git add .
                git commit -m "deployment created for veli's task and modified dockerfile"
                git push --set-upstream origin feature/veli
            ```
        - go to UI and create Pull request. then merge pull request to dev branch
        - see "This pull request can’t be merged" message
        - resolve conflict (veli)
            ```bash
            git pull
            git merge dev
            # choose the right file
            git add .
            git commit -m "resolved conflict"
            git push
            ```
        - merge pull request

# Pushing a Local Repository to Remote Repository

## Introduction
This guide explains how to push a local Git repository to Bitbucket, allowing you to store your project remotely and collaborate with others.

## Prerequisites
- You have a local Git repository initialized.
- You have a Bitbucket account.
- You have created a new repository on Bitbucket.

## Step 1: Configure Your Local Repository
Ensure your local repository is configured with your identity:
```bash
git config --global user.name "Your Name"
git config --global user.email "your_email@example.com"
```
## Step 2: Add Remote Repository
Add your Bitbucket repository as a remote to your local repository:
```bash
mkdir from_local && cd from_local
git init
git checkout -b dev
echo "print('hello world')" > test.py
git add .      
git commit -m "dev changes"

git remote add origin https://<username>@bitbucket.org/<username>/<repository>.git
git remote -v
```
## Step 3: Initial Push
Push your local repository to Bitbucket:
```bash
git push -u origin dev
```
- go to remote repository UI. Check  branch and commit history

# Repository Settings for Bitbucket

1. Navigate to repository settings. # for bitbucket
2. Add a user with read/write/admin permissions in "Repository permissions" Menu.
3. Configure access keys and tokens.
4. Set up workflow rules:
    - Who can merge into specific branches directly or only with pull requests.
    - Check that pull requests meet certain conditions before merging (e.g., number of approvals or passing builds).
    - Create a restriction for the main branch:
        - Branch permissions.
        - Merge settings.
5. Merge strategies:
- Merge commit:
    ```css
    A---B---C---D---E (main)
        \       /
        F---G---H (feature)
    ```
- Squash (all commits in the PR are squashed into one):
    ```css
    A---B---C---D---E (main)
                    |
                    S (squashed commit from feature)
    ```
- Fast Forward (no changes in the destination branch):
     ```css
    A---B---C---D---E---F---G---H (main)
    ```

6. Project level settings.
7. Configure webhooks.
8. Download (Left Menu)

# Repository Settings for Azuredevops

# .gitignore
## Introduction
This guide explains the purpose and usage of the `.gitignore` file in Git to specify intentionally untracked files to ignore.

## Step 1: Create a .gitignore File
If your project doesn’t have a `.gitignore` file, you can create one in your project's root directory:
```bash
touch .gitignore
```

## Step 2: Configuring .gitignore
Add rules in the `.gitignore` file to specify the files you want to ignore. Here’s how to add some common patterns:
- Ignore all `.log` files: `*.log`
- Ignore a specific directory: `logs/`
- Ignore a specific file: `config.env`

## Step 3: Check Ignored Files
To see which files are ignored by your current .gitignore settings, you can use:
```bash
git status --ignored
```

## Step 4: Commit .gitignore
After setting up your `.gitignore` file, add it to your repository and commit:
```bash
git add .gitignore
git commit -m "Add .gitignore file"
```

## Step 5: Test .gitignore

```bash
touch abc.log
mkdir logs
touch logs/abc
touch logs/cds.py

git status # see there is no change
```

## SSH Settings
1. Generate a new SSH key on local computer:
   ```bash
   ssh-keygen -t rsa -b 2048
    ```

2. Go to User Settings -> SSH Public keys.
    - for azuredevops: https://learn.microsoft.com/en-us/azure/devops/repos/git/use-ssh-keys-to-authenticate?view=azure-devops
    - for bitbucket: https://support.atlassian.com/bitbucket-cloud/docs/configure-ssh-and-two-step-verification/
    
3. Add the public key via  GUI.
4. Configure SSH:
    ```bash
    ssh-add ~/.ssh/{ssh-key-name}
    ```
5. Add the following to your SSH config file (~/.ssh/config):
    - for bitbucket;
        ```bash
        Host bitbucket.org
            AddKeysToAgent yes
            IdentityFile ~/.ssh/{ssh-key-name}
        ```
    - for azuredevops;
        ```bash
        Host ssh.dev.azure.com
            IdentityFile ~/.ssh/{ssh-key-name}
            IdentitiesOnly yes
        ```