terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  backend "s3" {
    bucket       = "up-app-terraform-state"
    key          = "environments/dev/terraform.tfstate"
    region       = "eu-central-1"
    use_lockfile = true
  }
}

resource "aws_iam_openid_connect_provider" "github" {
  url = "https://token.actions.githubusercontent.com"

  client_id_list = [
    "sts.amazonaws.com"
  ]

  # thumbprint_list = [
  #   "6938fd4d98bab03faadb97b34396831e3780aea1"
  # ]
}

resource "aws_iam_role" "github_actions" {
  name        = "up-api-dev-github-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"

    Statement = [
      {
        Effect = "Allow"

        Principal = {
          Federated = aws_iam_openid_connect_provider.github.arn
        }

        Action = "sts:AssumeRoleWithWebIdentity"

        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
          }

          StringLike = {
            "token.actions.githubusercontent.com:sub" = [
              "repo:vagnerdmitry37-sudo/UP:environment:Dev"
            ]
          }
        }
      }
    ]
  })
}

resource "aws_iam_policy" "github_actions_s3_upload" {
  name        = "GitHubActionsS3UploadPolicy"
  description = "Allows GitHub Actions to upload files to S3"

  policy = jsonencode({
    Version = "2012-10-17"

    Statement = [
      {
        Effect = "Allow"

        Action = [
          "s3:PutObject",
          "s3:GetObject",
          "s3:ListBucket"
        ]

        Resource = [
          "arn:aws:s3:::up-app-terraform-state",
          "arn:aws:s3:::up-app-terraform-state/*"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "github_actions_s3_upload" {
  role       = aws_iam_role.github_actions.name
  policy_arn = aws_iam_policy.github_actions_s3_upload.arn
}

# provider "aws" {
#   region = var.aws_region
# }

# data "aws_vpc" "default" {
#   default = true
# }

# data "aws_subnets" "default" {
#   filter {
#     name   = "vpc-id"
#     values = [data.aws_vpc.default.id]
#   }
# }

# resource "aws_security_group" "dev" {
#   name   = "up-dev"
#   vpc_id = data.aws_vpc.default.id

#   ingress {
#     description = "API"
#     from_port   = 8080
#     to_port     = 8080
#     protocol    = "tcp"
#     cidr_blocks = ["0.0.0.0/0"]
#   }

#   ingress {
#     description = "SSH"
#     from_port   = 22
#     to_port     = 22
#     protocol    = "tcp"
#     cidr_blocks = ["86.49.254.170/32"]
#   }

#   egress {
#     from_port   = 0
#     to_port     = 0
#     protocol    = "-1"
#     cidr_blocks = ["0.0.0.0/0"]
#   }
# }

# resource "aws_instance" "dev" {
#   ami           = var.ami_id
#   instance_type = var.instance_type

#   subnet_id = data.aws_subnets.default.ids[0]

#   vpc_security_group_ids = [
#     aws_security_group.dev.id
#   ]

#   key_name = var.key_name

#   user_data = templatefile("${path.module}/user_data.sh", {
#     docker_compose = file("${path.module}/compose.yaml")
#   })

#   tags = {
#     Name        = "up-dev"
#     Environment = "dev"
#   }
# }