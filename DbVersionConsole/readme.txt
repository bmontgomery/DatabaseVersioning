Synopsis
cinch -connStr <connectionstring> [-help] [-drop] [-scripts <path>] 
      [-other <path> [<path> <path> ...]] [-l <loglevel>]

Description
This tool will safely update a database to the latest version, by running any
necessary scripts from the scripts directory. This tool will create a 
VersionHistory table in the database to keep track of what the current version
of the database is. It uses this table to determine which scripts from the
scripts directory need to be run. Scripts in the scripts directory should be
named using this scheme: "<version> - <description>.sql". For example, a script
might be named "2.2.3.1 - add a column to the users table.sql".

When this tool is run, it is treated as one transaction. If any of the scripts
this tool runs fails, the entire upgrade will be rolled back.

Options
-connStr <connectionString>         The connection string to use to connect to
                                    the database. Make sure to use a user which
                                    has necessary permissions.
                                    
-drop                               Drop all stored procedures, functions, and
                                    views before running the scripts.
                                    
-scripts <path>                     Defines the directory containing the
                                    versioned script files.
                                    
-l <loglevel>                       Defines the log verbosity level (what
                                    displays in the console as the upgrade 
                                    runs). Choices:
                                      v: Verbose
                                      m: Medium
                                      e: Errors Only
                                      o: Off
                                    
-other <path> [<path> <path> ...]   Defines one to many script directories
                                    which contain unversioned scripts to run
                                    during the upgrade.
                                    
-help                               Display this help information.