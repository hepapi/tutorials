# Terraform Datasources Remote Backend Provisioners Hands-On

In this session, we'll explore resource dependencies, datasources and terraform import, version constraints, remote backends and terraform provisioners.

- Part 1 - Resource Dependencies, Datasources and Terraform Import
- Part 2 - Version Constraints
- Part 3 - Remote Backends
- Part 4 - Terraform Provisioners

## Part 1 - Resource Dependencies, Datasources and Terraform Import

## Implicit and Explicit Dependency

- Implicit dependency occurs automatically when one resource relies on the output or properties of another resource. Terraform identifies these relationships based on how resources reference each other.

- Explicit dependency is defined when the natural or implicit relationships between resources are not enough, or when resources should be created or destroyed in a specific order that Terraform can't infer. In these cases, you can use the `depends_on` argument to explicitly declare the dependency between resources.

### Important Note on AWS Region Selection

> **⚠️ IMPORTANT:** AWS has a default limit of 5 VPCs per region. To ensure all students can complete this hands-on exercise without hitting resource limits, **each student must use a different AWS region**. 
> 
> - Your instructor will provide a list assigning each student to a specific region
> - Replace `<your-assigned-region>` with the region assigned to you (e.g., `us-east-1`, `us-west-2`, `eu-west-1`, etc.)
> - Also replace `<your-name>` in resource names with your actual name to avoid naming conflicts
> 
> **Example:** If your name is "Ahmet" and you're assigned "eu-west-1":
> - Region: `eu-west-1`
> - VPC Name: `terraform-vpc-ahmet
> - Subnet Name: `terraform-subnet-ahmet`

### Creating Resources with Dependencies

In this example, we'll create AWS resources with both implicit and explicit dependencies. Make sure to customize the region and resource names as instructed above.

### Creating Resources with Dependencies

In this example, we'll create AWS resources with both implicit and explicit dependencies. Make sure to customize the region and resource names as instructed above.

**Create a folder called `dependency` and organize the code as follows:**

```bash
mkdir dependency
cd dependency
```

**Create provider.tf file:**
```t
# provider.tf
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}
```

**Create variables.tf file:**
```t
# variables.tf
variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "eu-west-1"  # Replace with your assigned region
}

variable "name" {
  description = "Name for resource naming"
  type        = string
  default     = "your-name"  # Replace with your actual name
}
```

**Create main.tf file:**
```t
# main.tf
resource "aws_vpc" "vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {
    Name = "terraform-vpc-${var.name}"
  }
}

resource "aws_subnet" "subnet" {
  vpc_id     = aws_vpc.vpc.id  # Implicit dependency
  cidr_block = "10.0.1.0/24"
  tags = {
    Name = "terraform-subnet-${var.name}"
  }
  depends_on = [
    aws_vpc.vpc  # Explicit dependency
  ]
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.vpc.id  # Implicit dependency
  tags = {
    Name = "terraform-igw-${var.name}"
  }
}

resource "aws_route_table" "route_table" {
  vpc_id = aws_vpc.vpc.id  # Implicit dependency
  
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id  # Implicit dependency
  }
  
  tags = {
    Name = "terraform-route-table-${var.name}"
  }
  
  depends_on = [
    aws_internet_gateway.igw  # Explicit dependency
  ]
}

resource "aws_route_table_association" "route_table_association" {
  subnet_id      = aws_subnet.subnet.id  # Implicit dependency
  route_table_id = aws_route_table.route_table.id  # Implicit dependency
  
  depends_on = [
    aws_subnet.subnet,
    aws_route_table.route_table  # Explicit dependencies
  ]
}
```

**Create outputs.tf file:**
```t
# outputs.tf
output "vpc_id" {
  value = aws_vpc.vpc.id
}

output "subnet_id" {
  value = aws_subnet.subnet.id
}

output "internet_gateway_id" {
  value = aws_internet_gateway.igw.id
}

output "route_table_id" {
  value = aws_route_table.route_table.id
}
```

**Run the following commands:**
```bash
terraform init
terraform plan
terraform apply
terraform destroy
```

## Datasources

- `Datasources` are used to query and use the attributes of existing resources within Terraform. With data sources, you can retrieve information from resources that were created by another process or method and use that data in your Terraform configuration.

- First, go to the AWS Management Console and manually create a VPC, a Subnet, and a Security Group with the naming convention specified above.

**Important: Creating Resources via AWS Console First**

> **⚠️ CRITICAL STEP:** Before using datasources, you must manually create resources through the AWS Console with specific naming conventions:
> - **VPC Name:** `terraform-vpc-<your-name>` (e.g., `terraform-vpc-ahmet`)
>   - **CIDR Block:** `10.0.0.0/16`
> - **Subnet Name:** `terraform-subnet-<your-name>` (e.g., `terraform-subnet-ahmet`)
>   - **CIDR Block:** `10.0.1.0/24`
>   - **Important:** Make sure to select the correct VPC (`terraform-vpc-<your-name>`) when creating this subnet
> - **Security Group Name:** `terraform-security-group-<your-name>` (e.g., `terraform-security-group-ahmet`)
>   - **Tags:** Add a tag with key `Name` and value `terraform-security-group-<your-name>`
>   - **Important:** Make sure to select the correct VPC (`terraform-vpc-<your-name>`) when creating this security group
>
> Replace `<your-name>` with your actual name to ensure unique resource names across all.

**Create a folder called `datasource` and organize the code as follows:**

```bash
mkdir datasource
cd datasource
```

**Create provider.tf file:**
```t
# provider.tf
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}
```

**Create variables.tf file:**
```t
# variables.tf
variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "eu-west-1"  # Replace with your assigned region
}

variable "name" {
  description = "Name for resource naming"
  type        = string
  default     = "your-name"  # Replace with your actual name
}
```

**Create data.tf file:**
```t
# data.tf
data "aws_vpc" "data_vpc" {
  tags = {
    Name = "terraform-vpc-${var.name}"
  }
}

data "aws_subnet" "data_subnet" {
  vpc_id = data.aws_vpc.data_vpc.id
  tags = {
    Name = "terraform-subnet-${var.name}"
  }
}

data "aws_security_group" "data_sg" {
  vpc_id = data.aws_vpc.data_vpc.id
  tags = {
    Name = "terraform-security-group-${var.name}"
  }
}
```

**Create outputs.tf file:**
```t
# outputs.tf
output "vpc_id" {
  value = data.aws_vpc.data_vpc.id
}

output "subnet_id" {
  value = data.aws_subnet.data_subnet.id
}

output "security_group_id" {
  value = data.aws_security_group.data_sg.id
}
```

**Create main.tf file:**

```t
resource "aws_internet_gateway" "igw" {
  vpc_id = data.aws_vpc.data_vpc.id
  tags = {
    Name = "terraform-igw"
  }
}

resource "aws_route_table" "public_route_table" {
  vpc_id = data.aws_vpc.data_vpc.id
  tags = {
    Name = "public-route-table"
  }
}

resource "aws_route" "default_route" {
  route_table_id         = aws_route_table.public_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.igw.id
}

resource "aws_route_table_association" "public_association" {
  subnet_id      = data.aws_subnet.data_subnet.id
  route_table_id = aws_route_table.public_route_table.id
}

```
```bash
terraform init
terraform plan
terraform apply
terraform destroy
```

## Terraform Import

- The terraform import command allows you to bring existing resources into Terraform management. 
- When you use the `terraform import` command, you're telling Terraform to take control of resources that already exist outside of Terraform's management.

### Step-by-Step Process for Terraform Import

**Step 1: Manually Create Resources via AWS Console**

> **⚠️ IMPORTANT:** Before importing, you must first manually create the following resources through the AWS Console:
> 
> 1. **Create a VPC:**
>    - Go to VPC Dashboard in AWS Console
>    - Click "Create VPC"
>    - **Name tag:** `terraform-import-vpc-<your-name>` (e.g., `terraform-import-vpc-ahmet`)
>    - **IPv4 CIDR block:** `10.0.0.0/16`
>    - Leave other settings as default
>    - Click "Create VPC"
>    - **Copy the VPC ID** (e.g., `vpc-0a1b2c3d4e5f6g7h8`) - you'll need this for the import command
>
> 2. **Create a Subnet:**
>    - In VPC Dashboard, go to "Subnets"
>    - Click "Create subnet"
>    - **VPC:** Select the VPC you just created (`terraform-import-vpc-<your-name>`)
>    - **Subnet name:** `terraform-import-subnet-<your-name>` (e.g., `terraform-import-subnet-ahmet`)
>    - **Availability Zone:** Select any available zone
>    - **IPv4 CIDR block:** `10.0.0.0/24`
>    - Click "Create subnet"
>    - **Copy the Subnet ID** (e.g., `subnet-8h7g6f5e4d3c2b1a0`) - you'll need this for the import command

**Step 2: Create Terraform Configuration**

Create a new folder and organize your Terraform configuration:

```bash
mkdir terraform-import
cd terraform-import
```

**Create a single main.tf file with all configurations:**

```t
# main.tf

# Provider Configuration
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# Variables
variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "eu-west-1"  # Replace with your assigned region
}

variable "name" {
  description = "Name for resource naming"
  type        = string
  default     = "your-name"  # Replace with your actual name
}

# Resources to Import
resource "aws_vpc" "imported_vpc" {
  cidr_block = "10.0.0.0/16"
  
  tags = {
    Name = "terraform-import-vpc-${var.name}"
  }
}

resource "aws_subnet" "imported_subnet" {
  vpc_id     = aws_vpc.imported_vpc.id
  cidr_block = "10.0.0.0/24"
  
  tags = {
    Name = "terraform-import-subnet-${var.name}"
  }
}
```

**Step 3: Initialize Terraform**

```bash
terraform init
```

**Step 4: Import Existing Resources**

Replace the IDs below with your actual resource IDs from AWS Console:

```bash
# Import the VPC (replace vpc-xxxxx with your actual VPC ID)
terraform import aws_vpc.imported_vpc vpc-0a1b2c3d4e5f6g7h8

# Import the Subnet (replace subnet-xxxxx with your actual Subnet ID)
terraform import aws_subnet.imported_subnet subnet-8h7g6f5e4d3c2b1a0
```

**Step 5: Verify Import and Apply**

After importing, run:

```bash
# Check the current state
terraform plan

# Apply to ensure configuration matches imported resources
terraform apply
```

**Step 6: Clean Up**

When finished, destroy the imported resources:

```bash
terraform destroy
```

## Part 2 - Version Constraints

```t
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "6.11.0"
    }
  }
}
```
```t
      version = "!= 6.13.0"
      version = "< 6.13.0"
      version = "~> 6.10.0"
      version = ">6.0.0, != 6.4.0, < 6.5.0"
```
- version = "6.13.0": Specifies that only version 6.13.0 of the AWS provider will be used. No other versions are allowed. This is the most restrictive constraint.

- version = "!= 6.13.0": Excludes version 6.13.0, meaning any other version can be used except for 6.13.0.

- version = "< 6.13.0": Specifies that any version less than 6.13.0 can be used. This is useful for avoiding breaking changes in newer major or minor releases.

- version = "~> 6.1.0": The tilde and greater-than operator (~>) iAllows only the right-most version component to increment. For ~> 6.1.0, this means any version from 6.1.0 up to, but not including, 6.2.0 is allowed (e.g., 6.1.1, 6.1.2).

- version = "> 6.0.0, != 6.5.0, < 6.10.0": You can combine multiple constraints with a comma. This complex constraint allows for versions greater than 6.0.0 and less than 6.10.0, while explicitly excluding version 6.5.0.

## Part 3 - Remote Backends
We used local state before; now we move Terraform state to a shared remote backend (S3 + DynamoDB) for safety, locking, and collaboration.

Why remote backend:
- Central single source of truth
- State locking (prevents concurrent writes)
- Versioning & encryption
- Works cleanly in CI/CD

AWS components:
- S3 bucket: stores terraform.tfstate
- DynamoDB table: manages state lock

Steps:
1. Create S3 bucket + DynamoDB table (backend-state)
2. Configure first project to use backend (state-1)
3. Use second folder to observe locking (state-2)

Notes:
- Bucket name must be globally unique (add name + random suffix)
- Use same region for all resources
- Backend block cannot use interpolations; values are static (or passed via -backend-config)

### 1. Create backend resources (backend-state)

```bash
mkdir backend-state
cd backend-state
```

Create `main.tf`:
```t
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }
}

provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile
}

variable "aws_region" {
  type        = string
  description = "AWS region"
  default     = "eu-west-1" # Change if you were assigned another region
}

variable "aws_profile" {
  type        = string
  description = "AWS CLI / SSO profile name"
  default     = "default" # Your profile name
}

variable "name" {
  type        = string
  description = "Name prefix (your name)"
  default     = "your-name" # Replace with your name
}

resource "random_string" "suffix" {
  length  = 6
  upper   = false
  numeric = true
  special = false
}

locals {
  bucket_name = "tfstate-${var.name}-${random_string.suffix.result}" # ex: tfstate-ahmet-381920
}

resource "aws_s3_bucket" "tfstate" {
  bucket = local.bucket_name

  tags = {
    Name        = "terraform-tfstate"
    Environment = "training"
  }
}

resource "aws_s3_bucket_versioning" "tfstate_versioning" {
  bucket = aws_s3_bucket.tfstate.id
  versioning_configuration { 
    status = "Enabled" 
    }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tfstate_sse" {
  bucket = aws_s3_bucket.tfstate.id
  rule {
    apply_server_side_encryption_by_default { 
      sse_algorithm = "AES256" 
      }
  }
}

resource "aws_dynamodb_table" "tf_locks" {
  name         = "terraform-locks-${var.name}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }

  tags = {
    Name = "terraform-locks"
  }
}

output "s3_bucket_name" { 
  value = aws_s3_bucket.tfstate.bucket 
  }
output "dynamodb_table_name" { 
  value = aws_dynamodb_table.tf_locks.name 
  }
```

Run:
```bash
terraform init
terraform plan
terraform apply -auto-approve
```

Record the outputs: `s3_bucket_name` and `dynamodb_table_name` (used in backend config).

### 2. First project using the backend (state-1)

```bash
cd ..
mkdir state-1
cd state-1
```

`provider.tf`:
```t
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
  backend "s3" {
  bucket         = "<REPLACE_BUCKET_NAME>"          # from backend-state output
  key            = "envs/dev/network/terraform.tfstate" # logical separation via folders
  region         = "eu-west-1"                       # your region
    dynamodb_table = "<REPLACE_DYNAMODB_TABLE_NAME>"   # lock tablosu
    encrypt        = true
    profile        = "<profile_name>" # ensure backend uses same SSO profile
  }
}

provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile
}
```

`variables.tf`:
```t
variable "aws_region" { 
    type = string 
    default = "eu-west-1" 
    }
variable "aws_profile" { 
    type = string 
    default = "<profile_name>"
}
variable "name" { 
    type = string 
    default = "ahmet" 
}
```

`main.tf` (simple VPC setup):
```t
resource "aws_vpc" "vpc" {
  cidr_block = "10.20.0.0/16"
  tags = { 
    Name = "remote-vpc-${var.name}" 
    }
}

resource "aws_subnet" "subnet" {
  vpc_id     = aws_vpc.vpc.id
  cidr_block = "10.20.1.0/24"
  tags = { 
    Name = "remote-subnet-${var.name}" 
    }
}

output "vpc_id" { 
  value = aws_vpc.vpc.id 
  }
output "subnet_id" { 
  value = aws_subnet.subnet.id 
  }
```

On first `terraform init` you will be asked to migrate state:
```bash
terraform init
terraform plan
terraform apply -auto-approve
```

Check S3 for `envs/dev/network/terraform.tfstate` and DynamoDB for a temporary LockID item during apply.

### 3. Observe locking with second folder (state-2)

```bash
cd ..
mkdir state-2
cd state-2
```

`provider.tf` (aynı backend'i intentionally kullanıyoruz):
```t
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
  backend "s3" {
    bucket         = "<REPLACE_BUCKET_NAME>"
    key            = "envs/dev/network/terraform.tfstate"
    region         = "eu-west-1"
    dynamodb_table = "<REPLACE_DYNAMODB_TABLE_NAME>"
    encrypt        = true
    profile        = "<profile_name>" # ensure backend uses same SSO profile
  }
}

provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile
}
```

`variables.tf` (aynı):
```t
variable "aws_region" { 
    type = string 
    default = "eu-west-1" 
    }
variable "aws_profile" { 
    type = string 
    default = "<profile_name>"
}
variable "name" { 
    type = string 
    default = "abuzittin" 
}
```

`main.tf` (aynı kaynakları bilerek tekrar tanımlıyoruz):
```t
resource "aws_vpc" "vpc" {
  cidr_block = "10.20.0.0/16"
  tags = { 
    Name = "remote-vpc-${var.name}" 
    }
}
```

Lock test
1. Ayrı bir terminalde `state-1` klasörüne gidip `terraform apply` başlatın (bitirmeden bekletin ya da plan sonrasında `-lock-timeout=300s` ile uzun bir işlem tetikleyin).
2. Aynı anda `state-2` klasöründe `terraform plan` çalıştırın.

Örnek hata çıktısı:
```text
Error: Error acquiring the state lock
Error message: ConditionalCheckFailedException: The conditional request failed
Lock Info:
  ID:        <uuid>
  Path:      envs/dev/network/terraform.tfstate
  Operation: OperationTypeApply
  Who:       your-user@host
  Version:   1.9.6
  Created:   2025-09-27 12:34:56.123 +0000 UTC
```

Do not manually remove the lock unless a run is stuck; it auto releases. Manual delete in DynamoDB is last resort.

Cleanup:
```bash
cd ../state-1
terraform destroy -auto-approve
cd ../backend-state
terraform destroy -auto-approve
```

Note: If destroy hangs, ensure bucket objects/versions are emptied (versioned deletions can take time).

Tip: Reconfigure backend
Backend config değiştiğinde:
```bash
terraform init -reconfigure
```

Tip: Pass values via CLI
`bucket` veya `dynamodb_table` gibi değerleri kodda sabitlemek istemiyorsanız backend-state çıktısından sonra ilk init ederken:
```bash
terraform init \
  -backend-config="bucket=tfstate-abuzittin-381920" \
  -backend-config="key=envs/dev/network/terraform.tfstate" \
  -backend-config="region=eu-west-1" \
  -backend-config="dynamodb_table=terraform-locks-abuzittin" \
  -backend-config="encrypt=true"
```

Remote backend (AWS S3 + DynamoDB) setup complete.

## Part 4 - Terraform Provisioners

- Provisioners can be used to model specific actions on the local machine or on a remote machine in order to prepare servers or other infrastructure objects for service.

- The `local-exec` provisioner invokes a local executable after a resource is created. This invokes a process on the machine running Terraform, not on the resource.

- The `remote-exec` provisioner invokes a script on a remote resource after it is created. This can be used to run a configuration management tool, bootstrap into a cluster, etc. To invoke a local process, see the local-exec provisioner instead. The remote-exec provisioner supports both ssh and winrm type connections.

- The `file` provisioner is used to copy files or directories from the machine executing Terraform to the newly created resource. The file provisioner supports both ssh and winrm type connections.

- Most provisioners require access to the remote resource via SSH or WinRM, and expect a nested connection block with details about how to connect. Connection blocks don't take a block label, and can be nested within either a resource or a provisioner.

- The `self` object represents the provisioner's parent resource, and has all of that resource's attributes. For example, use `self.public_ip` to reference an aws_instance's public_ip attribute.

- Take your `pem file` to your local instance's home folder for using `remote-exec` provisioner.

- Go to your local machine and run the following command. 

```bash
scp -i ~/.ssh/<your pem file> <your pem file> ec2-user@<terraform instance public ip>:/home/ec2-user
```

- Or you can drag and drop your pem file to VS Code. Then change permissions of the pem file.

```bash
chmod 400 <your pem file>
```

```bash
cd ..
mkdir provisioner
cd provisioner
```

```t
terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

resource "aws_instance" "instance" {
  ami = var.ec2_ami
  instance_type = var.ec2_type
  key_name = var.ec2_key_name
  vpc_security_group_ids = [ aws_security_group.tf-sec-gr.id ]
  tags = {
    Name = "terraform-instance-with-provisioner"
  }

  provisioner "local-exec" {
      command = "echo http://${self.public_ip} > public_ip.txt"
  
  }

  connection {
    host = self.public_ip
    type = "ssh"
    user = "ubuntu"
    private_key = file("~/ozia.pem") 
  }

  provisioner "remote-exec" {
    inline = [
      "sudo apt -y update",
      "sudo apt -y install nginx",
      "sudo systemctl enable nginx",
      "sudo systemctl start nginx"
    ]
  }

  provisioner "file" {
    content = self.public_ip
    destination = "/home/ubuntu/my_public_ip.txt"
  }
}

resource "aws_security_group" "tf-sec-gr" {
  name = "tf-provisioner-sg"
  tags = {
    Name = "tf-provisioner-sg"
  }

  ingress {
    from_port   = 80
    protocol    = "tcp"
    to_port     = 80
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
      from_port = 22
      protocol = "tcp"
      to_port = 22
      cidr_blocks = [ "0.0.0.0/0" ]
  }

  egress {
      from_port = 0
      protocol = -1
      to_port = 0
      cidr_blocks = [ "0.0.0.0/0" ]
  }
}

variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "us-east-1"
}

variable "ec2_ami" {
  description = "EC2 instance AMI"
  type        = string
  default     = "ami-0360c520857e3138f" 
}

variable "ec2_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t2.micro"
}

variable "ec2_key_name" {
  description = "EC2 Key"
  type        = string
  default     = "ozia"
}

```

- Go to the Provisioners folder and run the terraform file.

```bash
terraform init
terraform apply
```

- Check the resources that created by terraform.

- Terminate the resources.

```bash
terraform destroy
```