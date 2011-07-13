Imports DatabaseVersioning

Module Module1

  Private Const ARG_BEGIN_STR As String = "-"
  Private Const ARG_VALUE_SEPARATOR As String = ","

  Private Const CONN_STR_ARG As String = "connStr"
  Private Const DROP_ARG As String = "drop"
  Private Const SCRIPTS_DIR_ARG As String = "scripts"
  Private Const PATCHES_DIR_ARG As String = "patches"
  Private Const LOG_LEVEL_ARG As String = "l"
  Private Const OTHER_KEY As String = "other"
  Private Const HELP_ARG As String = "help"
  
  Private validArgs As New HashSet(Of String)()
  Private args As New Dictionary(Of String, String)
  Private dbVerMgr As DbVersionManager

  Sub Main()

    Try

      WriteInfo()

      GenerateCommandLineArgDefinitions()
      ParseCommandLineArgs()

      If args.ContainsKey(HELP_ARG) Then
        PrintHelp()
      Else

        InitDbVersionManager()
        dbVerMgr.Upgrade()

      End If

    Catch ex As Exception

      Console.WriteLine(ex.Message)
      Console.WriteLine(ex.StackTrace)

    End Try

  End Sub

  Private Sub WriteInfo()

    Console.WriteLine("")
    Console.WriteLine("CINCH")
    Console.WriteLine("by Brandon Montgomery")
    Console.WriteLine("2010")
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
    validArgs.Add(PATCHES_DIR_ARG)
    validArgs.Add(OTHER_KEY)
    validArgs.Add(LOG_LEVEL_ARG)
    validArgs.Add(HELP_ARG)

  End Sub

  Private Sub ParseCommandLineArgs()

    Dim argStrings As String() = Environment.GetCommandLineArgs()
    Dim currentArg As String = Nothing

    For i As Int32 = 1 To argStrings.Length - 1

      If validArgs.Contains(argStrings(i).TrimStart(ARG_BEGIN_STR)) Then

        currentArg = argStrings(i).TrimStart(ARG_BEGIN_STR)
        args.Add(currentArg, Nothing)

      Else

        If String.IsNullOrEmpty(args(currentArg)) Then
          args(currentArg) = argStrings(i)
        Else
          args(currentArg) += ARG_VALUE_SEPARATOR + argStrings(i)
        End If

      End If

    Next

  End Sub

  Private Sub InitDbVersionManager()

    Dim connStr As String = String.Empty
    Dim drop As Boolean = False
    Dim scriptsDir As String = String.Empty
    Dim patchesDir As String = String.Empty
    Dim otherDirs As String() = Nothing

    If args.ContainsKey(CONN_STR_ARG) Then
      connStr = args(CONN_STR_ARG)
    End If

    drop = args.ContainsKey(DROP_ARG)

    If args.ContainsKey(SCRIPTS_DIR_ARG) Then
      scriptsDir = args(SCRIPTS_DIR_ARG)
    End If

    If args.ContainsKey(PATCHES_DIR_ARG) Then
      patchesDir = args(PATCHES_DIR_ARG)
    End If

    If args.ContainsKey(OTHER_KEY) Then
      otherDirs = args(OTHER_KEY).Split(",")
    End If

    dbVerMgr = New DbVersionManager(connStr, drop, scriptsDir, patchesDir, otherDirs)
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
