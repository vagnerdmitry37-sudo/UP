resource "aws_iam_openid_connect_provider" "github" {
  url = "https://token.actions.githubusercontent.com"

  client_id_list = [
    "sts.amazonaws.com"
  ]

  thumbprint_list = [
    "6938fd4d98bab03faadb97b34396831e3780aea1"
  ]
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
                "token.actions.githubusercontent.com:sub" = "repo:vagnerdmitry37-sudo/UP:*"
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
