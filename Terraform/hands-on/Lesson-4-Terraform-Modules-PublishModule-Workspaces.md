# Terraform Modules, Publish Module and Workspaces Hands-On

In this session, we'll explore using the terraform modules, publishing modules and terraform workspaces.

- Part 1 - Using The Terraform Modules
- Part 2 - Publishing Module
- Part 3 - Terraform Workspace

## Part 1 -  Using The Terraform Modules

## Using Local Module

- Go to the terraform-project folder.

```txt
/terraform-project
│
├── /modules               
│   ├── /resource_group
│   ├── /virtual_network
│   └── /virtual_machine
│   ├── /acr
│
├── /environments
│   ├── /dev               
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   └── terraform.tfvars
│   │
│   └── /prod             
│       ├── main.tf
│       ├── variables.tf
│       ├── outputs.tf
│       └── terraform.tfvars

```
- Go to the modules/resource_group folder. Create main.tf, variable.tf and output.tf.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}"
  }
}
```
```t
variable "rg_name" {
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    type = string
    description = "resource_group location"
}
```
```t
output "rg_name" {
  value = azurerm_resource_group.resource_group.name
}
output "rg_location" {
  value = azurerm_resource_group.resource_group.location
}
```

- Go to the modules/acr folder. Create main.tf, variable.tf and output.tf.

```t
resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = var.rg_name
  location            = var.rg_location
  sku                 = var.acr_sku
  admin_enabled       = var.acr_admin
}
```
```t
variable "rg_name" {
  description = "Resource Group name, this should come from the resource group module"
  type        = string
}
variable "rg_location" {
  description = "Location, this should come from the resource group module"
  type        = string
}
variable "acr_name" {
    type = string
    description = "acr name"
}
variable "acr_sku" {
    type = string
    description = "acr_sku type"
}
variable "acr_admin" {
    type = bool
    description = "acr admin value"
}
```
```t
output "acr_id" {
  value = azurerm_container_registry.acr.id
}
output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}
```

- Go to the modules/virtual_network folder. Create main.tf, variable.tf and output.tf.

```t
resource "azurerm_virtual_network" "vnet" {
  name                = var.vnet_name
  location            = var.rg_location  
  resource_group_name = var.rg_name
  address_space       = var.vnet_cidr
}
resource "azurerm_subnet" "subnet" {
  name                 = var.subnet_name
  resource_group_name  = var.rg_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = var.subnet_cidr
}
resource "azurerm_public_ip" "public_ip" {
  name                = var.public_ip_name
  resource_group_name = var.rg_name
  location            = var.rg_location
  allocation_method   = var.public_ip_allocation
  tags = {
    Name = var.public_ip_name
  }
}
resource "azurerm_network_interface" "nic" {
  name                = var.nic_name
  location            = var.rg_location
  resource_group_name = var.rg_name
  
  ip_configuration {
    name                          = var.nic_ip_conf_name
    subnet_id                    = azurerm_subnet.subnet.id
    private_ip_address_allocation = var.nic_ip_conf_private_ip
    public_ip_address_id         = azurerm_public_ip.public_ip.id
  }
}
resource "azurerm_network_security_group" "security_group" {
  name                = var.security_group_name
  location            = var.rg_location
  resource_group_name = var.rg_name

  dynamic "security_rule" {
    for_each = var.allowed_ports
    content {
      name                       = "allow-${security_rule.value}"
      priority                   = 1000 + security_rule.value
      direction                  = "Inbound"
      access                     = "Allow"
      protocol                   = "Tcp"
      source_port_range          = "*"
      destination_port_range     = security_rule.value
      source_address_prefix      = "*"
      destination_address_prefix = "*"
    }
  }
}
resource "azurerm_network_interface_security_group_association" "nic_security_group_association" {
  network_interface_id = azurerm_network_interface.nic.id
  network_security_group_id = azurerm_network_security_group.security_group.id
}
```
```t
variable "rg_name" {
  description = "Resource Group name, this should come from the resource group module"
  type        = string
}
variable "rg_location" {
  description = "Location, this should come from the resource group module"
  type        = string
}
variable "vnet_name" {
    type = string
    description = "vnet name"
}
variable "vnet_cidr" {
    type = list
    description = "vnet cidr block"
}
variable "subnet_name" {
    type = string
    description = "subnet name"
}
variable "subnet_cidr" {
    type = list
    description = "subnet cidr block"
}
variable "nic_name" {
    type = string
    description = "network interface name"
}
variable "nic_ip_conf_name" {
    type = string
    description = "network interface ip configuration name"
}
variable "nic_ip_conf_private_ip" {
    type = string
    description = "network interface ip configuration private ip address allocation"
}
variable "public_ip_name" {
    type = string
    description = "public ip name"
}
variable "public_ip_allocation" {
    type = string
    description = "public ip allocation method"
}
variable "allowed_ports" {
  description = "List of allowed ports"
  type        = list(number)
}
variable "security_group_name" {
    type = string
    description = "security group name"
}
```
```t
output "vnet_id" {
  description = "The ID of the virtual network."
  value       = azurerm_virtual_network.vnet.id
}
output "vnet_address_space" {
  description = "The address space of the virtual network."
  value       = azurerm_virtual_network.vnet.address_space
}
output "subnet_ids" {
  description = "The IDs of the subnets."
  value       = azurerm_subnet.subnet[*].id
}
output "public_ip_ids" {
  description = "The IDs of the public IP addresses."
  value       = azurerm_public_ip.public_ip[*].id
}
output "nic_ids" {
  description = "List of network interface IDs"
  value       = azurerm_network_interface.nic[*].id
}
output "security_group_id" {
  description = "The ID of the network security group."
  value       = azurerm_network_security_group.security_group.id
}
output "security_group_name" {
  description = "The name of the network security group."
  value       = azurerm_network_security_group.security_group.name
}
output "vm_public_ip" {
  value = azurerm_public_ip.public_ip.ip_address
}
```

- Go to the modules/virtual_machine folder. Create main.tf, variable.tf and output.tf.

```t
resource "azurerm_virtual_machine" "vm" {
  name                = var.vm_name
  location            = var.rg_location
  resource_group_name = var.rg_name
  network_interface_ids = var.nic_ids
  vm_size             = var.vm_size

  delete_os_disk_on_termination    = true
  delete_data_disks_on_termination = true

  storage_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts"
    version   = "latest"
  }

  storage_os_disk {
    name              = "${var.vm_name}-osdisk"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Standard_LRS"
  }

  os_profile {
    computer_name  = var.vm_name
    admin_username = var.admin_username
    admin_password = var.admin_password
    custom_data    = var.custom_data
  }

  os_profile_linux_config {
    disable_password_authentication = false
  }

  tags = var.tags
}
```
```t
variable "rg_name" {
  description = "Resource Group name, this should come from the resource group module"
  type        = string
}
variable "rg_location" {
  description = "Location, this should come from the resource group module"
  type        = string
}
variable "nic_ids" {
  description = "List of network interface IDs attached to the Virtual Machine"
  type        = list(string)
}
variable "vm_name" {
  description = "Virtual Machine name"
  type        = string
}
variable "vm_size" {
  description = "Size of the Virtual Machine"
  type        = string
}
variable "admin_username" {
  description = "Admin username for the Virtual Machine"
  type        = string
}
variable "admin_password" {
  description = "Admin password for the Virtual Machine"
  type        = string
  sensitive   = true
}
variable "custom_data" {
  description = "Custom data (Cloud-init or bash script)"
  type        = string
  default     = ""
}
variable "tags" {
  description = "Tags for the Virtual Machine"
  type        = map(string)
  default     = {}
}
```
```t
output "vm_name" {
  description = "The name of the Virtual Machine"
  value       = azurerm_virtual_machine.vm.name
}
output "vm_network_interface_ids" {
  description = "The list of network interface IDs attached to the VM"
  value       = azurerm_virtual_machine.vm.network_interface_ids
}
output "vm_id" {
  value = azurerm_virtual_machine.vm[*].id
}
```

- Go to the /environments/dev and /environments/prod folders. Create provider.tf, main.tf, variable.tf, terraform.tfvars and output.tf.

```t
module "resource_group" {
  source      = "../../modules/resource_group"
  rg_name     = var.rg_name
  rg_location = var.rg_location
}

module "acr" {
  source      = "../../modules/acr"
  rg_name     = module.resource_group.rg_name
  rg_location = module.resource_group.rg_location
  acr_name    = var.acr_name
  acr_sku     = var.acr_sku
  acr_admin   = var.acr_admin
}

module "virtual_network" {
  source                 = "../../modules/virtual_network"
  rg_name                = module.resource_group.rg_name
  rg_location            = module.resource_group.rg_location
  vnet_name              = var.vnet_name
  vnet_cidr              = var.vnet_cidr
  subnet_name            = var.subnet_name
  subnet_cidr            = var.subnet_cidr
  nic_name               = var.nic_name
  nic_ip_conf_name       = var.nic_ip_conf_name
  nic_ip_conf_private_ip = var.nic_ip_conf_private_ip
  public_ip_name         = var.public_ip_name
  public_ip_allocation   = var.public_ip_allocation
  allowed_ports          = var.allowed_ports
  security_group_name    = var.security_group_name
}

module "virtual_machine" {
  source         = "../../modules/virtual_machine"
  vm_name        = var.vm_name
  rg_location    = module.resource_group.rg_location
  rg_name        = module.resource_group.rg_name
  nic_ids        = module.virtual_network.nic_ids
  vm_size        = var.vm_size
  admin_username = var.admin_username
  admin_password = var.admin_password
  custom_data    = var.custom_data
  tags           = var.tags
  depends_on = [ module.virtual_network, module.resource_group ]
}
```
```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
}
provider "azurerm" {
  features {
  }
  subscription_id = <your_subscription_id>  #change me
}
```
```t
## resource group
variable "rg_name" {
  type        = string
  description = "The name of the Resource Group"
}
variable "rg_location" {
  type        = string
  description = "The Azure location"
}
## acr
variable "acr_name" {
  type        = string
  description = "The name of the ACR instance"
}
variable "acr_sku" {
  type        = string
  description = "acr_sku type"
}
variable "acr_admin" {
  type        = bool
  description = "acr admin value"
}
## virtual network
variable "vnet_name" {
  default     = "terraform-vnet"
  type        = string
  description = "vnet name"
}
variable "vnet_cidr" {
  type        = list(any)
  description = "vnet cidr block"
}
variable "subnet_name" {
  type        = string
  description = "vnet name"
}
variable "subnet_cidr" {
  type        = list(any)
  description = "subnet cidr block"
}
variable "nic_name" {
  type        = string
  description = "network interface name"
}
variable "nic_ip_conf_name" {
  type        = string
  description = "network interface ip configuration name"
}
variable "nic_ip_conf_private_ip" {
  type        = string
  description = "network interface ip configuration private ip address allocation"
}
variable "public_ip_name" {
  type        = string
  description = "public ip name"
}
variable "public_ip_allocation" {
  type        = string
  description = "public ip allocation method"
}
variable "allowed_ports" {
  description = "List of allowed ports"
  type        = list(number)
}
variable "security_group_name" {
  type        = string
  description = "security group name"
}
## virtual machine
variable "vm_name" {
  description = "virtual machine"
  type        = string
}
variable "vm_size" {
  description = "virtual machine size"
  type        = string
}
variable "admin_username" {
  description = "virtual machine username"
  type        = string
}
variable "admin_password" {
  description = "virtual machine password"
  type        = string
}
variable "custom_data" {
  description = "virtual machine custom data"
  type        = string
}
variable "tags" {
  description = "virtual machine tags"
  type        = map(string)
}
```
```t
rg_name                = "terraform-gokhan-dev"
rg_location            = "West Europe"
acr_name               = "terraformgokhanacrdev"
acr_sku                = "Premium"
acr_admin              = "false"
vnet_name              = "terraform-vnet"
vnet_cidr              = ["10.0.0.0/16"]
subnet_name            = "terraform-subnet"
subnet_cidr            = ["10.0.1.0/24"]
nic_name               = "terraform-nic"
nic_ip_conf_name       = "internal"
nic_ip_conf_private_ip = "Dynamic"
public_ip_name         = "terraformgokhanpublicipdev"
public_ip_allocation   = "Static"
allowed_ports          = [22, 80, 443]
security_group_name    = "terraform-security-group"
admin_username         = "testadmin"
admin_password         = "Password1234!"
vm_size                = "Standard_DS1_v2"
vm_name                = "terraform-vm"
custom_data            = <<-EOF
  #!/bin/bash
  sudo apt update
  sudo apt install -y nginx
  sudo systemctl start nginx
  sudo systemctl enable nginx
EOF
tags = {
  environment = "terraform"
}
```
```t
output "vm_public_ip" {
  value = module.virtual_network.vm_public_ip
}
```
- Go to the prod folder.

```bash
terraform init
terraform apply
```

- Go to the dev folder.

```bash
terraform init
terraform apply
```

- Go to the `prod` and  `dev` folders and run the command below.

```bash
terraform destroy
```

## Using Registry Module

- Go to the terraform registry and search public modules.

- Go to the registry folder. Create provider.tf and main.tf.

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.11, < 4.0"
    }
  }
}
provider "azurerm" {
  features {
  }
  subscription_id = <your_subscription_id>  #change me
}
```
```t
module "vnet" {
  source  = "Azure/vnet/azurerm"
  version = "4.1.0"
  resource_group_name = azurerm_resource_group.resource_group.name
  vnet_location = azurerm_resource_group.resource_group.location
  use_for_each = false
}
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-gokhan"
  location = "West Europe"
  tags = {
    Name = "terraform-gokhan"
  }
}
```

```bash
terraform init
terraform apply

```bash
terraform destroy
```

## Part 2 -  Publishing Module

- Anyone can publish and share modules on the Terraform Registry.

- Published modules support versioning, automatically generate documentation, allow browsing version histories, show examples and READMEs, and more. Terraform recommend publishing reusable modules to a registry.

- Public modules are managed via ``Git`` and ``GitHub``. Once a module is published, you can release a new version of a module by simply pushing a properly formed Git tag.

## Requirements

- The list below contains all the requirements for publishing a module:

* ``GitHub``. The module must be on GitHub and must be a ``public`` repo. This is only a requirement for the public registry. If you're using a private registry, you may ignore this requirement.

* ``Named`` terraform-<PROVIDER>-<NAME>. Module repositories must use this three-part name format, where <NAME> reflects the type of infrastructure the module manages and <PROVIDER> is the main provider where it creates that infrastructure. The <NAME> segment can contain additional hyphens. Examples: terraform-azurerm-vnet-vm or terraform-google-vault or terraform-aws-ec2-instance.

* ``Repository description``. The GitHub repository description is used to populate the short description of the module. This should be a simple one sentence description of the module.

* ``Standard module structure``. The module must adhere to the standard module structure. This allows the registry to inspect your module and generate documentation, track resource usage, parse submodules and examples, and more.

* ``x.y.z tags for releases``. The registry uses tags to identify module versions. Release tag names must be a semantic version, which can optionally be prefixed with a v. For example, v1.0.4 and 0.9.2. To publish a module initially, at least one release tag must be present. Tags that don't look like version numbers are ignored. (https://semver.org/)

- source link: https://www.terraform.io/registry/modules/publish

## Create a Github repository for our terraform module

- Create a `public` github repo and name it as `terraform-azurerm-vnet-vm folder`.

- Clone the repository to your local.

```bash
git clone https://github.com/<your-github-account>/terraform-azurerm-vnet-vm.git
```

- Go the terraform-azurerm-vnet-vm folder. Create provider.tf, resource_group.tf, virtual_network.tf, virtual_machine.tf and variable.tf.

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.4.0"
    }
  }
}
provider "azurerm" {
  features {
  }
  subscription_id = var.subscription_id
}
```
```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}"
  }
}
``` 
```t
resource "azurerm_virtual_network" "vnet" {
  name                = var.vnet_name
  location            = azurerm_resource_group.resource_group.location 
  resource_group_name = azurerm_resource_group.resource_group.name
  address_space       = var.vnet_cidr
}
resource "azurerm_subnet" "subnet" {
  name                 = var.subnet_name
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = var.subnet_cidr
}
resource "azurerm_public_ip" "public_ip" {
  name                = var.public_ip_name
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  allocation_method   = var.public_ip_allocation
  tags = {
    Name = var.public_ip_name
  }
}
resource "azurerm_network_interface" "nic" {
  name                = var.nic_name
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  
  ip_configuration {
    name                          = var.nic_ip_conf_name
    subnet_id                    = azurerm_subnet.subnet.id
    private_ip_address_allocation = var.nic_ip_conf_private_ip
    public_ip_address_id         = azurerm_public_ip.public_ip.id
  }
}
resource "azurerm_network_security_group" "security_group" {
  name                = var.security_group_name
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name

  dynamic "security_rule" {
    for_each = var.allowed_ports
    content {
      name                       = "allow-${security_rule.value}"
      priority                   = 1000 + security_rule.value
      direction                  = "Inbound"
      access                     = "Allow"
      protocol                   = "Tcp"
      source_port_range          = "*"
      destination_port_range     = security_rule.value
      source_address_prefix      = "*"
      destination_address_prefix = "*"
    }
  }
}
resource "azurerm_network_interface_security_group_association" "nic_security_group_association" {
  network_interface_id = azurerm_network_interface.nic.id
  network_security_group_id = azurerm_network_security_group.security_group.id
}
```
```t
resource "azurerm_virtual_machine" "vm" {
  name                = var.vm_name
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  network_interface_ids = [azurerm_network_interface.nic.id]
  vm_size             = var.vm_size

  delete_os_disk_on_termination    = true
  delete_data_disks_on_termination = true

  storage_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts"
    version   = "latest"
  }

  storage_os_disk {
    name              = "${var.vm_name}-osdisk"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Standard_LRS"
  }

  os_profile {
    computer_name  = var.vm_name
    admin_username = var.admin_username
    admin_password = var.admin_password
    custom_data    = var.custom_data
  }

  os_profile_linux_config {
    disable_password_authentication = false
  }

  tags = var.tags
}
```
```t
variable "subscription_id" {
    type = string
    description = "your subscription id"
}
variable "rg_name" {
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    type = string
    description = "resource_group location"
}
variable "vnet_name" {
    type = string
    description = "vnet name"
    default = "terraform-vnet"
}
variable "vnet_cidr" {
    type = list
    description = "vnet cidr block"
    default = ["10.0.0.0/16"]
}
variable "subnet_name" {
    type = string
    description = "subnet name"
    default = "terraform-subnet"
}
variable "subnet_cidr" {
    type = list
    description = "subnet cidr block"
    default = ["10.0.1.0/24"]
}
variable "nic_name" {
    type = string
    description = "network interface name"
    default = "terraform-nic"
}
variable "nic_ip_conf_name" {
    type = string
    description = "network interface ip configuration name"
    default = "internal"
}
variable "nic_ip_conf_private_ip" {
    type = string
    description = "network interface ip configuration private ip address allocation"
    default = "Dynamic"
}
variable "public_ip_name" {
    type = string
    description = "public ip name"
    default = "terraform-public-ip"
}
variable "public_ip_allocation" {
    type = string
    description = "public ip allocation method"
    default = "Static"
}
variable "allowed_ports" {
  description = "List of allowed ports"
  type        = list(number)
  default = [22, 80, 443]
}
variable "security_group_name" {
    type = string
    description = "security group name"
    default = "terraform-sec-grp"
}
variable "vm_name" {
  description = "Virtual Machine name"
  type        = string
  default = "terraform-vm"
}
variable "vm_size" {
  description = "Size of the Virtual Machine"
  type        = string
}
variable "admin_username" {
  description = "Admin username for the Virtual Machine"
  type        = string
}
variable "admin_password" {
  description = "Admin password for the Virtual Machine"
  type        = string
  sensitive   = true
}
variable "custom_data" {
  description = "Custom data (Cloud-init or bash script)"
  type        = string
  default     = ""
}
variable "tags" {
  description = "Tags for the Virtual Machine"
  type        = map(string)
  default     = {}
}
```

- Next, ``push`` the files to github repo and give a tag to version our module. You should give a semantic version to your module.

```bash
git add .
git commit -m "should define your key file"
git push
git tag v0.0.1
git push --tags
```

- Go to the `Terraform Registry` and sign in with your `Github Account`.

- Next, `Publish` your module.

* Terraform Registry --> Sign in --> Github account --> Publish --> Modules --> Select the module repo in Github (terraform-azurerm-vnet-vm) --> Click Agree in Terms --> Publish Module

- Go to the my_module folder. Create main.tf

```t
module "vnet-vm" {
  source  = "gokhanwell/vnet-vm/azurerm"
  version = "0.0.3"
  # insert the 6 required variables here
  admin_password = "password1234!"
  admin_username = "testadmin"
  rg_location = "West Europe"
  rg_name = "terraform_rg_gokhan_test"
  subscription_id = <your_subscription_id>  #change me
  vm_size = "Standard_DS1_v2"
}
```

```bash
terraform init
terraform apply
terraform destroy
```

## Part 3 -  Terraform Workspace

## When to use Multiple Workspaces

- Terraform relies on state to associate resources with real-world objects, so if you run the same configuration multiple times with completely separate state data, Terraform can manage many non-overlapping groups of resources. In some cases you'll want to change variable values for these different resource collections (like when specifying differences between staging and production deployments), and in other cases you might just want many instances of a particular infrastructure pattern.

- The simplest way to maintain multiple instances of a configuration with completely separate state data is to use multiple working directories.

- `Workspaces` allow you to use the same working copy of your configuration and the same plugin and module caches, while still keeping separate states for each collection of resources you manage.

- Every initialized working directory has at least one workspace. (If you haven't created other workspaces, it is a workspace named ``default``.)

- For a given working directory, only one workspace can be selected at a time.

- A common use for multiple workspaces is to create a parallel, distinct copy of a set of infrastructure in order to test a set of changes before modifying the main production infrastructure. For example, a developer working on a complex set of infrastructure changes might create a new temporary workspace in order to freely experiment with changes without affecting the default workspace.


- Go to the workspace folder. 

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
}
provider "azurerm" {
  features {
  }
  subscription_id = <your_subscription_id>  #change me
}
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}"
  }
}
variable "rg_name" {
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    type = string
    description = "resource_group location"
}
```
- Create prod.tfvars file.
```t
rg_name                = "terraform-gokhan-prod"
rg_location            = "West Europe"
```

- Create dev.tfvars file.
```t
rg_name                = "terraform-gokhan-dev"
rg_location            = "West Europe"
```

- Workspaces are managed with the ``terraform workspace`` set of commands. We can see the command options with `--help` flag.

```bash
terraform workspace --help
terraform workspace list
terraform workspace show
```

- Create two workspaces with names `dev` and `prod`.

```bash
terraform workspace new dev
terraform workspace new prod
terraform workspace list
terraform workspace show
terraform workspace select dev
```

- After creating namespaces, terraform creates new folders for new workspaces. Check the `workspace` folder and see the new folders.(`terraform.tfstate.d`)

- Run the following terraform commands to create resources in `dev` and `prod` workspaces.

```bash
terraform init

terraform workspace select prod
terraform workspace show
terraform plan -var-file prod.tfvars
terraform apply --auto-approve -var-file prod.tfvars

terraform destroy --auto-approve -var-file prod.tfvars

terraform workspace select dev
terraform workspace show
terraform plan -var-file dev.tfvars
terraform apply --auto-approve -var-file dev.tfvars

terraform destroy --auto-approve -var-file dev.tfvars

terraform workspace select default
terraform workspace show
```
- ``Delete`` the workspaces.

```bash
terraform workspace list
terraform workspace show
terraform workspace delete prod
terraform workspace delete dev
```