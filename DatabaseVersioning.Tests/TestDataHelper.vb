Public Class TestDataHelper

  Public Enum VersionStepMode
    Major
    Minor
    Build
    Revision
  End Enum

  Public Shared Sub GenerateTestScripts(ByVal testScriptsDir As String, ByVal beginVersion As Version, ByVal endVersion As Version, ByVal stepMode As VersionStepMode, ByVal stepLimit As Int32)

    If endVersion > beginVersion Then

      While beginVersion <= endVersion

        Dim filePath As String = IO.Path.Combine(testScriptsDir, beginVersion.ToString() + ".sql")
        If Not IO.File.Exists(filePath) Then IO.File.Create(filePath)

        IncrementVersion(beginVersion, stepMode, stepLimit)

      End While

    Else
      Throw New ApplicationException("End Version must be after Start Version, or else you'll cause an infinite loop, you dummy!")
    End If

  End Sub

  Private Shared Sub IncrementVersion(ByVal versionToIncrement As Version, ByVal stepMode As VersionStepMode, ByVal stepLimit As Int32)

    Dim newMajor As Int32 = versionToIncrement.Major
    Dim newMinor As Int32 = versionToIncrement.Minor
    Dim newBuild As Int32 = versionToIncrement.Build
    Dim newRevision As Int32 = versionToIncrement.Revision

    Select Case stepMode
      Case VersionStepMode.Major
        newMajor += 1

      Case VersionStepMode.Minor
        If newMinor = stepLimit Then
          newMajor += 1
          newMinor = 0
        Else
          newMinor += 1
        End If

      Case VersionStepMode.Build
        If newBuild = stepLimit Then
          newMinor += 1
          newBuild = 0
        Else
          newBuild += 1
        End If

      Case VersionStepMode.Revision
        If newRevision = stepLimit Then
          newBuild += 1
          newRevision = 0
        Else
          newRevision += 1
        End If

    End Select

    versionToIncrement = New Version(newMajor, newMinor, newBuild, newRevision)

  End Sub

End Class
