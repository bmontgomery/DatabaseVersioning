Imports NUnit.Framework
Imports Rhino.Mocks

<TestFixture()> _
Public Class DbVersionManagerTests

  Dim mockery As MockRepository
  Dim mockDbProvider As IDatabaseProvider
  Dim dbVerMgr As DbVersionManager

  Private Const connStr As String = "server=.\SQLEXPRESS;database=Test"
  Private Const drop As Boolean = True
  Private Const scriptsDir As String = "C:\DbScriptsTest"
  Private Const otherDir1 As String = "C:\DbViews"
  Private Const otherDir2 As String = "C:\DbFunctions"
  Private Const otherDir3 As String = "C:\DbSPs"

  <SetUp()> _
  Public Sub SetupTest()

    mockery = New MockRepository()
    mockDbProvider = mockery.DynamicMock(Of IDatabaseProvider)()
    dbVerMgr = New DbVersionManager(connStr, drop, scriptsDir)
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
    Throw New NotImplementedException()
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
