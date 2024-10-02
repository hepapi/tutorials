# Terraform Datasources RemoteBackend Provisioners

In this session, we'll explore resource dependencies, datasources and terraform import, version constraints, remote backends and terraform provisioners.

- Part 1 - Resource Dependencies, Datasources and Terraform Import
- Part 2 - Version Constraints
- Part 3 - Remote Backends
- Part 4 - Terraform Provisioners

## Part 1 - Resource Dependencies, Datasources and Terraform Import

## Implicit and Explicit Dependency

- Implicit dependency occurs automatically when one resource relies on the output or properties of another resource. Terraform identifies these relationships based on how resources reference each other.

- Explicit dependency is defined when the natural or implicit relationships between resources are not enough, or when resources should be created or destroyed in a specific order that Terraform can't infer. In these cases, you can use the `depends_on` argument to explicitly declare the dependency between resources.

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
  features {}
  subscription_id = <your_subscription_id>  #change me
}
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
  depends_on = [ azurerm_resource_group.resource_group ]
}
resource "azurerm_subnet" "subnet" {
  name                 = "terraform-subnet"
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.1.0/24"]
  depends_on = [ azurerm_virtual_network.vnet ]
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
    depends_on = [ azurerm_subnet.subnet ]
}
resource "azurerm_public_ip" "public_ip" {
  name                = "acceptanceTestPublicIp1"
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  allocation_method   = "Static"
  tags = {
    environment = "terraform_public_ip"
  }
  depends_on = [ azurerm_resource_group.resource_group ]
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
  depends_on = [ azurerm_virtual_network.vnet ]
}
resource "azurerm_network_interface_security_group_association" "nic_sec_association" {
  network_interface_id      = azurerm_network_interface.nic.id
  network_security_group_id = azurerm_network_security_group.security_group.id
  depends_on = [ azurerm_network_interface.nic, azurerm_network_security_group.security_group ]
}
```

```bash
terraform apply
terraform destroy
```

## Datasources

- `Datasources` are used to query and use the attributes of existing resources within Terraform. With data sources, you can retrieve information from resources that were created by another process or method and use that data in your Terraform configuration.

- Go to the Azure Portal. Create resource group, virtual network and subnet.

```t
data "azurerm_resource_group" "data_rg" {
  name = "terraform-rg-gokhan"
}
data "azurerm_virtual_network" "data_vnet" {
  name                = "terraform-vnet"
  resource_group_name = "terraform-rg-gokhan"
}
data "azurerm_subnet" "data_subnet" {
  name                 = "terraform-subnet"
  virtual_network_name = "terraform-vnet"
  resource_group_name  = "terraform-rg-gokhan"
}
output "id" {
  value = data.azurerm_resource_group.data_rg.id
}
output "virtual_network_id" {
  value = data.azurerm_virtual_network.data_vnet.id
}
output "subnet_id" {
  value = data.azurerm_subnet.data_subnet.id
}
```
```t
resource "azurerm_network_interface" "nic" {
  name                = "terraform-nic"
  location            = data.azurerm_resource_group.data_rg.location
  resource_group_name = data.azurerm_resource_group.data_rg.name
  ip_configuration {
    name                          = "internal"
    subnet_id                    = data.azurerm_subnet.data_subnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id = azurerm_public_ip.public_ip.id 
  }
}
resource "azurerm_public_ip" "public_ip" {
  name                = "acceptanceTestPublicIp1"
  resource_group_name = data.azurerm_resource_group.data_rg.name
  location            = data.azurerm_resource_group.data_rg.location
  allocation_method   = "Static"
  tags = {
    environment = "terraform_public_ip"
  }
 }
resource "azurerm_network_security_group" "security_group" {
  name                = "terraform-sec-grp"
  location            = data.azurerm_resource_group.data_rg.location
  resource_group_name = data.azurerm_resource_group.data_rg.name
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
```
```bash
terraform apply
terraform destroy
```

## Terraform Import

- The terraform import command allows you to bring existing resources into Terraform management. 

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
  location            = "West Europe"
  resource_group_name = "terraform-rg-gokhan"
  address_space       = ["10.0.0.0/16"]
}
```
```bash
terraform import azurerm_resource_group.resource_group /subscriptions/12345678-1234-9876-4563-123456789012/resourceGroups/terraform-rg-gokhan
terraform import azurerm_virtual_network.vnet /subscriptions/12345678-1234-9876-4563-123456789012/resourceGroups/terraform-rg-gokhan/providers/Microsoft.Network/virtualNetworks/terraform-vnet
```

```bash
terraform destroy
```

## Part 2 - Version Constraints

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
}
```
```t
      version = "!= 4.5.0"
      version = "< 4.5.0"
      version = "~> 4.1.0"
      version = ">4.0.0, != 4.4.0, < 4.5.0"
```
- version = "4.3.0": This specifies that only version 4.3.0 of the azurerm provider will be used. No other versions are allowed.

- version = "!= 4.5.0": This excludes version 4.5.0, meaning any other version can be used except for 4.5.0. If no additional constraints are added, any version except 4.5.0 is allowed.

- version = "< 4.5.0": This specifies that any version less than 4.5.0 can be used.

- version = "~> 4.1.0": This allows for versions in the 4.1.x range, meaning it supports 4.1.0 and other compatible patch releases (e.g., 4.1.1, 4.1.2). However, it does not include 4.2.0 and above.

- version = "> 4.0.0, != 4.4.0, < 4.5.0": This more complex constraint allows for versions greater than 4.0.0 and less than 4.5.0, while explicitly excluding version 4.4.0.

## Part 3 - Remote Backends

- When using remote backends in Terraform, it's important to set up your configuration properly to manage state files securely and collaboratively. The example you provided outlines the setup for using Azure as a remote backend with Terraform.

- Create backend-state folder and go to the folder.

```t
terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
}
provider "azurerm" {
  features {}
  subscription_id = <your_subscription_id>  #change me
}
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg-tfstate"
  location = "West Europe"
  tags = {
    Name = "terraform-rg-tfstate"
  }
}
resource "azurerm_storage_account" "terraform_storage" {
  name                     = "terraformstoragegokhan"
  resource_group_name      = azurerm_resource_group.resource_group.name
  location                 = azurerm_resource_group.resource_group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags = { 
    Name = "terraformstorage"
  }
}
resource "azurerm_storage_container" "tfstate_container" {
  name                  = "terraformcontainergokhan"
  storage_account_name  = azurerm_storage_account.terraform_storage.name
  container_access_type = "private"
}
```
```bash
terraform init
terraform apply
```
- Create state-1 folder and go to the folder.

```t
terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "terraform-rg-gokhan"
    storage_account_name = "terraformstoragegokhan"
    container_name       = "terraformcontainergokhan"
    key                  = "terraform.tfstate"
  }
}
provider "azurerm" {
  features {}
  subscription_id = <your_subscription_id>  #change me
}
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg-gokhan"
  location = "West Europe"
  tags = {
    Name = "terraform-rg"
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
```

```bash
terraform init
terraform apply
```

```bash
terraform init
Initializing the backend...

Successfully configured the backend "azurerm"! Terraform will automatically
use this backend unless the backend configuration changes.
Initializing provider plugins...
```

- Create state-2 folder and go to the folder.

```t
terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "4.3.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "terraform-rg-tfstate"
    storage_account_name = "terraformstoragegokhan"
    container_name       = "terraformcontainergokhan"
    key                  = "terraform.tfstate"
  }
}
provider "azurerm" {
  features {}
  subscription_id = <your_subscription_id>  #change me
}
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg-gokhan"
  location = "West Europe"
  tags = {
    Name = "terraform-rg"
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
```
```bash
terraform init
terraform apply
```
```bash
terraform init
Initializing the backend...

Successfully configured the backend "azurerm"! Terraform will automatically
use this backend unless the backend configuration changes.
Initializing provider plugins...
```

- Error when a command is run by two different users at the same time

```bash
terraform apply
╷
│ Error: Error acquiring the state lock
│ 
│ Error message: state blob is already locked
│ Lock Info:
│   ID:        7b278ca0-93bc-8347-05ec-b4c29861b96b
│   Path:      terraformcontainergokhan/terraform.tfstate
│   Operation: OperationTypeApply
│   Who:       gokhanyardimci@Gokhans-MacBook-Air.local
│   Version:   1.9.6
│   Created:   2024-10-02 10:55:27.026982 +0000 UTC
│   Info:      
│ 
│ 
│ Terraform acquires a state lock to protect the state from being written
│ by multiple users at the same time. Please resolve the issue above and try
│ again. For most commands, you can disable locking with the "-lock=false"
│ flag, but this is not recommended.
╵
```

```bash
terraform destroy
```

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