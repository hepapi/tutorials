# Session-10: CloudWatch

After completing this hands-on, you should know how to do the following:

- Create an SNS topic and subscription to enable email notifications for CloudWatch alarms
- Create CloudWatch Alarms to monitor EC2 instance metrics like CPU utilization and trigger notifications
- Set up Composite Alarms 
- Create a CloudWatch Dashboard to visualize metrics and alarms
- Install and configure the CloudWatch Agent on an EC2 instance to send system logs to CloudWatch


## Part 1 - AWS CloudWatch Alarm Setup and Configuration Guide

- This guide provides step-by-step instructions for creating and configuring an alerting mechanism in AWS CloudWatch using the Simple Notification Service (SNS) for email notifications

### Task 1: Create an SNS Topic

- At the top of the AWS Management Console, in the search bar, search for and choose `SNS (Simple Notification Service)`
- Click on `Next step`

```text
Type                            : Standard
Name                            : Lab-CPUAlert-<YourName>
```

- Click `Create Topic` to finalize


### Task 2: Create a Subscription

- After creating the topic, click on `Create subscription`

```text
Topic ARN                       :  arn:aws:sns:us-east-1:100453955201:Lab-CPUAlert-<YourName>
Protocol                        :  Email
Endpoint                        :  <your email address>
```

- Click `Create subscription`


### Task 3: Confirm the Subscription

- Check your email inbox for a message from AWS Notifications
- Open the email and click on the `Confirm Subscription` link
- Once confirmed, the subscription becomes active

> `Note:` If you do not confirm the subscription, you will not receive any notifications


## Part 2 - Setting Up and Configuring AWS CloudWatch Alarms

- This guide provides step-by-step instructions to create an EC2 instance, simulate high CPU utilization, and configure an AWS CloudWatch alarm to monitor CPU usage. Notifications will be sent via email when the alarm condition is triggered.


### Task 1: Launch an EC2 Instance

- At the top of the AWS Management Console, in the search bar, search for and choose `EC2`
- Click on `Launch Instance`

```text
Name                            : Lab-CPU-Instance-<YourName>
AMI                             : Amazon Linux 2023 AMI
Instance Type                   : t3.micro
Key pair name                   : Lab-Key-<YourName>
Network settings 
    VPC                         : default
    Subnet                      : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name              : default
    Rules                       : TCP --- > 22 ---> Anywhere
                                  All ICMP IPv4  ---> Anywhere
```

- Click `Launch Instance`


### Task 2: Configure a CloudWatch Alarm

- At the top of the AWS Management Console, in the search bar, search for and choose `CloudWatch`
- Go to the `All alarms` section and click `Create alarm`
- Click on `Select metric` ---> `EC2` ---> `Per-Instance Metrics`
- In the `Search bar`, type `CPUUtilization` and press `Enter`
- In the `Search bar`, type `<Your-Instance-ID>` and press `Enter`
- Select the `CPUUtilization` metric
- Click `Select metric`


```text
Metric name                                         : CPUUtilization
InstanceId                                          : Your Instance ID
Statistic                                           : Maximum
Period                                              : 1 minute
Conditions                                          : Static
Whenever CPUUtilization is                          : Greater > threshold
than…                                               : 65
```

- Click `Next`

```text
Alarm state trigger                                 : In alarm
Send a notification to the following SNS topic      : Select an existing SNS topic
Send a notification to…                             : Lab-CPUAlert-<YourName>
```

- Click `Next`

```text
Alarm name                                         : Lab-CPUAlert-<YourName>
Alarm description - optional                       : THE CPU VALUE OF EC2 INSTANCE HAS INCREASED OVER 65%
```

- Click `Next` and `Create alarm`


### Task 3: Connect to the EC2 Instance

- Go to the `EC2 Dashboard` again
- Select Lab-CPU-Instance-<YourName> ---> `Connect` ---> `EC2 Instance Connect` ---> `Connect`
- Run the following command 

```bash
sudo yum install stress -y
stress -c 1
```

### Task 4: Monitor the Alarm and Check Notifications

- Use the `stress` command to increase CPU utilization and trigger the alarm
- When the CPU usage exceeds the threshold, the alarm will switch to the `ALARM` state
- An email notification will be sent via the `SNS Topic`

- Stop the `stress` command:

```bash
CTRL + C
```

- Terminate your Instance


## Part 3 - Create a Composite Alarm

- This document provides an overview and step-by-step guide for setting up `Composite Alarms` using AWS CloudFormation. Composite Alarms allow grouping multiple alarms into a single logical unit, streamlining the monitoring and notification processes for your infrastructure


### Task 1: Upload CloudFormation Template

- Write a file named `composite_alarm_<Yourname>.yaml` and add the following lines into the YAML file:

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: CloudFormation Stack for EC2 Auto Scaling Group with dynamic VPC and Subnet selection

Parameters:
  UserIdentifier:
    Type: String
    Description: A unique identifier for the user (e.g., username or user ID)
    Default: necip # CHANGE YOUR NAME !!!

  LatestAmiId:
    Description: The latest Amazon Linux 2 AMI
    Type: 'AWS::SSM::Parameter::Value<String>'
    Default: '/aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-x86_64-gp2'

  VpcId:
    Type: AWS::EC2::VPC::Id
    Description: Select the VPC where the resources will be created

  SubnetIds:
    Type: List<AWS::EC2::Subnet::Id>
    Description: Select the Subnets for the Auto Scaling Group

Resources:
  MyEC2SecurityGroup:
    Type: 'AWS::EC2::SecurityGroup'
    Properties:
      GroupDescription: !Sub "Security group for ${UserIdentifier}"
      VpcId: !Ref VpcId
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 22
          ToPort: 22
          CidrIp: 0.0.0.0/0

  LaunchTemplate:
    Type: 'AWS::EC2::LaunchTemplate'
    Properties:
      LaunchTemplateName: !Sub "LaunchTemplate-${UserIdentifier}"
      LaunchTemplateData:
        InstanceType: t2.micro
        SecurityGroupIds:
          - !GetAtt MyEC2SecurityGroup.GroupId
        ImageId: !Ref LatestAmiId

  AutoScalingGroup:
    Type: 'AWS::AutoScaling::AutoScalingGroup'
    Properties:
      AutoScalingGroupName: !Sub "MyAutoScalingGroup-${UserIdentifier}"
      LaunchTemplate:
        LaunchTemplateId: !Ref LaunchTemplate
        Version: !GetAtt LaunchTemplate.LatestVersionNumber
      MinSize: 1
      MaxSize: 5
      DesiredCapacity: 1
      VPCZoneIdentifier: !Ref SubnetIds
      MetricsCollection:
        - Granularity: 1Minute

  ScaleUpPolicy:
    Type: 'AWS::AutoScaling::ScalingPolicy'
    Properties:
      AutoScalingGroupName:
        Ref: AutoScalingGroup
      AdjustmentType: ChangeInCapacity
      ScalingAdjustment: '1'
      Cooldown: '300'

  HighCpuAlarm:
    Type: 'AWS::CloudWatch::Alarm'
    Properties:
      AlarmName: !Sub "HighCPUUsage-${UserIdentifier}"
      AlarmDescription: Alarm when CPU exceeds 40 percent
      Namespace: AWS/EC2
      MetricName: CPUUtilization
      Statistic: Average
      Period: '60'
      EvaluationPeriods: '1'
      Threshold: '40'
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: AutoScalingGroupName
          Value:
            Ref: AutoScalingGroup
      AlarmActions:
        - Ref: ScaleUpPolicy

  HighASGSizeAlarm:
    Type: 'AWS::CloudWatch::Alarm'
    Properties:
      AlarmName: !Sub "HighASGSize-${UserIdentifier}"
      AlarmDescription: Alarm when ASG size exceeds 3
      Namespace: AWS/AutoScaling
      MetricName: GroupTotalInstances
      Statistic: Average
      Period: '60'
      EvaluationPeriods: '1'
      Threshold: '3'
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: AutoScalingGroupName
          Value:
            Ref: AutoScalingGroup


  VeryHighCpuAlarm:
    Type: 'AWS::CloudWatch::Alarm'
    Properties:
      AlarmName: !Sub "VeryHighCPUUsage-${UserIdentifier}"
      AlarmDescription: Alarm when CPU exceeds 75 percent
      Namespace: AWS/EC2
      MetricName: CPUUtilization
      Statistic: Average
      Period: '60'
      EvaluationPeriods: '1'
      Threshold: '75'
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: AutoScalingGroupName
          Value:
            Ref: AutoScalingGroup
      AlarmActions:
        - Ref: ScaleUpPolicy

Outputs:
  AutoScalingGroupName:
    Description: The name of the Auto Scaling Group
    Value: !Ref AutoScalingGroup
  LaunchTemplateName:
    Description: The name of the Launch Template
    Value: !Ref LaunchTemplate
  SecurityGroupId:
    Description: The ID of the Security Group
    Value: !GetAtt MyEC2SecurityGroup.GroupId
```

- At the top of the AWS Management Console, in the search bar, search for and choose `CloudFormation`
- Click `Create Stack`

```text
Prerequisite - Prepare template
    Prepare template                        : Choose an existing template
Specify template
    Template source                         : Upload a template file
```
- Click `Choose file`
- Upload the provided CloudFormation YAML template (`composite_alarm_<Yourname>.yaml`)
- Click `Next`


```text
Stack name                                  : CompositeAlarm-<YourName>
Parameters
    LatestAmiId                             : /aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-x86_64-gp2
    SubnetIds                               : Choose all the subnets of the default VPC (Type 172.)
    UserIdentifier                          : YourName
    VpcId                                   : DEFAULT
```

- Click `Next`

```text
Tags - optional
    Key: Name
    Value: YourName
```

- Click `Next`
- Click `Submit`

- Wait approximately 2-3 minutes for the stack status to reach `CREATE_COMPLETE`


### Task 2: Configure Composite Alarm

- Open `EC2` and `Auto Scaling Group` in a New Tab

- At the top of the AWS Management Console, in the search bar, search for and choose `CloudWatch`
- Click on `All Alarms` on the left side
- Select the three alarms:
   - `HighCPUUsage`
   - `HighASGSize`
   - `VeryHighCPUUsage`
- Click `Create composite alarm`


```text
ALARM("HighCPUUsage-necip") AND ALARM("HighASGSize-necip") OR ALARM("VeryHighCPUUsage-necip")
```

- Click `Next`

```text
Alarm state trigger                             : In Alarm
Send a notification to the following SNS topic  : Select an existing SNS topic
Send a notification to...                       : Lab-CPUAlert-<Your-Name>            
```

- Click `Next`

```text
Alarm name                                      : CompositeAlarm-<Your-Name>
```

- Click `Next`
- Click `Create composite alarm`

- Refresh the page and confirm that the composite alarm has been created


### Task 3: Testing the Composite Alarm

- Go to `Auto Scaling Group`
- Click on the `MyAutoScalingGroup-<YourName>` that we created
- In the `MyAutoScalingGroup-<YourName>` details page, locate the `MyAutoScalingGroup-<YourName> Capacity overview` section on the top-right corner click on the `Edit` button within this section

```text
Desired capacity                : 4
Min desired capacity            : 4
Max desired capacity            : 5
```

- Click the `Update` button to save your changes
- Navigate back to the following pages to observe any changes:
   - `Instances`
   - `Auto Scaling Groups`


- Check your email to confirm that no alarm notification has been received.
---

- Go to the `EC2 Instances` section in the AWS Management Console
- Connect to an instance
- Select the instance you want to connect to and click on `Connect` ---> `EC2 Instance Connect` ---> `Connect`

- Run a resource-intensive process

```bash
yes > /dev/null &
```

- Check your email again; the required condition has been met due to the increased CPU value, and a notification has been received

---

### Task 4: Clean up

- First select and delete `Composite Alarm`
- Navigate to the `CloudFormation` service in the AWS Management Console
- Select the stack `cloudwatch-<Your-Name>`
-  Click `Delete` and `Delete`


- Check if the stack was not deleted. If it remains, review the `Resources` section to identify which resources were not deleted.
- Go to `CloudWatch` and manually first delete the composite alarm, and then proceed to delete the other alarms
- After deleting the alarms, try deleting the stack again
- Verify that all resources have been successfully deleted



## Part 4  - Create a Cloudwatch Dashboard

- This document provides an overview and step-by-step guide for setting up a `CloudWatch Dashboard`. A CloudWatch Dashboard allows you to visualize and monitor metrics and logs from multiple AWS services in a single, customizable interface, making it easier to manage and analyze the health and performance of your infrastructure.


### Task 1: Upload CloudFormation Template

- Write a file named `cloudwatch_dashboard_cloudformation_<Yourname>.yaml` and add the following lines into the YAML file:

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Parameters:
  LatestAmiId:
    Type: 'AWS::SSM::Parameter::Value<AWS::EC2::Image::Id>'
    Default: '/aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-x86_64-gp2'
Resources:
  MyEC2Instance:
    Type: AWS::EC2::Instance
    Properties:
      ImageId: !Ref LatestAmiId
      InstanceType: t2.micro
      IamInstanceProfile: !Ref InstanceProfile

  InstanceProfile:
    Type: AWS::IAM::InstanceProfile
    Properties:
      Roles:
        - Ref: EC2DynamoDBRole

  EC2DynamoDBRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ec2.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: EC2DynamoDBFullAccess
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - ec2:*
                  - dynamodb:*
                  - ssm:*
                Resource: '*'

Outputs:
  EC2InstanceID:
    Description: 'ID of the newly created EC2 instance'
    Value: !Ref MyEC2Instance
```

- At the top of the AWS Management Console, in the search bar, search for and choose `CloudFormation`
- Click `Create Stack`

```text
Prerequisite - Prepare template
    Prepare template                        : Choose an existing template
Specify template
    Template source                         : Upload a template file
```
- Click `Choose file`
- Upload the provided CloudFormation YAML template (`cloudwatch_dashboard_cloudformation_<Yourname>.yaml`)
- Click `Next`


```text
Stack name                                  : CloudwatchDashboard-<YourName>
Parameters
    LatestAmiId                             : /aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-x86_64-gp2
```

- Click `Next`

```text
Tags - optional
    Key: Name
    Value: <YourName>
```

- Click `Next`
- Click `Submit`

- Wait approximately 2-3 minutes for the stack status to reach `CREATE_COMPLETE`

- Navigate to the `EC2`service
- Verify that the `Instance State` is set to `Running`
- Go to the `Security` section of the EC2 instance and view the assigned `IAM Role`
- Select the EC2 instance, click on Connect, and connect using `EC2 Instance Connect`

### Task 2: Install the Application


```bash
sudo yum update -y
sudo su
cd
```


```bash
mkdir application_01
cd application_01
vim requirements.txt
```

```txt
boto3
```

```bash
pip3 install -r requirements.txt
vim app.py
```

```yaml
import boto3
import random
import time
import decimal
from botocore.exceptions import ClientError

dynamodb = boto3.resource('dynamodb', region_name='eu-west-1') # Region is important !
table_name = 'ShoppingData-necip' # Change Your Name

# Create DynamoDB table if it does not exist
def create_table():
    try:
        table = dynamodb.create_table(
            TableName=table_name,
            KeySchema=[
                {
                    'AttributeName': 'order_id',
                    'KeyType': 'HASH'
                },
            ],
            AttributeDefinitions=[
                {
                    'AttributeName': 'order_id',
                    'AttributeType': 'S'
                },
            ],
            ProvisionedThroughput={
                'ReadCapacityUnits': 5,
                'WriteCapacityUnits': 5
            }
        )
        print(f"Table {table_name} created successfully.")
        table.wait_until_exists()
    except ClientError as e:
        if e.response['Error']['Code'] == 'ResourceInUseException':
            print(f"Table {table_name} already exists.")
        else:
            print("Unexpected error: %s" % e)

def put_random_shopping_data():
    table = dynamodb.Table(table_name)
    counter = 0
    while True:
        order_id = f"order_{random.randint(1, 10000)}"
        item_name = f"item_{random.randint(1, 100)}"
        quantity = random.randint(1, 20)
        price = decimal.Decimal(str(round(random.uniform(1, 1000), 2)))
        response = table.put_item(
           Item={
                'order_id': order_id,
                'item_name': item_name,
                'quantity': quantity,
                'price': price
            }
        )
        print(f"PutItem succeeded: {order_id}, {item_name}, {quantity}, {price}")
        counter += 1
        if counter % 100 == 0:
            print("Sleeping for 1 minute...")
            time.sleep(60)

if __name__ == '__main__':
    create_table()
    put_random_shopping_data()
```

```bash
python3 app.py
```

### Task 3: Create a CloudWatch Dashboard


- At the top of the AWS Management Console, in the search bar, search for and choose `CloudWatch`
- Click `Dashboards` and `create dashboard`

```text
Dashboard Name:     Dashboard-<YourName>
```

- Click `Create dashboard` again

- Select a widget type to configure as `Line` and click `Next`
- Return to the EC2 console and copy the `Instance ID` of your EC2
- Go back to the CloudWatch console, paste the `Instance ID` into the search bar, and press Enter
- Select `EC2 --> Per-instance Metrics`
- Type `CPU` into the search bar and press Enter
- Select all CPU metrics
- Click `Create widget`

- Hover over the table, click the pencil icon, and rename the table

```text
Rename Widget:     All CPU Metrics
```
- click `Apply`

- Click the `+` symbol in the top right
- Select a widget type to configure as `Number` and click `Next`
- Go to the DynamoDB console and copy the table name
- Go back to the CloudWatch console, paste the `table name` (ShoppingData-necip) into the search bar, and press Enter
- Select `DynamoDB --> Table Operation Metrics`
- Select all metrics
- Click `Create widget`
- Adjust the table sizes according to your preference.

- Click the `+` symbol in the top right
- Select a widget type to configure as `Gauge` and click `Next`
- Go to the DynamoDB console and copy the table name
- Go back to the CloudWatch console, paste the `table name` (ShoppingData-necip) into the search bar, and press Enter
- Select `DynamoDB --> Table Metrics`
- Select `ConsumedWriteCapacityUnits`
- Click `Options`

```text
Gauge range
Min:    0
Max:    10
```
- Click `Create widget`

- Enable `Autosave: On` in the top right; otherwise, all your dashboard data will be lost


### Task 4: Create an Alarm

- Select `All alarms` on left hand pane
- Click `Create Alarm` and `Select metric`
- Select EC2 ---> Per-Instance Metrics 
- Return to the EC2 console and copy the `Instance ID` of your EC2
- Go back to the CloudWatch console, paste the `Instance ID` into the search bar, and press Enter
- Type `CPUUtilization` into the search bar, and press Enter
- Select the instance and click `Select metric`


```text
Metric name                                         : CPUUtilization<YourName>
InstanceId                                          : Your Instance ID
Statistic                                           : Maximum
Period                                              : 1 minute
Conditions                                          : Static
Whenever CPUUtilization is                          : Greater > threshold
than…                                               : 45
```

- Click `Next`

```text
Notification
  Alarm state trigger                                : In alarm
  Send a notification to the following SNS topic     : Select an existing SNS topic
  Send a notification to…                            : Lab-CPUAlert-<YourName>

EC2 action
  Select `Stop this instance`
```

- Click `Next`

```text
Alarm name                                         : Lab-CPUAlert-<YourName>
Alarm description - optional                       : THE CPU VALUE OF EC2 INSTANCE HAS INCREASED OVER 45%
```

- Click `Next` and `Create alarm`


### Task 5: Run the Stress Command on the EC2 Instance

- Go to the `EC2 Instances` section in the AWS Management Console
- Connect to an instance
- Select the instance you want to connect to and click on `Connect` ---> `EC2 Instance Connect` ---> `Connect`

- Run a resource-intensive process

```bash
yes > /dev/null &
```

- Go back to the `CloudWatch` console and add the alarm we created to the dashboard
- Select the dashboard you created
- Click the `+` symbol in the top right
- Select `Alarms` and click `Next` 
- Select the alarm you created and click `Create widget`

```text
Widget Name     : Alarm-<YorName>
```

- Click `Add to dashboard`

- Monitor the `CPUUtilization` metric and check if the EC2 instance stops when the CPU value exceeds the threshold you set
- Check if an email has been received regarding this situation


### Task 6: Clean up

1. Go to CloudFormation and delete the stack.
2. Go to CloudWatch and delete the alarm you just created
3. Go to CloudWatch and delete the dashboard
4. Go to DynamoDB and delete the table



## Part 5  - Create a Metric Filter

- These steps are used to create a `Metric Filter` in AWS CloudWatch. Metric Filters analyze log data to create metrics based on specific patterns


### Task 1: Create a Role for EC2

- This task involves creating an IAM role for EC2 to allow it to send logs to AWS CloudWatch

- Go to the `IAM` Console

```text
Trusted entity type         : AWS service
Use case                    : EC2
```

- Click `Next`

```text
Permissions policies
  Policy name               : CloudWatchFullAccess, CloudWatchLogsFullAccess
```

- Click `Next`


```text
Role name                   : Metrics-Filter-Role-<YourName>
```

- Click `Create role`


### Task 2: Launch an EC2 Instance

- Go to the `EC2` Console
- Click on `Launch Instance`

```text
Name                            : Metrics-Filter-Instance-<YourName>
AMI                             : Amazon Linux 2023 AMI
Instance Type                   : t2.micro
Key pair name                   : Lab-Key-<YourName>
Network settings 
    VPC                         : default
    Subnet                      : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name              : default
    Rules                       : TCP --- > 22 ---> Anywhere
                                  All ICMP IPv4  ---> Anywhere

Advanced details 
  IAM instance profile          : Metrics-Filter-Role-<YourName>
```

- Click `Launch Instance`

- Select Metrics-Filter-Instance-<YourName> ---> `Connect` ---> `EC2 Instance Connect` ---> `Connect`

```bash
sudo su
cd
vim generate_all.sh
```

```bash
# Number of messages to generate
num_messages=500

# Get the current timestamp in milliseconds
current_timestamp=$(date +%s%3N)

# Open the JSON array
echo "[" > events_all.json

# Array of possible HTTP response statuses
statuses=(200 400 404)

# Generate log messages
for i in $(seq 1 $num_messages); do
    # Format the timestamp and message
    timestamp=$((current_timestamp + i * 1))
    formatted_date=$(date -d @$((timestamp / 1000)) '+%d/%b/%Y:%H:%M:%S %z')

    # Randomly select a HTTP response status
    status=${statuses[RANDOM % ${#statuses[@]}]}

    # Select a file based on the status
    file="/apache_pb${status}.gif"

    message="127.0.0.1 - bob [${formatted_date}] \"GET $file HTTP/1.0\" $status 2326"
    escaped_message=$(echo "$message" | sed 's/"/\\"/g')

    # Create a JSON object for the log event
    json="{\"timestamp\": $timestamp, \"message\": \"$escaped_message\"}"

    # If this is not the first message, append a comma before the JSON object
    if [[ $i -gt 1 ]]; then
        echo "," >> events_all.json
    fi

    # Append the JSON object to the file
    echo -n "$json" >> events_all.json
done

# Close the JSON array
echo -e "\n]" >> events_all.json

```

- This script generates fake Apache logs containing random timestamps and HTTP status codes in JSON format


```bash
bash generate_all.sh
```

- Check the generated log file

```bash
ls 
tail -10 events_all.json
```

### Task 2: Create CloudWatch Log Group

- When running the following command

```bash
aws logs put-log-events --log-group-name MyApplicationLogs-necip --log-stream-name MyLogStream-necip --log-events file://events_all.json
```
-  [📄 AWS CLI Command Reference ](https://docs.aws.amazon.com/cli/latest/reference/logs/put-log-events.html)

- You may encounter this error

```bash
An error occurred (ResourceNotFoundException) when calling the PutLogEvents operation: The specified log group does not exist
```

- Before sending logs to CloudWatch, you need to create a `Log Group` and a `Log Stream`

- Go to the `CloudWatch` Console
- In the left sidebar, select `Log groups`
- Click on `Create log group`

```text
Log group name                   : MyApplicationLogs-necip
```

- Click `Create`


### Task 3: Create CloudWatch Log Stream

- Click `Log Groups`
- Click on `MyApplicationLogs-necip`
- Click on `Create log stream`

```text
Log stream name                   : MyLogStream-necip
```

- Click `Create`


- Go back to the EC2 terminal
- Now you can proceed to send logs to CloudWatch using the following command


```bash
aws logs put-log-events --log-group-name MyApplicationLogs-necip --log-stream-name MyLogStream-necip --log-events file://events_all.json
```

- Go to the log stream and verify that the logs have arrived



### Task 4: Create Metric Filter

- Go to the `CloudWatch` Console
- In the left sidebar, select `Log groups`
- Click on `MyApplicationLogs-<YourName>`
- Click `Actions` --> `Metric filters` ---> `Create metric filter` 

```text
Create filter pattern
  Filter pattern              : "HTTP/1.0\" 404"
Test pattern
  Select log data to test     : MyLogStream-<YourName>
```

- Click `Next`

```text
Create filter name
  Filter name               : HTTP404Filter-<YourName>

Metric details
  Metric namespace          : MyApplicationMetrics-<YourName>
  Metric name               : HTTP404ErrorCount-<YourName>
  Metric value              : 1
```

- Click `Next`
- Click `Create metric filter`


### Task 5: Create an Alarm for HTTP 404 Errors

- In the CloudWatch console, select `All Alarms` --->  `Create Alarm`
- Click `Select metric` ---> `MyApplicationMetrics-<YourName>` ---> `Metrics with no dimensions 1`
- Choose `HTTP404ErrorCount-<YourName>` and click `Select metric`


```text
Metric name                                         : HTTP404ErrorCount-<YourName>
Statistic                                           : Sum 
Period                                              : 5 minute
Conditions                                          : Static
Whenever HTTP404ErrorCount-<YourName> is            : Greater > threshold
than…                                               : 500
```

**Not**: - Don't forget Change the statistic from `Average` to `Sum`

- Click `Next`

```text
Notification
  Alarm state trigger                                : In alarm
  Send a notification to the following SNS topic     : Select an existing SNS topic
  Send a notification to…                            : Lab-CPUAlert-<YourName>

```

- Click `Next`

```text
Alarm name                                         : MetricAlarm-<YourName>
```

- Click `Next` and `Create alarm`


- Go back to the EC2 terminal again
- Continue running the following commands:

```bash
bash generate_all.sh
aws logs put-log-events --log-group-name MyApplicationLogs-<YourName> --log-stream-name MyLogStream-<YourName> --log-events file://events_all.json
```

- Go to your e-mail and confirm that the alarm notification has arrived after the threshold was exceeded


### Task 6: Clean up

1. Go to CloudWatch and delete the log group
2. Go to CloudWatch and delete the log stream 
3. Go to CloudWatch and delete the CloudWatch Alarm
4. Go to EC2 and terminate the EC2 Instance

- Don't delete IAM Role



## Part 6  - Install and Configure CloudWatch Agent

- These steps explains how to set up the CloudWatch Agent on an EC2 instance and send logs from this instance to CloudWatch Log Groups

### Task 1: Add Required Permissions to the IAM Role

- Go to the `IAM` service in the `AWS Console`
- Click on the `Roles` section and find the role you created (Metrics-Filter-Role-<YourName>)
- Use the `Add permisson` and  `Attach policies`  and option to add the following policy:

```text
CloudWatchAgentServerPolicy
```
- Click `Add permissons`

- This policy grants the CloudWatch Agent the necessary permissions to send logs to CloudWatch


### Task 2: Launch an EC2 Instance

- Go to the `EC2` Console
- Click on `Launch Instance`

```text
Name                            : Agent-Instance-<YourName>
AMI                             : Amazon Linux 2023 AMI
Instance Type                   : t2.micro
Key pair name                   : Lab-Key-<YourName>
Network settings 
    VPC                         : default
    Subnet                      : No prefence
Firewall (security groups)
Security Group    
    Sec.Group Name              : default
    Rules                       : TCP --- > 22 ---> Anywhere
                                  All ICMP IPv4  ---> Anywhere
                                  TCP --- > 80 ---> Anywhere
                                  All ICMP IPv4  ---> Anywhere

Advanced details 
  IAM instance profile         : Metrics-Filter-Role-<YourName>

  User data - optional 

#!/bin/bash
yum update -y

yum install nginx -y

systemctl start nginx
systemctl enable nginx

echo '<h1>Welcome to Nginx on Amazon Linux 2!</h1>' > /usr/share/nginx/html/index.html

```

- Select the EC2 instance, click on Connect, and connect using `EC2 Instance Connect`


### Task 3: Download and Install CloudWatch Agent


```bash
sudo yum install -y amazon-cloudwatch-agent
```

```bash
sudo su
vi /opt/aws/amazon-cloudwatch-agent/bin/cloudwatch-agent-config.json
```

```json
{
  "metrics": {
    "append_dimensions": {
      "InstanceId": "${aws:InstanceId}"
    },
    "metrics_collected": {
      "mem": {
        "measurement": [
          "mem_used_percent",
          "mem_available",
          "mem_total",
          "mem_used"
        ],
        "metrics_collection_interval": 60
      }
    }
  },
  "logs": {
    "logs_collected": {
      "files": {
        "collect_list": [
          {
            "file_path": "/var/log/nginx/access.log",
            "log_group_name": "nginx-access-logs-necip",
            "log_stream_name": "{instance_id}",
            "timezone": "UTC"
          }
        ]
      }
    }
  }
}

```

### Task 4: Create a CloudWatch Log Group

- In the `AWS Console`, go to `CloudWatch ---> Logs ---> Log Groups`
- Click on `Create Log Group`
- Enter the `Log Group Name` as specified in your configuration (`nginx-access-logs-necip`)


- Go to EC2 Terminal again

```bash
sudo /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
    -a fetch-config \
    -m ec2 \
    -c file:/opt/aws/amazon-cloudwatch-agent/bin/cloudwatch-agent-config.json \
    -s
```

- This command configures and starts the Amazon CloudWatch Agent on an EC2 instance using a specified configuration file

```bash
sudo /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
    -m ec2  \
    -a status 
```

```bash
sudo systemctl status amazon-cloudwatch-agent
```

- This commands checks and displays the current status of the Amazon CloudWatch Agent running on an EC2 instance.


### Task 5: Viewing Logs in CloudWatch

- Go to the `CloudWatch Console`.
- Under `Logs --> Log Groups`, find the `login-monitoring-<YourName>` log group.
- Verify that a `Log Stream` with the EC2 instance ID has been created.


### Task 7: View RAM Data in the CloudWatch Console

- Go to the AWS Console --> CloudWatch --> Metrics --> All metrics --> CWAgent namespace  --> InstanceId
- You will find metrics such as `mem_used_percent`, `mem_used`, and `mem_available`  
- You can add these metrics to a graph or create alarms


### Task 7: Clean up

1. Go to CloudWatch and delete the log Group 
2. Go to EC2 and terminate the EC2 Instance
3. Go to IAM end delete your role
4. Go to SNS and delete your topic