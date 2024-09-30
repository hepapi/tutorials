# Working With Terraform Hands-On

In this session, we'll explore using terraform variables, terraform commands, lifecycle rules, count, length function and for each.

- Part 1 - Understanding the Variable Block and Using Variables in Terraform

- Part 2 - Using Terraform Commands

- Part 3 - Using LifeCycle Rules

- Part 4 - Count, Length Function and For Each


## Part 1 - Understanding the Variable Block and Using Variables in Terraform

- Variables in Terraform allow you to customize and reuse your configuration without hardcoding values. By declaring variables, you can manage infrastructure across multiple environments with minimal changes to your Terraform code.

## Declaring
- Each input variable accepted by a module must be declared using a variable block.

- Go to the `main.tf` file.

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
    features {      
    }
    subscription_id = <your_subscription_id>  #change me
}
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}-dev"
  }
}
variable "rg_name" {
    default = "terraform-rg-gokhan"
}
variable "rg_location" {
    default = "West Europe"
}
resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  sku                 = var.acr_sku
  admin_enabled       = var.acr_admin
}
variable "acr_name" {
    default = "containerRegistrygokhan"
}
variable "acr_sku" {
    default = "Premium"
}
variable "acr_admin" {
    default = "false"
}
```

```bash
terraform apply
```
- Now, let's move the variables to a separate file called variables.tf.

- Create a file name `variables.tf`. Take the variables from `main.tf` file and paste into "variables.tf". 

```t
variable "rg_name" {
    default = "terraform-rg-gokhan"
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
variable "acr_name" {
    default = "containerRegistrygokhan"
    type = string
    description = "acr name"
}
variable "acr_sku" {
    default = "Premium"
    type = string
    description = "acr_sku type"
}
variable "acr_admin" {
    default = "false"
    type = bool
    description = "acr admin value"
}
```

## string, number, bool, map, any.

```t
resource "azurerm_virtual_network" "vnet" {
  name                = var.vnet_name
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  address_space       = var.vnet_cidr
}
variable "vnet_name" {
    default = "terraform-vnet"
    type = string
    description = "vnet name"
}
variable "vnet_cidr" {
    default = ["10.0.0.0/16", "10.10.0.0/16"]
    type = list
    description = "vnet cidr block"
}
```

```bash
terraform apply
```

```t
resource "azurerm_subnet" "subnet1" {
  name                 = "terraform-subnet1"
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = var.subnet_cidr["subnet1"]
}
resource "azurerm_subnet" "subnet2" {
  name                 = "terraform-subnet2"
  resource_group_name  = azurerm_resource_group.resource_group.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = var.subnet_cidr["subnet2"]
}
variable "subnet_cidr" {
    type = map
    description = "subnet cidr blocks"
    default = {
        "subnet1" = ["10.0.1.0/24"]
        "subnet2" = ["10.10.1.0/24"]
    }
}
```

```bash
terraform apply
terraform destroy
```

## Interactive Mode

- You can define variables without default values and provide them interactively when running Terraform.

```t
variable "rg_name" {
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    type = string
    description = "resource_group location"
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

```bash
terraform apply
terraform destroy
```

## Command Line Flags

- You can set variables using command line flags without modifying your code.

```t
terraform apply -var "rg_name=terraform-gokhan" -var "rg_location=West Europe" -var "acr_name=containerRegistrygokhan" -var "acr_sku=Premium" -var "acr_admin=false"
```
```t
 terraform destroy -var "rg_name=terraform-gokhan" -var "rg_location=West Europe" -var "acr_name=containerRegistrygokhan" -var "acr_sku=Premium" -var "acr_admin=false"
```

## Environment Variables

- You can export variables as environment variables using the `TF_VAR_ prefix`.

```t
export TF_VAR_rg_name="terraform-gokhan"
export TF_VAR_rg_location="West Europe"
export TF_VAR_acr_name="containerRegistrygokhan"
export TF_VAR_acr_sku="Premium"
export TF_VAR_acr_admin="false"
```

```bash
terraform apply
terraform destroy
```

## Variable Definition Files and Automatically Loaded

- Terraform supports defining variables in a separate file. By default, Terraform looks for a file named `terraform.tfvars`.

- Create a file name `terraform.tfvars`.

```t
rg_name = "terraform-gokhan"
rg_location = "West Europe"
acr_name = "containerRegistrygokhan"
acr_sku = "Premium"
acr_admin = "false"
```

```bash
terraform apply
terraform destroy
```

- Alternatively, you can create a custom variable file like variables.tfvars and pass it with the `-var-file flag`.

- Create a file name `variables.tfvars`.

```t
rg_name = "terraform-gokhan"
rg_location = "West Europe"
acr_name = "containerRegistrygokhan"
acr_sku = "Premium"
acr_admin = "false"
```

```bash
terraform apply -var-file variables.tfvars
terraform destroy -var-file variables.tfvars
```

## Part 2 - Using Terraform Commands

## `terraform validate` command.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg"
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
- Go to the terminal and run `terraform validate`. It validates the Terraform files syntactically correct and internally consistent.  

- Go to `main.tf` file and delete last curly bracket "}" and address_prefixes's of subnet last letter (address_prefixe). And Go to terminal and run the command `terraform validate`. After taking the errors correct them. Then run the command again.

```bash
terraform validate
╷
│ Error: Unclosed configuration block
│ 
│   on main.tf line 74, in resource "azurerm_subnet" "subnet":
│   74: resource "azurerm_subnet" "subnet" {
│ 
│ There is no closing brace for this block before the end of the file. This may be caused by incorrect brace nesting elsewhere in this file.

terraform validate 
╷
│ Error: Missing required argument
│ 
│   on main.tf line 74, in resource "azurerm_subnet" "subnet":
│   74: resource "azurerm_subnet" "subnet" {
│ 
│ The argument "address_prefixes" is required, but no definition was found.
╵
╷
│ Error: Unsupported argument
│ 
│   on main.tf line 78, in resource "azurerm_subnet" "subnet":
│   78:   address_prefixe     = ["10.0.1.0/24"]
│ 
│ An argument named "address_prefixe" is not expected here. Did you mean "address_prefixes"?

terraform validate

Success! The configuration is valid.
```

- Go to `main.tf` file and copy the azurerm_subnet block and paste it. And Go to terminal and run the command `terraform validate`. After taking the errors correct them. Then run the command again.

```bash
terraform validate 
╷
│ Error: Duplicate resource "azurerm_subnet" configuration
│ 
│   on main.tf line 81:
│   81: resource "azurerm_subnet" "subnet" {
│ 
│ A azurerm_subnet resource named "subnet" was already declared at main.tf:74,1-35. Resource names must be unique per type in each module.
```

- Go to `main.tf` file and delete the second azurerm_subnet.


## `terraform fmt` command.

- Go to `main.tf` file and add random indentations. Then go to terminal and run the command `terraform fmt`. "terraform fmt" command reformat your configuration file in the standard style.

```bash
terraform fmt
```
- Now, show `main.tf` file. It was formatted again.


## `terraform show` command.

```bash
terraform show
terraform show -json
```
- Go to the terminal and run `terraform show` or `terraform show -json`. You can see tfstate file or plan in the terminal. It is more readable than `terraform.tfstate`.


## `terraform providers` command.

- Go to the terminal and run `terraform providers`. You can see your providers.


## `terraform output` command.

- Output values make information about your infrastructure available on the command line, and can expose information for other Terraform configurations to use.

- Now add the followings to the `main.tf` file.  Then run the commands `terraform apply or terraform refresh` and `terraform output`. `terraform output` command is used for reading an output from a state file. It reads an output variable from a Terraform state file and prints the value. With no additional arguments, output will display all the outputs for the (parent) root module.  If NAME is not specified, all outputs are printed.

```go
output "resource_group_location" {
  value = azurerm_resource_group.resource_group.location
}

output "vnet_id" {
  value = azurerm_virtual_network.vnet.id
}

output "subnet_id" {
  value = azurerm_subnet.subnet1.id
}
```

```bash
terraform apply
terraform output
terraform output -json
terraform output vnet_id
```

## `terraform refresh` command.

- The `terraform apply -refresh-only` command is used to update the state file with the real-world infrastructure. This can be used to detect any drift from the last-known state, and to update the state file. First, check the current state of your resources with `terraform state list`. Then go to the Azure Portal and delete your subnet  `terraform-subnet1`. Display the state list again and refresh the state. Run the following commands.

```bash
terraform state list
azurerm_resource_group.resource_group
azurerm_virtual_network.vnet
azurerm_subnet.subnet

terraform apply -refresh-only

terraform state list
azurerm_resource_group.resource_group
azurerm_virtual_network.vnet
```

- Now, you can see the differences between files `terraform.tfstate` and `terraform.tfstate.backup`. From tfstate file subnet is deleted but in backup file you can see subnet.

- Run terraform apply -auto-approve and create subnet again.

```bash
terraform apply -auto-approve

```

## `terraform graph` command.

- Go to the terminal and run `terraform graph`. It creates a visual graph of Terraform resources. The output of "terraform graph" command is in the DOT format, which can easily be converted to an image by making use of dot provided by GraphViz.

- Copy the output and paste it to the `https://dreampuf.github.io/GraphvizOnline`. Then display it. If you want to display this output in your local, you can download graphviz (`brew install graphviz`) and take a `graph.svg` with the command `terraform graph | dot -Tsvg > graph.svg`. (NOTE: `https://graphviz.org/download/`)

```bash
terraform graph
terraform graph | dot -Tsvg > graph.svg
```

```bash
terraform destroy
```
## Part 3 - Using LifeCycle Rules

## create_before_destroy

- Terraform's `lifecycle` block can be used to customize how resource actions are carried out. For example, the `create_before_destroy` rule is used to create a new resource before destroying the existing one.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}-dev"
  }
  lifecycle {
    create_before_destroy = true
  }
}
variable "rg_name" {
    default = "terraform-rg-gokhan"
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform apply
```

```t
variable "rg_name" {
    default = "terraform-rg-gokhan-new"
    type = string
    description = "resource_group name"
}
```

```bash
terraform apply
```

```bash
Plan: 1 to add, 0 to change, 1 to destroy.

Do you want to perform these actions?
  Terraform will perform the actions described above.
  Only 'yes' will be accepted to approve.

  Enter a value: yes

azurerm_resource_group.resource_group: Creating...
azurerm_resource_group.resource_group: Still creating... [10s elapsed]
azurerm_resource_group.resource_group: Creation complete after 10s [id=/subscriptions/xxxxxxxxxxxxxxx/resourceGroups/terraform-rg-gokhan-new]
azurerm_resource_group.resource_group (deposed object 4b733fe9): Destroying... [id=/subscriptions/xxxxxxxxxxxxxxx/resourceGroups/terraform-rg-gokhan]
azurerm_resource_group.resource_group: Still destroying... [id=/subscriptions/xxxxxxxxxxxxxxx/resourceGroups/terraform-rg-gokhan, 10s elapsed]
azurerm_resource_group.resource_group: Destruction complete after 17s

Apply complete! Resources: 1 added, 0 changed, 1 destroyed.
```

## prevent_destroy

- To protect critical resources from being accidentally destroyed, you can use the `prevent_destroy` lifecycle rule.

- This ensures that Terraform won't allow a terraform destroy or any other command to destroy this resource unless prevent_destroy is explicitly removed from the configuration.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}-dev"
  }
  lifecycle {
    prevent_destroy = true
  }
}
variable "rg_name" {
    default = "terraform-rg-gokhan"
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform destroy
```

```bash
│ Error: Instance cannot be destroyed
│ 
│   on lifecycle.tf line 17:
│   17: resource "azurerm_resource_group" "resource_group" {
│ 
│ Resource azurerm_resource_group.resource_group has lifecycle.prevent_destroy set, but the plan calls for this resource to be destroyed. To avoid this
│ error and continue with the plan, either disable lifecycle.prevent_destroy or reduce the scope of the plan using the -target option.
```

## ignore_changes

- The `ignore_changes` argument tells Terraform to ignore changes to specified resource attributes, so Terraform won't attempt to update them.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name
  location = var.rg_location
  tags = {
    Name = "${var.rg_name}-dev-new"
  }
  lifecycle {
    ignore_changes = [ tags ]
  }
}
variable "rg_name" {
    default = "terraform-rg-gokhan"
    type = string
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform apply
azurerm_resource_group.resource_group: Refreshing state... [id=/subscriptions/xxxxxxxxxxxxxxxxxxxx/resourceGroups/terraform-rg-gokhan]

No changes. Your infrastructure matches the configuration.

Terraform has compared your real infrastructure against your configuration and found no differences, so no changes are needed.

Apply complete! Resources: 0 added, 0 changed, 0 destroyed.
```

## Part 4 - Count, Length Function and For Each

## Count

- By default, a resource block configures one real infrastructure object. However, sometimes you want to manage several similar objects (like a fixed pool of compute instances) without writing a separate block for each one. Terraform has two ways to do this: count and for_each.

- The `count` argument accepts a whole number, and creates that many instances of the resource or module. Each instance has a distinct infrastructure object associated with it, and each is separately created, updated, or destroyed when the configuration is applied.

- Go to the `variables.tf` file and create a new variable.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name[count.index]
  location = var.rg_location
  tags = {
    Name = "${var.rg_name[count.index]}-dev"
  }
  count = 2
}
variable "rg_name" {
    default = [
        "terraform-rg-gokhan",
        "terraform-rg-gokhan-count"
    ] 
    type = list
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform apply
terraform destroy
```

## Length Function

```t
resource "azurerm_resource_group" "resource_group" {
  name     = var.rg_name[count.index]
  location = var.rg_location
  tags = {
    Name = "${var.rg_name[count.index]}-dev"
  }
  count = length(var.rg_name)
}
variable "rg_name" {
    default = [
        "terraform-rg-gokhan",
        "terraform-rg-gokhan-count",
        "terraform-rg-gokhan-length"
    ] 
    type = list
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform apply
terraform destroy
```

## for each

- The for_each meta-argument accepts a map or a set of strings, and creates an instance for each item in that map or set. Each instance has a distinct infrastructure object associated with it, and each is separately created, updated, or destroyed when the configuration is applied.

- Go to the `variables.tf` file and create a new variable.

```t
resource "azurerm_resource_group" "resource_group" {
  name     = each.value
  location = var.rg_location
  tags = {
    Name = "${each.value}-dev"
  }
 for_each = var.rg_name
}
variable "rg_name" {
    default = [
        "terraform-rg-gokhan",
        "terraform-rg-gokhan-one",
        "terraform-rg-gokhan-two"
    ] 
    type = set(string)
    description = "resource_group name"
}
variable "rg_location" {
    default = "West Europe"
    type = string
    description = "resource_group location"
}
```

```bash
terraform apply
terraform destroy
```

```t
resource "azurerm_resource_group" "resource_group" {
  name     = each.value
  location = var.rg_location
  tags = {
    Name = "${each.value}-dev"
  }
  for_each = toset(var.rg_name)
}
variable "rg_name" {
  default = [
    "terraform-rg-gokhan",
    "terraform-rg-gokhan-one",
    "terraform-rg-gokhan-two",
    "terraform-rg-gokhan" 
  ]
  type        = list(string)
  description = "resource_group name"
}
variable "rg_location" {
  default     = "West Europe"
  type        = string
  description = "resource_group location"
}
```

```bash
terraform apply
terraform destroy
```

```t
resource "azurerm_resource_group" "resource_group" {
  name     = each.value
  location = each.key
  tags = {
    Name = "${each.value}-dev"
  }
  for_each = var.rg
}
variable "rg" {
  default = {
    "West Europe" = "terraform-rg-gokhan",
    "East US"     = "terraform-rg-gokhan-one",
    "Central US"  = "terraform-rg-gokhan-two"
  }
  type        = map(string)
  description = "resource_group key-value"
}
```

```bash
terraform apply
terraform destroy
```