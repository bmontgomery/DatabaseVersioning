Synopsis
cinch -connStr <connectionstring> [-help] [-drop] [-scripts <path>] 
      [-patches <path>] [-other <path> [<path> <path> ...]] [-l <loglevel>]

Description
This tool will safely update a database to the latest version, by running any
necessary scripts from the scripts directory. This tool will create a 
VersionHistory table in the database to keep track of what the current version
of the database is. It uses this table to determine which scripts from the
scripts directory need to be run. Scripts in the scripts directory should be
named using this scheme: "<version> <description>.sql". For example, a script
might be named "2.2.3.1 - add a column to the users table.sql". This tool will
find the version number by reading until it encounters the first space in the
file name, so you must have a space in the name between the version number and
the description.

Each script file (specified by the -scripts, -patches, or -other parameter) 
must have the .sql extension in order for this tool to run it.

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
                                    Defaults to "e" (Errors Only)
                                    
-other <path> [<path> <path> ...]   Defines one or more script directories
                                    which contain unversioned scripts to run
                                    during the upgrade.
                                    
-patches <path>                     Defines the directory containing the
                                    versioned patch files. The tool will ensure
                                    each script in this directory has been run,
                                    regardless of the version of the database.
                                    
-help                               Display this help information.