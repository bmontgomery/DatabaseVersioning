Public Class VersionedScriptFile

  Private mVersion As Version
  Public Property Version() As Version
    Get
      Return mVersion
    End Get
    Set(ByVal value As Version)
      mVersion = value
    End Set
  End Property

  Private mFilePath As String
  Public Property FilePath() As String
    Get
      Return mFilePath
    End Get
    Set(ByVal value As String)
      mFilePath = value
    End Set
  End Property

  Public Sub New(ByVal version As Version, ByVal filePath As String)

    mVersion = version
    mFilePath = filePath

  End Sub

End Class
