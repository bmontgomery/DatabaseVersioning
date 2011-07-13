Public Class DbVersionManager

#Region "Properties"

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

  Private mPatchesDirectory As String
  Public Property PatchesDirectory() As String
    Get
      Return mPatchesDirectory
    End Get
    Set(ByVal value As String)
      mPatchesDirectory = value
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

  Private mLogLevel As LoggingLevel = LoggingLevel.ErrorsOnly
  Public Property LogLevel() As LoggingLevel
    Get
      Return mLogLevel
    End Get
    Set(ByVal value As LoggingLevel)
      mLogLevel = value
    End Set
  End Property

  Public Property UpgradeToVersion As Version

#End Region

  Public Enum LoggingLevel
    Verbose = 3
    Medium = 2
    ErrorsOnly = 1
    Off = 0
  End Enum

  Public Event MessageLogged(ByVal sender As Object, ByVal e As MessageLoggedEventArgs)

  Public Sub New(ByVal connectionString As String, ByVal drop As Boolean, ByVal scriptsDir As String, ByVal patchesDir As String, ByVal ParamArray otherDirs As String())

    mConnectionString = connectionString
    mDrop = drop
    mScriptsDirectory = scriptsDir
    mPatchesDirectory = patchesDir
    mOtherDirectories = otherDirs

  End Sub

  Public Sub Upgrade()

    LogMessage("Opening database connection", LoggingLevel.Verbose)
    DatabaseProvider.OpenDatabaseConnection(mConnectionString)

    LogMessage("Beginning transaction", LoggingLevel.Verbose)
    DatabaseProvider.BeginTransaction()

    Try

      LogMessage("Ensuring existence of version history table", LoggingLevel.Verbose)
      DatabaseProvider.EnsureVersionHistoryTableExists()

      LogMessage("Getting current database version", LoggingLevel.Verbose)
      Dim currentDbVersion As Version = DatabaseProvider.GetDatabaseVersion()

      If mDrop Then

        LogMessage("Dropping items", LoggingLevel.Medium)
        DatabaseProvider.DropItems()

      End If

      'Scripts (inluding patches)
      Dim latestVersion As Version = RunVersionedScripts(currentDbVersion)

      'Other script directories
      RunOtherScripts()

      LogMessage("Committing transaction", LoggingLevel.Verbose)
      DatabaseProvider.CommitTransaction()
      LogMessage("Transaction committed", LoggingLevel.Verbose)

      LogMessage("Database upgraded to version " + latestVersion.ToString() + " successfully.", LoggingLevel.Medium)

    Catch ex As Exception

      LogMessage("Error: " + ex.Message, LoggingLevel.ErrorsOnly)
      DatabaseProvider.RollBackTransaction()
      mErrorMessage = ex.Message

    Finally

      LogMessage("Closing database connection", LoggingLevel.Verbose)
      DatabaseProvider.CloseDatabaseConnection()
      LogMessage("Database connection closed", LoggingLevel.Verbose)

    End Try

  End Sub

  ''' <summary>
  ''' Logs the message if the log level set by the user is set to lower than the specified log level.
  ''' </summary>
  ''' <param name="message">The message.</param>
  ''' <param name="logLevel">The log level of the message.</param>
  Private Sub LogMessage(ByVal message As String, ByVal logLevel As LoggingLevel)
    If mLogLevel >= logLevel Then RaiseEvent MessageLogged(Me, New MessageLoggedEventArgs() With {.Message = message, .DateLogged = Now, .LogLevel = logLevel})
  End Sub

  ''' <summary>
  ''' Runs the versioned scripts. Returns the version to which this method upgrades the database.
  ''' </summary>
  ''' <param name="currentDbVersion">The current version of the database.</param>
  ''' <returns></returns>
  Private Function RunVersionedScripts(ByVal currentDbVersion As Version) As Version

    Dim latestVersion As Version = currentDbVersion
    Dim versionedFiles As New List(Of VersionedScriptFile)

    'find upgrade scripts
    If Not String.IsNullOrEmpty(ScriptsDirectory) Then

      LogMessage("Examining scripts directory for .sql files", LoggingLevel.Verbose)
      Dim upgradeScripts As List(Of VersionedScriptFile) = GetVersionedScripts(ScriptsDirectory)

      Dim applicableScripts = From us As VersionedScriptFile In upgradeScripts Where us.Version > currentDbVersion
      If applicableScripts IsNot Nothing Then
        versionedFiles.AddRange(applicableScripts)
      End If

    End If

    'find patch scripts
    If Not String.IsNullOrEmpty(PatchesDirectory) Then

      LogMessage("Examining patches directory for .sql files", LoggingLevel.Verbose)
      Dim patchScripts As List(Of VersionedScriptFile) = GetVersionedScripts(PatchesDirectory)

      For Each patchScript As VersionedScriptFile In patchScripts
        If Not DatabaseProvider.IsPatchApplied(patchScript.Version) Then

          patchScript.IsPatch = True
          versionedFiles.Add(patchScript)

        End If
      Next

    End If

    'Run the scripts in order
    Dim orderedFilePaths = From vf As VersionedScriptFile In versionedFiles Order By vf.Version Ascending

    LogMessage("Running scripts in """ + ScriptsDirectory + """", LoggingLevel.Medium)

    For Each scriptFile As VersionedScriptFile In orderedFilePaths

      If UpgradeToVersion Is Nothing OrElse scriptFile.Version <= UpgradeToVersion Then

        LogMessage("Running script """ + IO.Path.GetFileName(scriptFile.FilePath) + """", LoggingLevel.Verbose)

        Try
          DatabaseProvider.RunScript(IO.File.ReadAllText(scriptFile.FilePath))
        Catch ex As Exception
          ThrowRunScriptException(ex, scriptFile.FilePath)
        End Try

        DatabaseProvider.UpdateVersion(IO.Path.GetFileName(scriptFile.FilePath), scriptFile.Version)

        LogMessage("Database succesfully upgraded to version """ + scriptFile.Version.ToString() + """", LoggingLevel.Verbose)

        If latestVersion Is Nothing OrElse scriptFile.Version > latestVersion Then latestVersion = scriptFile.Version

      End If

    Next

    Return latestVersion

  End Function

  ''' <summary>
  ''' Returns a list of VersionedScriptFile objects representing the versioned scripts found
  ''' in the specified directory
  ''' </summary>
  ''' <param name="directory">The directory which contains versioned script files.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function GetVersionedScripts(ByVal directory As String) As List(Of VersionedScriptFile)

    Dim versionedFiles As New List(Of VersionedScriptFile)

    For Each filePath As String In IO.Directory.GetFiles(directory)

      If Text.RegularExpressions.Regex.IsMatch(filePath, ".*\.sql$") Then

        Dim fileVersion As Version = GetVersionFromFilePath(filePath)
        versionedFiles.Add(New VersionedScriptFile(fileVersion, filePath))

      End If

    Next

    Return versionedFiles

  End Function

  ''' <summary>
  ''' Runs the non-versioned scripts.
  ''' </summary>
  Private Sub RunOtherScripts()

    If OtherDirectories IsNot Nothing Then

      For Each otherDir As String In OtherDirectories

        If Not String.IsNullOrEmpty(otherDir) Then

          LogMessage("Running scripts in """ + otherDir + """", LoggingLevel.Medium)
          For Each filePath As String In IO.Directory.GetFiles(otherDir, "*.sql")

            If Text.RegularExpressions.Regex.IsMatch(filePath, ".*\.sql$") Then

              LogMessage("Running other script """ + IO.Path.GetFileName(filePath) + """", LoggingLevel.Verbose)

              Try
                DatabaseProvider.RunScript(IO.File.ReadAllText(filePath))
              Catch ex As Exception
                ThrowRunScriptException(ex, filePath)
              End Try

              LogMessage("Success", LoggingLevel.Verbose)

            End If

          Next

        End If

      Next

    End If

  End Sub

  ''' <summary>
  ''' Gets the version from file path.
  ''' </summary>
  ''' <param name="filePath">The file path.</param>
  ''' <returns></returns>
  Private Function GetVersionFromFilePath(ByVal filePath As String) As Version

    Dim major As Int32 = 0
    Dim minor As Int32 = 0
    Dim build As Int32 = 0
    Dim revision As Int32 = 0

    Dim fileNameWithoutExt As String = IO.Path.GetFileNameWithoutExtension(filePath)
    Dim versionString As String

    Dim spaceIndex As Int32 = fileNameWithoutExt.IndexOf(" ")
    If spaceIndex > 0 Then

      'strip off the end of the string which isn't part of the version
      versionString = fileNameWithoutExt.Substring(0, spaceIndex)

    Else
      versionString = fileNameWithoutExt
    End If

    Dim versionStringSplit As String() = versionString.Split(".")

    Int32.TryParse(versionStringSplit(0), major)
    If versionStringSplit.Length >= 2 Then Int32.TryParse(versionStringSplit(1), minor)
    If versionStringSplit.Length >= 3 Then Int32.TryParse(versionStringSplit(2), build)
    If versionStringSplit.Length >= 4 Then Int32.TryParse(versionStringSplit(3), revision)

    Return New Version(major, minor, build, revision)

  End Function

  ''' <summary>
  ''' Gets a nicely formatted, very informational message about the SQL exception.
  ''' </summary>
  ''' <param name="sqlEx">The exception.</param>
  ''' <returns></returns>
  Private Shared Function GetSqlExceptionString(ByVal sqlEx As SqlClient.SqlException) As String

    If sqlEx IsNot Nothing Then
      Return String.Format("Msg {0}, Level {1}, State {2}, Line {3}" + Environment.NewLine + "{4}", _
                           sqlEx.Number, _
                           sqlEx.Class, _
                           sqlEx.State, _
                           sqlEx.LineNumber, _
                           sqlEx.Message)
    Else
      Return String.Empty
    End If

  End Function

  ''' <summary>
  ''' Throws the run script exception. This method creates an ApplicationException with some information which will be useful to the user, then throws it.
  ''' </summary>
  ''' <param name="ex">The ex.</param>
  ''' <param name="scriptFilePath">The script file path.</param>
  Private Shared Sub ThrowRunScriptException(ByVal ex As Exception, ByVal scriptFilePath As String)

    If TypeOf ex Is SqlClient.SqlException Then
      Throw New ApplicationException(String.Format("Error running script ""{0}"": " + Environment.NewLine + "{1}", scriptFilePath, GetSqlExceptionString(ex)))
    Else
      Throw New ApplicationException(String.Format("Error running script ""{0}"": {1}", scriptFilePath, ex.Message))
    End If

  End Sub

End Class
