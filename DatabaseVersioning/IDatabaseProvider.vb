Public Interface IDatabaseProvider
  Inherits IDisposable

  ''' <summary>
  ''' Opens a new database connection to use for the upgrade cycle.
  ''' </summary>
  ''' <param name="connStr">The connection string.</param>
  ''' <returns></returns>
  Function OpenDatabaseConnection(ByVal connStr As String)

  ''' <summary>
  ''' Closes the database connection.
  ''' </summary>
  ''' <returns></returns>
  Function CloseDatabaseConnection()

  ''' <summary>
  ''' Begins a transaction to use for the upgrade cycle. The transaction which is created
  ''' during this step should be used for the entire life of the upgrade cycle so that 
  ''' RollBackTransaction() and CommitTransaction() work with the transaction created
  ''' in this step.
  ''' </summary>
  ''' <returns></returns>
  Function BeginTransaction()

  ''' <summary>
  ''' Rolls back the transaction created in the BeginTransaction() method.
  ''' </summary>
  ''' <returns></returns>
  Function RollBackTransaction()

  ''' <summary>
  ''' Commits the transaction created in the BeginTransaction() method.
  ''' </summary>
  ''' <returns></returns>
  Function CommitTransaction()

  ''' <summary>
  ''' Ensures the version history table exists. If the version history table does not
  ''' exist, this method should created the necessary version history table.
  ''' </summary>
  ''' <returns></returns>
  Function EnsureVersionHistoryTableExists()

  ''' <summary>
  ''' Gets the current version of the database (from the version history table).
  ''' </summary>
  ''' <returns></returns>
  Function GetDatabaseVersion() As Version

  ''' <summary>
  ''' Drops all "items". By "items" we mean stored procedures, views, and functions (in that order).
  ''' This method should use the transaction created in the BeginTransaction() method.
  ''' </summary>
  ''' <returns></returns>
  Function DropItems()

  ''' <summary>
  ''' Runs a database script against the database. This method should use the transaction created in the 
  ''' BeginTransaction() method.
  ''' </summary>
  ''' <param name="scriptText">The text of the database script.</param>
  ''' <returns></returns>
  Function RunScript(ByVal scriptText As String)

  ''' <summary>
  ''' Updates the version of the database after a versioned update script is run. This method is only
  ''' called after a script inside the scriptsDir is run. This method should use the transaction created 
  ''' in the BeginTransaction() method.
  ''' </summary>
  ''' <param name="scriptName">The file name of the script (just the file name, not the full path).</param>
  ''' <param name="version">The version the database was upgraded to after the script was run.</param>
  ''' <returns></returns>
  Function UpdateVersion(ByVal scriptName As String, ByVal version As Version)

  ''' <summary>
  ''' This function is called when the manager needs to determine whether a patch script has been run
  ''' on the database or not. After patch scripts are run, the PatchApplied function is called.
  ''' </summary>
  ''' <param name="patchVersion">The version of the patch script.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Function IsPatchApplied(ByVal patchVersion As System.Version) As Boolean

End Interface
