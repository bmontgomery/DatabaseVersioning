Public Class DbVersionManager

  Private mConnectionString As String
  Public Property ConnectionString() As String
    Get
      Return mConnectionString
    End Get
    Set(ByVal value As String)
      mConnectionString = value
    End Set
  End Property

  Private mDrop As Boolean
  Public Property Drop() As Boolean
    Get
      Return mDrop
    End Get
    Set(ByVal value As Boolean)
      mDrop = value
    End Set
  End Property

  Private mScriptsDirectory As String
  Public Property ScriptsDirectory() As String
    Get
      Return mScriptsDirectory
    End Get
    Set(ByVal value As String)
      mScriptsDirectory = value
    End Set
  End Property

  Private mOtherDirectories As String()
  Public Property OtherDirectories() As String()
    Get
      Return mOtherDirectories
    End Get
    Set(ByVal value As String())
      mOtherDirectories = value
    End Set
  End Property

  Private mDatabaseProvider As IDatabaseProvider
  Public Property DatabaseProvider() As IDatabaseProvider
    Get
      Return mDatabaseProvider
    End Get
    Set(ByVal value As IDatabaseProvider)
      mDatabaseProvider = value
    End Set
  End Property

  Public Sub New(ByVal connectionString As String, ByVal drop As Boolean, ByVal scriptsDir As String, ByVal ParamArray otherDirs As String())

    mConnectionString = connectionString
    mDrop = drop
    mScriptsDirectory = scriptsDir
    mOtherDirectories = otherDirs

  End Sub

  Public Sub Go()

    DatabaseProvider.BeginTransaction()

    Try

      DatabaseProvider.EnsureVersionHistoryTableExists()

      If mDrop Then DatabaseProvider.DropItems()

      DatabaseProvider.CommitTransaction()

    Catch ex As Exception

      DatabaseProvider.RollBackTransaction()

    End Try
    
  End Sub

End Class
