# Checklist kiểm tra trên Windows

Chỉ đánh dấu sau khi thực sự chạy. Không chạy thao tác dọn dẹp hoặc cập nhật trên dữ liệu quan trọng trong lần thử đầu.

## Build và giao diện

- [ ] Build.cmd kết thúc thành công; Toolkit.Checks báo PASS; smoke-results.txt báo PASS.
- [ ] Mở EXE x64 trên Windows 11 x64; thử ARM64 trên máy/VM ARM64 riêng.
- [ ] Xem đủ 12 trang, đổi Việt/Anh, sáng/tối và mở lại app để kiểm tra lưu lựa chọn.
- [ ] Kiểm tra cỡ hiển thị 100%, 125%, 150%, kích thước tối thiểu và dùng bàn phím Tab/Space/Enter.
- [ ] Tìm phần mềm bằng tên, mô tả, ID và từ khóa không có kết quả.
- [ ] Đường dẫn dự án chứa dấu tiếng Việt, khoảng trắng hoặc dấu nháy vẫn build và cập nhật được.
- [ ] Mất mạng khi khởi động không khóa giao diện và không báo sai là phiên bản mới nhất.

## Phần mềm và công cụ

- [ ] Thiếu WinGet: thông báo hướng dẫn và mở trang App Installer.
- [ ] Cài một ứng dụng, cài nhiều ứng dụng; từ chối UAC; gói đã cài; mất mạng; ID gói không tồn tại.
- [ ] Kiểm tra giấy phép được thông báo trước khi chấp nhận; kết quả không báo thành công khi WinGet trả mã khác 0.
- [ ] Nhận diện đúng các ứng dụng có đăng ký gỡ cài đặt; không khẳng định nhận diện đầy đủ portable.
- [ ] Mở 21 công cụ; công cụ bị chặn phải báo lỗi thay vì đóng app.

## Dọn dẹp

- [ ] Tạo file thử trong LocalAppData/Temp có tuổi 1 ngày, đúng 7 ngày và 8 ngày; chỉ file hơn 7 ngày được liệt kê.
- [ ] Không file nào tự được chọn sau khi quét.
- [ ] Chỉ file đã chọn bị xóa sau khi xác nhận; chọn No không xóa.
- [ ] Sửa file sau quét, khóa file, đổi quyền: phải bỏ qua.
- [ ] Thử symlink/junction đến thư mục ngoài: không đi theo liên kết.
- [ ] Dừng quét/dọn dẹp: không báo hoàn tất toàn bộ; các file đã xóa không quay lại trong danh sách.
- [ ] Documents/Downloads không bị quét; không tự xóa thư mục.

## Tinh chỉnh

- [ ] Ghi nhận giá trị ban đầu trước khi thử. Áp dụng từng tổ hợp hai lựa chọn, kiểm tra registry và Explorer.
- [ ] Đóng/mở app rồi khôi phục, gồm trường hợp giá trị ban đầu không tồn tại.
- [ ] Từ chối xác nhận thì không thay đổi registry.

## Cập nhật (máy ảo / repository thử nghiệm)

- [ ] Chạy EXE portable 0.2.0; thử release 0.2.0 hoặc thấp hơn: không đề nghị cập nhật.
- [ ] Thử 0.3.0 ổn định, đúng kiến trúc và có digest: có thể tải.
- [ ] Thử draft/prerelease: không mời cập nhật.
- [ ] Thử 1.10.0 so với 1.9.0 để xác nhận so sánh số.
- [ ] Không có đúng asset, không có digest, sai SHA-256, sai PE/kiến trúc, sai product/version: không áp dụng.
- [ ] Repository riêng tư/không tồn tại, HTTP 403/429, mất mạng: trạng thái chưa kiểm tra được.
- [ ] Chạy từ build debug hoặc thư mục không có quyền ghi: từ chối cập nhật với lý do.
- [ ] Xác nhận cập nhật: app đóng, EXE mới được thay, giữ .previous và mở lại đúng phiên bản.
- [ ] Mô phỏng file EXE bị khóa, helper bị chính sách chặn, hết dung lượng và app không chịu đóng; kiểm tra nhật ký và file cũ.
- [ ] Xác minh khả năng khôi phục thủ công từ .previous. Lỗi bên trong app mới sau khi đã khởi động không được xem là rollback tự động thành công.

## Trước khi phát hành

- [ ] Đọc VALIDATION.md và chạy đầy đủ các kiểm tra còn thiếu.
- [ ] Chọn giấy phép và kiểm tra điều khoản phần mềm bên thứ ba.
- [ ] Kiểm tra repository đích, phiên bản, tên asset và quyền truy cập.
- [ ] Ghi rõ các chức năng chưa có trong ghi chú phát hành.

## Kiểm tra bổ sung cho 0.2.0

- [ ] Chữ nút xanh, nút trắng ở hero, thanh menu, ComboBox, ô tìm kiếm và các thẻ đều đọc được ở sáng/tối.
- [ ] Mở ComboBox danh mục: chữ được chọn và các mục popup đọc được trong cả hai giao diện.
- [ ] Đọc CPU, RAM, GPU, BIOS, bo mạch, ổ đĩa và mạng; đối chiếu với msinfo32, Task Manager và Windows Settings.
- [ ] Mất quyền CIM, PowerShell bị chặn hoặc truy vấn quá hạn: app vẫn thao tác được và có trạng thái rõ.
- [ ] Xuất báo cáo chỉ khi người dùng chọn, mở lại tệp và đối chiếu dữ liệu; không có product key/số sê-ri.
- [ ] Đủ 36 phần mềm, 38 thiết lập, 21 công cụ và 12 tiện ích. Lọc danh mục, tìm kiếm và hai bộ chọn hoạt động.
- [ ] Ghim yêu thích rồi lọc; công cụ trùng ID ở hai trang đồng bộ; mở lại app vẫn lưu.
- [ ] Mỗi URI mở trang Windows đúng trên thiết bị có tính năng tương ứng; trang không hỗ trợ không làm app thoát.
- [ ] Tính SHA-256 và so sánh với Get-FileHash. Báo cáo pin: thử máy có pin và máy không có pin.
- [ ] Thư mục Downloads được chuyển sang ổ khác vẫn được mở đúng qua known-folder của Windows.
- [ ] Chọn setup.exe không phải bộ cài, thiếu ảnh sources, chữ ký không hợp lệ hoặc không phải Microsoft: phải từ chối.
- [ ] Bộ cài Microsoft hợp lệ: xác minh, chọn No không mở; chọn Yes chỉ mở Windows Setup, không truyền tham số cài im lặng.
- [ ] Kiểm tra ảnh sáng/tối trong artifact WPF-check-results sau khi workflow chạy; ảnh chỉ được tạo trên Windows, chưa có trong gói nguồn.
