# Tình trạng kiểm tra

Ngày chuẩn bị mã nguồn: 19/10/2026.

## Đã thực hiện trong môi trường tạo dự án

- Kiểm tra XML cho XAML, csproj và application manifest.
- Đối chiếu khóa bản dịch được sử dụng trong XAML với danh sách bản dịch.
- Đối chiếu binding ở cấp ViewModel, sự kiện và các tham chiếu dự án.
- Kiểm tra cấu trúc workflow YAML, danh mục JSON và gói ZIP.
- Kiểm tra số lượng danh mục và tương phản bảng màu; đối chiếu 36 package-ID với thư mục kho microsoft/winget-pkgs.
- Rà soát luồng dọn file tạm, lưu/khôi phục registry, so sánh phiên bản và xác minh gói cập nhật bằng cách đọc mã.

Chi tiết kiểm tra tĩnh được ghi trong STATIC_CHECKS.txt đi kèm. Các kiểm tra này không thay thế trình biên dịch hoặc chạy chương trình.

## Chưa được thực hiện

- Biên dịch C# và WPF: môi trường hiện tại không có .NET SDK.
- Chạy Toolkit.Checks, smoke test WPF, xác nhận sửa lỗi chữ và xem giao diện thật trên Windows.
- Kiểm thử dữ liệu CIM trên máy thật và chức năng mở bộ cài Microsoft.
- Chạy cài phần mềm, xóa file tạm, sửa registry hoặc cập nhật thực tế trên Windows.
- Kiểm thử ARM64, hiển thị DPI cao và cơ chế cập nhật khi có lỗi.

Không khẳng định phần mềm không có lỗi hoặc đã sẵn sàng dùng trên máy thật. Build.ps1 và workflow Windows đã được chuẩn bị để chạy các kiểm tra tự động; kết quả chỉ có ý nghĩa sau khi chúng được thực thi thành công. Checklist kiểm thử thủ công nằm trong MANUAL_TESTS.md.
