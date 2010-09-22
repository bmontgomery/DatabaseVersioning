Imports System.Data.SqlClient

Public Class MsSqlDatabaseProvider
  Implements IDatabaseProvider

  Private connection As SqlConnection
  Private transaction As SqlTransaction

  Public Function BeginTransaction() As Object Implements IDatabaseProvider.BeginTransaction
    transaction = connection.BeginTransaction()
  End Function

  Public Function CloseDatabaseConnection() As Object Implements IDatabaseProvider.CloseDatabaseConnection
    connection.Close()
  End Function

  Public Function CommitTransaction() As Object Implements IDatabaseProvider.CommitTransaction
    transaction.Commit()
  End Function

  Public Function CreateDatabase() As Object Implements IDatabaseProvider.CreateDatabase

  End Function

  Public Function DatabaseExists() As Boolean Implements IDatabaseProvider.DatabaseExists

  End Function

  Public Function DropItems() As Object Implements IDatabaseProvider.DropItems

    Dim sql As New Text.StringBuilder()

    '--sps
    sql.AppendLine("Declare @procName varchar(500)")
    sql.AppendLine("Declare @procs cursor	")
    sql.AppendLine("Set @procs = Cursor For Select name From sysobjects Where type = 'P' and category = '0'")

    sql.AppendLine("Open @procs")
    sql.AppendLine("Fetch Next From @procs Into @procName")

    sql.AppendLine("While @@FETCH_STATUS = 0 Begin")
    sql.AppendLine("	If @procName <> '' Exec('DROP PROCEDURE [' + @procName + ']')")
    sql.AppendLine("	Fetch Next From @procs Into @procName")
    sql.AppendLine("End")

    sql.AppendLine("Close @procs")
    sql.AppendLine("Deallocate @procs")

    '--functions
    sql.AppendLine("Declare @functionName varchar(500)")
    sql.AppendLine("Declare @functions cursor	")
    sql.AppendLine("Set @functions = Cursor For Select name From sysobjects Where type = 'P' and category = '0'")

    sql.AppendLine("Open @functions")
    sql.AppendLine("Fetch Next From @functions Into @functionName")

    sql.AppendLine("While @@FETCH_STATUS = 0 Begin")
    sql.AppendLine("	If @functionName <> '' Exec('DROP PROCEDURE [' + @functionName + ']')")
    sql.AppendLine("	Fetch Next From @functions Into @functionName")
    sql.AppendLine("End")

    sql.AppendLine("Close @functions")
    sql.AppendLine("Deallocate @functions")

    '--views
    sql.AppendLine("Declare @viewName varchar(500)")
    sql.AppendLine("Declare @views cursor	")
    sql.AppendLine("Set @views = Cursor For Select name From sysobjects Where type = 'V' and category = '0'")

    sql.AppendLine("Open @views")
    sql.AppendLine("Fetch Next From @views Into @viewName")

    sql.AppendLine("While @@FETCH_STATUS = 0 Begin")
    sql.AppendLine("	If @viewName <> '' Exec('DROP PROCEDURE [' + @viewName + ']')")
    sql.AppendLine("	Fetch Next From @views Into @viewName")
    sql.AppendLine("End")

    sql.AppendLine("Close @views")
    sql.AppendLine("Deallocate @views")

    Dim dropItemsCommand As New SqlCommand(sql.ToString(), connection, transaction)
    dropItemsCommand.ExecuteNonQuery()

  End Function

  Public Function EnsureVersionHistoryTableExists() As Object Implements IDatabaseProvider.EnsureVersionHistoryTableExists

    Dim sql As New Text.StringBuilder()
    sql.AppendLine("If Not EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VersionHistory]') AND type in (N'U'))")
    sql.AppendLine("CREATE TABLE [dbo].[VersionHistory](")
    sql.AppendLine("  [VH_ID] [int] IDENTITY(1,1) NOT NULL,")
    sql.AppendLine("	[VH_FileName] [varchar](max) NOT NULL,")
    sql.AppendLine("	[VH_DateApplied] [datetime] NOT NULL,")
    sql.AppendLine("	[VH_Major] [int] NOT NULL,")
    sql.AppendLine("	[VH_Minor] [int] NOT NULL,")
    sql.AppendLine("	[VH_Build] [int] NOT NULL,")
    sql.AppendLine("	[VH_Revision] [int] NOT NULL,")
    sql.AppendLine(" CONSTRAINT [PK_VersionHistory] PRIMARY KEY CLUSTERED ")
    sql.AppendLine("(")
    sql.AppendLine("	[VH_ID] ASC")
    sql.AppendLine(")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]")
    sql.AppendLine(") ON [PRIMARY]")

    Dim createTableCommand As New SqlCommand(sql.ToString(), connection, transaction)
    createTableCommand.ExecuteNonQuery()

  End Function

  Public Function GetDatabaseVersion() As System.Version Implements IDatabaseProvider.GetDatabaseVersion

    Dim sql As New Text.StringBuilder()
    sql.AppendLine("Select Top 1")
    sql.AppendLine("	VH_Major,")
    sql.AppendLine("	VH_Minor,")
    sql.AppendLine("	VH_Build,")
    sql.AppendLine("	VH_Revision")
    sql.AppendLine("From")
    sql.AppendLine("	VersionHistory")
    sql.AppendLine("Order By")
    sql.AppendLine("	VH_Major DESC,")
    sql.AppendLine("	VH_Minor DESC,")
    sql.AppendLine("	VH_Build DESC,")
    sql.AppendLine("	VH_Revision DESC")

    Dim major As Int32 = 0
    Dim minor As Int32 = 0
    Dim build As Int32 = 0
    Dim revision As Int32 = 0

    Dim getVersionCommand As New SqlCommand(sql.ToString(), connection, transaction)
    Using oDR As IDataReader = getVersionCommand.ExecuteReader()

      If oDR.Read() Then

        If Not IsDBNull(oDR("VH_Major")) Then major = oDR("VH_Major")
        If Not IsDBNull(oDR("VH_Minor")) Then minor = oDR("VH_Minor")
        If Not IsDBNull(oDR("VH_Build")) Then build = oDR("VH_Build")
        If Not IsDBNull(oDR("VH_Revision")) Then revision = oDR("VH_Revision")

      End If

    End Using

    Return New Version(major, minor, build, revision)

  End Function

  Public Function OpenDatabaseConnection(ByVal connStr As String) As Object Implements IDatabaseProvider.OpenDatabaseConnection

    connection = New SqlConnection(connStr)
    connection.Open()

  End Function

  Public Function RollBackTransaction() As Object Implements IDatabaseProvider.RollBackTransaction
    transaction.Rollback()
  End Function

  Public Function RunScript(ByVal scriptText As String) As Object Implements IDatabaseProvider.RunScript

    'If there are GO statements we need to split those statements up into separate commands, or else there will be errors.
    Dim commands As String() = Text.RegularExpressions.Regex.Split(scriptText, "^\s*go\s*$", Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Multiline)

    For Each commandText As String In commands

      If Not String.IsNullOrEmpty(commandText) Then

        Dim scriptCommand As New SqlCommand(commandText, connection, transaction)
        scriptCommand.ExecuteNonQuery()

      End If

    Next

  End Function

  Public Function UpdateVersion(ByVal scriptName As String, ByVal version As System.Version) As Object Implements IDatabaseProvider.UpdateVersion

    Dim sql As New Text.StringBuilder()
    sql.AppendLine("INSERT INTO VersionHistory (")
    sql.AppendLine("  VH_FileName,")
    sql.AppendLine("  VH_DateApplied,")
    sql.AppendLine("  VH_Major,")
    sql.AppendLine("  VH_Minor,")
    sql.AppendLine("  VH_Build,")
    sql.AppendLine("  VH_Revision")
    sql.AppendLine(") VALUES (")
    sql.AppendFormat("  '{0}'," + Environment.NewLine, scriptName.Replace("'", "''"))
    sql.AppendLine("  getdate(),")
    sql.AppendFormat("  {0}," + Environment.NewLine, version.Major)
    sql.AppendFormat("  {0}," + Environment.NewLine, version.Minor)
    sql.AppendFormat("  {0}," + Environment.NewLine, version.Build)
    sql.AppendFormat("  {0}" + Environment.NewLine, version.Revision)
    sql.AppendLine(");")

    Dim updateVersionCommand As New SqlCommand(sql.ToString(), connection, transaction)
    updateVersionCommand.ExecuteNonQuery()
    
  End Function

End Class
