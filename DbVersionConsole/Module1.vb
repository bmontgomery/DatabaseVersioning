Imports DatabaseVersioning

Module Module1

  Private Const argBeginStr As String = "-"
  Private Const argValSeperator As String = ","

  Private Const CONN_STR_ARG As String = "connStr"
  Private Const DROP_ARG As String = "drop"
  Private Const SCRIPTS_DIR_ARG As String = "scriptsDir"
  Private Const OTHER_KEY As String = "other"

  Private validArgs As New HashSet(Of String)()
  Private args As New Dictionary(Of String, String)
  Private dbVerMgr As DbVersionManager

  Sub Main()

    GenerateCommandLineArgDefinitions()
    ParseCommandLineArgs()

    InitDbVersionManager()

    AddHandler dbVerMgr.MessageLogged, AddressOf MessageLogged

    dbVerMgr.Go()

    If Not String.IsNullOrEmpty(dbVerMgr.ErrorMessage) Then
      Console.WriteLine(dbVerMgr.ErrorMessage)
    Else
      Console.WriteLine("Database upgraded succesfully.")
    End If

  End Sub

  Private Sub MessageLogged(ByVal sender As Object, ByVal e As MessageLoggedEventArgs)
    Console.WriteLine(e.Message)
  End Sub

  Private Sub GenerateCommandLineArgDefinitions()

    validArgs.Add(CONN_STR_ARG)
    validArgs.Add(DROP_ARG)
    validArgs.Add(SCRIPTS_DIR_ARG)

  End Sub

  Private Sub ParseCommandLineArgs()

    Dim argsStrings As String() = Environment.GetCommandLineArgs()
    Dim arg As String = Nothing

    For i As Int32 = 1 To argsStrings.Length - 1

      If arg Is Nothing Then

        If validArgs.Contains(argsStrings(i).TrimStart(argBeginStr)) Then
          arg = argsStrings(i).TrimStart(argBeginStr)
        Else

          If Not args.ContainsKey(OTHER_KEY) Then
            args.Add(OTHER_KEY, argsStrings(i))
          Else
            args(OTHER_KEY) += argValSeperator + argsStrings(i)
          End If

        End If

      Else

        If Not args.ContainsKey(arg) Then
          args.Add(arg, argsStrings(i))
        End If

        arg = Nothing

      End If

    Next

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

  End Sub

End Module
