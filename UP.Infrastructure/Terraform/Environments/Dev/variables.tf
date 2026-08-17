variable "aws_region" {
  type    = string
  default = "eu-central-1"
}

variable "ami_id" {
  type = string
}

variable "instance_type" {
  type    = string
  default = "t3.micro"
}

variable "key_name" {
  type = string
}

variable "up_api_image_address" {
  description = "Docker image to deploy"
  type        = string
}