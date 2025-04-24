# Session-1: AWS Fundamentals

The purpose of this session is to give fundamental information about AWS Management Console and learn how to
interact with AWS Command Line Interface (CLI) to create resources.

- Part 1: AWS Management Console
   - Introduction to the console and navigation
   - Running AWS services through the console
- Part 2: AWS CLI (Command Line Interface)
   - [CLI installation](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) and configuration 
   - Running AWS services using CLI commands

NOTE: If multiple people are working under a single AWS account (or a Management Account), add your name to the end of each resource name to avoid confusion. 

Example: instead of “aws-training-vm,” use “aws-training-vm-john”.

## Part 1: AWS Management Console

### Introduction to the console and navigation

- Observe the console, you can access various services here.

- At the top-right corner of the console, you can see the account name.

- Click on account name to gain more information about the account, such as `Account ID`.

- At the top-right corner of the console, choose a Region in which you want to work.

- If you want to change the default Region, click on gear icon at the top-right corner of the console. Then, 
click on `More User Settings` and edit the Region under `Localization and default Region` section.

- In the home page of the console, explore the search box on the navigation bar, which provides a unified 
search tool for locating AWS services and features, service documentation, and the AWS Marketplace.

- Type the name of service you want to navigate.

- On the navigation bar, click on `Services` to open full list of services.

- From the left navigation menu, choose `All services` and add a specific service as a favorite by selecting 
the star to the left of the service name.

- Type the name of the service from the search bar. The console page of the service is displayed.

- Turn back to the AWS Management Console home page by clicking on the AWS logo in the upper-left-hand corner.

- To add a widget, choose `Add widget` in the home page.

- Choose the title bar at the top of the widget and drag it to a new location on the console page. You can 
remove the widget by choosing three vertical dots in the upper-right corner of the widget. Also, you can 
resize the widget by dragging the bottom-right corner of the widget.

### Running AWS services through the console

- Type `S3` in the search bar and go to its console page.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session1-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. Notice that the name of the bucket is displayed 
among the list of all the buckets for the account.

- In the Amazon S3 console, choose the bucket you created.

- Choose `Upload` and choose `Add files`.

- The `HappyFace.jpg` is located under `/pics` folder. Browse and choose `HappyFace.jpg`.

- Note that you can choose any file type you want, no restrictions.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

## Part 2: AWS CLI (Command Line Interface)

### CLI installation and configuration

- Open a new tab in the browser and go to `https://aws.amazon.com/cli/` to download the CLI.

- Run the following command on the terminal to check the status of download.

```bash
aws --version
```

- You need credentials to login to your AWS account. Open the AWS Management Console homepage.

- At the top-right corner of the console, click on the account name.

- In the listed menu, choose `Security Credentials`.

- Choose `Create access key` in the `Access keys` section.

```text
Use case: Command Line Interface (CLI)
```

- Check the `Confirmation` box, click `Next` and continue ...

```text
Description Tag Value: Credentials for Session-1
```

- Click `Create access key` and save the values of `Access key ID` and `Secret access key` on the notepad.

- Alternatively, you can download the `.csv` file and place it safely in the computer.

- Note that you shouldn't share or give your credentials.

- Open the terminal again and run the following command.

```bash
aws configure
```

- Fill the below with your credentials.

```text
AWS Access Key ID [None]: <Your Access key ID>
AWS Secret Access Key [None]: <Your Secret access key>
Default region name [None]: <Desired Region>
Default output format [None]: json
```

- After successful login, you can manage your account via CLI.
- Example CLI commands
```bash
aws s3 ls
aws s3 mb s3://<bucket_name> -- region
```

- You can also complete the last part of this session using the CLI. Besides, you can access to AWS 
services using EC2 Instance and proper permissions.

- Go to `IAM` service from the AWS Management Console.

- From the menu on the left, choose `Roles`. Then, choose `Create Role`.

```text
Trusted entity type: AWS Service
Use case: EC2
```

- Click `Next` and continue ...

```text
Policies : AmazonSSMManagedInstanceCore, AmazonS3FullAccess
```

- Click `Next` and continue ...

```text
Role Name: my-iam-role
Description: Using this role, EC2 instance can access to S3 and take Systems Manager functionality.
Leave the rest as default.
```

- Click `Create role` and finish.

- Go to `EC2` service from the AWS Management Console.

- From the menu on the left, choose `Instances`. Then, choose `Launch Instances`.

```text
Name: CLI-Instance
Amazon Machine Image (AMI): Amazon Linux 2023
Instance type: t2.micro
Key-pair name: Proceed without key-pair
Network settings: Create security group (Check the 'Allow HTTP traffic from the internet',
'Allow HTTPS traffic from the internet' and 'Allow SSH traffic from Anywhere' boxes.)
Advanced Details -> IAM Instance Profile: my-iam-role
Leave the rest as default.
```

- Under `Advanced Details -> User data - optional`, enter the following.

```bash
#!/bin/bash 
mkdir  /home/ssm-user
cd /home/ssm-user
wget https://hepapi-aws-session-files.s3.ap-northeast-1.amazonaws.com/Session1-AWS-Fundamentals/HappyFace.jpg
```

- Click `Launch instance` and finish.

### Running AWS services using CLI commands

- On the `Instances` page, choose the instance you created and click on `Connect`.

- Choose the `Session Manager` tab and click on `Connect`. By doing this, you can connect to instance without SSH.

- Open a new browser tab with a connection to the instance.

- Run the following command to list all of the buckets owned by user.

```bash
aws s3 ls
```

- Use a unique bucket name to create a new bucket, List your buckets, Copy a file into your bucket:
```bash
aws s3 mb s3://sessionclibucket-NUMBER
aws s3 ls
aws s3 cp <file_name> s3://sessionclibucket-NUMBER
```

- Delete the bucket
   - If the bucket is not empty, remove its contents first, Then remove the bucket
   - Alternatively, use --force to remove both objects and the bucket in a single step

```bash
aws s3 rb s3://sessionclibucket-NUMBER
aws s3 rm s3://sessionclibucket-NUMBER --recursive
aws s3 rb s3://sessionclibucket-NUMBER --force
```

- Run the following command to list objects under a specified bucket.

```bash
aws s3 ls s3://test-bucket-for-session1-xxxxx
```

- Notice the uploaded object in the newly created bucket in the output list.

- Don't forget to destroy the resources you created.
