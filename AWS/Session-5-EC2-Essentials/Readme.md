# Session-5: EC2 Essentials

After completing this hands-on, you should know how to do the following:

- Launch and manage Elastic Compute Cloud (EC2) instances
- Optimize costs with Spot Instances
- Perform backups and restores using Snapshots
- Create and manage Amazon Machine Images (AMIs)
- Configure and manage Elastic Block Store (EBS) volumes


## Part 1 - Launch an EC2 Instance

- At the top of the AWS Management Console, in the search bar, search for and choose **EC2**
- Scroll to the middle of the page and click the  **Launch Instance** button


```text
Name                 : Lab-1 Instance-<YourName>
AMI                  : Amazon Linux 2023 AMI
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : default
    Subnet           : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name   : default-<YourName>
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       HTTP --- > 80 ---> Anywhere
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

-  On the configuration review page, click the **Launch Instance** button located at the bottom right

- Click on the instance you created and copy its Public IP address
- Paste the Public IP address into your browser to access the instance

- Click on **Lab-1 Instance** ----> **Connect** ----> **SSH client**
- Open your local terminal and use SSH to connect to the EC2 instance

```bash
ssh -i "Lab-Key" ec2-user@<Lab-1 Instance-Public-IP>
```

- Click on **Lab-1 Instance** ----> **Instance state** ----> **Terminate (delete) instance**
- After terminating the instance, refresh your browser and verify that the connection to the Public IP is no longer accessible.

## Part 2 - Launch an EC2 Spot Instances

- Navigate to the **EC2 Dashboard**
- Select **Spot Requests** from the left-hand menu
- Click the **Request Spot Instances** button

```text
Launch parameters                    : Manually configure launch parameters
AMI                                  : Amazon Linux 2 AMI (HVM)
Key pair name                        : Lab-Key
Target capacity
   Total target capacity             : 2 (instance)
Instance type requirements  
   Manually select instance types
   Instance Types                    : t2.micro
Allocation strategy
   Price capacity optimized (recommended)
```

- Select the Spot Instance request you created
- Click on the **Pricing History** tab to view historical Spot pricing data


## Part 3 - Create a Custom AMI

## Task 1 - Create a First Snapshots

- At the top of the AWS Management Console, in the search bar, search for and choose **EC2**
- In the left-hand menu, find and click on **Snapshots** and **Create snapshot**

```text
Resource type                        : Instance
Instance ID                          : Lab-1 Instance
Description                          : This snapshot belongs to the Lab-1 Instance
Tags
   Key                               : Name
   Value - optional                  : Lab-1 Instance Snapshot-1-<YourName>
```

- Select the snapshot you created  ----> **Actions** ----> **Create image from snapshot**

```text
Image name                           : Lab-1-Instance-Image-<YourName>
```

- Click on the **AMIs** tab located on the left sidebar
- Select the AMI you created and click on **Launch instance from AMI**

```text
Name                 : Lab-1 Instance Snapshot-1-<YourName>
AMI                  : Lab-1-Instance-Image-<YourName>
Instance Type        : t2.micro
Key pair name        : Lab-Key
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
```

- Copy the public IP of **Lab-1 Instance Snapshot-1** and paste it into your browser

## Task 2 - Create a Second Snapshot

- Click on the **Instances** tab from the left menu
- Select **Lab-1 Instance** ----> **Actions** ----> **Image and templates** ----> **Create image**

```text
Image name           : Lab-1 Instance Snapshot-2-<YourName>
```

- Click on the **AMIs** tab located on the left sidebar
- Confirm that the newly created AMI appears in the **AMIs** tab
- Navigate to the **Snapshots** section and verify that AWS has automatically created a snapshot

- Go to the **Instances** tab and click on **Launch Instance**

```text
Name                 : Lab-1 Instance Snapshot-2-<YourName>
My AMIs              : Lab-1 Instance Snapshot-2-<YourName>
Instance Type        : t2.micro
Key pair name        : Lab-Key
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
Advanced details 
Purchasing option    : Spot instances
```

- Copy the public IP of **Lab-1 Instance Snapshot-2** and paste it into your browser

## Task 3 - Create a Snapshot from Root Volume

- In the left-hand menu, find and click on **Snapshots** and **Create snapshot**

```text
Resource type                        : Volume
Volume   ID                          : the **Volume ID** of the **Lab-1 Instance**
Tags
   Key                               : Name
   Value - optional                  : Lab-1 Instance Snapshot-3-<YourName>
```

- Select the snapshot you created  ----> **Actions** ----> **Create image from snapshot**

```text
Image name           : Lab-1 Instance Snapshot-3-<YourName>
```

- Click on the **AMIs** tab located on the left sidebar
- Confirm that the newly created AMI appears in the **AMIs** tab

- Go to the **Instances** tab and click on **Launch Instance**

```text
Name                 : Lab-1 Instance Snapshot-3-<YourName>
My AMIs              : Lab-1 Instance Snapshot-3-<YourName>
Instance Type        : t2.micro
Key pair name        : Lab-Key
Network settings 
    VPC              : default
    Subnet           : us-east-1b
Firewall (security groups)
Security Group    
    Sec.Group Name   : default
    Rules            : TCP --- > 22 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
                       HTTP --- > 80 ---> Anywhere
                       All ICMP IPv4  ---> Anywhere
Advanced details 
Purchasing option    : Spot instances
```

- Copy the public IP of **Lab-1 Instance Snapshot-3** and paste it into your browser


## Part 4 - Make Your AMIs Public 

- After creating the AMIs, by default, they are only visible to your account. If you want to make an AMI public:

1. Go to the **AMIs** section in the EC2 dashboard.
2. Select the AMI you want to make public.
3. Scroll down to the **Permissions** tab and click on **Edit AMI Permissions**.
4. If you see a restriction message, follow these steps:
   - Go to **Data Protection and Security**.
   - Click on **Manage** next to **Block public access for AMIs**.
   - Uncheck the **Block new public sharing** option and save the changes.
   - Return to the **Permissions** tab and set the AMI to **Public**.

5. Save the changes.

You can share the AMI ID with others, and they can find it in the **AMI Catalog**:
1. Go to the **Instances** section and choose **Launch Instance**.
2. Select **Community AMIs**.
3. Search for the AMI using its ID.

- By default, AMIs are available only in the region where they are created. If you want to use the AMI in multiple regions, follow these steps:

1. Go to the **AMIs** section in the EC2 dashboard.
2. Select the AMI you want to copy.
3. Click on **Actions --> Copy AMI**.
4. Choose the target region from the **Destination Region** dropdown.
5. Click **Copy AMI** to start the copying process.
6. Once the copying process is complete, the AMI will be available in the selected region.

- Repeat these steps for each additional region where you want the AMI to be available.


## Part 5: Lifecycle Manager - Automate Snapshot Creation

### Task 1: Create a Lifecycle Policy

1. Go to the AWS Management Console and search for **Lifecycle Manager** under **Elastic Block Store (EBS)**.
2. Click **Create Lifecycle Policy**.
3. Configure the policy:
   - **Create custom or default policy**: Custom policy
   - **Policy Type**: EBS Snapshot Policy
   - **Resource Type**: Volume
   - **Policy description**: First Lifecycle Manager Policy
   - **Target Resources by Tags**: Add a tag to identify the volumes for automatic snapshot creation.
   - **Policy Status**: Choose between **Enabled** (start immediately) or **Disabled** (set up the policy but start later).

### Task 2: Schedule Snapshots

1. On the **Schedule** page, configure the snapshot frequency:
   - **Frequency**: Daily
   - **Every**: 1
   - **Starting at**: 09:00 UTC
   - **Retention Rule**: Keep up to 6 snapshots and delete the oldest when a new one is created.
   - **Retention Type**: Age-based (e.g., retain snapshots from the last 7 days).

2. Click **Next** and review the policy details on the **Review Policy** page.

### Task 3: Verify Snapshot Creation

1. Go to the **Snapshots** section.
2. Check that the snapshots are being created automatically based on your schedule.
3. To verify, you can click on individual snapshots to see the details.


## Part 6: Expand the Root Volume Size  

1. Go to the **EC2 Dashboard** > **Volumes**.  
2. Select the volume attached to your **Lab-1 Instance** and click **Actions --> Modify Volume**

   ```text
   Current size: 8 GB  
   New size: 12 GB  
   ```

3. Click **Modify** to increase the volume size.  
   > **Note:** AWS only allows increasing the volume size; reducing it is not possible.  

4. Connect to your instance using SSH:

```bash
ssh -i "Lab-Key" ec2-user@<Lab-1 Instance-Public-IP>
```

5. Check the current volume and partition sizes using the `lsblk` command:

```bash
lsblk
```
Example output:

```text
NAME    MAJ:MIN RM SIZE RO TYPE MOUNTPOINT
xvda      202:0    0  12G  0 disk
├─xvda1   202:1    0   8G  0 part /
```


6. Expand the partition with the `growpart` command:

```bash
sudo growpart /dev/xvda 1
```

7. Verify that the partition size has increased:

```bash
lsblk
```
Example output:

```text
xvda      202:0    0  12G  0 disk
├─xvda1   202:1    0  12G  0 part /
```

8. Check which file system is used

```bash
sudo file -s /dev/xvda1 
```

9. Expand the filesystem with the `xfs_growfs` command:

```bash
df -h

sudo xfs_growfs /dev/xvda1
```

10. Verify the new filesystem size:

```bash
df -h
```

Example output:

```text
Filesystem      Size  Used Avail Use% Mounted on  
/dev/xvda1      12G  1.6G   11G  14% /
```


## Part 7: Create and Attach a Secondary Volume  

### Task 1: Create a New Volume  
1. Go to the **EC2 Dashboard** > **Volumes**.  
2. Click **Create Volume** and configure it as follows:  

```text
Size: 10 GB  
Availability Zone: Same as the root volume of Lab-1 Instance  
Tags
    Key:    Name 
    Value:  Lab-1-SecondVolume-<YourName>
```

3. Click **Create Volume**.  

4. Select the newly created volume and click **Actions --> Attach Volume**. Choose the **Lab-1 Instance** to attach.  
   > **Note:** Ensure that the volume and instance are in the same **Availability Zone (AZ)**; otherwise, the instance will not be listed.


### Task 2: Verify the Attached Volume in Linux  

1. Connect to your instance using SSH:
```bash
ssh -i "Lab-Key" ec2-user@<Lab-1 Instance-Public-IP>
```

2. Run the `lsblk` command to verify the attached volume:

```bash
lsblk
```

Example output:
```text
NAME    MAJ:MIN RM SIZE RO TYPE MOUNTPOINT
xvda    202:0    0  12G  0 disk /
xvdb      202:16   0  10G  0 disk
```

### Task 3: Create a Filesystem on the New Volume  

1. Check the filesystem on `/dev/xvdf`:

```bash
sudo file -s /dev/xvdb
```
Example output:

```text
/dev/xvdb: data
```
This indicates that no filesystem exists on the volume.

2. Create an `ext4` filesystem:

```bash
sudo mkfs -t ext4 /dev/xvdb
```

3. Verify the new filesystem:
```bash
sudo file -s /dev/xvdb
```
Example output:
```text
/dev/xvdb: Linux rev 1.0 ext4 filesystem data, UUID=66387523-aab5-4058-b41d-7e021b00a38b (extents) (64bit)
```


### Task 4: Mount the New Volume  

1. Create a mount point:

```bash
sudo mkdir /mnt/2nd-vol
```

2. Mount the volume to the newly created directory:

```bash
sudo mount /dev/xvdb /mnt/2nd-vol
```

3. Verify the mount:

```bash
lsblk
```
Example output:
```text
NAME    MAJ:MIN RM SIZE RO TYPE MOUNTPOINT
xvda    202:0    0  12G  0 disk /
xvdf    202:80   0   2G  0 disk /mnt/2nd-vol
```

4. Check the mounted filesystem:

```bash
df -h
```
Example output:

```text
Filesystem      Size  Used Avail Use% Mounted on  
/dev/xvda1      12G  1.6G   11G  14% /  
/dev/xvdb       9.8G   24K  9.3G   1% /mnt/2nd-vol
```


### Task 5: Add a File to the New Volume  

1. Change to the new mount directory:

```bash
cd /mnt/2nd-vol
```

2. Create a new file:

```bash
sudo touch lab.txt
```

3. List the contents of the directory:

```bash
ls
```

Example output:

```text
lab.txt  lost+found
```


### Clean-up

1. Go to the **AMIs** tab on the left menu
   - Select the created AMIs ----> **Actions** ----> **Deregister AMI**
   - If you have created a copy of the AMI in another region, don't forget to delete it too

2. Go to the **Snapshots** tab on the left menu
   - Select the created Snapshots ----> **Actions** ----> **Delete Snapshot** ----> **delete**
   - Terminate all instances except for **Lab-1 Instance-<YourName>**

3. Go to the **Lifecycle Manager** tab on the left menu
   - Select the created Lifecycle Manager ----> **Actions** ----> **Delete lifecycle policy** ----> **delete policy**

4. Go to the **Instances** tab on the left menu
   - Select the created Instances ----> **Instance state** ----> **Terminate (delete) instance** ----> **Terminate (delete)**

5. Go to the **Volumes** tab on the left menu
   - Select the created Volumes ----> **Actions** ----> **Delete volumes** ----> **delete**

