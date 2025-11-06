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
│   ├── main.tf
│   ├── outputs.tf
│   └── variables.tf
│
├── /environments
│   ├── /dev               
│   │   ├── dev-vpc.tf
│   │
│   └── /prod             
│       ├── prod-vpc.tf

```
- Go to the `modules/provider.tf` file, and add the following.
```t
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
- Go to the `modules/main.tf` file, and add the following.

```t
resource "aws_vpc" "module_vpc" {
  cidr_block = var.vpc_cidr_block
  tags = {
    Name = "terraform-vpc-${var.environment}"
  }
}

resource "aws_subnet" "public_subnet" {
  cidr_block = var.public_subnet_cidr
  vpc_id = aws_vpc.module_vpc.id
  tags = {
    Name = "terraform-public-subnet-${var.environment}"
  }
}

resource "aws_subnet" "private_subnet" {
  cidr_block = var.private_subnet_cidr
  vpc_id = aws_vpc.module_vpc.id
  tags = {
    Name = "terraform-private-subnet-${var.environment}"
  }
}
```

- This is not a public subnet since we didn't attach an internet gateway. 

- Go to the `modules/variables.tf` file, and add the following.


```t
variable "environment" {
  default = "hepapi"
}

variable "vpc_cidr_block" {
  default = "10.0.0.0/16"
  description = "this is our vpc cidr block"
}

variable "public_subnet_cidr" {
  default = "10.0.1.0/24"
  description = "this is our public subnet cidr block"
}

variable "private_subnet_cidr" {
  default = "10.0.2.0/24"
  description = "this is our private subnet cidr block"
}

variable "aws_region" {
  default     = "us-east-1"
  description = "AWS region for resources"
}
```


- Go to the `modules/outputs.tf` file, and add the following.

```t
output "vpc_id" {
  value = aws_vpc.module_vpc.id
}

output "vpc_cidr" {
  value = aws_vpc.module_vpc.cidr_block
}

output "public_subnet_cidr" {
  value = aws_subnet.public_subnet.cidr_block
}

output "private_subnet_cidr" {
  value = aws_subnet.private_subnet.cidr_block
}
```


- Go to the `dev/dev-vpc.tf` file, and add the following.

```go
module "tf-vpc" {
  source = "../../modules"
  environment = "DEV"
  }

output "vpc-cidr-block" {
  value = module.tf-vpc.vpc_cidr
}
```

- Go to the `prod/prod-vpc.tf` file, and add the following.

```go
module "tf-vpc" {
  source = "../../modules"
  environment = "PROD"
  }

output "vpc-cidr-block" {
  value = module.tf-vpc.vpc_cidr
}
```

- Go to the `dev` folder and run the command below.

```bash
terraform init

terraform apply
```

- Go to the AWS console and check the VPC and subnets.

- Go to the `prod` folder and run the command below.

```bash
terraform init

terraform apply
```

- Go to the AWS console and check the VPC and subnets.


- Go to the `prod` and  `dev` folders and run the command below.

```bash
terraform destroy -auto-approve
```

## Using Registry Module

- Go to the terraform registry and search public modules.

- Go to the registry folder. Create provider.tf and main.tf.

```t
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.16.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
}
```

```t
module "vpc" {
  source = "terraform-aws-modules/vpc/aws"

  name = "my-vpc-hepapi"
  cidr = "10.0.0.0/16"

  azs             = ["us-east-1a", "us-east-1b", "us-east-1c"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]

  tags = {
    Terraform = "true"
    Environment = "dev"
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

* ``Named`` terraform-[PROVIDER]-[NAME]. Module repositories must use this three-part name format, where <NAME> reflects the type of infrastructure the module manages and [PROVIDER] is the main provider where it creates that infrastructure. The [NAME] segment can contain additional hyphens. Examples: terraform-google-vault or terraform-aws-ec2-instance.

* ``Repository description``. The GitHub repository description is used to populate the short description of the module. This should be a simple one sentence description of the module.

* ``Standard module structure``. The module must adhere to the standard module structure. This allows the registry to inspect your module and generate documentation, track resource usage, parse submodules and examples, and more.

* ``x.y.z tags for releases``. The registry uses tags to identify module versions. Release tag names must be a semantic version, which can optionally be prefixed with a v. For example, v1.0.4 and 0.9.2. To publish a module initially, at least one release tag must be present. Tags that don't look like version numbers are ignored. (https://semver.org/)

- source link: https://www.terraform.io/registry/modules/publish


### Create a module to create an AWS instance with Amazon Linux 2023 ami (kernel 6.1).

- Create a directory for modules to publish.

```bash
cd && mkdir modules && cd modules && touch main.tf variables.tf outputs.tf versions.tf userdata.sh README.md .gitignore
```

- Go to the `versions.tf` file and copy the latest provider version from the Terraform documentation (https://registry.terraform.io/providers/hashicorp/aws/latest/docs).

```go
terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 6.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
}
```

- Go to the `variables.tf` and prepare your module variables.

```go
variable "instance_type" {
  type = string
  default = "t2.micro"
}

variable "key_name" {
  type = string
}

variable "tag" {
  type = string
  default = "Docker-Instance"
}

variable "server-name" {
  type = string
  default = "docker-instance"
}

variable "docker-instance-ports" {
  type = list(number)
  description = "docker-instance-sec-gr-inbound-rules"
  default = [22, 80, 8080]
}
```

- Go to the `main.tf` and prepare a config file to create an AWS instance with the latest Amazon Linux 2023 AMI.

```go
data "aws_ami" "amazon-linux-2023" {
  owners      = ["amazon"]
  most_recent = true

  filter {
    name   = "root-device-type"
    values = ["ebs"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }

  filter {
    name   = "architecture"
    values = ["x86_64"]
  }

  filter {
    name   = "owner-alias"
    values = ["amazon"]
  }

  filter {
    name   = "name"
    values = ["al2023-ami-2023*"]
  }
}

resource "aws_instance" "tfmyec2" {
  ami                    = data.aws_ami.amazon-linux-2023.id
  instance_type          = var.instance_type
  key_name               = var.key_name
  vpc_security_group_ids = [aws_security_group.tf-sec-gr.id]
  user_data              = templatefile("${abspath(path.module)}/userdata.sh", {})
  tags = {
    Name = var.tag
  }
}

resource "aws_security_group" "tf-sec-gr" {
  name = "${var.tag}-terraform-sec-grp"
  tags = {
    Name = var.tag
  }

  dynamic "ingress" {
    for_each = var.docker-instance-ports
    iterator = port
    content {
      from_port = port.value
      to_port = port.value
      protocol = "tcp"
      cidr_blocks = ["0.0.0.0/0"]
    }
  }

  egress {
    from_port =0
    protocol = "-1"
    to_port =0
    cidr_blocks = ["0.0.0.0/0"]
  }
}
```

- Go to the `outputs.tf` and write some outputs.

```go
output "instance_public_ip" {
  value = aws_instance.tfmyec2.*.public_ip
}

output "sec_gr_id" {
  value = aws_security_group.tf-sec-gr.id
}

output "instance_id" {
  value = aws_instance.tfmyec2.*.id
}
```

- Go to the `userdata.sh` file and write the following.

```bash
#!/bin/bash
dnf update -y
dnf install -y docker
systemctl start docker
systemctl enable docker
usermod -a -G docker ec2-user
# install docker-compose
curl -SL https://github.com/docker/compose/releases/download/v2.40.0/docker-compose-linux-x86_64 -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
```

- Go to the `.gitignore` file and write the following. 

```bash
# Local .terraform directories
**/.terraform/*

# Terraform lockfile
.terraform.lock.hcl

# .tfstate files
*.tfstate
*.tfstate.*

# Crash log files
crash.log

# Exclude all .tfvars files, which are likely to contain sensitive data, such as
# passwords, private keys, and other secrets. These should not be part of version
# control as they are data points which are potentially sensitive and subject
# to change depending on the environment.
*.tfvars
```

- Go to the `README.md` and make a description of your module.

---
Terraform module that provisions an AWS EC2 instance using the latest Amazon Linux 2023 AMI with Docker pre-installed.

This module is for demonstration purposes only and not intended for production use.

It serves as an example to illustrate how to create and publish a module on the Terraform Registry.

Usage:

```hcl

provider "aws" {
  region = "us-east-1"
}

module "docker_instance" {
    source = "<github-username>/docker-instance/aws"
    key_name = "mykey"
}
```


### Create a GitHub repository for our Terraform module

- Create a `public` GitHub repo and name it `terraform-aws-docker-instance`.

- Clone the repository to your local.

```bash
git clone https://github.com/<your-github-account>/terraform-aws-docker-instance.git
```

- ``Copy`` the module files to this repository folder.

- Next, ``push`` the files to github repo and give a tag to version our module. You should give a semantic version to your module. (https://semver.org/)

```bash
git add .
git commit -m "should define your key file"
git push
git tag v0.0.1
git push --tags
```

- Go to the `Terraform Registry` and sign in with your `Github Account`.

- Next, `Publish` your module.

* Terraform Registry --> Sign in --> Github account --> Publish --> Modules --> Select the module repo in Github (terraform-aws-docker-instance) --> Click Agree in Terms --> Publish Module

- Go to the ``Github Repository``. Define a description in the `About` part in github repository. (Click settings wheel)

```yml
- Description: Terraform module that creates a Docker instance resource on AWS.

- Website: https://registry.terraform.io/modules/<account>/docker-instance/aws/latest
```

### Create an EC2 instance on AWS that has Docker installed with your public module.

- Create a Terraform config file to create an AWS instance on AWS.

```bash
cd && mkdir cw-modules && cd cw-modules && touch main.tf
```

- Go to the module page in `Terraform Registry`.

- Copy `Provision Instructions` or `Usage` part. Next, paste it into the `main.tf` and add your `key file` name.

```go
provider "aws" {
  region = "us-east-1"
}

module "docker-instance" {
  source  = "<github-username>/docker-instance/aws"
  key_name = "mykey"
}
```

- Run the Terraform file.

```bash
terraform init

terraform apply --auto-approve
```

- After checking the instance, you can terminate it.

```bash
terraform destroy --auto-approve
```

## Part 3 -  Terraform Workspace

## When to use Multiple Workspaces

- Terraform relies on state to associate resources with real-world objects, so if you run the same configuration multiple times with completely separate state data, Terraform can manage many non-overlapping groups of resources. In some cases you'll want to change variable values for these different resource collections (like when specifying differences between staging and production deployments), and in other cases you might just want many instances of a particular infrastructure pattern.

- The simplest way to maintain multiple instances of a configuration with completely separate state data is to use multiple working directories.

- `Workspaces` allow you to use the same working copy of your configuration and the same plugin and module caches, while still keeping separate states for each collection of resources you manage.

- Every initialized working directory has at least one workspace. (If you haven't created other workspaces, it is a workspace named ``default``.)

- For a given working directory, only one workspace can be selected at a time.

- A common use for multiple workspaces is to create a parallel, distinct copy of a set of infrastructure in order to test a set of changes before modifying the main production infrastructure. For example, a developer working on a complex set of infrastructure changes might create a new temporary workspace in order to freely experiment with changes without affecting the default workspace.


### Using Workspaces

- Create a directory name `workspaces` to learn terraform workspaces. Next, create a Terraform config file named `workspace.tf`.

```bash
cd && mkdir workspaces && cd workspaces && touch workspace.tf
```

- Add the following.

```go
terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 6.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
}

resource "aws_instance" "tfmyec2" {
  ami = var.myami
  instance_type = var.instance_type
  key_name = "keyname"       # change me
  tags = {
    Name = "${terraform.workspace}-server"
  }
}

variable "myami" {
  type = string
  default = "ami-01b799c439fd5516a"   # Amazon Linux 2023
}

variable "instance_type" {
  type = string
  default = "t2.micro"
}


output "ami" {
  value = aws_instance.tfmyec2.*.ami
}

output "type" {
  value = aws_instance.tfmyec2.*.instance_type
}

output "tags" {
  value = aws_instance.tfmyec2.*.tags
}
```

- Create prod.tfvars file.
```t
instance_type                = "t2.small"
myami                        = "ami-0ecb62995f68bb549"
```

- Create dev.tfvars file.
```t
instance_type                = "t2.micro"
myami                        = "ami-01b799c439fd5516a"
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

- After creating namespaces, Terraform creates new folders for new workspaces. Check the `workspace` folder and see the new folders.(`terraform.tfstate.d`)

- Run the following Terraform commands to create instances in `dev` and `default` workspaces.

```bash
terraform init
terraform plan

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
