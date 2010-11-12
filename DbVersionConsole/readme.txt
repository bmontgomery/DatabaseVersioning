Synopsis
cinch -connStr <connectionstring> [-drop] [-scriptsDir <path>] [-l <loglevel>] 
      [-other <path>] [-help]")

Description
This tool will safely update a database to the latest version, by running any
necessary scripts from the scripts directory. This tool will create a 
VersionHistory table in the database to keep track of what the current version
of the database is. It uses this table to determine which scripts from the
scripts directory need to be run.

you wont want to run scripts manually unless you update the Versionhistory table manually also

bug fix scripts

versioned Script naming

what it will warn you about

what it will do if an error occurs

Options