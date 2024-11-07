# Hands-on 1: Exploring and Interacting with the AWS Management Console and AWS CLI

OVERVIEW

The Amazon Web Services (AWS) environment is an integrated collection of hardware and software services 
designed to provide quick and inexpensive use of resources. The AWS API sits atop the AWS environment. An 
API represents a way to communicate with a resource. There are different ways to interact with AWS resources, 
but all interaction uses the AWS API. The AWS Management Console provides a simple web interface for AWS. The 
AWS Command Line Interface (AWS CLI) is a unified tool to manage your AWS services through the command line. 
Whether you access AWS through the AWS Management Console or using the command line tools, you are using tools 
that make calls to the AWS API.

![Image](pics/Overall.png)

After completing this hands-on, you should be able to do the following:

- Explore and interact with the AWS Management Console.
- Create resources using the AWS Management Console.
- Explore and interact with the AWS CLI.
- Create resources using the AWS CLI.

## Task 1: Explore and configure the AWS Management Console

The AWS Management Console provides secure sign-in using your AWS account root user credentials or AWS 
Identity and Access Management (IAM) account credentials. When you first sign in, the user credentials 
are authenticated and the home page is displayed. The home page provides access to each service console 
and offers a single place to access the information you need to perform your AWS related tasks.

- Observe the AWS Management Console. You can access and configure various services here.

### Task 1.1: Choose an AWS Region 

Regions are sets of AWS resources located in the same geographical area. 

- At the top-right corner of the console, choose a Region and complete the lab in this Region.
- If you want to change the default Region, click gear icon at the top-right corner of the console. Then, 
click on 'More User Settings' and edit the Region under 'Localization and default Region' section.

### Task 1.2: Search with the AWS Management Console

- In the home page of the console, explore the search box on the navigation bar, which provides a unified 
search tool for locating AWS services and features, service documentation, and the AWS Marketplace.
- Type the name of service you want to navigate.

### Task 1.3: Add and remove favorites

- On the navigation bar, click on 'Services' to open full list of services.
- From the left navigation menu, choose 'All services' and add a specific service as a favorite by selecting 
the star to the left of the service name.

### Task 1.4: Open a console for a service

- Type the name of the service from the search bar. The console page of the service is displayed. You can turn 
back to the AWS Management Console home page by clicking on the AWS logo in the upper-left-hand corner.

### Task 1.5: Create and use dashboard widgets

The widgets display important information about your AWS environment and provide shortcuts to your services. 
You can customize your experience by adding and removing widgets, rearranging them, or changing their size.

- To add a widget, choose 'Add widget' in the home page.
- Choose the title bar at the top of the widget and drag it to a new location on the console page. You can 
remove the widget by choosing three vertical dots in the upper-right corner of the widget. Also, you can 
resize the widget by dragging the bottom-right corner of the widget.

## Task 2: Create an Amazon S3 bucket using the AWS Management Console

Amazon S3 is an object storage service that offers industry-leading scalability, data availability, security, 
and performance. Customers can use Amazon S3 to store and protect any amount of data for a range of use cases, 
such as data lakes, websites, mobile applications, backup and restore, archive, enterprise applications, 
Internet of Things (IoT) devices, and big data analytics.

- Type 'S3' in the search bar and go to its console page.
- On the left-hand of the console, choose 'Buckets' and click on 'Create bucket'.
- Give a bucket a specific name and choose 'Create bucket' at the bottom of the screen. Notice that the name of 
the bucket is displayed among the list of all the buckets for the account.

## Task 3: Upload an object into the Amazon S3 bucket using the S3 console

- In the Amazon S3 console, choose the bucket you created.
- Choose 'Upload' and choose 'Add files'.
- The 'HappyFace.jpg' is located in the same location with 'Lab-1.md'. Browse and choose 'HappyFace.jpg'.
- Choose 'Upload' and wait for 'Upload succeeded' message to be displayed on top of the screen.

## Task 4: Launch an EC2 Instance with necesarry permissions

### Task 4.1: Create IAM Role

- Type 'IAM' in the search bar and navigate to service page.
- From the left-hand side of the page, choose 'Roles'. Then, choose 'Create role'.
- Follow these settings: 

```text
Trusted entity type: AWS Service
Use case: EC2
```

Click 'Next' and continue ...

```text
Policies : AmazonSSMManagedInstanceCore, AmazonS3FullAccess
```

Click 'Next' and continue ...

```text
Role Name: my-iam-role
Description: Using this role, EC2 instance can access to S3 and take Systems Manager functionality.
```

Click 'Create role' and finish this part.

### Task 4.2: Launch an EC2 Instance

- Type 'EC2' in the search bar and navigate to service page.
- From the left-hand side of the page, choose 'Instances'. Then, choose 'Launch instances'.
- Follow these settings: 

```text
Name: CLI-Instance
Amazon Machine Image (AMI): Amazon Linux 2023
Instance type: t2.micro
Key-pair name: Proceed without key-pair
Network settings: Create security group (Check the 'Allow HTTP traffic from the internet' box)
Advanced Details -> IAM Instance Profile: my-iam-role
```

Click 'Launch instance' and finish this part.

## Task 5: Create an Amazon S3 bucket and uploading an object using the AWS CLI

The AWS CLI is an open-source tool that you can use to interact with AWS services using commands in your 
command line shell.

### Task 5.1: Create a connection to the Command Host using Session Manager

- Search for 'EC2' in the search box and choose it.
- From the left-hand side of the service page, choose 'Instances' and choose the instance.
- After choosing the instance, click on 'Connect', choose the 'Session Manager' tab and click on 'Connect'.

With Session Manager, you can connect to Amazon EC2 instances without having to expose the SSH port on your 
firewall or Amazon VPC security group.

### Task 5.2: Use high-level S3 commands with the AWS CLI

- Open the new browser tab with a connection to the instance
- Type the following command to list all of the buckets owned by user.

```bash
aws s3 ls
```

- Type the following command to create a bucket. Enter the 'NUMBER' in a way that the bucket name will be unique.

```bash
aws s3 mb s3://labclibucket-NUMBER
aws s3 ls
```

- Type the following command to copy the file to a specified bucket.

```bash
aws s3 cp /home/ssm-user/HappyFace.jpg s3://labclibucket-NUMBER
```

- Type the following command to list objects under a specified bucket.

```bash
aws s3 ls s3://labclibucket-NUMBER
```

- Notice the uploaded object in the newly created bucket in the output list.
