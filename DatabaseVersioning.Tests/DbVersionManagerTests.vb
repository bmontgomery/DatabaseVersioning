Imports NUnit.Framework
Imports Rhino.Mocks

<TestFixture()> _
Public Class DbVersionManagerTests

  Dim mockery As MockRepository
  Dim mockDbProvider As IDatabaseProvider
  Dim dbVerMgr As DbVersionManager

  Private Const CONN_STR As String = "server=.\SQLEXPRESS;database=Test"
  Private Const DROP As Boolean = True
  Private Const SCRIPTS_BASE_DIR As String = "C:\Source Code\DatabaseVersioning\DatabaseVersioning.Tests\TestData\"
  Private scriptsDir As String = IO.Path.Combine(SCRIPTS_BASE_DIR, "Scripts")
  Private otherDir1 As String = IO.Path.Combine(SCRIPTS_BASE_DIR, "Views")
  Private otherDir2 As String = IO.Path.Combine(SCRIPTS_BASE_DIR, "Functions")
  Private otherDir3 As String = IO.Path.Combine(SCRIPTS_BASE_DIR, "StoredProcedures")
  Private patchesDir As String = IO.Path.Combine(SCRIPTS_BASE_DIR, "Patches")

  <SetUp()> _
  Public Sub SetupTest()

    mockery = New MockRepository()
    mockDbProvider = mockery.DynamicMock(Of IDatabaseProvider)()
    dbVerMgr = New DbVersionManager(CONN_STR, DROP, scriptsDir, "", otherDir1, otherDir2, otherDir3)
    dbVerMgr.DatabaseProvider = mockDbProvider

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_OpensDatabaseConnection()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.OpenDatabaseConnection(CONN_STR)).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_Success_ClosesDatabaseConnection()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CloseDatabaseConnection()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_Errors_ClosesDatabaseConnection()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists()).Throw(New ApplicationException(""))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CloseDatabaseConnection()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_BeginsTransaction()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.BeginTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_Errors_RollsBackTransaction()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists()).Throw(New ApplicationException(""))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RollBackTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_Succeeds_CommitsTransaction()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.CommitTransaction()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_DropAllTrue_DropsAllItems()

    'Arrange
    dbVerMgr.Drop = True
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.DropItems()).Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_DropAllFalse_DoesNotDropItems()

    'Arrange
    dbVerMgr.Drop = False
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.DropItems()).Repeat.Never()

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_EnsuresVersionHistoryTableExists()

    'Arrange
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.EnsureVersionHistoryTableExists())

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_RunsScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_RunsOnlySqlScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.1.txt", New Version(1, 0, 0, 1))).Repeat.Never()

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_RunsScriptsInOrderOfVersion()

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
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_RunsOtherScriptsAfterScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(5)
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.UpdateVersion("", Nothing)).IgnoreArguments().Return(True).Repeat.Times(5)

    Using mockery.Ordered()
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(12)
    End Using

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Mgr_Upgrade_RunsOtherDirScriptsInOrder()

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
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_FreshDatabase_RunsAllScripts()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Return(True).Repeat.Times(5)

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_PartiallyUpdatedDatabase_RunsOnlyNecessaryScripts()

    'Arrange
    Dim strictDbProvider As IDatabaseProvider = mockery.StrictMock(Of IDatabaseProvider)()
    dbVerMgr.DatabaseProvider = strictDbProvider

    strictDbProvider.Stub(Function(p As IDatabaseProvider) p.OpenDatabaseConnection(CONN_STR)).Return(True)
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
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub MgrUpgrade_Errors_ReturnsErrorMessage()

    'Arrange
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.RunScript("")).IgnoreArguments().Throw(New ApplicationException(""))

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    Assert.IsFalse(String.IsNullOrEmpty(dbVerMgr.ErrorMessage))

  End Sub

  <Test()> _
  Public Sub Upgrade_WithPatchBeforeCurrentVersionAndNotPreviouslyRun_RunsPatch()

    'if the patch hasn't been run yet, and the patch is before the current db version, 
    'the patch needs to be run (at least at some point). there are more tests which 
    'test more complex requirements.

    'Arrange
    dbVerMgr.PatchesDirectory = patchesDir

    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.IsPatchApplied(New System.Version(1, 0, 0, 4))).Return(False)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--patch script 1.0.0.4"))

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Upgrade_WithPatch_UpdatesVersionTable()

    'after each patch is run, the VersionHistory table needs to be upgraded to record that the patch has been
    'run so it is not run again in the future.

    'Arrange
    dbVerMgr.PatchesDirectory = patchesDir

    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.IsPatchApplied(New System.Version(1, 0, 0, 4))).Return(False)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.4.sql", New System.Version(1, 0, 0, 4)))

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Upgrade_WithPreviouslyRunPatch_DoesNotRunPatchScript()

    'if the patch has already been run, it should not run again.

    'Arrange
    dbVerMgr.PatchesDirectory = patchesDir

    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.IsPatchApplied(New System.Version(1, 0, 0, 4))).Return(True)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--patch script 1.0.0.4")).Repeat.Never()
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.4.sql", New System.Version(1, 0, 0, 4))).Repeat.Never()

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Upgrade_WithPatchAndUpgradeScript_UpgradesDatabaseInOrder()

    'as scripts are run, the patches need to be run in order. i.e a 1.0.0.1 patch script cannot be run
    'before a 1.0.0.0 script (whether it's patch or upgrade)

    'Arrange
    dbVerMgr.PatchesDirectory = patchesDir

    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(0, 0, 0, 0))

    Using mockery.Ordered()

      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.0.sql", New Version(1, 0, 0, 0))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.2.sql", New Version(1, 0, 0, 2))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.00.0.003.sql", New Version(1, 0, 0, 3))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.0.4.sql", New Version(1, 0, 0, 4))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("1.0.1.0.sql", New Version(1, 0, 1, 0))).Return(True)
      mockDbProvider.Expect(Function(p As IDatabaseProvider) p.UpdateVersion("01.2.0.0.sql", New Version(1, 2, 0, 0))).Return(True)

    End Using

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

  <Test()> _
  Public Sub Upgrade_WithPatchBehindCurrentVersion_RunsPatch()

    'patch scripts need to be run even if they are behind the current version of the database
    'i.e. if the db is at version 5.4.3.2, and a patch which is versioned at 4.5.0.1 is defined
    '(and not yet run on the db), then the 4.5.0.1 patch needs to be run

    'Arrange
    dbVerMgr.PatchesDirectory = patchesDir

    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.GetDatabaseVersion()).Return(New Version(2, 0, 0, 0))
    mockDbProvider.Stub(Function(p As IDatabaseProvider) p.IsPatchApplied(New System.Version(1, 0, 0, 4))).Return(False)
    mockDbProvider.Expect(Function(p As IDatabaseProvider) p.RunScript("--patch script 1.0.0.4"))

    mockery.ReplayAll()

    'Action
    dbVerMgr.Upgrade()

    'Assert
    mockery.VerifyAll()

  End Sub

End Class
