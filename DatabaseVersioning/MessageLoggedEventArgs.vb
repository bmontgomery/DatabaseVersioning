Public Class MessageLoggedEventArgs

  Private mMessage As String
  Public Property Message() As String
    Get
      Return mMessage
    End Get
    Set(ByVal value As String)
      mMessage = value
    End Set
  End Property

  Private mDateLogged As DateTime
  Public Property DateLogged() As DateTime
    Get
      Return mDateLogged
    End Get
    Set(ByVal value As DateTime)
      mDateLogged = value
    End Set
  End Property

End Class
