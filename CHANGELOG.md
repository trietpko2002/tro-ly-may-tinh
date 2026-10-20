# Thay đổi

## 0.2.0 — 19/10/2026

### Sửa giao diện

- Bỏ setter Foreground dùng chung trên TextBlock đã ghi đè màu chữ của nút và thanh điều hướng.
- Đặt màu chữ mặc định ở cửa sổ và kế thừa màu từ từng control.
- Tách màu nền nút khỏi màu nhấn trên giao diện tối.
- Thêm template ComboBox/ComboBoxItem để kiểm soát màu danh mục và popup.
- Kiểm tra tĩnh tương phản các cặp màu chữ chính đạt ít nhất 4,5:1 trên nền màu đặc. Chưa xác nhận bằng chạy giao diện thật.
- Bổ sung kiểm tra Windows về kế thừa màu chữ và xuất ảnh các trang chính trong workflow.

### Tổng quan

- CPU, số nhân/luồng, xung khai báo, RAM tổng/khả dụng/từng thanh, GPU và driver.
- Hãng/dòng máy, bo mạch chủ, BIOS, Windows và lần khởi động gần nhất.
- Ổ vật lý, dung lượng phân vùng và kết nối mạng/IP/gateway/DNS.
- Truy vấn nền có thời gian chờ, hiển thị rõ nguồn dữ liệu không đọc được và xuất báo cáo TXT khi yêu cầu.

### Công cụ và phần mềm

- 36 phần mềm, 8 nhóm, bộ chọn văn phòng/lập trình và tìm kiếm.
- Đối chiếu 36 package-ID với thư mục kho microsoft/winget-pkgs.
- 38 trang thiết lập Windows, 21 công cụ hệ thống và 12 tiện ích nhanh.
- Tìm kiếm, lưu yêu thích và đồng bộ công cụ trùng ID giữa các trang.
- Báo cáo pin, thông tin mạng, tính SHA-256, mở thư mục, chụp màn hình và công cụ trợ năng.
- Mở trang tải Windows và xác minh chữ ký Microsoft trước khi chạy setup.exe từ bộ cài.

### Chưa có

- Cài lại Windows theo cơ chế WinToHDD, chia phân vùng hoặc ghi boot.
- Trình cài MSI/MSIX và chữ ký số phát hành.
- Kết quả biên dịch/chạy Windows trong môi trường chuẩn bị mã nguồn này; xem VALIDATION.md.
