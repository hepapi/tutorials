# Session-4: IAM (Identity and Access Management)

The purpose of this session is to introduce the basics of AWS Identity and Access Management (IAM), focusing 
on managing users, groups, roles, and policies, as well as exploring IAM Identity Center for centralized access 
control and CloudTrail for monitoring account activity.

- Part 1: Users, Groups, Roles, and Policies
- Part 2: IAM Identity Center
   - Managing centralized identity and access
- Part 3: CloudTrail
   - Logging and monitoring AWS account activity

NOTE: If you do this session using an IAM-User under an organization account, you should attach
`-<Your-Name>` at the end of the each resource name. This would avoid confusion between IAM-Users
because the resources would be created under one account, namely `Management Account`.

Example: If the resource name is `aws-training-billing-alarm`, which is a CloudWatch alarm name; 
make it `aws-training-billing-alarm-john` so that you now that you created this resource.

## Part 1: Users, Groups, Roles, and Policies

- Go to `IAM` service on the AWS Management Console. 

- Choose `Users` from the menu on the left. Then, choose `Create User`.

```text
User name: training-user
(Check) Provide user access to the AWS Management Console
(Check) I want to create an IAM user.
(Check) Custom password: TrainingPassword123
(Uncheck) Users must create a new password at next sign-in
```

- Click `Next` and continue ...

- Click `Next` and continue ...

- Click `Create user`. Then, click `Return to users list` and notice the user you created.

- Note the `Account ID` by clicking on the account name on the top-right corner of the screen.

- Click `Sign out` from current session and login again as previously created user.

- After successful login, go to `S3` service and choose `Buckets` from the menu on the left.

- Notice that you get error while listing the buckets because you don't have permissions.

- Go back to homepage of the `S3` service and click on `Create Bucket`.

```text
Bucket name: aws-training-test-bucket-XXXXX
Leave the rest as default.
```

- Change `XXXXX` so that the bucket name will be unique. Then click on `Create Bucket`.

- Notice the error for failing to create a bucket.

- Sign out from this account and sign in with your main account.

- Go to `IAM` service on the AWS Management Console. Choose `Users` from the menu on the left.

- Choose the user you created. On the displayed page, choose `Add Permissions -> Add Permissions`.

```text
(Check) Attach policies directly
Permission policies: AmazonS3FullAccess
```

- You can click on the policy to view its details in a new tab.

- Click `Next` and continue ...

- Click `Add Permissions`.

- Now, sign out from your main account and sign in to your user account.

- Go to `S3` service and choose `Buckets` from the menu on the left.

- Notice that you can see the list of buckets because the account has the permissions.

- Go back to homepage of the `S3` service and click on `Create Bucket`.

```text
Bucket name: aws-training-test-bucket-XXXXX
Leave the rest as default.
```

- Change `XXXXX` so that the bucket name will be unique. Then click on `Create Bucket`.

- Notice that you can create a bucket now because the account has the permissions.

- Sign out from this account and sign in with your main account.

- Go to `IAM` service on the AWS Management Console. Choose `Users` from the menu on the left.

- Choose the user you created. Select the policy you gave under `Permissions policies`.

- Click on `Remove`, then click on `Remove` again. Now, the permission is removed.

- Choose `Users` from the menu on the left. Choose `Create User`.

```text
User name: training-user-2
(Check) Provide user access to the AWS Management Console
(Check) I want to create an IAM user.
(Check) Custom password: TrainingPassword12345
(Uncheck) Users must create a new password at next sign-in
```

- Click `Next` and continue ...

- Click `Next` and continue ...

- Click `Create user`. Then, click `Return to users list` and notice the user you created.

- Choose `User groups` from the menu on the left. Choose `Create group`.

```text
User name: aws-training
Add users to the group: training-user, training-user-2
Attach permissions policies: AmazonS3FullAccess
```

- Click `Create group`. Notice the group you created on the list of user groups.

- Sign out from this account and login as first user you created. 

- After successful login, go to `S3` service and choose `Buckets` from the menu on the left.

- Notice that you can see the list of buckets because the account has the permissions coming from the group.

- Sign out from this account and sign in with your main account.

- Go to `IAM` service on the AWS Management Console. Choose `User groups` from the menu on the left.

- Choose the user group you created. On the displayed page, select `Permissions` section.

- Select the policy and choose `Remove`. The permission is removed from the user group.

- Go to `IAM` service on the AWS Management Console. Choose `Roles` from the menu on the left.

- Click on `Create role`.

```text
Trusted entity type: AWS account
An AWS account: This account
```

- Click `Next` and continue ...

```text
Permissions policies: AmazonS3FullAccess
```

- Click `Next` and continue ...

```text
Role name: AWS-Training-Role
Description: This role is created in the scope of AWS Training.
Leave the rest as default.
```

- Click `Create role`. The role is successfully created. Copy the name of the role on the notepad.

- Go to `IAM` service on the AWS Management Console. Choose `Users` from the menu on the left.

- Choose the first user you created. On the displayed page, choose `Add Permissions -> Create inline policy`.

```text
Select a service: STS
Actions allowed: AssumeRole
Resources: Specific
Add ARNs: Copy and paste the name of the role you created (AWS-Training-Role)
Leave the rest as default.
```

- Click `Next` and continue ...

```text
Policy name: AssumeRoleForTraining
```

- Click `Create Policy`.

- Go to `Roles` from the menu on the left. Choose the role you created.

- Copy and paste the link of `Link to switch roles in console` in a new tab.

```text
Display name: S3-Access-Role-For-Training
Display color: Any color you want.
```

- Go to `S3` service and choose `Buckets` from the menu on the left.

- Notice that you can see the list of buckets because the account has assumed the role.

- Go to `EC2` service and choose `Instances` from the menu on the left.

- Notice that you cannot see the list of instances because the role doesn't include necessary permission.

- You can turn back to your main account.

## Part 2: IAM Identity Center (Single Sign On - SSO)

### Managing centralized identity and access

- Go to `IAM Identity Center` in the `Region` you enabled.

- From the menu on the left, choose `Settings`.

- Under `Identity source` section, copy and paste `AWS access portal URL` in a new tab.

- Observe the page, you don't need to login.

- Under `Identity source` section, notice the `Change identity source`. Don't modify it.

- From the menu on the left, choose `AWS accounts`. See the accounts in your organization.

- From the menu on the left, choose `Users`. Then, choose `Add user`.

```text
Username: training-user1
Password: Send an email to this user with password setup instructions.
Email address: <Your dummy email address>
Confirm email address: <Your dummy email address>
First name: training-user1
Last name: training-user1
Display name: training-user1
Leave the rest as default.
```

- Click `Next` and continue ...

- Click `Add user`.

- In a new tab, go to your email and confirm the invitation.

- You will directed to a page for creating a password. Create the password and confirm.

```text
Password: Password-Training-User1
```

- Go back to `IAM Identity Center` in the `Region` you enabled.

- From the menu on the left, choose `Permission sets`. Choose `Create permission set`.

```text
Type: Predefined permission set
Policy for predefined permission set: AdministratorAccess
```

- Click `Next` and continue ...

```text
Permission set name: AdministratorAccess
Description: Permission Set of Administrator Access for AWS Training.
Session duration: 2 hours
Leave the rest as default.
```

- Click `Next` and continue ...

- Click `Create`.

- From the menu on the left, choose `Permission sets`. Choose `Create permission set`.

```text
Type: Predefined permission set
Policy for predefined permission set: ViewOnlyAccess
```

- Click `Next` and continue ...

```text
Permission set name: ViewOnlyAccess
Description: Permission Set of View Only Access for AWS Training.
Session duration: 2 hours
Leave the rest as default.
```

- Click `Next` and continue ...

- Click `Create`.

- From the menu on the left, choose `AWS accounts`.

- Choose the `Management account`. Then, choose `Assign users or groups`.

- Under `Users`, choose the user you created and click `Next`.

- Choose `AdministratorAccess` and click `Next`.

- Review and click `Submit`.

- From the menu on the left, choose `AWS accounts`.

- Choose a dummy account. Then, choose `Assign users or groups`.

- Under `Users`, choose the user you created and click `Next`.

- Choose `ViewOnlyAccess` and click `Next`.

- Review and click `Submit`.

- From the menu on the left, choose `Settings`.

- Under `Identity source` section, copy and paste `AWS access portal URL` in a new tab.

- Login as SSO user you created. Notice that you have access to two accounts with different permissions.

- Under the `Management account`, notice the `Access keys` which you should keep as secret.

- Click on `AdministratorAccess` and notice that you login as a SSO user to the main account.

- Go to `S3`. From the menu on the left, choose `Buckets`. Choose `Create bucket`.

```text
Bucket name: test-bucket-for-session-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. Notice that you are able to create bucket.

- Login as SSO user you created again. Notice that you have access to two accounts with different permissions.

- Under the dummy account, notice the `Access keys` which you should keep as secret.

- Click on `ViewOnlyAccess` and notice that you login as a SSO user to the dummy account.

- Go to `S3`. From the menu on the left, choose `Buckets`. Choose `Create bucket`.

```text
Bucket name: test-bucket-for-session-xxxxx
Leave the rest as default.
```

- To give a bucket a specific name, change `xxxxx` as required. 

- Then, choose `Create bucket`. Notice that you are not able to create bucket.

## Part 3: CloudTrail

### Logging and monitoring AWS account activity

- Go to `CloudTrail` on the AWS Management Console.

- From the menu on the left, choose `Event history`.

- Observe the `Event history` in the account. Select one of the events to see details.

- You can see the details such as Source IP address, User name, etc.

- From the menu on the left, choose `Dashboard`. Choose `Create trail`.

```text
Trail name: aws-training-demo
(Uncheck) Log file SSE-KMS encryption - Enabled
(Uncheck) Log file validation - Enabled
(Check) CloudWatch Logs - Enabled
Role name: aws-training-ct-cw-role
Leave the rest as default.
```

- Click `Next` and continue ...

- Click `Next` and continue ...

- You can see the trail you created. Choose `S3 bucket` link from the trail details.

- Alternatively, you can also see the bucket in `S3`.

- There are nested folders in the `S3`, choose each folder untill you see the object.

- In the page of log object, choose `Open` in a new tab. Observe the log.

- Note that this log is in the same format as in the `Event record` under a log in `Event history`.

- Go to `CloudWatch`. Choose `Log groups` from the menu on the left.

- Notice the newly created log group in the list and choose it.

- Observe the log group and notice the `Log streams` below. Choose one of the log streams.

- On the displayed page, you can click on the any log to see its details in `JSON` format.

- Don't forget to destroy the resources you created.
