DatabaseVersioning
===
written by: Brandon Montgomery

Summary
===
This project was started out of a need for automated database versioning and deployment. The goal is to provide a command-line interface for providing developers with the ability to keep databases up to date in an automated way.

Strategy
===
The update scripts are created by developers. Anything that changes a database in any way must be scripted to a file in order for this tool to work effectively.The tool uses a specific database versioning strategy, and it needs to make some assumptions about the files it processes.

Scripts should be sorted into separate directories depending on the type of script. This tool then treats each directory a little bit differently depending on the type of scripts that directory contains. The caller of the tool controls how the tool treats each directory by passing the path to each directory in different arguments.

Types of Scripts
---
First, there are different types of scripts that can be run against a database, and they fall into two categories: 1) change scripts; and 2) definition scripts. For example, a "definition script" would be a script which drops and re-adds a stored procedure, view, or function. There's no data involved, so you can just drop and re-add the item. On the other hand, a "change script" moves a database schema from one state to another state. A change script would handle adding/removing columns, tables, relationships, and manipulating data to support the new schema.

Here's the important difference, which you may have already guessed. Change scripts only need to run once, while definition scripts can run and re-run all day and not break the database.

Definition Scripts
---
Definition scripts can live in one or many directories. For instance, you could have a Stored Procedures directory and a Functions directory. Both of these directory paths can be passed to the tool in the "other" argument, and the tool will run each and every script in that directory. It will only run these scripts after it's run the change scripts, however.

Change Scripts
---
There are two directories which can contain change scripts. The first directory is the main directory. Each change script is named with a version number, and then a title. The tool parses the version number from the front of the file name and uses that to order scripts and determine whether or not each one needs to be run against the database. If a change script has not been run and is versioned at a higher number than the database is currently, the tool will runt the script. The second directory that can contain change scripts is a patches directory. The tool treats the scripts this directory a bit differently; if the script has not been run on the database, it will run the script, even if the database is at a higher version that the script. This allows bug fixes to be rolled out into multiple code branches with minimal effort on the developer's part.

Database Versioning
---
Every time the tool runs a script, it creates a record in a "VersionHistory" table in the database. This keeps track of every script which has been run, and this is the table the tool uses to determine the version of the database.

Project Structure and Architecture
===
The core of the update logic is contained in the DatabaseVersioning project. The DbVersionConsole project, which produces cinch.exe, the command line tool, simply provides a command-line interface to use the DatabaseVersioning logic. The code is organized in such a way as to provide the ability to use dependency injection to potentially version any database. A Microsoft SQL Server implementation has been written.
