Imports System.Data.SqlClient

Public Class FormSales
    Private Sub FormSales_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadCustomers()
        LoadProductsToGrid()
    End Sub

    Private Sub LoadCustomers()
        Try
            OpenConnection()
            Dim sql As String = "SELECT CustomerID, CustomerCode + ' - ' + FullName AS Display FROM Customers"
            Dim cmd As New SqlCommand(sql, Conn)
            Dim adapter As New SqlDataAdapter(cmd)
            Dim dt As New DataTable()
            adapter.Fill(dt)
            cmbCustomers.DataSource = dt
            cmbCustomers.ValueMember = "CustomerID"
            cmbCustomers.DisplayMember = "Display"
        Catch ex As Exception
            MessageBox.Show("Lỗi: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub LoadProductsToGrid()
        Try
            OpenConnection()
            Dim sql As String = "SELECT ProductID, ProductCode, ProductName, UnitPrice, QuantityInStock FROM Products"
            Dim cmd As New SqlCommand(sql, Conn)
            Dim adapter As New SqlDataAdapter(cmd)
            Dim dt As New DataTable()
            adapter.Fill(dt)
            dgvProducts.DataSource = dt
            dgvProducts.Columns("ProductID").Visible = False
            dgvProducts.Columns("ProductCode").Visible = False
        Catch ex As Exception
            MessageBox.Show("Lỗi: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub

    Private Sub btnAddItem_Click(sender As Object, e As EventArgs) Handles btnAddItem.Click
        If dgvProducts.CurrentRow Is Nothing Then Return
        Dim pid = Convert.ToInt32(dgvProducts.CurrentRow.Cells("ProductID").Value)
        Dim pname = dgvProducts.CurrentRow.Cells("ProductName").Value.ToString()
        Dim price = Convert.ToDecimal(dgvProducts.CurrentRow.Cells("UnitPrice").Value)
        Dim stock = Convert.ToInt32(dgvProducts.CurrentRow.Cells("QuantityInStock").Value)

        ' Kiểm tra đã có trong danh sách chưa
        For Each row As DataGridViewRow In dgvInvoiceItems.Rows
            If Convert.ToInt32(row.Cells("ProductID").Value) = pid Then
                MessageBox.Show("Sản phẩm đã có trong hóa đơn. Thay đổi số lượng nếu muốn.")
                Return
            End If
        Next

        ' Thêm với số lượng mặc định 1
        Dim idx As Integer = dgvInvoiceItems.Rows.Add(pid, pname, 1, price, price * 1)
        UpdateTotal()
    End Sub

    Private Sub dgvInvoiceItems_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvInvoiceItems.CellValueChanged
        If e.RowIndex < 0 Then Return
        If dgvInvoiceItems.Columns(e.ColumnIndex).Name = "Quantity" Then
            Dim qty = Convert.ToInt32(dgvInvoiceItems.Rows(e.RowIndex).Cells("Quantity").Value)
            Dim price = Convert.ToDecimal(dgvInvoiceItems.Rows(e.RowIndex).Cells("UnitPrice").Value)
            dgvInvoiceItems.Rows(e.RowIndex).Cells("LineTotal").Value = qty * price
            UpdateTotal()
        End If
    End Sub

    Private Sub UpdateTotal()
        Dim total As Decimal = 0
        For Each r As DataGridViewRow In dgvInvoiceItems.Rows
            total += Convert.ToDecimal(r.Cells("LineTotal").Value)
        Next
        lblTotal.Text = total.ToString("N0")
    End Sub

    Private Sub btnSaveInvoice_Click(sender As Object, e As EventArgs) Handles btnSaveInvoice.Click
        If dgvInvoiceItems.Rows.Count = 0 Then
            MessageBox.Show("Chưa có sản phẩm trong hóa đơn.")
            Return
        End If

        Dim customerId As Integer = If(cmbCustomers.SelectedValue IsNot Nothing, Convert.ToInt32(cmbCustomers.SelectedValue), DBNull.Value)
        Dim invoiceNumber As String = "INV" & DateTime.Now.ToString("yyyyMMddHHmmss")
        Dim totalAmount As Decimal = 0
        For Each r As DataGridViewRow In dgvInvoiceItems.Rows
            totalAmount += Convert.ToDecimal(r.Cells("LineTotal").Value)
        Next

        Try
            OpenConnection()
            Dim tran As SqlTransaction = Conn.BeginTransaction()

            ' Insert Invoice
            Dim sqlInv As String = "INSERT INTO Invoices (InvoiceNumber, CustomerID, InvoiceDate, TotalAmount) VALUES (@num,@cust,@date,@total); SELECT SCOPE_IDENTITY();"
            Dim cmdInv As New SqlCommand(sqlInv, Conn, tran)
            cmdInv.Parameters.AddWithValue("@num", invoiceNumber)
            If TypeOf customerId Is Integer Then
                cmdInv.Parameters.AddWithValue("@cust", customerId)
            Else
                cmdInv.Parameters.AddWithValue("@cust", DBNull.Value)
            End If
            cmdInv.Parameters.AddWithValue("@date", DateTime.Now)
            cmdInv.Parameters.AddWithValue("@total", totalAmount)
            Dim invoiceIdObj = cmdInv.ExecuteScalar()
            Dim invoiceId As Integer = Convert.ToInt32(invoiceIdObj)

            ' Insert items và cập nhật tồn
            Dim sqlItem As String = "INSERT INTO InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice) VALUES (@inv,@prod,@qty,@price)"
            Dim sqlUpdateStock As String = "UPDATE Products SET QuantityInStock = QuantityInStock - @qty WHERE ProductID=@prod AND QuantityInStock >= @qty"
            For Each r As DataGridViewRow In dgvInvoiceItems.Rows
                Dim prodId = Convert.ToInt32(r.Cells("ProductID").Value)
                Dim qty = Convert.ToInt32(r.Cells("Quantity").Value)
                Dim price = Convert.ToDecimal(r.Cells("UnitPrice").Value)

                ' Cập nhật kiểm tra tồn kho (nếu không đủ -> rollback)
                Dim cmdUpdStock As New SqlCommand(sqlUpdateStock, Conn, tran)
                cmdUpdStock.Parameters.AddWithValue("@qty", qty)
                cmdUpdStock.Parameters.AddWithValue("@prod", prodId)
                Dim rowsAffected = cmdUpdStock.ExecuteNonQuery()
                If rowsAffected = 0 Then
                    tran.Rollback()
                    MessageBox.Show($"Không đủ tồn kho cho sản phẩm ID {prodId}. Giao dịch bị huỷ.")
                    Return
                End If

                Dim cmdItem As New SqlCommand(sqlItem, Conn, tran)
                cmdItem.Parameters.AddWithValue("@inv", invoiceId)
                cmdItem.Parameters.AddWithValue("@prod", prodId)
                cmdItem.Parameters.AddWithValue("@qty", qty)
                cmdItem.Parameters.AddWithValue("@price", price)
                cmdItem.ExecuteNonQuery()
            Next

            tran.Commit()
            MessageBox.Show("Lưu hóa đơn thành công! Số hóa đơn: " & invoiceNumber)
            dgvInvoiceItems.Rows.Clear()
            UpdateTotal()
            LoadProductsToGrid()
        Catch ex As Exception
            MessageBox.Show("Lỗi khi lưu: " & ex.Message)
        Finally
            CloseConnection()
        End Try
    End Sub
End Class
