Imports System.Data.SqlClient

Module ModuleDB
    Public Conn As SqlConnection
    ' Nếu bạn dùng Docker trên Mac/host, chuỗi kết nối từ app Windows (trên cùng máy hoặc khác) có thể là:
    ' Server=localhost,1433;Database=SalesDB;User Id=sa;Password=Your_password123;
    Public ConnString As String = "Server=localhost,1433;Database=SalesDB;User Id=sa;Password=Your_password123;"

    Public Sub OpenConnection()
        If Conn Is Nothing Then Conn = New SqlConnection(ConnString)
        If Conn.State = ConnectionState.Closed Then Conn.Open()
    End Sub

    Public Sub CloseConnection()
        Try
            If Conn IsNot Nothing AndAlso Conn.State = ConnectionState.Open Then Conn.Close()
        Catch ex As Exception
            ' ignore
        End Try
    End Sub
End Module
