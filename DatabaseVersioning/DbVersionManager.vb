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

  Private mErrorMessage As String
  Public Property ErrorMessage() As String
    Get
      Return mErrorMessage
    End Get
    Set(ByVal value As String)
      mErrorMessage = value
    End Set
  End Property

  Public Event MessageLogged(ByVal sender As Object, ByVal e As MessageLoggedEventArgs)

  Public Sub New(ByVal connectionString As String, ByVal drop As Boolean, ByVal scriptsDir As String, ByVal ParamArray otherDirs As String())

    mConnectionString = connectionString
    mDrop = drop
    mScriptsDirectory = scriptsDir
    mOtherDirectories = otherDirs

  End Sub

  Public Sub Go()

    RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Opening database connection", .DateLogged = Now})
    DatabaseProvider.OpenDatabaseConnection(mConnectionString)

    RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Beginning transaction", .DateLogged = Now})
    DatabaseProvider.BeginTransaction()

    Try

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Checking for existence of database", .DateLogged = Now})
      If Not DatabaseProvider.DatabaseExists() Then

        RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Creating database", .DateLogged = Now})
        DatabaseProvider.CreateDatabase()

      End If

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Ensuring existence of version history table", .DateLogged = Now})
      DatabaseProvider.EnsureVersionHistoryTableExists()

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Getting current database version", .DateLogged = Now})
      Dim currentDbVersion As Version = DatabaseProvider.GetDatabaseVersion()

      If mDrop Then

        RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Dropping items", .DateLogged = Now})
        DatabaseProvider.DropItems()

      End If

      Dim latestVersion As Version = currentDbVersion

      'Scripts first
      Dim versionedFiles As New List(Of VersionedScriptFile)

      If ScriptsDirectory IsNot Nothing Then

        RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Examining scripts directory for .sql files", .DateLogged = Now})
        For Each filePath As String In IO.Directory.GetFiles(ScriptsDirectory)

          If Text.RegularExpressions.Regex.IsMatch(filePath, ".*\.sql$") Then

            Dim fileVersion As Version = GetVersionFromFilePath(filePath)
            If fileVersion > currentDbVersion Then versionedFiles.Add(New VersionedScriptFile(fileVersion, filePath))

          End If

        Next

        'Run the scripts in order
        Dim orderedFilePaths = From vf As VersionedScriptFile In versionedFiles Order By vf.Version Ascending


        For Each scriptFile As VersionedScriptFile In orderedFilePaths

          RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Running script """ + IO.Path.GetFileName(scriptFile.FilePath) + """", .DateLogged = Now})

          DatabaseProvider.RunScript(IO.File.ReadAllText(scriptFile.FilePath))
          DatabaseProvider.UpdateVersion(IO.Path.GetFileName(scriptFile.FilePath), scriptFile.Version)

          RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Database succesfully upgraded to version """ + scriptFile.Version.ToString() + """", .DateLogged = Now})

          If latestVersion Is Nothing OrElse scriptFile.Version > latestVersion Then latestVersion = scriptFile.Version

        Next

      End If

      'Other script directories
      If OtherDirectories IsNot Nothing Then

        For Each otherDir As String In OtherDirectories

          For Each filePath As String In IO.Directory.GetFiles(otherDir, "*.sql")

            If Text.RegularExpressions.Regex.IsMatch(filePath, ".*\.sql$") Then

              RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Running other script """ + IO.Path.GetFileName(filePath) + """", .DateLogged = Now})
              DatabaseProvider.RunScript(IO.File.ReadAllText(filePath))
              RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Success", .DateLogged = Now})

            End If

          Next

        Next

      End If

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Committing transaction", .DateLogged = Now})

      DatabaseProvider.CommitTransaction()

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Transaction committed. Database upgraded to version " + latestVersion.ToString() + " successfully.", .DateLogged = Now})

    Catch ex As Exception

      DatabaseProvider.RollBackTransaction()
      mErrorMessage = ex.Message

    Finally

      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Closing database connection", .DateLogged = Now})
      DatabaseProvider.CloseDatabaseConnection()
      RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = "Database connection closed", .DateLogged = Now})

    End Try

  End Sub

  Private Function GetVersionFromFilePath(ByVal filePath As String) As Version

    Dim major As Int32 = 0
    Dim minor As Int32 = 0
    Dim build As Int32 = 0
    Dim revision As Int32 = 0

    Dim versionStringSplit As String() = IO.Path.GetFileNameWithoutExtension(filePath).Split(".")

    Int32.TryParse(versionStringSplit(0), major)
    If versionStringSplit.Length >= 2 Then Int32.TryParse(versionStringSplit(1), minor)
    If versionStringSplit.Length >= 3 Then Int32.TryParse(versionStringSplit(2), build)
    If versionStringSplit.Length >= 4 Then Int32.TryParse(versionStringSplit(3), revision)

    Return New Version(major, minor, build, revision)

  End Function

End Class
