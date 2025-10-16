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
> **Example:** If your name is "Zülüf" and you're assigned "eu-west-1":
> - Region: `eu-west-1`
> - VPC Name: `terraform-vpc-
> - Subnet Name: `terraform-subnet-abuzittin`

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
> - **VPC Name:** `terraform-vpc-<your-name>` (e.g., `terraform-vpc-abuzittin`)
>   - **CIDR Block:** `10.0.0.0/16`
> - **Subnet Name:** `terraform-subnet-<your-name>` (e.g., `terraform-subnet-abuzittin`)
>   - **CIDR Block:** `10.0.1.0/24`
>   - **Important:** Make sure to select the correct VPC (`terraform-vpc-<your-name>`) when creating this subnet
> - **Security Group Name:** `terraform-security-group-<your-name>` (e.g., `terraform-security-group-abuzittin`)
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
      version = "~> 6.13.0"
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
>    - **Name tag:** `terraform-import-vpc-<your-name>` (e.g., `terraform-import-vpc-abuzittin`)
>    - **IPv4 CIDR block:** `10.0.0.0/16`
>    - Leave other settings as default
>    - Click "Create VPC"
>    - **Copy the VPC ID** (e.g., `vpc-0a1b2c3d4e5f6g7h8`) - you'll need this for the import command
>
> 2. **Create a Subnet:**
>    - In VPC Dashboard, go to "Subnets"
>    - Click "Create subnet"
>    - **VPC:** Select the VPC you just created (`terraform-import-vpc-<your-name>`)
>    - **Subnet name:** `terraform-import-subnet-<your-name>` (e.g., `terraform-import-subnet-abuzittin`)
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
      version = "~> 6.13.0"
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
      version = "~> 6.13.0"
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
  bucket_name = "tfstate-${var.name}-${random_string.suffix.result}" # ex: tfstate-abuzittin-381920
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
  versioning_configuration { status = "Enabled" }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tfstate_sse" {
  bucket = aws_s3_bucket.tfstate.id
  rule {
    apply_server_side_encryption_by_default { sse_algorithm = "AES256" }
  }
}

resource "aws_dynamodb_table" "tf_locks" {
  name         = "terraform-locks-${var.name}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute { name = "LockID" type = "S" }

  tags = {
    Name = "terraform-locks"
  }
}

output "s3_bucket_name" { value = aws_s3_bucket.tfstate.bucket }
output "dynamodb_table_name" { value = aws_dynamodb_table.tf_locks.name }
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
      version = "~> 6.13.0"
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
    default = "abuzittin" 
}
```

`main.tf` (simple VPC setup):
```t
resource "aws_vpc" "vpc" {
  cidr_block = "10.20.0.0/16"
  tags = { Name = "remote-vpc-${var.name}" }
}

resource "aws_subnet" "subnet" {
  vpc_id     = aws_vpc.vpc.id
  cidr_block = "10.20.1.0/24"
  tags = { Name = "remote-subnet-${var.name}" }
}

output "vpc_id" { value = aws_vpc.vpc.id }
output "subnet_id" { value = aws_subnet.subnet.id }
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
      version = "~> 6.13.0"
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
  tags = { Name = "remote-vpc-${var.name}" }
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

- `Provisioners` in Terraform allow you to execute scripts or commands on your resources after they are created. This is particularly useful for configuring applications or services, performing setup tasks, or running scripts to install software.

- In this part, we will use two types of provisioners; 
  `local-exec`: Executes a command on the machine where Terraform is run. This is often used to perform local actions or logging tasks.
  `remote-exec`: Executes commands on a remote resource, such as a virtual machine. This is typically used to configure software on the newly created VM after it's been provisioned.
  
- `Custom Data` allows you to pass configuration scripts or data directly to a newly created resource, such as a virtual machine. This is often used to automate the initial setup of the VM by executing scripts upon boot.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg-gokhan"
  location = "West Europe"
  tags = {
    Name = "terraform-rg-gokhan"
  }
}
resource "azurerm_virtual_network" "vnet" {
  name                = "terraform-vnet"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  address_space       = ["10.0.0.0/16"]
}
resource "azurerm_subnet" "subnet" {
  name                 = "terraform-subnet"
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.1.0/24"]
}
resource "azurerm_network_interface" "nic" {
  name                = "terraform-nic"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  ip_configuration {
    name                          = "internal"
    subnet_id                    = azurerm_subnet.subnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id = azurerm_public_ip.public_ip.id 
  }
}
resource "azurerm_public_ip" "public_ip" {
  name                = "acceptanceTestPublicIp1"
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  allocation_method   = "Static"
  tags = {
    environment = "terraform_public_ip"
  }
 }
resource "azurerm_network_security_group" "security_group" {
  name                = "terraform-sec-grp"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  security_rule {
    name                       = "allow-ssh"
    priority                   = 1000
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
  security_rule {
    name                       = "allow-http"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "80"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}
resource "azurerm_network_interface_security_group_association" "nic_sec_association" {
  network_interface_id      = azurerm_network_interface.nic.id
  network_security_group_id = azurerm_network_security_group.security_group.id
  depends_on = [ azurerm_network_interface.nic, azurerm_network_security_group.security_group ]
}
resource "azurerm_virtual_machine" "vm" {
  name                  = "terraform-vm"
  location              = azurerm_resource_group.resource_group.location
  resource_group_name   = azurerm_resource_group.resource_group.name
  network_interface_ids = [azurerm_network_interface.nic.id]
  vm_size               = "Standard_DS1_v2"

  delete_os_disk_on_termination = true

  delete_data_disks_on_termination = true

  storage_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts"
    version   = "latest"
  }
  storage_os_disk {
    name              = "myosdisk2"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Standard_LRS"
  }
  os_profile {
    computer_name  = "hostname"
    admin_username = "testadmin"
    admin_password = "Password1234!"
  }
  os_profile_linux_config {
    disable_password_authentication = false
  }
  provisioner "local-exec" {
    command = "echo Azure VM oluşturuldu: ${azurerm_virtual_machine.vm.name} IP Adresi: ${azurerm_network_interface.nic.private_ip_address} > local-exec.txt " 
  }
  provisioner "remote-exec" {
    connection {
      type     = "ssh"
      user     = "testadmin"
      password = "Password1234!"
      host     = azurerm_public_ip.public_ip.ip_address
      timeout  = "20m"
    }
    inline = [
      "echo Azure VM oluşturuldu: ${azurerm_virtual_machine.vm.name} IP Adresi: ${azurerm_network_interface.nic.private_ip_address} > /home/testadmin/remote-exec.txt "
    ]
  }
  tags = {
    environment = "terraform"
  }
}
output "vm_ip" {
  value = azurerm_network_interface.nic.private_ip_address
}
output "vm_public_ip" {
  value = azurerm_public_ip.public_ip.ip_address
}
```

```bash
terraform apply
terraform destroy
```

```t
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg-gokhan"
  location = "West Europe"
  tags = {
    Name = "terraform-rg-gokhan"
  }
}
resource "azurerm_virtual_network" "vnet" {
  name                = "terraform-vnet"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  address_space       = ["10.0.0.0/16"]
}
resource "azurerm_subnet" "subnet" {
  name                 = "terraform-subnet"
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.1.0/24"]
}
resource "azurerm_network_interface" "nic" {
  name                = "terraform-nic"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  ip_configuration {
    name                          = "internal"
    subnet_id                    = azurerm_subnet.subnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id = azurerm_public_ip.public_ip.id 
  }
}
resource "azurerm_public_ip" "public_ip" {
  name                = "acceptanceTestPublicIp1"
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  allocation_method   = "Static"
  tags = {
    environment = "terraform_public_ip"
  }
 }
resource "azurerm_network_security_group" "security_group" {
  name                = "terraform-sec-grp"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  security_rule {
    name                       = "allow-ssh"
    priority                   = 1000
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
  security_rule {
    name                       = "allow-http"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "80"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}
resource "azurerm_network_interface_security_group_association" "nic_sec_association" {
  network_interface_id      = azurerm_network_interface.nic.id
  network_security_group_id = azurerm_network_security_group.security_group.id
  depends_on = [ azurerm_network_interface.nic, azurerm_network_security_group.security_group ]
}
resource "azurerm_virtual_machine" "vm" {
  name                  = "terraform-vm"
  location              = azurerm_resource_group.resource_group.location
  resource_group_name   = azurerm_resource_group.resource_group.name
  network_interface_ids = [azurerm_network_interface.nic.id]
  vm_size               = "Standard_DS1_v2"

  delete_os_disk_on_termination = true

  delete_data_disks_on_termination = true

  storage_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts"
    version   = "latest"
  }
  storage_os_disk {
    name              = "myosdisk2"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Standard_LRS"
  }
  os_profile {
    computer_name  = "hostname"
    admin_username = "testadmin"
    admin_password = "Password1234!"
    custom_data    = <<-EOF
                #!/bin/bash
                sudo apt update
                sudo apt install -y nginx
                sudo systemctl start nginx
                sudo systemctl enable nginx
                EOF
  }
  os_profile_linux_config {
    disable_password_authentication = false
  }
  tags = {
    environment = "terraform"
  }
}
output "vm_ip" {
  value = azurerm_network_interface.nic.private_ip_address
}
output "vm_public_ip" {
  value = azurerm_public_ip.public_ip.ip_address
}
```

```bash
terraform apply
terraform destroy
```