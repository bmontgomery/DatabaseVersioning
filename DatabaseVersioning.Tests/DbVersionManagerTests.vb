Imports NUnit.Framework
Imports Rhino.Mocks

<TestFixture()> _
Public Class DbVersionManagerTests

  Dim mockery As MockRepository
  Dim mockDbProvider As IDatabaseProvider
  Dim dbVerMgr As DbVersionManager

  Private Const CONN_STR As String = "server=.\SQLEXPRESS;database=Test"
  Private Const DROP As Boolean = True
  Private Const SCRIPTS_DIR As String = "C:\Projects\DatabaseVersioning\DatabaseVersioning.Tests\TestData\Scripts"
  Private Const OTHER_DIR1 As String = "C:\Projects\DatabaseVersioning\DatabaseVersioning.Tests\TestData\Views"
  Private Const OTHER_DIR2 As String = "C:\Projects\DatabaseVersioning\DatabaseVersioning.Tests\TestData\Functions"
  Private Const OTHER_DIR3 As String = "C:\Projects\DatabaseVersioning\DatabaseVersioning.Tests\TestData\StoredProcedures"

  <SetUp()> _
  Public Sub SetupTest()

    mockery = New MockRepository()
    mockDbProvider = mockery.DynamicMock(Of IDatabaseProvider)()
    dbVerMgr = New DbVersionManager(CONN_STR, DROP, SCRIPTS_DIR, OTHER_DIR1, OTHER_DIR2, OTHER_DIR3)
    dbVerMgr.DatabaseProvider = mockDbProvider

  End Sub

  <Test()> _
  Public Sub Mgr_Go_OpensDatabaseConnection()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.OpenDatabaseConnection(CONN_STR)).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_Success_ClosesDatabaseConnection()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CloseDatabaseConnection()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_Errors_ClosesDatabaseConnection()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists()).Throw(New ApplicationException(""))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CloseDatabaseConnection()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_BeginsTransaction()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.BeginTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_Errors_RollsBackTransaction()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists()).Throw(New ApplicationException(""))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RollBackTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_Succeeds_CommitsTransaction()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CommitTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_ChecksForDatabaseExistence()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.DatabaseExists()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_NoDatabase_CreatesDatabase()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.DatabaseExists()).Return(False)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CreateDatabase()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_DatabaseExists_DoesNotCreateDatabase()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.DatabaseExists()).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CreateDatabase()).Repeat.Never()

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_DropAllTrue_DropsAllItems()

    'Arrange
    dbVerMgr.Drop = True
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.DropItems()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_DropAllFalse_DoesNotDropItems()

    'Arrange
    dbVerMgr.Drop = False
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.DropItems()).Repeat.Never()

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_EnsuresVersionHistoryTableExists()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists())

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsScriptsInOrderOfVersion()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    Using mockery.Ordered()

      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.0.sql", New Version(1, 0, 0, 0))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.2.sql", New Version(1, 0, 0, 2))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.00.0.003.sql", New Version(1, 0, 0, 3))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.1.0.sql", New Version(1, 0, 1, 0))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.2.0.0.sql", New Version(1, 2, 0, 0))).Return(True)

    End Using

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsOtherScriptsAfterScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(5)
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.UpdateVersion("", Nothing)).IgnoreArguments().Return(True).Repeat.Times(5)

    Using mockery.Ordered()
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(12)
    End Using

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsOtherDirScriptsInOrder()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    Using mockery.Ordered()

      'Views
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--view 1")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--view 2")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--view 3")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--view 4")).Return(True)

      'Functions
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--function 1")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--function 2")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--function 3")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--function 4")).Return(True)

      'Stored procedures
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--stored procedure 1")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--stored procedure 2")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--stored procedure 3")).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--stored procedure 4")).Return(True)

    End Using

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_FreshDatabase_RunsAllScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(5)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_PartiallyUpdatedDatabase_RunsOnlyNecessaryScripts()

    'Arrange
    Dim strictDbProvider As IDatabaseProvider = mockery.StrictMock(Of IDatabaseProvider)()
    dbVerMgr.DatabaseProvider = strictDbProvider

    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.OpenDatabaseConnection(CONN_STR)).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.DatabaseExists()).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.BeginTransaction()).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.DropItems()).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(1, 0, 0, 3))
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists()).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.CommitTransaction()).Return(True)
    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.CloseDatabaseConnection()).Return(True)

    strictDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.1.0.sql", New Version(1, 0, 1, 0))).Return(True)
    strictDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.2.0.0.sql", New Version(1, 2, 0, 0))).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_Errors_ReturnsErrorMessage()

    'Arrange
    Dim testErrorMessage As String = "Test error message"
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Throw(New ApplicationException(testErrorMessage))

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    Assert.AreEqual(testErrorMessage, dbVerMgr.ErrorMessage)

  End Sub

End Class
