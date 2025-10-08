Imports System.Data.SqlClient

Public Class FormProducts
    Private Sub FormProducts_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadProducts()
    End Sub

    Private Sub LoadProducts()
        Try
            OpenConnection()
            Dim sql As String = "SELECT ProductID, ProductCode, ProductName, Category, UnitPrice, QuantityInStock FROM Products"
            Dim cmd As New SqlCommand(sql, Conn)
            Dim adapter As New SqlDataAdapter(cmd)
            Dim dt As New DataTable()
            adapter.Fill(dt)
            dgvProducts.DataSource = dt
            dgvProducts.Columns("ProductID").Visible = False
        Catch ex As Exception
            MessageBox.Show("Lỗi khi tải sản phẩm: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub ClearInputs()
        txtCode.Clear()
        txtName.Clear()
        txtCategory.Clear()
        txtPrice.Clear()
        txtQty.Clear()
    End Sub

    Private Function ValidateInputs() As Boolean
        If String.IsNullOrWhiteSpace(txtCode.Text) OrElse String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Mã và tên sản phẩm không được để trống.")
            Return False
        End If
        If Not Decimal.TryParse(txtPrice.Text, New Decimal()) Then
            MessageBox.Show("Giá không hợp lệ.")
            Return False
        End If
        If Not Integer.TryParse(txtQty.Text, New Integer()) Then
            MessageBox.Show("Số lượng không hợp lệ.")
            Return False
        End If
        Return True
    End Function

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If Not ValidateInputs() Then Return
        Try
            OpenConnection()
            Dim sql As String = "INSERT INTO Products (ProductCode, ProductName, Category, UnitPrice, QuantityInStock) VALUES (@code,@name,@cat,@price,@qty)"
            Dim cmd As New SqlCommand(sql, Conn)
            cmd.Parameters.AddWithValue("@code", txtCode.Text.Trim())
            cmd.Parameters.AddWithValue("@name", txtName.Text.Trim())
            cmd.Parameters.AddWithValue("@cat", txtCategory.Text.Trim())
            cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtPrice.Text))
            cmd.Parameters.AddWithValue("@qty", Convert.ToInt32(txtQty.Text))
            cmd.ExecuteNonQuery()
            MessageBox.Show("Thêm thành công")
            LoadProducts()
            ClearInputs()
        Catch ex As Exception
            MessageBox.Show("Lỗi thêm: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub dgvProducts_SelectionChanged(sender As Object, e As EventArgs) Handles dgvProducts.SelectionChanged
        If dgvProducts.CurrentRow Is Nothing Then Return
        txtCode.Text = dgvProducts.CurrentRow.Cells("ProductCode").Value.ToString()
        txtName.Text = dgvProducts.CurrentRow.Cells("ProductName").Value.ToString()
        txtCategory.Text = If(dgvProducts.CurrentRow.Cells("Category").Value IsNot Nothing, dgvProducts.CurrentRow.Cells("Category").Value.ToString(), "")
        txtPrice.Text = dgvProducts.CurrentRow.Cells("UnitPrice").Value.ToString()
        txtQty.Text = dgvProducts.CurrentRow.Cells("QuantityInStock").Value.ToString()
    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If dgvProducts.CurrentRow Is Nothing Then Return
        If Not ValidateInputs() Then Return
        Dim id = Convert.ToInt32(dgvProducts.CurrentRow.Cells("ProductID").Value)
        Try
            OpenConnection()
            Dim sql As String = "UPDATE Products SET ProductCode=@code, ProductName=@name, Category=@cat, UnitPrice=@price, QuantityInStock=@qty WHERE ProductID=@id"
            Dim cmd As New SqlCommand(sql, Conn)
            cmd.Parameters.AddWithValue("@code", txtCode.Text.Trim())
            cmd.Parameters.AddWithValue("@name", txtName.Text.Trim())
            cmd.Parameters.AddWithValue("@cat", txtCategory.Text.Trim())
            cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtPrice.Text))
            cmd.Parameters.AddWithValue("@qty", Convert.ToInt32(txtQty.Text))
            cmd.Parameters.AddWithValue("@id", id)
            cmd.ExecuteNonQuery()
            MessageBox.Show("Cập nhật thành công")
            LoadProducts()
        Catch ex As Exception
            MessageBox.Show("Lỗi cập nhật: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If dgvProducts.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Bạn có chắc muốn xóa sản phẩm?", "Xóa", MessageBoxButtons.YesNo) <> DialogResult.Yes Then Return
        Dim id = Convert.ToInt32(dgvProducts.CurrentRow.Cells("ProductID").Value)
        Try
            OpenConnection()
            Dim sql As String = "DELETE FROM Products WHERE ProductID=@id"
            Dim cmd As New SqlCommand(sql, Conn)
            cmd.Parameters.AddWithValue("@id", id)
            cmd.ExecuteNonQuery()
            MessageBox.Show("Xóa thành công")
            LoadProducts()
            ClearInputs()
        Catch ex As Exception
            MessageBox.Show("Lỗi xóa: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        ClearInputs()
    End Sub
End Class
