terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Configure the AWS Provider
provider "aws" {
  region = "eu-central-1"
}

# Define the EC2 instance
resource "aws_instance" "example" {
  ami           = "ami-0229b8f55e5178b65" # Amazon Linux 2 AMI (use a valid AMI ID for your region)
  instance_type = "t2.micro"              # Free-tier eligible instance type

  tags = {
    Name = "terraform-instance"
  }
}
