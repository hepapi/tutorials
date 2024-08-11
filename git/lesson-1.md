
# Git Training Session 1: Introduction and Basics

## Introduction to Version Control Systems
- **What is version control?** Understanding the need for version control in software development.
- **Types of version control systems:**
  - Centralized Version Control (e.g., SVN)
  - Distributed Version Control (e.g., Git)

## What is Git?
- Git is a distributed version control system designed to handle everything from small to very large projects with speed and efficiency.

## Installation of Git
- Download Git from [Git SCM](https://git-scm.com/downloads).
- Install Git on Windows, macOS, or Linux following the provided instructions.

## Installing Visual Studio Code
- Download and install VSCode to use as our editor [VSCode Download](https://code.visualstudio.com/download).
- **Recommended VSCode Extensions:**
  - Git Graph
  - Git History
  - GitLens — Git supercharged

How to create a Git repository?

if your os is windows, use git bash

## Configuring Git
Set up your Git environment with your name and email, which are important for commit messages.
```bash
git --version  # Check Git version
git config --list
git config --global user.name "Your Name"
git config --global user.email "your_email@example.com"
git config --global init.defaultBranch main  # default branch will be main 
 git config --global core.editor "code --wait" # default editor will be VScode
git config --list  # Verify the configuration
```

## Creating Your First Git Repository
```bash
mkdir myrepo && cd myrepo  # Create a new directory and enter it
git init  # Initialize an empty Git repository
ls -al  # View the hidden .git directory
git help  # List all Git commands and their options
```

### Basic Git Commands Workflow
```bash
git status  # View the status of files (tracked/untracked)
```

### Adding and Committing Changes
```bash
touch newfile.txt  # Create a new file
git status  # Check the status, file should be untracked
git add newfile.txt  # Stage the new file
git status 
git commit -m "Added newfile.txt"  # Commit the file with a message
```

### Viewing Commit History
```bash
git log  # View the commit history
```

### Modifying Files and Committing Changes
```bash
echo "Hello, Git!" > newfile.txt  # Modify file content
git status
git add .  # Stage all changes
git status
git commit -m "Updated newfile with greeting"  # Commit with a message
git log  # Check the updated history
```


### git commit -am "commit message" (add + commit)
```bash
# Change newfile content
echo "Another line" >> newfile.txt
git commit -am "Added another line to newfile"
```

### Git commit --amend
```bash
# Amend the last commit with new changes
echo "Additional change" >> newfile.txt
git add newfile.txt
git commit --amend -m "Amended the last commit with additional changes"
git log
```

## Branches

### What is a Branch?
- A branch in Git represents an independent line of development. It allows you to work on different features or bug fixes independently from the main codebase.

### Creating and Using Branches
```bash
# create new folder
cd ..
mkdir branches && cd branches
# List all branches
git branch # see "not a git repository" error
# Initialize a new Git repository
git init
git branch # List all branches (should only show 'master' or 'main')

# Make some changes and commit
echo "Some changes" > main.txt
git add main.txt
git commit -m "Made some changes in main-branch"

# Create a new branch named 'feature-branch'
git branch feature-branch

# Switch to the new branch
git checkout feature-branch # or git switch feature-branch

# Alternatively, create and switch to a new branch in one command
git checkout -b feature-branch
git branch

# Make some changes and commit
echo "Some changes" > feature.txt
git add feature.txt
git status
git commit -m "Made some changes in feature-branch"
git log
git checkout main # Changes in feature branch are not in main
```

#### Fast-forward Merge

```bash
git checkout feature-branch
# make additional commit
echo "additional changes" >> feature.txt
git add feature.txt
git status
git commit -m "additional changes in feature-branch"
git log
# Switch back to the main branch
git checkout main
git merge feature-branch # Fast-forward Merge.
git log # see just 3 commit. 
```

#### Three-way Merge

```bash
# new changes main branch
echo "new main branch changes" >> main.txt
git add main.txt
git status
git commit -m "New changes in main-branch"
git log

# Make some changes and commit on feature-branch-2
git checkout -b feature-branch-2

echo "feature-2 branch changes" > feature2.txt
git add feature2.txt
git status
git commit -m "Some changes in feature2-branch"

# Make some changes and commit on main
git checkout main
echo "changes from other developer" >> main.txt
git add main.txt
git commit -m "other changes in main-branch"
git log

# Merge 'feature-branch' into 'main'
git merge feature-branch-2 # Three-way Merge. 
git log
```