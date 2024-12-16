# Session-7: RDS (Relational Database Service)

The purpose of this session is to explore the setup and management of Amazon RDS, including 
creating and managing relational databases, as well as configuring backups and monitoring for 
optimal performance and reliability.

- Part 1: RDS Database Setup
   - Creating and managing relational databases
   - Configuring backups and monitoring

NOTE: If you do this session using an IAM-User under an organization account, you should attach
`-<Your-Name>` at the end of the each resource name. This would avoid confusion between IAM-Users
because the resources would be created under one account, namely `Management Account`.

Example: If the resource name is `aws-training-billing-alarm`, which is a CloudWatch alarm name; 
make it `aws-training-billing-alarm-john` so that you now that you created this resource.

## Part 1: RDS Database Setup

### Creating and managing relational databases

- Go to `RDS` from AWS Management Console. From the menu on the left, choose `Databases`.

- Choose `Create Databases`.

Choose a database creation method

```text
Choose a database creation method: Standard Create
Engine options: PostgreSQL
Templates: Free Tier
DB instance identifier: my-first-db
Master username: postgres
Master password: password
Confirm master password: password
DB instance class: Burstable classes (includes t classes) - db.t4g.micro
Storage type: General Purpose SSD (gp2)
Allocated storage: 20 GiB
Virtual Private Cloud (VPC): Default VPC
DB subnet group: default
Public access: Yes
VPC security group (firewall): Create New
New VPC security group name: my-db-sg-training
Additional configuration -> Enable automated backups: Unchecked
Leave the rest as default.
```

- Choose `Create database` and wait for the launch of the database.

- Make sure that the `Status` of the database is `Available`.

- After your database is created, select it.

- Observe the `Connectivity & security` section. Copy the `Endpoint` to text editor.

- Go to [pgAdmin](https://www.pgadmin.org) and download it.

- After successful installation, open the pgAdmin.

- Under the `Quick Links`, choose `Add New Server`.

- Under `General` section, configure the following:

```text
Name: aws-rds
Leave the rest as default.
```

- Under `Connection` section, configure the following:

```text
Host name/address: <Your-RDS-Endpoint>
Password: password
Leave the rest as default.
```

- Replace the `<Your-RDS-Endpoint>` with your previously copied database `Endpoint`.

- Choose `Save`. 

- On the top-left corner, choose the database server under `Servers`.

- Notice that the `postgres` database is listed under `Databases`.

- Right-click on the `Databases` and choose `Create -> Database`.

```text
Database: my_app
Leave the rest as default.
```

- Choose `Save` and notice that it is listed under `Databases`. 

### Configuring backups and monitoring

- Go to `RDS` from AWS Management Console. From the menu on the left, choose `Databases`.

- Go to the database you created.

- Choose the `Modify` option on the upper-right corner.

```text
Additional configuration -> Backup retention period: 7
Leave the rest as default.
```

- Choose `Continue` ... 

```text
Schedule modifications: Apply immediately
```

- Choose `Modify DB Instance`.

- From the menu on the left, choose `Automated Backups`. Notice the backup of your database.

- From the menu on the left, choose `Databases`. Go to the database you created.

- Under the `Maintenance & backups` section, you can see the backup configuration.

- Also, notice the snapshot of the database under `Maintenance & backups -> Snapshots`.

- Under the `Monitoring` section, observe the graphs related to different metrics.

- Choose `Monitoring -> Performance Insights`. Go to opened tab.

- Choose `Metrics` section and observe the `Metrics Dashboard`.

- Close this tab and go to previous tab.

- From the menu on the left, choose `Databases`. Choose the database you created.

- Choose `Actions -> Delete`.

```text
(Uncheck) Create final snapshot
(Uncheck) Retain automated backups
(Check) I acknowledge that ...
To confirm deletion, type delete me into the field.
```

- Choose `Delete` and wait for the deletion of the database.

- From the menu on the left, check `Snapshots` and `Automated backups` whether the resources are deleted.

- Don't forget to destroy the resources you created.
