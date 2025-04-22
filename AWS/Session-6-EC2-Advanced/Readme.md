# Session-5: EC2 Advanced

After completing this hands-on, you should know how to do the following:

- Create and Manage Launch Templates
- Standardize Instance Configurations
- Configure and Manage Load Balancers
- Set Up and Manage Auto Scaling Groups (ASG)



## Steps to Follow

This guide outlines the sequential steps for setting up the following AWS resources:

1. Launch Template
2. Target Group
3. Load Balancer
4. Auto Scaling Group

## Part 1 - Create a Security Group

- At the top of the AWS Management Console, in the search bar, search for and choose `EC2`

- Choose the Security Groups on left-hand menu

- Click the `Create Security Group`

```text
Security Group Name  : Lab-SecGroup-<YourName>
Description          : ASG Security Group
VPC                  : Default VPC
Inbound Rules:
    - Type: SSH ----> Source: Anywhere
    - Type: HTTP ---> Source: Anywhere
Outbound Rules: Keep it as default
Tag:
    - Key   : Name
      Value : Lab-SecGroup-<YourName>
```

- Click `Create Security Group` button

## Part 2 - Create Launch Templates


- Select `Launch Templates` from the left-hand menu and click `Create launch template`

```text
Launch template name - required      : Lab-Template-<YourName>
Auto Scaling guidance (click)
Launch template contents             : Quick Start
Amazon Machine Image (AMI)           : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key-<YourName>
Network settings 
    VPC              : default
    Subnet           : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name   : default
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
Resource tags        :
    Key     : Name
    Value   : Lab-Template-Instance-<YourName>

Advanced details 
User data - optional
```
```bash
#!/bin/bash
# To connect to your EC2 instance and install the Apache web server with PHP
yum update -y
yum install -y httpd php8.1
systemctl enable httpd.service
systemctl start httpd
cd /var/www/html
wget https://hepapi-aws-session-files.s3.ap-northeast-1.amazonaws.com/Session2-Networking%26Security/instanceData.zip
unzip instanceData.zip
```

- Click "Create Launch template"

- Select the launch template you created --> Action --> Launch Instance from template --> Number of instances: 2


## Part 3 - Create Target Group

- In the EC2 menu on the left side, click on `Target Group` under the `Load Balancing` section
- On the page that opens, click on `Create Target Group` located in the upper-right corner. 

```text
Choose a target type        : Instances
Target group name           : LabTargetGroup-<YourName>
Protocol : Port             : HTTP
Port                        : 80
IP address type             : IPv4
VPC                         : DEFAULT
Protocol version            : HTTP1

Health checks
    Health check protocol   : HTTP
    Health check path       : /


Advance health check settings
    Port                    : Traffic port
    Healthy threshold       : 4
    Unhealthy threshold     : 3
    Timeout                 : 10 seconds
    Interval                : 15 seconds
    Success codes           : 200

Tags

    Key                     : Name
    Value                   : LabTargetGroup-<YourName>
```

- Click `next` button
- Select two EC2 instances and click `Include as pending below` button.
- Click `Create Target Group` 
- Now, my target group is created


## Part 4 - Create Load Balancer

- In the EC2 menu on the left side, click on `Load Balancers` under the `Load Balancing` section
- On the page that opens, click on `Create load balancer` located in the upper-right corner
- Select `Application Load Balancer` from the options and click `Create`

```text
Load balancer name                   : LabLoadBlancer-<YourName>
Scheme                               : Internet-facing
Load balancer IP address type        : IPv4

Network mapping
    VPC                              : DEFAULT
    Mappings
        Availability Zones           : Select All Availability Zones
Security groups                      : default       
Listeners and routing
    Protocol                         : HTTP
    Port                             : 80
    Select a target group            : LabTargetGroup-<YourName>
```

- Click on the `Create Load Balancer` button located in the bottom-right corner
- The load balancer has been successfully created


## Part 5 - Testing Load Balancer & Traffic Distribution

- Copy the DNS Name of your Load Balancer.
- Paste it into a browser
- Refresh the page multiple times to observe Round-Robin traffic distribution.


## Part 6 - Simulating Failures

- Test 1: Stop Apache on One Instance
- Connect to one instance via SSH
- Run the command:

```bash
sudo systemctl status httpd

sudo systemctl disable httpd

sudo systemctl stop httpd
```

- ELB will detect the instance as unhealthy and stop routing traffic to it.
- Restart Apache and wait for ELB to mark it as healthy again:

```bash
sudo systemctl start httpd

sudo systemctl status httpd
```

- Test 2: Terminate an Instance
- Go to EC2 Instances.
- Select two instance and Terminate it


## Part 7 - Create Auto Scaling Group

- In the `EC2` menu, navigate to the left-hand side
- Scroll to the bottom and click on `Auto Scaling Groups`, located under the `Auto Scaling` section
- On the new page, click the `Create Auto Scaling group` button in the top-right corner

```text
Auto Scaling group name                         : LabAutoScaling-<YourName>
Launch template                                 : Lab-Template-<YourName>
```

- Click `Next`

```text
Network
    VPC                                         : DEFAULT
    Availability Zones and subnets              : Select All Availability Zones
Availability Zone distribution - new            : Balanced best effort
```

- Click `Next`

```text
Load balancing                                  : Attach to an existing load balancer
Attach to an existing load balancer
    Choose from your load balancer target groups
    Existing load balancer target groups        : LabTargetGroup-<YourName>
VPC Lattice integration options                 : No VPC Lattice service
Health checks                                   : Turn on Elastic Load Balancing health checks
Health check grace period                       : 200
```

- Click `Next`

```text
Desired capacity                                : 1
Min desired capacity                            : 1
Max desired capacity                            : 1
Automatic scaling - optional                    : No scaling policies
Instance maintenance policy                     : No policy
Additional capacity settings                    : Default
```

- Click `Next`

```text
Add notifications - optional
```

- Click `Next`

```text
Add tags - optional 

    Key                     : Name
    Value                   : LabTargetGroup-<YourName>
```


- Click `Next`
- Click `Create Auto Scaling group`
- The Auto Scaling Group has been successfully created


Open the following menus in separate tabs:
   - `Instances`
   - `Target Groups`
   - `Load Balancers`

- Copy the `Public IP` of the `Instance` and `DNS Name` of the `Load Balancer` into your browser's address bar and verify the application is running


### Task 1: Change Values of Auto Scaling groups

- Click on the `Auto Scaling Groups` menu located on the left-hand side
- Click on the `LabAutoScaling-<YourName>` that we created
- In the `LabAutoScaling` details page, locate the `LabAutoScaling Capacity overview` section on the top-right corner click on the `Edit` button within this section

```text
Desired capacity                : 2
Min desired capacity            : 1
Max desired capacity            : 5
```
- click the `Update` button to save your changes

- Navigate back to the following pages to observe any changes:
   - `Instances`
   - `Target Groups`
   - `Load Balancers`
- Review and note any updates or modifications reflected in these sections


### Task 2:  Manually Terminate One of The Running Instance

- To test Auto Scaling, manually terminate one of the running instances
- Go to the `Instances` page.
- Select one of the instances in the list.
- Click on the `Actions` dropdown menu.
- Choose `Instance State` and then click `Terminate (delete) instance`

- Observe how Auto Scaling responds by launching a replacement instance


## Part 8 - Create Dynamic Scaling Policiy-1

- On the left-hand menu, navigate to `Auto Scaling Groups`
- Click on the Auto Scaling Group you created (`LabAutoScaling-<YourName>`)
- Go to the third tab in the Auto Scaling Group details page, titled `Automatic Scaling`.
- Click on `create dynamic scaling policy`

### Task 1: Creating Target Tracking Scaling Policy

```text
Policy type                     : Target tracking scaling
Scaling policy name             : Target Tracking Policy-Lab-<YourName>
Metric type                     : Average CPU utulization
Target value                    : 40
```

- Click on `Create`


### Task 2: Testing the Scaling Policies

- Connect to an instance
- Go to the `EC2 Instances` section in the AWS Management Console
- Select the instance you want to connect to and click on `Connect` ---> `EC2 Instance Connect` ---> `Connect`

- Run a resource-intensive process

```bash
yes > /dev/null &
```

- Observe the number of instances increasing in the `Auto Scaling Groups` or `Instances` section
- connect to the instance again
- Run the following command to terminate the CPU-intensive processes

```bash
pkill yes
CTRL + C
```


## Part 9 - Create Dynamic Scaling Policiy-2

- On the left-hand menu, navigate to `Auto Scaling Groups`
- Click on the Auto Scaling Group you created (`LabAutoScaling-<YourName>`)
- Go to the third tab in the Auto Scaling Group details page, titled `Automatic Scaling`.
- Click on `create dynamic scaling policy`

### Task 1: Creating The Policy for Increase

```text
Policy type                     : Simple scaling
Scaling policy name             : SimpleScalingHigh-<YourName>
```
- Click on `Create a CloudWatch alarm`
- Click on `select metric` ---> `EC2` ---> `By Auto Scaling Group`

- In the `Search bar`, type `CPUUtilization` and press `Enter`
- Select the `LabAutoScaling-<YourName>` option from the list
- Click on `Select metric`

```text
Metric name                         : CPUUtilization
AutoScalingGroupName                : LabAutoScaling-<YourName>
Statistic                           : Average
Period                              : 30 seconds

Conditions
    Threshold type                  : Static
    Whenever CPUUtilization is...   : Greater > treshold
    than…                           : 40
```
- Click on `Next`
- In the `Notification` section, click `Remove` to skip it and Click `Next` to proceed

```text
Alarm name                          : LabAutoScalingAlarmHigh-<YourName>
```

- Click on `Next` and `Create alarm`

- After completing the alarm setup, return to the `Auto Scaling` page
- Go to the `CloudWatch Alarms` section and refresh the page
- Select the alarm you just created

```text
Policy type                     : Simple scaling
Scaling policy name             : SimpleScalingHigh-<YourName>
CloudWatch alarm                : LabAutoScalingAlarmHigh-<YourName>
Take the action
    Add   ---    1   ---   Capacity Unit
And then wait                   : 40
```

- Click on `Create`

- After creating the policy for `Increase`, you also need to create a policy for `Decrease`

### Task 2: Creating The Policy for Decrease

- On the left-hand menu, navigate to `Auto Scaling Groups`
- Click on the Auto Scaling Group you created (`LabAutoScaling-<YourName>`)
- Go to the third tab in the Auto Scaling Group details page, titled `Automatic Scaling`.
- Click on `Create dynamic scaling policy`

```text
Policy type                     : Simle scaling
Scaling policy name             : SimpleScalingLow-<YourName>
```
- Click on `Create a CloudWatch alarm`
- Click on `select metric` ---> `EC2` ---> `By Auto Scaling Group`

- In the `Search bar`, type `CPUUtilization` and press `Enter`
- Select the `LabAutoScaling-<YourName>` option from the list
- Click on `Select metric`

```text
Metric name                         : CPUUtilization
AutoScalingGroupName                : LabAutoScaling-<YourName>
Statistic                           : Average
Period                              : 30 seconds

Conditions
    Threshold type                  : Static
    Whenever CPUUtilization is...   : Lower/Equal <= treshold
    than…                           : 30
```
- Click on `Next`
- In the `Notification` section, click `Remove` to skip it and Click `Next` to proceed

```text
Alarm name                          : LabAutoScalingAlarmLow-<YourName>
```

- Click on `Create alarm`
- In the `Notification` section, click `Remove` to skip it and Click `Next` to proceed

```text
Alarm name                          : LabAutoScalingAlarmLow-<YourName>
```

- Click on `Next` and `Create alarm`

- After completing the alarm setup, return to the `Auto Scaling` page
- Go to the `CloudWatch Alarms` section and refresh the page
- Select the alarm you just created

```text
Policy type                     : Simple scaling
Scaling policy name             : SimpleScalingLow-<YourName>
CloudWatch alarm                : LabAutoScalingAlarmLow-<YourName>
Take the action
    Remove   ---    1   ---   Capacity Unit
And then wait                   : 40
```

- Click on `Create`
- The `Decrease` policy has been successfully created and is now ready for use


- **Important**

- Navigate to the `EC2 Console`. On the left-hand menu, click `Auto Scaling Groups` ---> `LabAutoScaling-<YourName>` ---> `Automatic Scaling` ---> `Target Tracking Policy-Lab-<YourName` ---> `Actions` ---> `Disable` 

- Navigate to the `EC2 Console`. On the left-hand menu, click `Auto Scaling Groups` ---> `LabAutoScaling-<YourName>` ---> `Automatic Scaling` ---> Select both policies by their names ---> `Actions` ---> `Enable` 


### Task 3: Testing the Scaling Policies

- Simulate high resource usage to trigger the `Increase` scaling policy
- Connect to an instance
- Go to the `EC2 Instances` section in the AWS Management Console
- Select the instance you want to connect to and click on `Connect` ---> `EC2 Instance Connect` ---> `Connect`

- Run a resource-intensive process

```bash
sudo yum install stress -y
stress -c 1
```

- After executing the command, navigate to the `CloudWatch` service in the AWS Management Console
- On the left-hand menu, click on `All Alarms`
- From the `All Alarms` list, select one of the alarms
- Examine the CPU status and metrics associated with the selected alarm


- Observe the number of instances increasing to 2 in the `Auto Scaling Groups` or `Instances` section
- connect to the instance again
- Run the following command to terminate the CPU-intensive processes

```bash
CTRL + C
```


---

## Part 10-CLEAN UP 

### 1. Delete the Auto Scaling Group
1. Navigate to the `Auto Scaling Groups` section in the AWS Management Console
2. Select the Auto Scaling Group you created (e.g., `LabAutoScaling`)
3. Click on `Actions` and choose `Delete`
4. Confirm the deletion


### 2. Delete the Load Balancer
1. Navigate to the `Load Balancers` section in the AWS Management Console.
2. Select the Load Balancer you created
3. Click on `Actions` and choose `Delete`
4. Confirm the deletion


### 3. Delete the Target Group
1. Navigate to the `Target Groups` section in the AWS Management Console.
2. Select the Target Group you created
3. Click on `Actions` and choose `Delete`
4. Confirm the deletion



### Note
- Make sure to delete **both the Auto Scaling Group and the Load Balancer** to avoid incurring unnecessary charges.