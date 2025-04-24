# Session-2: VPC Basics

After completing this hands-on, you should know how to do the following:

- Create a VPC
- Create public and private subnets
- Create an internet gateway
- Configure Route Table 
- Create a NAT gateway
- Create a Bastion Host
- Create a Interface Endpoint

## Task 1: Create a VPC

- Type 'VPC' in the search bar and choose it. In the left navigation pane, choose 'Your VPCs'. 
- Choose 'Create VPC' and follow these settings:

```text
Resources to create: Choose 'VPC only'
Name tag: Lab-VPC-<YourName>
IPv4 CIDR: 10.0.0.0/16
```

and click 'Create VPC'.


## Task 2: Create public subnets and private subnets

- In the left navigation pane, choose 'Subnets' and 'Create subnet'.
- Follow these settings:

### Step-1 
```text
VPC ID: Lab-VPC-<YourName>
Subnet name: Lab-az1a-public-subnet-<YourName>
Availability Zone: us-east-1a
IPv4 subnet CIDR block: 10.0.1.0/24
```

### Step-2 
```text
VPC ID: Lab-VPC
Subnet name: Lab-az1a-private-subnet-<YourName>
Availability Zone: us-east-1a
IPv4 subnet CIDR block: 10.0.2.0/24
```


## Task 3: Create an internet gateway

- In the left navigation pane, choose 'Internet gateways'.
- Choose 'Create internet gateway' and follow these settings:

### Step-1 
```text
Name tag: Lab-IGW-<YourName>
```

### Step-2
Now attach the internet gateway 'Lab-IGW-<YourName>' to the vpc 'Lab-VPC-<YourName>'

- Select the internet gateway and choose 'Actions' and 'Attach to VPC'.
- From available VPCs, choose 'Lab-VPC-<YourName>' and choose 'Attach internet gateway'.


## Task 4: Configure Route Table

- In the left navigation pane, choose 'Route tables'.
- Choose 'Create route table' and follow these settings:

### Step-1 
```text
Name: Public Route Table-<YourName>
VPC: Lab-VPC-<YourName>
```

and choose 'Create route table'.

- Choose 'Edit routes' and 'Add route'.

```text
Destination: 0.0.0.0/0
Target: Choose Internet Gateway' in the dropdown menu, and then choose the displayed internet gateway ID.
```

### Step-2 
```text
Name: Private Route Table-<YourName>
VPC: Lab-VPC
```

and choose 'Create route table'.

### Step-3 
- show the routes in the route table "Public Route Table",
- click Subnet association button and show the route table 
- Click Edit subnet association
- select public subnets;
  - Lab-az1a-public-subnet-<YourName>
  - and click save

### Step-4 
- show the routes in the route table "Private Route Table",
- click Subnet association button and show the route table 
- Click Edit subnet association
- select public subnets;
  - Lab-az1a-private-subnet-<YourName>
  - and click save


## Task 5: Enable auto-assign Public IPv4 address for Public Subnet

- In the left navigation page, choose 'Subnets'
- Select 'Lab-az1a-public-subnet-<YourName>' subnet and choose 'Actions' and 'Edit subnet settings'
- Click 'Enable auto-assign public IPv4 address' and 'Save'


## Task 6: Launch an Amazon EC2 Instance into a Public Subnet

- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'

- Configure Public instance
```text
Name                 : Public Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t3.micro
Key pair name        : Create a key pair
Network settings 
    VPC              : Lab-VPC-<YourName>
    Subnet           : Lab-az1a-public-subnet-<YourName>
Firewall (security groups)
Security Group    
    Sec.Group Name   : SSH+HTTP-Sec.-Group-<YourName>
    Rules            : HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
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

## Task 7: Launch an Amazon EC2 Instance into a Private Subnet


- Configure Private instance
```text
Name                 : Private Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t3.micro
Key pair name        : <YourName>-key-pair
Network settings 
    VPC              : Lab-VPC
    Subnet           : Lab-az1a-private-subnet-<YourName>
Firewall (security groups)
Security Group    
    Sec.Group Name   : SSH+HTTP-Sec.-Group-<YourName>
    Rules            : HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
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

## Task 8: Create a NAT gateway

### Step-1 
- Create Elastic IP
- In the VPC left navigation page, choose 'Elastic IPs'
- Click "Allocate Elastic IP address"

Elastic IP address settings

```text
Amazon's pool of IPv4 addresses

Network border group : Keep it as is (us-east-1)

Tags-optional
Key    : Name 
Value  : Lab-Elastic IP-<YourName>
```

### Step-2
- Create a NAT Gateway
- In the VPC left navigation page, choose 'NAT gateways'
- Choose "Create NAT gateway"

```text
Name                      : Lab-NAT-gateway-<YourName>
Subnet                    : Lab-az1a-public-subnet-<YourName>
Elastic IP allocation ID  : Lab-Elastic IP-<YourName>
```

- click "create Nat Gateway" button

### Step-3
- Change Route Table of Private Subnet
- In the VPC left navigation page, choose 'Route tables'
- Select "Private Route Table" and go to "Routes"
- Click "Edit routes" and Add route

```text
Destination     : 0.0.0.0/0
Target          : Nat Gateway
                  nat-Lab-NAT-gateway-<YourName>
```text
- Click "Save changes"
```


## Task 9: Create a Bastion Host

- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'

- Configure Bastion Host

```text
Name                 : Bastion Host-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t3.micro
Key pair name        : <YourName>-key-pair
Network settings 
    VPC              : Lab-VPC
    Subnet           : Lab-az1a-public-subnet-<YourName>
Firewall (security groups)
Security Group    
    Sec.Group Name   : Bastion-Sec.-Group
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

- Try to connect ssh via Bastion Host
- go to your local terminal

```bash
eval "$(ssh-agent)"
```

-  Add the your private ssh key to the ssh-agent.

```bash
ssh-add ./[your pem file name]
```

- Confirm that the Private Key has been installed to the agent

```bash
ssh-add -L
```

- Connect to Bastion Host

```bash
ssh -A ec2-user@<Bastion Host Public IP>
```

- First logged into the Bastion Host, then connect to the Private Instance

```bash
ssh ec2-user@<Private Instance Private IP>
```


## Task 10: Create a Interface Endpoint

- At the top of the AWS Management Console, in the search bar, search for and choose 'VPC'
- In the left navigation page, choose 'Endpoints'
- Click 'Create endpoints'

```text
Name tag - optional       : Lab-endpoint-<YourName>
Type                      : EC2 Instance Connect Endpoint
Network settings
   VPC                    : Lab-VPC-<YourName>
   Security groups        : SSH+HTTP-Sec.-Group-<YourName>
   Subnet                 : Lab-az1a-private-subnet
```
- Click 'Create endpoints'


- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'
- Select 'Private Instace' and click 'Connect'

```text
Connect to instance              : EC2 Instance Connect
Connection Type                  : Connect using EC2 Instance Connect Endpoint
EC2 Instance Connect Endpoint    : Lab-endpoint
```

- Click 'Connect'


## Task 11: CLEAN UP 

1. Terminate EC2 Instances
2. Delete NAT Gateway
3. Delete Interface Endpoint
4. Release Elastic IP


