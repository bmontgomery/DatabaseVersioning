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
    dbVerMgr = New DbVersionManager(CONN_STR, DROP, SCRIPTS_DIR)
    dbVerMgr.DatabaseProvider = mockDbProvider

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
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CommitTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrGo_NoDatabase_CreatesDatabase()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub MgrGo_DatabaseExists_DoesNotCreateDatabase()
    Throw New NotImplementedException()
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
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(5, 5)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsScriptsInOrderOfVersion()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.0.sql", New Version(1, 0, 0, 0))).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.2.sql", New Version(1, 0, 0, 2))).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.00.0.003.sql", New Version(1, 0, 0, 3))).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.1.0.sql", New Version(1, 0, 1, 0))).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.2.0.0.sql", New Version(1, 2, 0, 0))).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Go()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsOtherScriptsAfterScripts()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub Mgr_Go_RunsOtherDirScriptsInOrder()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub MgrGo_FreshDatabase_RunsAllScripts()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub MgrGo_PartiallyUpdatedDatabase_RunsOnlyNecessaryScripts()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub Mgr_Go_UpdatesVersionForEachScript()
    Throw New NotImplementedException()
  End Sub

  <Test()> _
  Public Sub MgrGo_Errors_ReturnsErrorMessage()
    Throw New NotImplementedException()
  End Sub

End Class
