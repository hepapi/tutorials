# Session-9: S3(Advanced) & CloudFront

The purpose of this session is to explore the most commonly used features of AWS CloudFront and 
Redshift, including content delivery and caching with CloudFront, as well as data warehousing 
and analytics setup with Redshift.

- Part 1: S3 (Advanced)
   - Static Website Hosting
   - Access Points
- Part 2: Cloudfront
   - Content delivery and caching

NOTE: If you do this session using an IAM-User under an organization account, you should attach
`-<Your-Name>` at the end of the each resource name. This would avoid confusion between IAM-Users
because the resources would be created under one account, namely `Management Account`.

Example: If the resource name is `aws-training-billing-alarm`, which is a CloudWatch alarm name; 
make it `aws-training-billing-alarm-john` so that you now that you created this resource.

## Part 1: S3 (Advanced)

### Part 1.a: Static Website Hosting

- Go to `S3` from the AWS Management Console.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session9-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Select the bucket you created and choose `Copy ARN`. Save it on a notepad.

- Choose the bucket you created. Choose `Upload` and choose `Add files`.

- The `index.html` and the `error.html` are located under `/part1-files` folder. Browse and choose them.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Under `Properties`, you can see the `Static website hosting`. Choose `Edit`.

```text
Static website hosting: Enable
Index document: index.html
Error document: error.html
Leave the rest as default.
```

- Under `Properties`, you can see `Bucket website endpoint`. Open it in a new tab.

- Notice that you don't have permission to open the website.

- Go to bucket page. Under `Permissions`, choose `Edit` for `Block public access (bucket settings)`.

```text
(Uncheck) Block all public access
```

- Click `Save changes`. Type `confirm` for `Edit Block public access (bucket settings)`.

- Choose `Edit` for `Bucket Policy`. Copy and paste the following for the `Policy`.

```text
{
	"Version": "2012-10-17",
	"Statement": [
		{
			"Sid": "AllowPublic",
			"Principal": "*",
			"Effect": "Allow",
			"Action": [
				"s3:GetObject"
			],
			"Resource": [
			    "arn:aws:s3:::test-bucket-for-session9-xxxxx/*"
			]
		}
	]
}
```

- Don't forget to change the ARN. Also, append `/*` at the end of the ARN.

- Click `Save changes`.

- Under `Properties`, you can see `Bucket website endpoint`. Open it in a new tab again.

- Notice that you have permission to open the website now.

- Don't forget to destroy the resources you created.

### Part 1.b: Access Points

- Go to `S3` from the AWS Management Console.

- From the menu on the left, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session9-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Choose the bucket you created. Then, choose `Create folder`.

```text
Folder name: folder1
Leave the rest as default.
```

- Choose `Create folder`. 

- Upload the `Butterfly.jpg` under `/part1-files` to the first folder you created.

- In the bucket page, navigate to `Access Points` and choose `Create access point`.

```text
Access point name: session9-accesspoint
Network origin: Internet
Leave the rest as default.
```

- Choose `Create access point`.

- Go to `IAM`, choose `Policies` from the menu on the left. Choose `Create policy`.

```text
Policy editor: JSON
Network origin: Internet
Leave the rest as default.
```

- Copy and paste the following for the policy.

```text
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "arn:aws:s3:::*",
                "arn:aws:s3:*:<your-aws-account-id>:accesspoint/session9-accesspoint",
                "arn:aws:s3:*:<your-aws-account-id>:accesspoint/session9-accesspoint/*"
            ]
        },
        {
            "Effect": "Deny",
            "Action": [
                "s3:GetObject",
                "s3:PutObject"
            ],
            "Resource": "*",
            "Condition": {
                "StringNotLike": {
                    "s3:DataAccessPointArn": "arn:aws:s3:*:<your-aws-account-id>:accesspoint/*"
                }
            }
        }
    ]
}
```

- Choose `Next`.

```text
Policy name: test-policy-session9 
Description: test-policy-session9 
Leave the rest as default.
```

- Choose `Create policy`.

- Go to `IAM`, choose `Users` from the menu on the left. Choose `Create User`.

```text
User name: test-user-session9
(Check) Provide user access to the AWS Management Console - optional
User type: I want to create an IAM user
Console password: Custom password (Password123)
(Uncheck) Users must create a new password at next sign-in - Recommended
Leave the rest as default.
```

- Click `Next` and continue ...

```text
Permissions options: Attach policies directly
Permissions policies: test-policy-session9
Leave the rest as default.
```

- Click `Next` and continue ...

- Choose `Create user`.

- Open a private tab and login as the user you created.

- Open the bucket page you created and navigate to `Access Points`.

- Select the access point you created and choose `Edit policy`.

- Copy and paste the following.

```text
{
	"Version": "2012-10-17",
	"Statement": [
		{
			"Sid": "GetObject",
			"Effect": "Allow",
			"Principal": {
				"AWS": "arn:aws:iam::<your-aws-account-id>:user/test-user-session9"
			},
			"Action": [
				"s3:GetObject",
				"s3:PutObject"
			],
			"Resource": "arn:aws:s3:<your-region>:<your-aws-account-id>:accesspoint/session9-accesspoint/object/*"
		}
	]
}
```

- Choose `Save`.

- Now, open the private tab in which your IAM User is present.

- Go to `S3` and choose `Buckets` from the menu on the left

- Open your bucket and go inside the folder. Choose the image.

- In the object page, choose `Open`. Notice that you can't open it.

- Now, choose `Access points` from the menu on the left. Choose your access point.

- Go inside the folder. Choose the image.

- In the object page, choose `Open`. Notice that you can open it.

- Notice the difference coming from the policies.

- Don't forget to destroy the resources you created.

## Part 2: CloudFront

### Content delivery and caching

- Go to `S3` from AWS Management Console. From the menu on the left, choose `Buckets`.

- Choose `Create bucket`.

```text
Bucket name: test-bucket-for-session9-xxxxx
Object Ownership: ACLs disabled (recommended)
(Check) Block all public access
Bucket versioning: Disable
Encryption type: Server-side encryption with Amazon S3 managed keys (SSE-S3)
Bucket key: Enable
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- In the Amazon S3 console, choose the bucket you created.

- Choose `Upload` under `Objects`. Then, choose `Add files`.

- The `cat.jpg` and `index.html` is located under `/part2-files` folder. Browse and choose them.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Choose the object you uploaded to see its details.

- Copy and paste the `Object URL` in a new tab. Notice that it is not opened.

- Go back to object page and choose `Open`. Notice that it is opened.

- Go to `CloudFront` from AWS Management Console. From the menu on the left, choose `Distributions`.

- Choose `Create a CloudFront distribution`.

```text
Origin domain: <your-bucket-name>.s3.amazonaws.com
Origin access: Origin access control settings (recommended)
Origin access control: Create new OAC -> Create with Default Settings
Notice the "You must update the S3 bucket policy" warning.
Web Application Firewall (WAF): Do not enable security protections
Default root object - optional: index.html
Leave the rest as default.
```

- Choose `Create distribution`.

- The creation of distribution can take some minutes. You can proceed.

- Notice that a warning message is appeared on the screen.

- Choose `Copy policy`.

- Go to your bucket page. Choose `Permissions` tab.

- In the `Bucket policy` section, choose `Edit`.

- Paste the policy and analyze it.

- Choose `Save Changes`.

- Go back to your distribution page.

- Check that the `Last Modified` status turns from `Deploying` to a date.

- Copy the `Distribution domain name` and paste it on a new tab.

- Notice that a web page is opened.

- If you refresh the page, the content will be delivered from cache which is faster.

- Go to `CloudFront` from AWS Management Console. From the menu on the left, choose `Distributions`.

- Select the distribution you created and choose `Disable`.

- The disabling of distribution can take some minutes. You can proceed.

- Go to your bucket. Empty and delete it.

- Check that the `Last Modified` status turns from `Deploying` to a date.

- Select your distribution again and choose `Delete`.

- From the menu on the left, choose `Origin access`.

- Select the origin access you created. Then, choose `Delete`.
