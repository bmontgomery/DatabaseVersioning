Public Interface IDatabaseProvider

  Sub InitDatabaseConnection(ByVal connStr As String)
  Sub CloseDatabaseConnection()

  Sub BeginTransaction()
  Sub RollBackTransaction()
  Sub CommitTransaction()

  Sub CreateDatabase()

  Sub EnsureVersionHistoryTableExists()

  Function GetDatabaseVersion() As Version
  Sub DropItems()
  Sub RunScript(ByVal scriptText As String)
  Sub UpdateVersion(ByVal scriptName As String, ByVal version As Version, ByVal dateApplied As DateTime)

End Interface
