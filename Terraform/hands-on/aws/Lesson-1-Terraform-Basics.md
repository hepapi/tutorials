# Terraform Basics Hands-On

## Installing Terraform

### Pre-requisites

1. **Install Terraform**

    - Follow the instructions here: [Terraform Documentation](https://developer.hashicorp.com/terraform/install)
    - Macos

    ```bash
    brew tap hashicorp/tap
    brew install hashicorp/tap/terraform
    ```
    - Linux

    ```bash
    wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
    echo "deb [signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
    sudo apt update && sudo apt install terraform
    ```
    - Windows

    ```bash
    https://releases.hashicorp.com/terraform/1.13.2/terraform_1.13.2_windows_amd64.zip
    or
    https://releases.hashicorp.com/terraform/1.13.2/terraform_1.13.2_windows_386.zip
    ```

    - Verify that the installation

    ```bash
    terraform version
    ```
2. **Install AWS-CLI**

  - Follow the instructions here: [AWS CLI Documentation](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
    - Macos

    ```bash
    curl "https://awscli.amazonaws.com/AWSCLIV2.pkg" -o "AWSCLIV2.pkg"
    sudo installer -pkg AWSCLIV2.pkg -target /
    ```
    - Linux Ubuntu/Debian

    ```bash
    sudo yum remove awscli
    sudo apt install unzip
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
    unzip awscliv2.zip
    sudo ./aws/install

    ```
    - Windows

    Download and run the AWS CLI MSI installer for Windows (64-bit)

    ```bash
    https://awscli.amazonaws.com/AWSCLIV2.msi
    ```

    Alternatively, you can run the msiexec command to run the MSI installer.

    ```bash
    msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi
    ```

    - Verify that the installation

    ```bash
    which aws
    aws --version
    ```
    - Your AWS configured locally. 

    ```bash
    aws configure
    ```
    - Hard-coding credentials into any Terraform configuration is not recommended, and risks secret leakage should this file ever be committed to a public version control system. Using IAM roles for server-based authentication is a more secure practice.

    - We will use IAM role (temporary credentials) for accessing your AWS account. 

### Create a role in IAM management console.

- Secure way to make API calls is to create a role and assume it. It gives temporary credentials for access your account and makes API calls.

- Go to the IAM service, click "roles" in the navigation panel on the left then click "create role". 

- Under the use cases, Select `EC2`, click "Next Permission" button.

- In the search box write EC2 and select `AmazonEC2FullAccess` then click "Next: Tags" and "Next: Reviews".

- Name it `terraform`.

- Attach this role to your EC2 instance. 


### Terraform Basics

- list Terraform's available subcommands.

    ```bash
    terraform -help
    Usage: terraform [-version] [-help] <command> [args]

    The available commands for execution are listed below.
    The most common, useful commands are shown first, followed by
    less common or more advanced commands. If you are just getting
    started with Terraform, stick with the common commands. For the
    other commands, please read the help and docs before usage.
    ```

- Add any subcommand to terraform -help to learn more about what it does and available options.

    ```bash
    terraform -help apply
    or
    terraform apply -help
    ```

### Write your first configuration

- The set of files used to describe infrastructure in Terraform is known as a Terraform configuration. You'll write your first configuration file to launch a single AWS EC2 instance.

- Each configuration should be in its own directory. Create a directory ("terraform-az") for the new configuration and change into the directory.

```bash
mkdir terraform-aws && cd terraform-aws && touch main.tf
```
se
- Install the `HashiCorp Terraform` extension in VSCode.

- Create a file named `main.tf` for the configuration code and copy and paste the following content. 

```t
terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "6.13.0"
    }
  }
}

provider "aws" {
  region  = "us-east-1"
}

resource "aws_instance" "terraform-ec2" {
  ami           = "ami-0360c520857e3138f"
  instance_type = "t2.micro"
  tags = {
    "Name" = "Terraform-Instance"
  }
}
```

- Explain the each block via the following section.

### Providers

The `provider` block configures the name of provider, in our case `aws`, which is responsible for creating and managing resources. A provider is a plugin that Terraform uses to translate the API interactions with the service. A provider is responsible for understanding API interactions and exposing resources. Because Terraform can interact with any API, you can represent almost any infrastructure type as a resource in Terraform.

The `profile` attribute in your provider block refers Terraform to the AWS credentials stored in your AWS Config File, which you created when you configured the AWS CLI. HashiCorp recommends that you never hard-code credentials into `*.tf configuration files`.

### Resources

The `resource` block defines a piece of infrastructure. A resource might be a physical component such as an EC2 instance.

The resource block must have two required data for EC2. : the resource type and the resource name. In the example, the resource type is `aws_instance` and the local name is `tf-ec2`. The prefix of the type maps to the provider. In our case "aws_instance" automatically tells Terraform that it is managed by the "aws" provider.

The arguments for the resource are within the resource block. The arguments could be things like machine sizes, disk image names, or VPC IDs. For your EC2 instance, you specified an AMI for `Amazon Linux 2` and instance type will be `t2.micro`.


### Initialize the directory

When you create a new configuration you need to initialize the directory with `terraform init`.

- Initialize the directory.   değişecek !!!!!!

```bash
terraform init

Initializing the backend...
Initializing provider plugins...
- Finding hashicorp/aws versions matching "6.13.0"...
- Installing hashicorp/aws v6.13.0...
- Installed hashicorp/aws v6.13.0 (signed by HashiCorp)
Terraform has created a lock file .terraform.lock.hcl to record the provider
selections it made above. Include this file in your version control repository
so that Terraform can guarantee to make the same selections by default when
you run "terraform init" in the future.

Terraform has been successfully initialized!

You may now begin working with Terraform. Try running "terraform plan" to see
any changes that are required for your infrastructure. All Terraform commands
should now work.

If you ever set or change modules or backend configuration for Terraform,
rerun this command to reinitialize your working directory. If you forget, other
commands will detect it and remind you to do so if necessary.

```

Terraform downloads the `aws` provider and installs it in a hidden subdirectory (.terraform) of the current working directory. The output shows which version of the plugin was installed.

- Show the `.terraform` folder and inspect it.

### Create infrastructure

- Run `terraform plan`. You should see an output similar to the one shown below.

```bash
terraform plan

Terraform used the selected providers to generate the following execution plan. Resource actions are indicated with the following symbols:
  + create

Terraform will perform the following actions:

  # aws_instance.terraform-ec2 will be created
  + resource "aws_instance" "terraform-ec2" {
      + ami                                  = "ami-0360c520857e3138f"
      + arn                                  = (known after apply)
      + associate_public_ip_address          = (known after apply)
      + availability_zone                    = (known after apply)
      + disable_api_stop                     = (known after apply)
      + disable_api_termination              = (known after apply)
      + ebs_optimized                        = (known after apply)
      + enable_primary_ipv6                  = (known after apply)
      + force_destroy                        = false
      + get_password_data                    = false
      + host_id                              = (known after apply)
      + host_resource_group_arn              = (known after apply)
      + iam_instance_profile                 = (known after apply)
      + id                                   = (known after apply)
      + instance_initiated_shutdown_behavior = (known after apply)
      + instance_lifecycle                   = (known after apply)
      + instance_state                       = (known after apply)
      + instance_type                        = "t2.micro"
      + ipv6_address_count                   = (known after apply)
      + ipv6_addresses                       = (known after apply)
      + key_name                             = (known after apply)
      + monitoring                           = (known after apply)
      + outpost_arn                          = (known after apply)
      + password_data                        = (known after apply)
      + placement_group                      = (known after apply)
      + placement_group_id                   = (known after apply)
      + placement_partition_number           = (known after apply)
      + primary_network_interface_id         = (known after apply)
      + private_dns                          = (known after apply)
      + private_ip                           = (known after apply)
      + public_dns                           = (known after apply)
      + public_ip                            = (known after apply)
      + region                               = "us-east-1"
      + secondary_private_ips                = (known after apply)
      + security_groups                      = (known after apply)
      + source_dest_check                    = true
      + spot_instance_request_id             = (known after apply)
      + subnet_id                            = (known after apply)
      + tags                                 = {
          + "Name" = "Terraform-Instance"
        }
      + tags_all                             = {
          + "Name" = "Terraform-Instance"
        }
      + tenancy                              = (known after apply)
      + user_data_base64                     = (known after apply)
      + user_data_replace_on_change          = false
      + vpc_security_group_ids               = (known after apply)

      + capacity_reservation_specification (known after apply)

      + cpu_options (known after apply)

      + ebs_block_device (known after apply)

      + enclave_options (known after apply)

      + ephemeral_block_device (known after apply)

      + instance_market_options (known after apply)

      + maintenance_options (known after apply)

      + metadata_options (known after apply)

      + network_interface (known after apply)

      + primary_network_interface (known after apply)

      + private_dns_name_options (known after apply)

      + root_block_device (known after apply)
    }

Plan: 1 to add, 0 to change, 0 to destroy.

────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

Note: You didn't use the -out option to save this plan, so Terraform can't guarantee to take exactly these actions if you run "terraform
apply" now.

```
- This output shows the execution plan, describing which actions Terraform will take in order to change real infrastructure to match the configuration. 

- Run `terraform apply`. You should see an output similar to the one shown above.

```bash
terraform apply
```

- Terraform will wait for your approval before proceeding. If anything in the plan seems incorrect it is safe to abort (ctrl+c) here with no changes made to your infrastructure.

- If the plan is acceptable, type "yes" at the confirmation prompt to proceed. Executing the plan will take a few minutes since Terraform waits for the EC2 instance to become available.

```txt
Do you want to perform these actions?
  Terraform will perform the actions described above.
  Only 'yes' will be accepted to approve.

  Enter a value: yes

aws_instance.terraform-ec2: Creating...
aws_instance.terraform-ec2: Still creating... [00m10s elapsed]
aws_instance.terraform-ec2: Creation complete after 13s [id=i-066cc122f0519730f]

Apply complete! Resources: 1 added, 0 changed, 0 destroyed.
```

- Visit the EC2 console to see the created EC2 instance.

### Inspect state

- When you applied your configuration, Terraform fetched data from resources into a file called terraform.tfstate. It keeps track of resources' metadata.

### Manually Managing State

- Terraform has a command called `terraform state` for advanced state management. For example, if you have a long state file (detailed) and you just want to see the name of your resources, which you can get them by using the `list` subcommand.

```bash
terraform state list
aws_instance.terraform-ec2
```
### Creating a AWS S3 Bucket

- Create a S3 bucket. Go to the `main.tf` and add the followings.

```t
terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "6.13.0"
    }
  }
}

provider "aws" {
  region  = "us-east-1"
}

resource "aws_instance" "terraform-ec2" {
  ami           = "ami-0360c520857e3138f"
  key_name      = "ozia"    # write your pem file without .pem extension>
  instance_type = "t2.micro"
  tags = {
    "Name" = "Terraform-Instance"
  }
}

resource "aws_s3_bucket" "example" {
  bucket = "hepapi-terraform"

  tags = {
    Name        = "hepapi-terraform"
  }
}

```
- Write your pem file without .pem extension and change the "addwhateveryouwant" part of the bucket name. Because bucket name must be unique.

- Run the command `terraform plan` and `terraform apply`.

```bash
terraform plan

aws_instance.terraform-ec2: Refreshing state... [id=i-066cc122f0519730f]

Terraform used the selected providers to generate the following execution plan. Resource actions are indicated with the following symbols:
  + create
-/+ destroy and then create replacement

Terraform will perform the following actions:

  # aws_instance.terraform-ec2 must be replaced
-/+ resource "aws_instance" "terraform-ec2" {
      ~ arn                                  = "arn:aws:ec2:us-east-1:995194808144:instance/i-066cc122f0519730f" -> (known after apply)
      ~ associate_public_ip_address          = true -> (known after apply)
      ~ availability_zone                    = "us-east-1b" -> (known after apply)
      ~ disable_api_stop                     = false -> (known after apply)
      ~ disable_api_termination              = false -> (known after apply)
      ~ ebs_optimized                        = false -> (known after apply)
      + enable_primary_ipv6                  = (known after apply)
      - hibernation                          = false -> null
      + host_id                              = (known after apply)
      + host_resource_group_arn              = (known after apply)
      + iam_instance_profile                 = (known after apply)
      ~ id                                   = "i-066cc122f0519730f" -> (known after apply)
      ~ instance_initiated_shutdown_behavior = "stop" -> (known after apply)
      + instance_lifecycle                   = (known after apply)
      ~ instance_state                       = "running" -> (known after apply)
      ~ ipv6_address_count                   = 0 -> (known after apply)
      ~ ipv6_addresses                       = [] -> (known after apply)
      + key_name                             = "ozia" # forces replacement
      ~ monitoring                           = false -> (known after apply)
      + outpost_arn                          = (known after apply)
      + password_data                        = (known after apply)
      + placement_group                      = (known after apply)
      + placement_group_id                   = (known after apply)
      ~ placement_partition_number           = 0 -> (known after apply)
      ~ primary_network_interface_id         = "eni-05952141fc35087b4" -> (known after apply)
      ~ private_dns                          = "ip-172-31-29-41.ec2.internal" -> (known after apply)
      ~ private_ip                           = "172.31.29.41" -> (known after apply)
      ~ public_dns                           = "ec2-98-81-153-252.compute-1.amazonaws.com" -> (known after apply)
      ~ public_ip                            = "98.81.153.252" -> (known after apply)
      ~ secondary_private_ips                = [] -> (known after apply)
      ~ security_groups                      = [
          - "default",
        ] -> (known after apply)
      + spot_instance_request_id             = (known after apply)
      ~ subnet_id                            = "subnet-b07951fa" -> (known after apply)
        tags                                 = {
            "Name" = "Terraform-Instance"
        }
      ~ tenancy                              = "default" -> (known after apply)
      + user_data_base64                     = (known after apply)
      ~ vpc_security_group_ids               = [
          - "sg-1b225758",
        ] -> (known after apply)
        # (8 unchanged attributes hidden)

      ~ capacity_reservation_specification (known after apply)
      - capacity_reservation_specification {
          - capacity_reservation_preference = "open" -> null
        }

      ~ cpu_options (known after apply)
      - cpu_options {
          - core_count       = 1 -> null
          - threads_per_core = 1 -> null
            # (1 unchanged attribute hidden)
        }

      - credit_specification {
          - cpu_credits = "standard" -> null
        }

      ~ ebs_block_device (known after apply)

      ~ enclave_options (known after apply)
      - enclave_options {
          - enabled = false -> null
        }

      ~ ephemeral_block_device (known after apply)

      ~ instance_market_options (known after apply)

      ~ maintenance_options (known after apply)
      - maintenance_options {
          - auto_recovery = "default" -> null
        }

      ~ metadata_options (known after apply)
      - metadata_options {
          - http_endpoint               = "enabled" -> null
          - http_protocol_ipv6          = "disabled" -> null
          - http_put_response_hop_limit = 2 -> null
          - http_tokens                 = "required" -> null
          - instance_metadata_tags      = "disabled" -> null
        }

      ~ network_interface (known after apply)

      ~ primary_network_interface (known after apply)
      - primary_network_interface {
          - delete_on_termination = true -> null
          - network_interface_id  = "eni-05952141fc35087b4" -> null
        }

      ~ private_dns_name_options (known after apply)
      - private_dns_name_options {
          - enable_resource_name_dns_a_record    = false -> null
          - enable_resource_name_dns_aaaa_record = false -> null
          - hostname_type                        = "ip-name" -> null
        }

      ~ root_block_device (known after apply)
      - root_block_device {
          - delete_on_termination = true -> null
          - device_name           = "/dev/sda1" -> null
          - encrypted             = false -> null
          - iops                  = 3000 -> null
          - tags                  = {} -> null
          - tags_all              = {} -> null
          - throughput            = 125 -> null
          - volume_id             = "vol-041d310011bf00def" -> null
          - volume_size           = 8 -> null
          - volume_type           = "gp3" -> null
            # (1 unchanged attribute hidden)
        }
    }

  # aws_s3_bucket.example will be created
  + resource "aws_s3_bucket" "example" {
      + acceleration_status         = (known after apply)
      + acl                         = (known after apply)
      + arn                         = (known after apply)
      + bucket                      = "hepapi-terraform"
      + bucket_domain_name          = (known after apply)
      + bucket_prefix               = (known after apply)
      + bucket_region               = (known after apply)
      + bucket_regional_domain_name = (known after apply)
      + force_destroy               = false
      + hosted_zone_id              = (known after apply)
      + id                          = (known after apply)
      + object_lock_enabled         = (known after apply)
      + policy                      = (known after apply)
      + region                      = "us-east-1"
      + request_payer               = (known after apply)
      + tags                        = {
          + "Name" = "hepapi-terraform"
        }
      + tags_all                    = {
          + "Name" = "hepapi-terraform"
        }
      + website_domain              = (known after apply)
      + website_endpoint            = (known after apply)

      + cors_rule (known after apply)

      + grant (known after apply)

      + lifecycle_rule (known after apply)

      + logging (known after apply)

      + object_lock_configuration (known after apply)

      + replication_configuration (known after apply)

      + server_side_encryption_configuration (known after apply)

      + versioning (known after apply)

      + website (known after apply)
    }

Plan: 2 to add, 0 to change, 1 to destroy.
```

```bash
terraform apply
```

```txt
Error: Error creating S3 bucket: AccessDenied: Access Denied
        status code: 403, request id: 8C5E290CD1CD3F71, host id: NT6nPSh0nW9rripGZrOAo48qJpZ2yToKCiGxDl6gfKIXY97uVH67lcvBiQjX9bsJRX3cL1oNVNM=
```

- Attach `S3FullAccess` policy to the "terraform" role.

```bash
terraform apply -auto-approve
```

```bash
Plan: 1 to add, 0 to change, 0 to destroy.
aws_s3_bucket.example: Creating...
aws_s3_bucket.example: Creation complete after 0s [id=hepapi-terraform]

Apply complete! Resources: 1 added, 0 changed, 0 destroyed.
```

- `-auto-approve` means to skip the approval of plan before applying.

- Go to the AWS console, check the S3 bucket. Then check the `terraform.tfstate` and `terraform.tfstate.backup` file.

- Now we will use `terraform plan -out ec2output`. This command will create an execution plan and it will save it in a file. It will be a binary file. Lets comment the EC2 instance resource block.


```bash
terraform plan -out=justs3
```
- Now we have just an S3 bucket in justs3. Check that `terraform.tfstate` file has both ec2 and s3 bucket (real infrastructure). If we apply justs3 file it will delete the EC2 instance and modify the tfstate file. You can save your plans with -out flag. First, you can uncomment the EC2 instance.

```bash
terraform apply justs3
```

### Destroy

The `terraform destroy` command terminates resources defined in your Terraform configuration. This command is the reverse of terraform apply in that it terminates all the resources specified by the configuration. It does not destroy resources running elsewhere that are not described in the current configuration. 

```bash
terraform destroy
```