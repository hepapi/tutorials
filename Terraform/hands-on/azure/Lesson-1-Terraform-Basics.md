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
    https://releases.hashicorp.com/terraform/1.9.5/terraform_1.9.5_windows_amd64.zip
    or
    https://releases.hashicorp.com/terraform/1.9.5/terraform_1.9.5_windows_386.zip
    ```

    - Verify that the installation

    ```bash
    terraform version
    ```
2. **Install Azure-CLI**

  - Follow the instructions here: [Azure CLI Documentation](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
    - Macos

    ```bash
    brew update
    brew install azure-cli
    ```
    - Linux Ubuntu/Debian

    ```bash
    curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
    ```
    - Windows

    ```bash
    https://aka.ms/installazurecliwindows
    or
    https://aka.ms/installazurecliwindowsx64
    ```

    - Verify that the installation

    ```bash
    az version
    ```
    - Your Azure configured locally. 

    ```bash
    az login
    ```
    - Hard-coding credentials into any Terraform configuration is not recommended, and risks secret leakage should this file ever be committed to a public version control system. Using Azure credentials in server is not recommended.

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

- The set of files used to describe infrastructure in Terraform is known as a Terraform configuration. You'll write your first configuration file to launch a single Azure Resource Group.

- Each configuration should be in its own directory. Create a directory ("terraform-az") for the new configuration and change into the directory.

```bash
mkdir terraform-az && cd terraform-az && touch main.tf
```

- Install the `HashiCorp Terraform` extension in VSCode.

- Create a file named `main.tf` for the configuration code and copy and paste the following content. 

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.2.0"
    }
  }
}

provider "azurerm" {
  features {
  } 
  subscription_id = <your_subscription_id>  #change me
}

resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg"
  location = "West Europe"

  tags = {
    Name = "terraform-rg"
  }
}
```

- Explain the each block via the following section.

### Providers

The `provider` block configures the name of provider, in our case `azurerm`, which is responsible for creating and managing resources. A provider is a plugin that Terraform uses to translate the API interactions with the service. A provider is responsible for understanding API interactions and exposing resources. Because Terraform can interact with any API, you can represent almost any infrastructure type as a resource in Terraform.

The `profile` attribute in your provider block refers Terraform to the AWS credentials stored in your AWS Config File, which you created when you configured the AWS CLI. HashiCorp recommends that you never hard-code credentials into `*.tf configuration files`.

### Resources

The `resource` block defines a piece of infrastructure. A resource might be a physical component such as a Resource Group.

The resource block must have two required data for Resource Group. : the resource type and the resource name. In the example, the resource type is `azurerm_resource_group` and the local name is `resource_group`. The prefix of the type maps to the provider. In our case "azurerm_resource_group" automatically tells Terraform that it is managed by the "azurerm" provider.

The arguments for the resource are within the resource block. The arguments could be things like resource group names, resource group locations , or resource groups tags. For your Resource Group, you specified a name for `terraform-rg` and location will be `West Europe`.

### Initialize the directory

When you create a new configuration you need to initialize the directory with `terraform init`.

- Initialize the directory.

```bash
terraform init

Initializing the backend...

Initializing provider plugins...
- Finding hashicorp/azurerm versions matching "4.1.0"...
- Installing hashicorp/azurerm v4.1.0...
- Installed hashicorp/azurerm v4.1.0 (signed by HashiCorp)

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

Terraform downloads the `azurerm` provider and installs it in a hidden subdirectory (.terraform) of the current working directory. The output shows which version of the plugin was installed.

- Show the `.terraform` folder and inspect it.

### Create infrastructure

- Run `terraform plan`. You should see an output similar to the one shown below.

```bash
terraform plan

An execution plan has been generated and is shown below.
Resource actions are indicated with the following symbols:
  + create

Terraform used the selected providers to generate the following execution plan. Resource actions are indicated with the following symbols:
  + create

Terraform will perform the following actions:

  # azurerm_resource_group.resource_group will be created
  + resource "azurerm_resource_group" "resource_group" {
      + id       = (known after apply)
      + location = "westeurope"
      + name     = "terraform-rg"
      + tags     = {
          + "Name" = "terraform-rg"
        }
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

- If the plan is acceptable, type "yes" at the confirmation prompt to proceed. Executing the plan will take a few minutes since Terraform waits for the Resource Group to become available.

```txt
Do you want to perform these actions?
  Terraform will perform the actions described above.
  Only 'yes' will be accepted to approve.

  Enter a value: yes

azurerm_resource_group.resource_group: Creating...
azurerm_resource_group.resource_group: Still creating... [10s elapsed]
azurerm_resource_group.resource_group: Creation complete after 12s [id=/subscriptions/044e891f-d925-4ac5-b5b7-dfec1f645a88/resourceGroups/terraform-rg]

Apply complete! Resources: 1 added, 0 changed, 0 destroyed.
```

- Visit the Resource Group console to see the created Resource Group.

### Inspect state

- When you applied your configuration, Terraform fetched data from resources into a file called terraform.tfstate. It keeps track of resources' metadata.

### Manually Managing State

- Terraform has a command called `terraform state` for advanced state management. For example, if you have a long state file (detailed) and you just want to see the name of your resources, which you can get them by using the `list` subcommand.

```bash
terraform state list
azurerm_resource_group.resource_group
```
### Creating a Azure Blob Storage

- Create a Azure Blob Storage bucket. Go to the `main.tf` and add the followings.

```t
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.2.0"
    }
  }
}

provider "azurerm" {
  features {
  }
  subscription_id =  <your_subscription_id>  #change me
}

resource "azurerm_resource_group" "resource_group" {
  name     = "terraform-rg"
  location = "West Europe"

  tags = {
    Name = "terraform-rg"
  }
}

resource "azurerm_storage_account" "terraform_storage" {
  name                     = "terraformstoragehepapi"
  resource_group_name      = azurerm_resource_group.resource_group.name
  location                 = azurerm_resource_group.resource_group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    Name = "terraformstorage"
  }
}

resource "azurerm_storage_container" "tfstate" {
  name                  = "terraformcontainerhepapi"
  storage_account_name  = azurerm_storage_account.terraform_storage.name
  container_access_type = "private"
}

```

```bash
terraform plan

An execution plan has been generated and is shown below.
Resource actions are indicated with the following symbols:
  + create

Terraform used the selected providers to generate the following execution plan. Resource actions are indicated with the following symbols:
  + create

Terraform will perform the following actions:

  # azurerm_storage_account.terraform_storage will be created
  + resource "azurerm_storage_account" "terraform_storage" {
      + access_tier                        = (known after apply)
      + account_kind                       = "StorageV2"
      + account_replication_type           = "LRS"
      + account_tier                       = "Standard"
      + allow_nested_items_to_be_public    = true
      + cross_tenant_replication_enabled   = false
      + default_to_oauth_authentication    = false
      + dns_endpoint_type                  = "Standard"
      + https_traffic_only_enabled         = true
      + id                                 = (known after apply)
      + infrastructure_encryption_enabled  = false
      + is_hns_enabled                     = false
      + large_file_share_enabled           = (known after apply)
      + local_user_enabled                 = true
      + location                           = "westeurope"
      + min_tls_version                    = "TLS1_2"
      + name                               = "terraformstorage"
      + nfsv3_enabled                      = false
      + primary_access_key                 = (sensitive value)
      + primary_blob_connection_string     = (sensitive value)
      + primary_blob_endpoint              = (known after apply)
      + primary_blob_host                  = (known after apply)
      + primary_blob_internet_endpoint     = (known after apply)
      + primary_blob_internet_host         = (known after apply)
      + primary_blob_microsoft_endpoint    = (known after apply)
      + primary_blob_microsoft_host        = (known after apply)
      + primary_connection_string          = (sensitive value)
      + primary_dfs_endpoint               = (known after apply)
      + primary_dfs_host                   = (known after apply)
      + primary_dfs_internet_endpoint      = (known after apply)
      + primary_dfs_internet_host          = (known after apply)
      + primary_dfs_microsoft_endpoint     = (known after apply)
      + primary_dfs_microsoft_host         = (known after apply)
      + primary_file_endpoint              = (known after apply)
      + primary_file_host                  = (known after apply)
      + primary_file_internet_endpoint     = (known after apply)
      + primary_file_internet_host         = (known after apply)
      + primary_file_microsoft_endpoint    = (known after apply)
      + primary_file_microsoft_host        = (known after apply)
      + primary_location                   = (known after apply)
      + primary_queue_endpoint             = (known after apply)
      + primary_queue_host                 = (known after apply)
      + primary_queue_microsoft_endpoint   = (known after apply)
      + primary_queue_microsoft_host       = (known after apply)
      + primary_table_endpoint             = (known after apply)
      + primary_table_host                 = (known after apply)
      + primary_table_microsoft_endpoint   = (known after apply)
      + primary_table_microsoft_host       = (known after apply)
      + primary_web_endpoint               = (known after apply)
      + primary_web_host                   = (known after apply)
      + primary_web_internet_endpoint      = (known after apply)
      + primary_web_internet_host          = (known after apply)
      + primary_web_microsoft_endpoint     = (known after apply)
      + primary_web_microsoft_host         = (known after apply)
      + public_network_access_enabled      = true
      + queue_encryption_key_type          = "Service"
      + resource_group_name                = "terraform-rg"
      + secondary_access_key               = (sensitive value)
      + secondary_blob_connection_string   = (sensitive value)
      + secondary_blob_endpoint            = (known after apply)
      + secondary_blob_host                = (known after apply)
      + secondary_blob_internet_endpoint   = (known after apply)
      + secondary_blob_internet_host       = (known after apply)
      + secondary_blob_microsoft_endpoint  = (known after apply)
      + secondary_blob_microsoft_host      = (known after apply)
      + secondary_connection_string        = (sensitive value)
      + secondary_dfs_endpoint             = (known after apply)
      + secondary_dfs_host                 = (known after apply)
      + secondary_dfs_internet_endpoint    = (known after apply)
      + secondary_dfs_internet_host        = (known after apply)
      + secondary_dfs_microsoft_endpoint   = (known after apply)
      + secondary_dfs_microsoft_host       = (known after apply)
      + secondary_file_endpoint            = (known after apply)
      + secondary_file_host                = (known after apply)
      + secondary_file_internet_endpoint   = (known after apply)
      + secondary_file_internet_host       = (known after apply)
      + secondary_file_microsoft_endpoint  = (known after apply)
      + secondary_file_microsoft_host      = (known after apply)
      + secondary_location                 = (known after apply)
      + secondary_queue_endpoint           = (known after apply)
      + secondary_queue_host               = (known after apply)
      + secondary_queue_microsoft_endpoint = (known after apply)
      + secondary_queue_microsoft_host     = (known after apply)
      + secondary_table_endpoint           = (known after apply)
      + secondary_table_host               = (known after apply)
      + secondary_table_microsoft_endpoint = (known after apply)
      + secondary_table_microsoft_host     = (known after apply)
      + secondary_web_endpoint             = (known after apply)
      + secondary_web_host                 = (known after apply)
      + secondary_web_internet_endpoint    = (known after apply)
      + secondary_web_internet_host        = (known after apply)
      + secondary_web_microsoft_endpoint   = (known after apply)
      + secondary_web_microsoft_host       = (known after apply)
      + sftp_enabled                       = false
      + shared_access_key_enabled          = true
      + table_encryption_key_type          = "Service"
      + tags                               = {
          + "Name" = "terraformstorage"
        }
    }

  # azurerm_storage_container.tfstate will be created
  + resource "azurerm_storage_container" "tfstate" {
      + container_access_type             = "private"
      + default_encryption_scope          = (known after apply)
      + encryption_scope_override_enabled = true
      + has_immutability_policy           = (known after apply)
      + has_legal_hold                    = (known after apply)
      + id                                = (known after apply)
      + metadata                          = (known after apply)
      + name                              = "terraformcontainer"
      + resource_manager_id               = (known after apply)
      + storage_account_name              = "terraformstorage"
    }

Plan: 2 to add, 0 to change, 0 to destroy.

------------------------------------------------------------------------

Note: You didn't specify an "-out" parameter to save this plan, so Terraform can't guarantee that exactly these actions will be performed if "terraform apply" is subsequently run.
```

```bash
terraform apply
```
```txt
Do you want to perform these actions?
  Terraform will perform the actions described above.
  Only 'yes' will be accepted to approve.

  Enter a value: yes

azurerm_resource_group.resource_group: Creating...
azurerm_resource_group.resource_group: Creation complete after 9s [id=/subscriptions/044e891f-d925-4ac5-b5b7-dfec1f645a88/resourceGroups/terraform-rg]
azurerm_storage_account.terraform_storage: Creating...
azurerm_storage_account.terraform_storage: Still creating... [10s elapsed]
azurerm_storage_account.terraform_storage: Still creating... [20s elapsed]
azurerm_storage_account.terraform_storage: Still creating... [30s elapsed]
azurerm_storage_account.terraform_storage: Still creating... [40s elapsed]
azurerm_storage_account.terraform_storage: Still creating... [50s elapsed]
azurerm_storage_account.terraform_storage: Still creating... [1m0s elapsed]
azurerm_storage_account.terraform_storage: Creation complete after 1m6s [id=/subscriptions/044e891f-d925-4ac5-b5b7-dfec1f645a88/resourceGroups/terraform-rg/providers/Microsoft.Storage/storageAccounts/terraformstoragehepapi]
azurerm_storage_container.tfstate: Creating...
azurerm_storage_container.tfstate: Creation complete after 1s [id=https://terraformstoragehepapi.blob.core.windows.net/terraformcontainerhepapi]

Apply complete! Resources: 3 added, 0 changed, 0 destroyed.
```

- Visit the Resource Group console to see the created Resource Group.

```bash
terraform apply -auto-approve
```

- `-auto-approve` means to skip the approval of plan before applying.

- Go to the Azure console, check the Blob Storage bucket. Then check the `terraform.tfstate` and `terraform.tfstate.backup` file.

- Now we will use `terraform plan -out namewhateveryouwant`. This command will create an execution plan and it will save it in a file. It will be a binary file.


```bash
terraform plan -out=hepapitf
```

```bash
terraform apply hepapitf
```

### Destroy

The `terraform destroy` command terminates resources defined in your Terraform configuration. This command is the reverse of terraform apply in that it terminates all the resources specified by the configuration. It does not destroy resources running elsewhere that are not described in the current configuration. 

```bash
terraform destroy
```