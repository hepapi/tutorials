# Session-3: VPC Advanced

After completing this hands-on, you should know how to do the following:

- Security Group 
- Network ACLs (NACLs)
- VPC Peering
- VPC Endpoints



## Part 0: Create a VPC,  public subnet and private subnet, internet gateway and NAT gateway

- At the top of the AWS Management Console, in the search bar, search for and choose 'VPC'
- Choose 'Create VPC' and 'VPC and more' follow these settings:


```text
Auto-generate                                           : Lab-<YourName>
IPv4 CIDR block                                         : 10.0.0.0/16
Number of Availability Zones (AZs)                      : 1
    Customize AZs                                       : us-east-1a
Number of public subnets                                : 1
Number of private subnets                               : 1
    Customize subnets CIDR blocks       
    Public subnet CIDR block in us-east-1a              : 10.0.1.0/24
    Private subnet CIDR block in us-east-1a             : 10.0.2.0/24
NAT gateways ($)                                        : In 1 AZ
VPC endpoints                                           : None
```

- Click 'Create VPC'

## Part 1 - Create Security Group

## Task 1: Create a Server-1 Instance

- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'

- Configure Server-1 instance

```text
Name                 : Server-1 Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC-<YourName>
    Subnet           : Lab-az1a-public-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : SSH-Sec.-Group-<YourName>
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

## Task 2: Connect to Server-1 Instance

- Open your local terminal and connect to the Server-1 instance

```bash
ssh -i Lab-Key.pem ec2-user@<Server-1 Instance Public IP>
```

## Task 3: Change the Security Group for the Server-1 Instance

- Return to the EC2 page and select the Server-1 instance
- Click on Security and open the Security Group we created in a new tab
- Select Edit inbound rules, delete the SSH rule, and then click Save rules

- Go back to your local terminal and try to reconnect to the Server-1 instance

```bash
ssh -i Lab-Key.pem ec2-user@<Server-1 Instance Public IP>
```

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Click on Create Security Group located in the top-right corner

```text
Security group name             : Webserver-SG-<YourName>
Description                     : Security for Web Application
VPC                             : Lab-VPC
Inbound rules 
   TCP --- > 22 ---> Anywhere
   All ICMP IPv4  ---> Anywhere
```

- Attach the newly created Webserver-SG to the Server-1 instance
- Select the Server-1 instance, then go to Actions ---> Security ---> Change Security Groups
- Remove 'SSH-Sec.-Group', then select 'Webserver-SG' and click Add Security Group. Finally, click Save

- Go back to your local terminal and try to reconnect to the Server-1 Instance

```bash
ssh -i Lab-Key.pem ec2-user@<Server-1 Instance Public IP>
```

- Run the following commands in Server-1 Instance terminal

```bash
sudo yum install nginx -y 
sudo systemctl start nginx
curl localhost
```

- Copy the public IP address of the Server-1 instance and paste it into your browser

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Select the Webserver-SG security group ---> Inbound rules ---> Edit inbound rules
- Click 'Add Rule', select 'HTTP', set the source to 'Anywhere' and then click 'Save rules'.

- Try accessing the public IP address of the Server-1 instance in your browser again


## Task 4: Modify the Outbound Rules for the Server-1 Instance's Security Group

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Select the Webserver-SG security group ---> Outbound rules ---> Edit outbound rules
- Delete the existing rule and Click 'Save rules'

- Copy the public IP address of the Server-1 instance and paste it into your browser again to verify the connection

- Run the following command in Server-1 Instance terminal

```bash
ping 8.8.8.8
```

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Select the Webserver-SG security group ---> Outbound rules ---> Edit outbound rules
- Click 'Add rule', select 'All traffic', set the destination to 'Anywhere' and click 'Save rules'

- Run the following command in your local terminal again

```bash
ping 8.8.8.8
```

## Task 4: Merge Two Security Groups

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Click on Create Security Group located in the top-right corner

```text
Security group name             : SSH-SG-<YourName>
Description                     : Allow SSH Access
VPC                             : Lab-VPC
Inbound rules 
   TCP --- > 22 ---> Anywhere
   All ICMP IPv4  ---> Anywhere
Tags
    Key                         : Name
    Value                       : SSH-SG-<YourName>
```

```text
Security group name             : HTTP-SG-<YourName>
Description                     : Allow HTTP Access
VPC                             : Lab-VPC
Inbound rules 
   TCP --- > 80 ---> Anywhere
   All ICMP IPv4  ---> Anywhere
Tags
    Key                         : Name
    Value                       : HTTP-SG-<YourName>
```

- Go to the EC2 Console 
- Select the Server-1 instance, then go to Actions ---> Security ---> Change Security Groups
- Remove 'Webserver-SG', then select 'HTTP-SG', 'SSH-SG' and click Add Security Group. Finally, click Save

- Scroll down to the Security section
- Under Security Groups, you’ll see the list of Security Groups attached to the instance

## Task 5: Implement Security Group Chaining

### Step-1

- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'

- Configure Server-1 instance

```text
Name                 : Server-2 Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC
    Subnet           : Lab-az1a-public-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : SSH-Sec.-Group-<YourName>
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

- Configure Private instance

```text
Name                 : Private Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC
    Subnet           : Lab-az1a-Private-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : SSH-Sec.-Group-<YourName>
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

### Step-2

- Try to connect Server-1 Instance ---> Private Instance via ssh (Don't forget to move the private key to Server-1 Instance)
- Try to connect Server-2 Instance ---> Private Instance via ssh Don't forget to move the private key to Server-2 Instance

### Step-3

- Return to the EC2 console and select 'Security Groups' from the left-hand menu
- Click on Create Security Group located in the top-right corner

```text
Security group name             : Server-1-SG-<YourName>
Description                     : Allow SSH Access
VPC                             : Lab-VPC
Inbound rules 
   TCP --- > 22 ---> Anywhere
   All ICMP IPv4  ---> Anywhere
Tags
    Key                         : Name
    Value                       : Server-1-SG-<YourName>
```

```text
Security group name             : Server-2-SG-<YourName>
Description                     : Allow SSH Access
VPC                             : Lab-VPC
Inbound rules 
   TCP --- > 22 ---> Anywhere
   All ICMP IPv4  ---> Anywhere
Tags
    Key                         : Name
    Value                       : Server-2-SG-<YourName>
```


```text
Security group name             : Private-SG-<YourName>
Description                     : Allow SSH Access
VPC                             : Lab-VPC
Inbound rules 
   TCP    --- > 22 
   Source  ---> Server-1-SG-<YourName>
Tags
    Key                         : Name
    Value                       : Private-SG-<YourName>
```

### Step-4

- Attach the newly created 'Server-1-SG-<YourName>' to the 'Server-1' instance
- Select the 'Server-1' instance, then go to Actions ---> Security ---> Change Security Groups
- Remove 'SSH-Sec.-Group', then select 'Server-1-SG-<YourName>' and click Add Security Group. Finally, click Save

- Attach the newly created 'Server-2-SG-<YourName>' to the Server-2 instance
- Select the 'Server-2' instance, then go to Actions ---> Security ---> Change Security Groups
- Remove 'SSH-Sec.-Group', then select 'Server-2-SG-<YourName>' and click Add Security Group. Finally, click Save

- Attach the newly created 'Private-SG-<YourName>' to the Private Instance-<YourName>
- Select the 'Private Instance-<YourName>', then go to Actions ---> Security ---> Change Security Groups
- Remove 'SSH-Sec.-Group', then select 'Private Instance-<YourName>' and click Add Security Group. Finally, click Save

### Step-5

- Try to connect Server-1 Instance ---> Private Instance via ssh (Don't forget to move the private key to Server-1 Instance)
- Try to connect Server-2 Instance ---> Private Instance via ssh Don't forget to move the private key to Server-2 Instance

## Part 2 - Create NACLs

## Task 1: Modify the Inbound Rules for the NACLs

- Go to the EC2 Console 
- Select the Server-1 instance, then go to Actions ---> Security ---> Change Security Groups
- Remove 'Server-1-SG-<YourName>', then select 'Webserver-SG' and click Add Security Group. Finally, click Save


- First, check the Server-1 instance in the browser to see if it's running.

- At the top of the AWS Management Console, in the search bar, search for and choose 'VPC'
- Select 'Network ACLs' from the left-hand menu

- Click the default Network ACL associated with Lab-VPC then go to Inbound Rules ---> Edit inbound rules

```text
Rule number                     : 100
Type                            : SSH (22)
Port range                      : 22
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow
```

- Go to the local terminal and connect to the Server-1 instance via SSH

```bash
ssh -i Lab-Key.pem ec2-user@<Server-1 Instance Public IP>
```

- Check the Server-1 instance in the browser to see if it's running again

- Return to the Network ACLs menu and add a new rule
- Click the default Network ACL associated with Lab-VPC then go to Inbound Rules ---> Edit inbound rules ---> Add new rule

```text
Rule number                     : 100
Type                            : SSH (22)
Port range                      : 22
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow

Rule number                     : 120
Type                            : HTTP (80)
Port range                      : 22
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow
```

- Refresh the Server-1 instance in the browser to see if it's running

- Click the default Network ACL associated with Lab-VPC then go to Inbound Rules ---> Edit inbound rules ---> Add new rule

```text
Rule number                     : 100
Type                            : SSH (22)
Port range                      : 22
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow

Rule number                     : 120
Type                            : HTTP (80)
Port range                      : 22
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow

Rule number                     : 90
Type                            : SSH (22)
Port range                      : 22
Source                          : <Your IP>/32
Allow/Deny                      : Deny
```

- Go to the local terminal and connect to the Server-1 instance via SSH

```bash
ssh -i Lab-Key.pem ec2-user@<Server-1 Instance Public IP>
```

## Task 2: Modify the Outbound Rules for the NACLs

- Click the default Network ACL associated with Lab-VPC then go to Outbound Rules ---> Edit outbound rules

```text
Rule number                     : 100
Type                            : All trafic
Port range                      : All
Source                          : 0.0.0.0/0
Allow/Deny                      : Deny
```

- Refresh the Server-1 instance in the browser to see if it's running


## Part 3 - Create VPC Peering

## Task 1: Preparation

- Modify the Rules for the Lab-VPC Network ACL
- Click the default Network ACL associated with Lab-VPC then go to Inbound Rules ---> Edit inbound rules

```text
Rule number                     : 100
Type                            : All trafic
Port range                      : All
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow
```

- Click the default Network ACL associated with Lab-VPC then go to Outbound Rules ---> Edit outbound rules

```text
Rule number                     : 100
Type                            : All trafic
Port range                      : All
Source                          : 0.0.0.0/0
Allow/Deny                      : Allow
```

- At the top of the AWS Management Console, in the search bar, search for and choose 'EC2'

- Configure Default instance

```text
Name                 : Default Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Default
    Subnet           : No preference
Firewall (security groups)
Security Group    
    Sec.Group Name   : Default
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

- Configure Lab-VPC instance

```text
Name                 : Lab-VPC Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC
    Subnet           : lab-az1a-public-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : Default
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

## Task 2: Create VPC Peering

- At the top of the AWS Management Console, in the search bar, search for and choose 'VPC'
- On the left side of the VPC console, click 'Peering connections' and Push 'Create peering connection'

```text
Name-optional                : Lab-Peering-<YourName>
VPC ID (Requester)           : default
VPC ID (Accepter)            : Lab-VPC
```

- Click on 'Create peering connection'

- Return to the 'Peering Connections' section on the left side of the page
- Select 'Lab-Peering' ----> Action ---> Accept Request ----> Accept Request
- Verify that the status has changed from 'Pending' to 'Active'

- Open your local terminal and attempt to connect from the 'Lab-VPC Instance' to the 'Default Instance' using its 'Private IP'


```bash
ssh -i Lab-Key.pem ec2-user@<Lab-VPC instance Public IP>
```

```bash
curl <Default Instance Private IP>
```

- Navigate to 'Route Tables' located on the left side of the 'VPC' page
-  Select the 'Public Route Table' of 'Lab-VPC' ----> Routes ----> Edit Routes ----> Add Route

```text
Destination       : 172.31.0.0/16 (Default VPC)
Target            : Peering Connection (Lab Peering)
```

- Repeat the same steps for the 'Default VPC'
- Navigate to 'Route Tables' located on the left side of the 'VPC' page
-  Select the 'Route Table' of 'Default-VPC' ----> Routes ----> Edit Routes ----> Add Route


```text
Destination       : 10.0.0.0/16 (Lab-VPC)
Target            : Peering Connection (Lab Peering)
```

-  Return to your local terminal

```bash
ssh -i Lab-Key.pem ec2-user@<Lab-VPC instance Public IP>
```

```bash
curl <Default Instance Private IP>
```

## Part 4 - Create VPC EndPoint

## Task 1: Preparation

-  At the top of the AWS Management Console, in the search bar, search for and choose 'S3'
- Click 'Create Bucket' located at the top-right corner

```text
Bucket name          : lab-bucket-<YourName>
```

- Open the newly created 'lab-bucket' and upload a file in 'PNG format'


- Configure Private Instance

```text
Name                 : Private Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC-<YourName>
    Subnet           : lab-az1a-private-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : Webserver-SG-<YourName>
    Rules            : HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

- Configure Bastion Instance

```text
Name                 : Bastion Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : Lab-VPC-<YourName>
    Subnet           : lab-az1a-public-subnet
Firewall (security groups)
Security Group    
    Sec.Group Name   : Webserver-SG-<YourName>
    Rules            : HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
```

- Now switch to the local terminal
- Connect to the Bastion Instance

```bash
eval "$(ssh-agent)"

ssh-add ./Lab-Key.pem

ssh -A ec2-user@<Bastion Instance Public IP>
```

- Connect to the Private Instance

```bash
ssh ec2-user@<Private Instance Private IP>
```

```bash
aws s3 ls
```

- Enable Outbound Traffic from a Private Instance;
- Allocate an Elastic IP
- Create a NAT Gateway
- Update the Route Table

- Switch to the local terminal again

```bash
aws s3 ls
aws configure
aws s3 ls
```

- Remove NAT Gateway from Private Route Table

```bash
aws s3 ls
```

## Task 2: Create VPC Endpoint

-  At the top of the AWS Management Console, in the search bar, search for and choose 'VPC'
- Click on the 'Endpoints' tab located on the left side of the page and select 'Create Endpoint'

```text
Name tag - optional  : Lab-Endpoint-<YourName>
Type                 : AWS services
Services
   Service Type      : Gateway
   Service Name      : com.amazonaws.us-east-1.s3
Network settings
   VPC               : Lab-VPC-<YourName>
   Route tables      : Private Route Table-<YourName>
```

- Navigate to the 'Route Tables' section.
- Select the 'Private Route Table'
- In the 'Routes' tab, confirm that the endpoint has been automatically added

- Reconnect to the Private Instance's Local Terminal

```bash
aws s3 ls
aws s3 ls lab-bucket
aws s3 cp s3://lab-bucket/<FILE.png> .
ls
```

## Part 5 CLEAN UP 

1. Terminate EC2 Instances
2. Delete NAT Gateway
3. Delete Endpoint
4. Release Elastic IP
5. Delete S3 Bucket
6. Delete Role
7. Delete VPC Peering
7. Delete Lab-VPC