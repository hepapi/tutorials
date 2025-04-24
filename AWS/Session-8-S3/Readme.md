# Session-8: S3 (Simple Storage Service)

The purpose of this session is to explore the most commonly used features of Amazon S3, including 
scalable storage management, secure access control, versioning for data recovery, lifecycle rules 
for cost optimization, cross-region replication for disaster recovery, and static website hosting.

- Part 1: Amazon S3 (Simple Storage Service) Setup
   - Creating and managing buckets
- Part 2: Permissions and Access Control
- Part 3: Versioning
   - Enabling and managing object versions
- Part 4: Lifecycle Rules
   - Configuring lifecycle management
- Part 5: Cross-Region Replication
   - Configuring replication across regions
- Part 6: Static Website Hosting on S3

NOTE: If you do this session using an IAM-User under an organization account, you should attach
`-<Your-Name>` at the end of the each resource name. This would avoid confusion between IAM-Users
because the resources would be created under one account, namely `Management Account`.

Example: If the resource name is `aws-training-billing-alarm`, which is a CloudWatch alarm name; 
make it `aws-training-billing-alarm-john` so that you now that you created this resource.

## Part 1: Amazon S3 (Simple Storage Service) Setup

### Creating and managing buckets

- Go to `S3` from AWS Management Console. From the menu on the left, choose `Buckets`.

- Choose `Create bucket`.

```text
Bucket name: test-bucket-for-session8-xxxxx
Object Ownership: ACLs disabled (recommended)
(Check) Block all public access
Bucket versioning: Disable
Encryption type: Server-side encryption with Amazon S3 managed keys (SSE-S3)
Bucket key: Enable
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. Notice that the name of the bucket is displayed among 
the list of all the buckets for the account.

- In the Amazon S3 console, choose the bucket you created.

- Analyze the bucket details under `Properties`.

- Choose `Upload` under `Objects`. Then, choose `Add files`.

- The `HappyFace.jpg` is located under `/pics` folder. Browse and choose `HappyFace.jpg`.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Choose the object you uploaded to see its details.

- Copy and paste the `Object URL` in a new tab. Notice that it is not opened.

- Go back to object page and choose `Open`. Notice that it is opened.

- Go back to main page of the bucket. Choose `Create folder`.

```text
Folder name: test
Leave the rest as default.
```

- Choose `Create folder`. Then, choose the folder you created.

- Choose `Upload` under `Objects`. Then, choose `Add files`.

- The `Computer.jpg` and the `Butterfly.jpg` are located under `/pics` folder. 

- Browse and choose them.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Choose `Close` after upload. Go to main page of the bucket.

- Under the folder, choose one of the objects you uploaded. Examine its `S3 URI`.

- Go to main page of the bucket. Select the `HappyFace.jpg`. Then, choose `Delete`.

- On the displayed page, type `permanently delete` and choose `Delete object`.

- Choose `Close` and choose the folder you created. Choose `Create folder`.

```text
Folder name: test-inner
Leave the rest as default.
```

- Choose `Create folder`. Then, choose the folder you created.

- Go to page of the first folder you created. Notice the second folder you created.

- Select one of the objects you uploaded. Then, choose `Actions -> Move`.

- On the displayed page, choose `Browse S3` under `Destination`.

- Select the second folder you created and choose `Choose destination`.

- Choose `Close`. Go to second folder you created.

- Notice that the object is moved inside the second folder.

- Go to `S3`. From the menu on the left, choose `Buckets`.

- Select the bucket you created and choose `Empty`.

- On the displayed page, type `permanently delete` and choose `Empty`.

- Again, select the bucket you created and choose `Delete`.

- On the displayed page, type the name of the bucket and choose `Delete`.

## Part 2: Permissions and Access Control

- Go to `IAM`, choose `Users` from the menu on the left. Choose `Create User`.

```text
User name: test-user
(Check) Provide user access to the AWS Management Console - optional
User type: I want to create an IAM user
Console password: Custom password (Password123)
(Uncheck) Users must create a new password at next sign-in - Recommended
Leave the rest as default.
```

```text
Permissions options: Attach policies directly
Permissions policies -> Create policy
Select a service: S3 
Access level -> List -> ListBucket, ListAllBuckets
Leave the rest as default.
```

- Click `Next` and continue ...

```text
Policy name: test-user-session8-policy
Description: test-user-session8-policy
Leave the rest as default.
```

- Click `Create policy` and go back to `Set Permissions` page.

- Refresh the `Permissions policies` and search for the policy you created.

- Click `Next` and continue ...

- Choose `Create User` and open AWS Management Console in a private tab. 

- Login as a user you created. You can call it as test user.

```text
Account ID: <Your 12-digit Account ID>
User name: test-user
Password: Password123
```

- Now, you have two users. One in your browser, and one in your private tab.

- Go to `S3` from your main account. From the menu on the left, choose `Buckets`.

- Choose `Create bucket`.

```text
Bucket name: test-bucket-for-session8-xxxxx
Object Ownership: ACLs disabled (recommended)
(Check) Block all public access
Bucket versioning: Disable
Encryption type: Server-side encryption with Amazon S3 managed keys (SSE-S3)
Bucket key: Enable
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`.

- Choose `Upload` under `Objects`. Open the `/folder` folder in your directory.

- Drag and drop the files to `S3` and check that they uploaded.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Choose one of the object and choose `Open`. Also, copy and paste its `Object URL` in a new tab.

- Open the private tab in which you loging as test user. Go to `S3`. Choose the bucket you created.

- Choose one of the object and choose `Open`. Also, copy and paste its `Object URL` in a new tab.

- Notice the difference.

- Go to bucket page from your main account. Under `Permissions`, choose `Edit` for `Bucket Policy`.

- Copy and paste the following. Don't forget to change the `Bucket ARN` and `Account ID`.

- You can find `Account ID` in the top-right corner of the page.

- You can find `Bucket ARN` in the page `Edit Bucket Policy` (which you currently in).

```text
{
	"Version": "2012-10-17",
	"Statement": [
		{
			"Sid": "test-user-s3-policy",
			"Principal": {
			    "AWS": [
                    "arn:aws:iam::<Your Account ID>:user/test-user"
                 ]
			},
			"Effect": "Allow",
			"Action": [
				"s3:GetObject"
			],
			"Resource": [
				"<Your Bucket ARN>/pics-folder/*"
			]
		}
	]
}
```

- Now, open the bucket page as test user. Choose one of the objects in `/pics-folder` and `Open` it.

- Notice the effect.

- Go to bucket page from your main account. Under `Permissions`, choose `Edit` for `Bucket Policy`.

- Copy and paste the following. Don't forget to change the `Bucket ARN` and `Account ID`.

```text
{
	"Version": "2012-10-17",
	"Statement": [
		{
			"Sid": "test-user-s3-policy",
			"Principal": {
			    "AWS": [
                    "arn:aws:iam::<Your Account ID>:user/test-user"
                 ]
			},
			"Effect": "Allow",
			"Action": [
				"s3:GetObject"
			],
			"Resource": [
				"<Your Bucket ARN>/pics-folder/*"
			]
		},
		{
			"Sid": "test-user-s3-policy-2",
			"Principal": {
			    "AWS": [
                    "arn:aws:iam::<Your Account ID>:user/test-user"
                 ]
			},
			"Effect": "Allow",
			"Action": [
				"s3:DeleteObject"
			],
			"Resource": [
				"<Your Bucket ARN>/files-folder/*"
			]
		}
	]
}
```

- Now, open the bucket page as test user. Choose one of the objects in `/files-folder` and `Delete` it.

- Notice the effect. Go back to your main account.

- Go to `Permissions` in the bucket page. Choose `Edit` for `Block public access (bucket settings)`.

```text
(Uncheck) Block all public access
```

- Choose `Save changes`. On the displayed page, type `confirm` and choose `Confirm`.

- Go back to bucket page. Under `Permissions`, choose `Edit` for `Bucket Policy`.

- Copy and paste the following. Don't forget to change the `Bucket ARN` and `Account ID`.

```text
{
	"Version": "2012-10-17",
	"Statement": [
		{
			"Sid": "test-user-s3-policy",
			"Effect": "Allow",
			"Principal": {
				"AWS": "arn:aws:iam::<Your Account ID>:user/test-user"
			},
			"Action": [
				"s3:GetObject",
			],
			"Resource": [
				"<Your Bucket ARN>/pics-folder/*",
			]
		},
		{
			"Sid": "test-user-s3-policy-2",
			"Effect": "Allow",
			"Principal": {
				"AWS": "arn:aws:iam::<Your Account ID>:user/test-user"
			},
			"Action": "s3:DeleteObject",
			"Resource": "<Your Bucket ARN>/files-folder/*"
		},
		{
			"Sid": "test-user-s3-policy-3",
			"Effect": "Allow",
			"Principal": "*",
			"Action": "s3:GetObject",
			"Resource": "<Your Bucket ARN>/HappyFace.jpg"
		}
	]
}
```

- Now, open the bucket page as test user. Choose `HappyFace.jpg`.

- Copy and paste its `Object URL` in a new tab. Notice the effect.

- Don't forget to destroy the resources you created in this part.

## Part 3: Versioning

### Enabling and managing object versions

- Type `S3` in the search bar and go to its console page.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session8-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Choose `Upload` under `Objects`.

- The `file1.txt` is located under `/files` folder. Browse and choose `file1.txt`.

- The `file1.txt` has the following content.

```text
This is version 1
```

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Choose the object to see its content. Then, choose `Open`.

- You will update the file content and upload several times.

- Change the `file1.txt` content as follows and upload.

```text
This is version 2
```

- `Open` the content of object and notice the difference.

- Go to bucket page and `Delete` the object.

- Under `Properties`, choose `Edit` for `Bucket Versioning`.

```text
Bucket Versioning: Enable
```

- Choose `Save changes`.

- Change the `file1.txt` content as follows and upload.

```text
This is version 1
```

- Now, change the `file1.txt` content as follows and upload.

```text
This is version 2
```

- Go to bucket page and turn on `Show versions`. Notice the structure.

- `Open` the content of objects seperately and notice the use of versioning.

- Now, change the `file1.txt` content as follows and upload.

```text
This is version 3
```

- Go to bucket page. Select the object and choose `Delete`.

- On the displayed page, type `delete` and choose `Delete objects`.

- Go to bucket page and turn on `Show versions`. Notice the structure.

- There is a `Delete Marker`. Select it to remove and choose `Delete`.

- On the displayed page, type `permanently delete` and choose `Delete objects`.

- Go to bucket page and turn on `Show versions`. Notice the structure.

- Now, select the second version and choose `Delete`.

- On the displayed page, type `permanently delete` and choose `Delete objects`.

- Go to bucket page and turn on `Show versions`. Notice the structure.

- Notice that the second version of the object is permanently deleted.

- Under `Properties`, choose `Edit` for `Bucket Versioning`.

```text
Bucket Versioning: Suspend
(Check) I acknowledge the outcomes of suspending Bucket Versioning.
```

- Choose `Save changes`.

- Change the `file1.txt` content as follows and upload.

```text
This is version 4
```

- Go to bucket page and turn on `Show versions`. Notice the structure.

- Note that the `Version ID` of the last object is `null`.

- Change the `file1.txt` content as follows and upload.

```text
This is version 5
```

- Go to bucket page and turn on `Show versions`. Notice the structure.

- The `Version ID` will remain as `null` because the versioning is suspended.

- Create a new file named `file2.txt` with the following content and upload

```text
This is version 1
```

- Go to bucket page and notice its `Version ID`.

- Now, repeat the process by changing the content of `file2.txt` as follows and upload it.

```text
This is version 2
```

- Go to bucket page and notice its `Version ID` is still `null` but the content is changed.

- Delete the bucket at the end of this part.

## Part 4: Lifecycle Rules

### Configuring lifecycle management

- Type `S3` in the search bar and go to its console page.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session8-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Choose `Upload` under `Objects`.

- The `HappyFace.jpg` is located under `/pics` folder. Browse and choose `HappyFace.jpg`.

- Under `Properties`, choose `Standard`.

- Choose `Upload` and wait for `Upload succeeded` message to be displayed on top of the screen.

- Go to bucket page. Under `Management`, choose `Create lifecycle rule`.

```text
Lifecycle rule name: test-rule
Choose a rule scope: Apply to all objects in the bucket
(Check) I acknowledge that this rule will apply to all objects in the bucket.
Lifecycle rule actions: Transition current versions of objects between storage classes
(Check) I acknowledge that this lifecycle rule will incur a transition cost per request
Choose storage class transitions: Standard-IA
Days after object creation: 30
Leave the rest as default.
```

- Choose `Create rule`. Notice the rule for transitioning the objects after determined days.

- Delete the bucket at the end of this part.

## Part 5: Cross-Region Replication

### Configuring replication across regions

- Go to `S3` from the AWS Management Console in Region `us-west-1`.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-source-bucket-for-session8-xxxxx
Bucket versioning: Enable
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Go to `S3` from the AWS Management Console in Region `us-west-2`.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-destination-bucket-for-session8-xxxxx
Bucket versioning: Enable
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- In the source bucket, choose `Create replication rule` under `Management` section.

```text
Replication rule name: replication-test-rule
Choose a rule scope: Apply to all objects in the bucket
Destination: Choose a bucket in this account (Browse and choose destination bucket.)
IAM role: Create new role
Leave the rest as default.
```

- Choose `Save`. Then, select `No` and choose `Submit` on the displayed question.

- Upload the `HappyFace.jpg` to source bucket.

- Then, go to destination bucket and check if the object exists. Wait for a few minutes.

- Delete the bucket at the end of this part.

## Part 6: Static Website Hosting on S3

- Go to `S3` from the AWS Management Console.

- On the left-hand of the console, choose `Buckets` and click on `Create bucket`.

```text
Bucket name: test-bucket-for-session8-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. 

- Select the bucket you created and choose `Copy ARN`. Save it on a notepad.

- Choose the bucket you created. Choose `Upload` and choose `Add files`.

- The `index.html` and the `error.html` are located under `/files` folder. Browse and choose them.

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
			    "arn:aws:s3:::test-bucket-for-session8-xxxxx/*"
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
