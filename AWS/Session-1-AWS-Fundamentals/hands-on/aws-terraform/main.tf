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
  region = "us-east-1"
}

# Define the EC2 instance
resource "aws_instance" "example" {
  ami           = "ami-0c02fb55956c7d316" # Amazon Linux 2 AMI (use a valid AMI ID for your region)
  instance_type = "t2.micro"              # Free-tier eligible instance type

  tags = {
    Name = "example-instance"
  }
}
