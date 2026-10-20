# Nâng cấp từ mã nguồn 0.1.0

1. Giải nén gói 0.2.0 vào một thư mục mới.
2. Cài .NET 10 SDK nếu máy chưa có, rồi chạy Build.cmd.
3. Chỉ dùng EXE mới khi build và các kiểm tra báo thành công.
4. EXE nằm tại artifacts/win-x64/TroLyMayTinh-win-x64.exe; bản ARM64 dùng Build.ps1 -Runtime win-arm64.
5. Dữ liệu thiết lập cũ tại %LOCALAPPDATA%/TroLyMayTinh được giữ lại và bổ sung trường yêu thích khi lưu lần tiếp theo.

Khi đưa lên GitHub, đặt nội dung bên trong thư mục dự án ở thư mục gốc repository, gồm cả .github. Chờ workflow thành công và xem artifact WPF-check-results để kiểm tra chữ và bố cục sáng/tối. Gói nguồn này chưa chứa EXE đã biên dịch.

Chức năng cập nhật qua GitHub chỉ hoạt động khi repository/bản phát hành và asset đã được chuẩn bị đúng như README. Không cần tạo Release chỉ để build hoặc thử giao diện.
