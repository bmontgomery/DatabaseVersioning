Public Interface IDatabaseProvider

  Function InitDatabaseConnection(ByVal connStr As String)
  Function CloseDatabaseConnection()

  Function BeginTransaction()
  Function RollBackTransaction()
  Function CommitTransaction()

  Function CreateDatabase()

  Function EnsureVersionHistoryTableExists()

  Function GetDatabaseVersion() As Version
  Function DropItems()
  Function RunScript(ByVal scriptText As String)
  Function UpdateVersion(ByVal scriptName As String, ByVal version As Version, ByVal dateApplied As DateTime)

End Interface
