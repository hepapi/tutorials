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
    aws = {
      source = "hashicorp/aws"
      version = "6.16.0"
    }
  }
}

provider "aws" {
  region  = "us-east-1"
}

resource "aws_instance" "terraform-ec2" {
  ami           = var.ec2_ami
  key_name      = var.ec2_key_name
  instance_type = var.ec2_type
  tags = {
    "Name" = "${var.ec2_name}-Instance"
  }
}

resource "aws_ecr_repository" "terraform-ecr" {
  name                 = var.ecr_name
  image_tag_mutability = var.image_tag_mutability

  image_scanning_configuration {
    scan_on_push = var.scan_on_push
  }
}

resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = var.s3_bucket_name

  tags = {
    "Name" = var.s3_bucket_name
  }
}


variable "ec2_name" {
  default = "Terraform"
}
variable "ec2_type" {
  default = "t2.micro"
}
variable "ec2_ami" {
  default = "ami-0360c520857e3138f"
}
variable "ec2_key_name" {
  default = "ozia"
}

variable "ecr_name" {
    default = "ecr_terraform"
}
variable "image_tag_mutability" {
    default = "MUTABLE"
}
variable "scan_on_push" {
    default = "true"
}

variable "s3_bucket_name" {
    default = "hepapi-terraform"
}
```

```bash
terraform apply
```
- Now, let's move the variables to a separate file called variables.tf.

- Create a file name `variables.tf`. Take the variables from `main.tf` file and paste into "variables.tf". 

```t
variable "ec2_name" {
  default = "Terraform"
}
variable "ec2_type" {
  default = "t2.micro"
}
variable "ec2_ami" {
  default = "ami-0360c520857e3138f"
}
variable "ec2_key_name" {
  default = "ozia"       ## change me
}
variable "ecr_name" {
    default = "ecr_terraform"
}
variable "image_tag_mutability" {
    default = "MUTABLE"
}
variable "scan_on_push" {
    default = "true"
}
variable "s3_bucket_name" {
    default = "hepapi-terraform"
}
```

## string, number, bool, map, any.

```t
resource "aws_vpc" "terraform-vpc" {
  cidr_block = var.vpc_cidr
  tags = {
    Name = var.vpc_name
  }
}


variable "vpc_name" {
    default = "hepapi-vpc"
    type = string
    description = "VPC name"
}
variable "vpc_cidr" {
    default = "10.0.0.0/16"
    type = string
    description = "VPC CIDR block"
}

```

```bash
terraform apply
```

```t
resource "aws_subnet" "subnet1" {
  vpc_id     = aws_vpc.terraform-vpc.id
  cidr_block = var.subnet_cidr["subnet1"]
  tags = {
    Name = "Subnet1"
  }
}
resource "aws_subnet" "subnet2" {
  vpc_id     = aws_vpc.terraform-vpc.id
  cidr_block = var.subnet_cidr["subnet2"]
  tags = {
    Name = "Subnet2"
  }
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
variable "ec2_name" {
    type = string
    description = "ec2 name"
}
variable "ec2_type" {
    type = string
    description = "ec2 type"
}
variable "ec2_ami" {
    type = string
    description = "ec2 ami"
}
variable "ec2_key_name" {
    type = string
    description = "ec2 key name"
}
variable "ecr_name" {
    type = string
    description = "ecr name"
}
variable "image_tag_mutability" {
    type = string
    description = "ecr image_tag_mutability"
}
variable "scan_on_push" {
    type = bool
    description = "ecr scan on push value"
}
variable "s3_bucket_name" {
    type = string
    description = "s3 bucket name"
}
```

```bash
terraform apply
terraform destroy
```

## Command Line Flags

- You can set variables using command line flags without modifying your code.

```t
terraform apply -var "ec2_name=Terraform" -var "ec2_type=t2.micro" -var "ec2_ami=ami-0360c520857e3138f" -var "ec2_key_name=ozia" -var "ecr_name=ecr_terraform" -var "image_tag_mutability=MUTABLE" -var "scan_on_push=true" -var "s3_bucket_name=hepapi-terraform-123456"
```
```t
terraform destroy -var "ec2_name=Terraform" -var "ec2_type=t2.micro" -var "ec2_ami=ami-0360c520857e3138f" -var "ec2_key_name=ozia" -var "ecr_name=ecr_terraform" -var "image_tag_mutability=MUTABLE" -var "scan_on_push=true" -var "s3_bucket_name=hepapi-terraform-123456"
```

## Environment Variables

- You can export variables as environment variables using the `TF_VAR_ prefix`.

```t
export TF_VAR_ec2_name="Terraform-Instance"
export TF_VAR_ec2_type="t2.micro"
export TF_VAR_ec2_ami="ami-0360c520857e3138f"
export TF_VAR_ec2_key_name="ozia"
export TF_VAR_ecr_name="ecr_terraform"
export TF_VAR_image_tag_mutability="MUTABLE"
export TF_VAR_scan_on_push="true"
export TF_VAR_s3_bucket_name="hepapi-terraform-2025"
```

```bash
terraform apply
terraform destroy
```

## Variable Definition Files and Automatically Loaded

- Terraform supports defining variables in a separate file. By default, Terraform looks for a file named `terraform.tfvars`.

- Create a file name `terraform.tfvars`.

```t
ec2_name = "Terraform-Instance"
ec2_type = "t2.micro"
ec2_ami = "ami-0360c520857e3138f"
ec2_key_name = "ozia"
ecr_name = "ecr_terraform"
image_tag_mutability = "MUTABLE"
scan_on_push = "true"
s3_bucket_name = "hepapi-terraform-2025"
```

```bash
terraform apply
terraform destroy
```

- Alternatively, you can create a custom variable file like variables.tfvars and pass it with the `-var-file flag`.

- Create a file name `variables.tfvars`.

```t
ec2_name = "Dev-Instance"
ec2_type = "t2.small"
ec2_ami = "ami-0360c520857e3138f"
ec2_key_name = "ozia"
ecr_name = "ecr_terraform_dev"
image_tag_mutability = "MUTABLE"
scan_on_push = "true"
s3_bucket_name = "hepapi-dev-2025"
```

```bash
terraform apply -var-file variables.tfvars
terraform destroy -var-file variables.tfvars
```

## Part 2 - Using Terraform Commands

## `terraform validate` command.

```t
resource "aws_vpc" "hepapi-vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {
    Name = "terraform-vpc"
  }
}
resource "aws_subnet" "subnet" {
  vpc_id     = aws_vpc.hepapi-vpc.id
  cidr_block = "10.0.1.0/24"
  tags = {
    Name = "terraform-subnet"
  }
}
```
- Go to the terminal and run `terraform validate`. It validates the Terraform files syntactically correct and internally consistent.  

- Go to `main.tf` file and delete last curly bracket "}" and cidr_block's of subnet last letter (cidr_bloc). And Go to terminal and run the command `terraform validate`. After taking the errors correct them. Then run the command again.

```bash
terraform validate 
╷
│ Error: Unclosed configuration block
│ 
│   on main.tf line 9, in resource "aws_subnet" "subnet":
│   9: resource "aws_subnet" "subnet" {
│ 
│ There is no closing brace for this block before the end of the file. This may be caused by incorrect brace nesting elsewhere in this file.

terraform validate 
╷
│ Error: Unsupported argument
│ 
│   on main.tf line 10, in resource "aws_subnet" "subnet":
│   10:   cidr_blocks = "10.0.1.0/24"
│ 
│ An argument named "cidr_blocks" is not expected here. Did you mean "cidr_block"?
╵
╷
│ Error: Unsupported argument
│ 
│   on main.tf line 78, in resource "aws_subnet" "subnet":
│   9:   cidr_bloc     = ["10.0.1.0/24"]
│ 
│ An argument named "cidr_bloc" is not expected here. Did you mean "cidr_block"?

terraform validate

Success! The configuration is valid.
```

- Go to `main.tf` file and copy the azurerm_subnet block and paste it. And Go to terminal and run the command `terraform validate`. After taking the errors correct them. Then run the command again.

```bash
terraform validate 
╷
│ Error: Duplicate resource "aws_subnet" configuration
│ 
│   on main.tf line 15:
│   15: resource "aws_subnet" "subnet" {
│ 
│ A aws_subnet resource named "subnet" was already declared at main.tf:9,1-30. Resource names must be unique per type in each module.
```

- Go to `main.tf` file and delete the second aws_subnet.


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
output "instance_id" {
  value = aws_instance.terraform-ec2.id
}

output "vpc_id" {
  value = aws_vpc.terraform-vpc.id
}

output "subnet_id" {
  value = aws_subnet.subnet1.id
}
```

```bash
terraform apply
terraform output
terraform output -json
terraform output vpc_id
```

## `terraform refresh` command.

- The `terraform apply -refresh-only` command is used to update the state file with the real-world infrastructure. This can be used to detect any drift from the last-known state, and to update the state file. First, check the current state of your resources with `terraform state list`. Then go to the AWS Console and delete your subnet  `terraform-subnet1`. Display the state list again and refresh the state. Run the following commands.

```bash
terraform state list
aws_instance.terraform-ec2
aws_vpc.terraform-vpc
aws_subnet.subnet1

terraform apply -refresh-only

terraform state list
aws_instance.terraform-ec2
aws_vpc.terraform-vpc
```

- Now, you can see the differences between files `terraform.tfstate` and `terraform.tfstate.backup`. From tfstate file subnet is deleted but in backup file you can see subnet.

- Run terraform apply -auto-approve and create subnet again.

```bash
terraform apply -auto-approve
```

## `terraform graph` command.

- Go to the terminal and run `terraform graph`. It creates a visual graph of Terraform resources. The output of "terraform graph" command is in the DOT format, which can easily be converted to an image by making use of dot provided by GraphViz.

- Copy the output and paste it to the `https://dreampuf.github.io/GraphvizOnline`. Then display it. If you want to display this output in your local, you can download graphviz (`brew install graphviz or sudo apt install graphviz`) and take a `graph.svg` with the command `terraform graph | dot -Tsvg > graph.svg`. (NOTE: `https://graphviz.org/download/`)

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
resource "aws_instance" "terraform-ec2" {
  ami           = var.ec2_ami
  key_name      = var.ec2_key_name
  instance_type = var.ec2_type
  tags = {
    "Name" = "${var.ec2_name}-Instance"
  }
  lifecycle {
    create_before_destroy = true
  }
}

variable "ec2_name" {
    default = "Terraform"
    type = string
    description = "ec2 name"
}
variable "ec2_type" {
    default = "t2.micro"
    type = string
    description = "ec2 type"
}
variable "ec2_ami" {
    default = "ami-0360c520857e3138f"
    type = string
    description = "ec2 ami"
}
variable "ec2_key_name" {
    default = "ozia"
    type = string
    description = "ec2 key name"
}
```

```bash
terraform apply
```

```t
variable "ec2_type" {
    default = "t2.small"
    type = string
    description = "ec2 type"
}
```

```bash
terraform apply
```

```bash
Plan: 1 to add, 0 to change, 1 to destroy.

xxxxxxxxxxxxxxxxx

xxxxxx
xxxxx
```

## prevent_destroy

- To protect critical resources from being accidentally destroyed, you can use the `prevent_destroy` lifecycle rule.

- This ensures that Terraform won't allow a terraform destroy or any other command to destroy this resource unless prevent_destroy is explicitly removed from the configuration.

```t
resource "aws_instance" "terraform-ec2" {
  ami           = var.ec2_ami
  key_name      = var.ec2_key_name
  instance_type = var.ec2_type
  tags = {
    "Name" = "${var.ec2_name}-Instance"
  }
  lifecycle {
    prevent_destroy = true
  }
}

variable "ec2_name" {
    default = "Terraform"
    type = string
    description = "ec2 name"
}
variable "ec2_type" {
    default = "t2.micro"
    type = string
    description = "ec2 type"
}
variable "ec2_ami" {
    default = "ami-0360c520857e3138f"
    type = string
    description = "ec2 ami"
}
variable "ec2_key_name" {
    default = "ozia"
    type = string
    description = "ec2 key name"
}
```

```bash
terraform destroy
```

```bash
│ Error: Instance cannot be destroyed
│ 
xxxxxxxxxx
xxxxxx
xxxxxxxxx
```

## ignore_changes

- The `ignore_changes` argument tells Terraform to ignore changes to specified resource attributes, so Terraform won't attempt to update them.

```t
resource "aws_instance" "terraform-ec2" {
  ami           = var.ec2_ami
  key_name      = var.ec2_key_name
  instance_type = var.ec2_type
  tags = {
    "Name" = "${var.ec2_name}-Instance"
  }
  lifecycle {
    ignore_changes = [ tags ]
  }
}

variable "ec2_name" {
    default = "Terraform-new"
    type = string
    description = "ec2 name"
}
variable "ec2_type" {
    default = "t2.micro"
    type = string
    description = "ec2 type"
}
variable "ec2_ami" {
    default = "ami-0360c520857e3138f"
    type = string
    description = "ec2 ami"
}
variable "ec2_key_name" {
    default = "ozia"
    type = string
    description = "ec2 key name"
}


```

```bash
terraform apply
xxxxxxxxxxxxxx
xxxxxxxxxxxx
xxxxxxxxxx
```

## Part 4 - Count, Length Function and For Each

## Count

- By default, a resource block configures one real infrastructure object. However, sometimes you want to manage several similar objects (like a fixed pool of compute instances) without writing a separate block for each one. Terraform has two ways to do this: count and for_each.

- The `count` argument accepts a whole number, and creates that many instances of the resource or module. Each instance has a distinct infrastructure object associated with it, and each is separately created, updated, or destroyed when the configuration is applied.

- Go to the `variables.tf` file and create a new variable.

```t
resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = var.s3_bucket_name[count.index]

  tags = {
    "Name" = "${var.s3_bucket_name[count.index]}-dev"
  }
  count = 2
}

variable "s3_bucket_name" {
    default = [
        "hepapi-terraform",
        "hepapi-terraform-count"
    ] 
    type = list
    description = "bucket name"
}
```

```bash
terraform apply
terraform destroy
```

## Length Function

```t
resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = var.s3_bucket_name[count.index]

  tags = {
    "Name" = "${var.s3_bucket_name[count.index]}-dev"
  }
  count = length(var.s3_bucket_name)
}

variable "s3_bucket_name" {
    default = [
        "hepapi-terraform",
        "hepapi-terraform-count",
        "hepapi-terraform-length"
    ] 
    type = list
    description = "bucket name"
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
resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = each.value

  tags = {
    "Name" = "${each.value}-dev"
  }
  for_each = var.s3_bucket_name
}

variable "s3_bucket_name" {
    default = [
        "hepapi-terraform",
        "hepapi-terraform-one",
        "hepapi-terraform-two"
    ] 
    type = set(string)
    description = "bucket name"
}
```

```bash
terraform apply
terraform destroy
```

```t
resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = each.value

  tags = {
    "Name" = "${each.value}-dev"
  }
  for_each = toset(var.s3_bucket_name)
}

variable "s3_bucket_name" {
    default = [
        "hepapi-terraform",
        "hepapi-terraform-one",
        "hepapi-terraform-two",
        "hepapi-terraform"
    ] 
    type        = list(string)
    description = "bucket name"
}
```

```bash
terraform apply
terraform destroy
```

```t

resource "aws_s3_bucket" "terraform-s3-bucket" {
  bucket = each.value
  region = each.key

  tags = {
    "Name" = "${each.value}-dev"
  }
  for_each = var.s3_bucket_name
}

variable "s3_bucket_name" {
    default = [
      "eu-central-1" = "hepapi-terraform",
      "us-east-1"    = "hepapi-terraform-one",
      "eu-west-1"    =  "hepapi-terraform-two"
    ] 
    type        = map(string)
    description = "bucket key-value"
}
```

```bash
terraform apply
terraform destroy
```