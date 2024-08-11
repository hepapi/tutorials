# Git Training Session 2: Conflict Resolution & Navigating History & Rebase

## Conflict Resolution

```bash
# Create a merge no-conflict scenario
cd ..
mkdir conflict && cd conflict  
git init
# add a line in main branch
echo "main line-1" > file.txt
git add file.txt
git commit -m "main line-1"

git checkout -b feat
# edit line-1
echo "feat line-1" > file.txt
git add file.txt
git commit -m "feat line-1"
git checkout main
git merge feat # There is no conflict

# Create a merge conflict scenario
# edit line-1
echo "main line-1" > file.txt
git commit -am "revert main"

git checkout -b feature/ali
# edit line-1
echo "ali line-1" > file.txt
git add file.txt
git commit -m "ali line-1"

git checkout main
git checkout -b feature/veli 
echo "veli line-1" > file.txt
git add file.txt
git commit -m "veli line-1"

# first conflict
git checkout main
# ask a question - if we merge feature/ali or feature/veli into main, would there be a conflict?
echo "no-name line-1" > file.txt
git add file.txt
git commit -m "no-name main line-1"
git merge feature/ali

# open text editor and solve conflict
git status
git add .
git commit -m "resolved conflict"
git merge feature/ali

# second conflict
git merge feature/veli
# open text editor and solve conflict with vscode current/incomming gui
git add .
git commit -m "resolved secondconflict"
git merge feature/veli
```

## Stash and Pop Usage

### Using git stash and git pop
- **git stash:** Temporarily stores changes in a dirty working directory.
- **git pop:** Restores the most recently stashed changes.

```bash
# Make some changes
git checkout -b feature/stash
echo "Temporary changes" > temp.txt

# had to change branch and work for different item
git checkout main # see temp.txt in main branch, you dont want

git checkout feature/stash
git add temp.txt
git checkout main # see temp.txt. you dont want
rm temp.txt


# Stash the changes
git checkout feature/stash
# removed your changes
echo "stash line-1" > file.txt
git stash
git checkout main # there is not any change

# List stashed changes
git checkout feature/stash
git stash list

# Apply the stashed changes
git stash pop

echo "Temporary changes" > temp.txt

git stash apply stash@{0} # restore to last changes

# Delete the stashed changes
git stash clear
```

## Navigating History
- restore
- reset
- checkout
- revert
- diff

### Restore
- This section covers the operations for returning from the Working Directory and staging area.
```bash
cd ..
mkdir undoredo && cd undoredo
git init
#create first commit
echo "line-1" > file.txt
git add .
git commit -m "line-1 added"
# revert from working directory
echo "line-2" >> file.txt
git status
git restore file.txt # revert last commit before add to stage
git status # no changes
# revert from staging area
echo "line-2" >> file.txt
git add file.txt
git status
git restore --staged file.txt # from stage to working directory. new line here but not in stage
git commit -am "line-2 added"
git log
```

###  Reset
#### Understanding Git Reset
- **Soft Reset:** Moves HEAD to another commit and stages changes.
```bash
echo "line-3" >> file.txt
git commit -am "line-3 added"
echo "line-4" >> file.txt
git commit -am "line-4 added"
echo "line-5" >> file.txt
git commit -am "line-5 added"
git log
git reset --soft HEAD^  # Move the last commit back to staging, can use previous hash in place of HEAD^ or HEAD~1
git log
git commit -am "line-5 added"
git log
```

- **Mixed Reset:** Moves HEAD to another commit and un-stages changes.
```bash
git reset HEAD^  # Unstage the last commit, changes are kept in working directory
git status # see changes not staged. in working directory
git commit -am "line-5 added"
```

- **Hard Reset:** Removes all changes in the working directory related to the last commit.
```bash
git reset --hard HEAD^  # Remove all changes and commits back to the specified commit
git status # there is not any change
git log # 
```

### Checkout

```bash
echo "line-1" >> file2.txt
git add .
git commit -m "line-1 for file-2"
git log
git checkout <hash> # see  file2.txt doesnt exist
git switch -c <new-branch-name> # 
git checkout main
git log # See that the new branch is one step behind
```

###  Revert
Undo changes using `revert`, which creates a new commit:
use last commit. if you want to return more previous commit, conflicts can be
```bash
git log
echo "line-5" >> file.txt
git commit -am "line-5 added"
echo "line-6" >> file.txt
git commit -am "line-6 added"
git log
git revert <commit-hash> # The hash value of the commit you want to undo is written. For this "line-6 added" commit
# save commit message in VScode
git log # See that commit line-6 ​​is not deleted and a new commit is created for revert.
```

## Using Git Diff
View changes between commits or stages:
```bash
# make a change
echo "line-6" >> file.txt
git diff # see changes

git add .
git diff # no changes
git diff HEAD # see changes in stage

git log
git diff <commit-hash1> <commit-hash2> # see changes in different commits
```
## Rebasing
```bash
cd ..
mkdir rebase && cd rebase
git init
# create 2 commit in main branch
touch main.txt feat.txt
git add .
git commit -m "main first commit"
echo "line-1 main" > main.txt
git add .
git commit -m "main second commit" 
git log

# switch to feat branch and create 2 commits
git checkout -b feat   
echo "line-1 feat" > feat.txt
git add .
git commit -m "feat ilk commit" 
echo "line-2 feat" >> feat.txt
git add .
git commit -m "feat second commit" 
git log

# switch to main branch and create another commit
git checkout main
echo "line-2 main" >> main.txt
git add .
git commit -m "main third commit" 
git log

#  switch to feat branch and merge main branch to feat
git checkout feat
git merge main # modify commit message in VSCode "first merge commit"

# create another commit in feat branch
echo "line-3 feat" >> feat.txt
git add .
git commit -m "feat third commit" 

# switch to main branch and create another commit
git checkout main
echo "line-3 main" >> main.txt
git add .
git commit -m "main fourth commit" 
git log # see only main branch commits

#  switch to feat branch and merge main branch to feat
git checkout feat
git merge main  # modify commit message in VSCode "second merge commit"
git log # see alot of commits (9 commit)

git rebase main # remove merge commits
git log # removed merge commits, the chronological order of the log is disrupted
```