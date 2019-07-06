# Existing database to efcore
A small Windows forms application to convert tables of an existing database to entity framework core classes.
The idea of the project is to be be able to

 - Create c# (or really any code) from an existing table.
 - Be able to plug in new databases and code generation

## Current build status
[![Build Status](https://travis-ci.org/ShiveringSquirrel/existing-database-to-efcore.svg?branch=master)](https://travis-ci.org/ShiveringSquirrel/existing-database-to-efcore)
[![Sonarcloud Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=alert_status)](https://sonarcloud.io/dashboard?id=ShiveringSquirrel_existing-database-to-efcore)
[![Sonarcloud Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=ShiveringSquirrel_existing-database-to-efcore)
[![SonarCloud Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=security_rating)](https://sonarcloud.io/dashboard?id=ShiveringSquirrel_existing-database-to-efcore)
[![Sonarcloud Code Smells](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=code_smells)](https://sonarcloud.io/dashboard?id=ShiveringSquirrel_existing-database-to-efcore)
[![SonarCloud Bugs](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=bugs)](https://sonarcloud.io/component_measures/metric/reliability_rating/list?id=ShiveringSquirrel_existing-database-to-efcore)
[![SonarCloud Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ShiveringSquirrel_existing-database-to-efcore&metric=vulnerabilities)](https://sonarcloud.io/component_measures/metric/security_rating/list?id=ShiveringSquirrel_existing-database-to-efcore)

## Screenshots
A few screenshots of the program.

*Main screen*

[![Main](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/main_small.png "Main")](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/main.png)

*Create/edit configuration*

[![Connection](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/connection.png "Connection")](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/screenshots/connection.png)

## Example configuration file
A sample ini config file used by this program.
~~~~
[Connection]
DisplayName = Test
String = Your connection string
Type = MySQL

[CodeGeneration]
Namespace = MyNamespace22
SealedClasses = True
~~~~

## Works with
![Logo MYSQL](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/docs/logo-mysql.png "MySQL")
![Logo MS SQL Server](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/docs/logo-sqlserver.png "MS SQL Server")
![Logo Oracle](https://raw.github.com/ShiveringSquirrel/existing-database-to-efcore/master/docs/logo-oracle.png "Oracle")
