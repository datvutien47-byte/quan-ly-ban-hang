# quan-ly-ban-hang
Quản lý bán hàng VB.NET
# SalesManagement
Ứng dụng Quản lý bán hàng (VB.NET WinForms) kết nối SQL Server.

## Yêu cầu
- Visual Studio (Windows) hoặc Visual Studio for Mac (nên dùng Windows để chạy WinForms)
- SQL Server (hoặc SQL Server Express)
- .NET Framework (tuỳ version project)

## Cài đặt
1. Tạo database: chạy file `database/SalesDB.sql` (script tạo bảng + dữ liệu mẫu) trên SQL Server.
2. Mở solution `SalesManagement.sln` trong Visual Studio.
3. Chỉnh `ModuleDB.vb` chuỗi kết nối `ConnString`.
4. Build & Run.

## Chức năng
- Quản lý Sản phẩm (thêm/sửa/xóa)
- Quản lý Khách hàng (thêm/sửa/xóa)
- Tạo Hóa đơn (thêm nhiều sản phẩm, lưu hóa đơn, cập nhật tồn kho)

## Hướng dẫn nộp
- Tạo repo trên GitHub, push toàn bộ source + file SQL.
