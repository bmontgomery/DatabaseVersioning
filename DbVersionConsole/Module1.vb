Imports DatabaseVersioning

Module Module1

  Private Const ARG_BEGIN_STR As String = "-"
  Private Const ARG_VALUE_SEPARATOR As String = ","

  Private Const CONN_STR_ARG As String = "connStr"
  Private Const DROP_ARG As String = "drop"
  Private Const SCRIPTS_DIR_ARG As String = "scriptsDir"
  Private Const LOG_LEVEL_ARG As String = "l"
  Private Const OTHER_KEY As String = "other"
  Private Const HELP_ARG As String = "help"

  Private validArgs As New HashSet(Of String)()
  Private args As New Dictionary(Of String, String)
  Private dbVerMgr As DbVersionManager

  Sub Main()

    WriteInfo()

    GenerateCommandLineArgDefinitions()
    ParseCommandLineArgs()

    If args.ContainsKey(HELP_ARG) Then
      PrintHelp()
    Else

      InitDbVersionManager()
      dbVerMgr.Go()

    End If

  End Sub

  Private Sub WriteInfo()

    Console.WriteLine("")
    Console.WriteLine("Cinch")
    Console.WriteLine("by Brandon Montgomery")
    Console.WriteLine("")

  End Sub

  Private Sub MessageLogged(ByVal sender As Object, ByVal e As MessageLoggedEventArgs)

    If e.LogLevel = DbVersionManager.LoggingLevel.ErrorsOnly Then Console.WriteLine("")
    Console.WriteLine(e.Message)

  End Sub

  Private Sub GenerateCommandLineArgDefinitions()

    validArgs.Add(CONN_STR_ARG)
    validArgs.Add(DROP_ARG)
    validArgs.Add(SCRIPTS_DIR_ARG)
    validArgs.Add(LOG_LEVEL_ARG)
    validArgs.Add(HELP_ARG)

  End Sub

  Private Sub ParseCommandLineArgs()

    Dim argsStrings As String() = Environment.GetCommandLineArgs()
    Dim arg As String = Nothing

    For i As Int32 = 1 To argsStrings.Length - 1

      If arg Is Nothing Then

        If validArgs.Contains(argsStrings(i).TrimStart(ARG_BEGIN_STR)) Then
          arg = argsStrings(i).TrimStart(ARG_BEGIN_STR)
        Else

          If Not args.ContainsKey(OTHER_KEY) Then
            args.Add(OTHER_KEY, argsStrings(i))
          Else
            args(OTHER_KEY) += ARG_VALUE_SEPARATOR + argsStrings(i)
          End If

        End If

      Else

        If Not args.ContainsKey(arg) Then
          args.Add(arg, argsStrings(i))
        End If

        arg = Nothing

      End If

    Next

    If Not args.ContainsKey(arg) Then
      args.Add(arg, String.Empty)
    End If

  End Sub

  Private Sub InitDbVersionManager()

    Dim connStr As String = String.Empty
    Dim drop As Boolean = True
    Dim scriptsDir As String = String.Empty
    Dim otherDirs As String() = Nothing

    If args.ContainsKey(CONN_STR_ARG) Then
      connStr = args(CONN_STR_ARG)
    End If

    If args.ContainsKey(DROP_ARG) AndAlso args(DROP_ARG).Equals("false", StringComparison.OrdinalIgnoreCase) Then
      drop = False
    End If

    If args.ContainsKey(SCRIPTS_DIR_ARG) Then
      scriptsDir = args(SCRIPTS_DIR_ARG)
    End If

    If args.ContainsKey(OTHER_KEY) Then
      otherDirs = args(OTHER_KEY).Split(",")
    End If

    dbVerMgr = New DbVersionManager(connStr, drop, scriptsDir, otherDirs)
    dbVerMgr.DatabaseProvider = New MsSqlDatabaseProvider()
    dbVerMgr.LogLevel = DbVersionManager.LoggingLevel.Medium

    If args.ContainsKey(LOG_LEVEL_ARG) Then

      Select Case args(LOG_LEVEL_ARG)

        Case "v"
          dbVerMgr.LogLevel = DbVersionManager.LoggingLevel.Verbose

        Case "e"
          dbVerMgr.LogLevel = DbVersionManager.LoggingLevel.ErrorsOnly

        Case "o"
          dbVerMgr.LogLevel = DbVersionManager.LoggingLevel.Off

        Case "m"
          dbVerMgr.LogLevel = DbVersionManager.LoggingLevel.Medium

      End Select

    End If

    AddHandler dbVerMgr.MessageLogged, AddressOf MessageLogged

  End Sub

  Private Sub PrintHelp()

    Dim helpFilePath As String = IO.Path.Combine(Environment.CurrentDirectory, "readme.txt")
    If IO.File.Exists(helpFilePath) Then
      Console.Write(IO.File.ReadAllText(helpFilePath))
    Else
      Console.WriteLine("The readme.txt file is missing at " + helpFilePath + "!")
    End If

  End Sub

End Module
