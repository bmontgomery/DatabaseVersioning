Public Interface IDatabaseProvider

  Function OpenDatabaseConnection(ByVal connStr As String)
  Function CloseDatabaseConnection()

  Function BeginTransaction()
  Function RollBackTransaction()
  Function CommitTransaction()

  Function DatabaseExists() As Boolean
  Function CreateDatabase()

  Function EnsureVersionHistoryTableExists()

  Function GetDatabaseVersion() As Version
  Function DropItems()
  Function RunScript(ByVal scriptText As String)
  Function UpdateVersion(ByVal scriptName As String, ByVal version As Version)

End Interface
