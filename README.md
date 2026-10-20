<p align="center"><img src="assets/logo.png" width="96" alt="Logo Trợ Lý Máy Tính"></p>

# Trợ Lý Máy Tính · v0.2.0

**Chữ rõ hơn, thông tin máy đầy đủ hơn**

Ứng dụng C# / WPF tập trung công cụ hỗ trợ Windows, giúp cài phần mềm, truy cập tiện ích và quản lý các thao tác thường dùng. **36 ứng dụng · 8 nhóm** trong phiên bản này. Giao diện **Tiếng Việt / English**, **Sáng / Tối**.

**Tác giả: Triết Võ.**

![Minh họa tính năng v0.2.0](assets/overview.png)

*Ảnh minh họa tính năng, không phải ảnh chụp ứng dụng đang chạy. Logo dùng thống nhất cho bộ tài liệu.*

## Tải ứng dụng

Tải `TroLyMayTinh-win-x64.exe` trong **Assets** dành cho Windows x64 rồi mở để sử dụng. Nếu Release có bản ARM64, chọn `TroLyMayTinh-win-arm64.exe` đúng kiến trúc máy.

**Tải EXE và sử dụng trực tiếp, không cần tự build, Visual Studio hay .NET SDK.** Bản self-contained đã kèm runtime .NET phục vụ app. Dung lượng/khả năng dùng công cụ bên ngoài phụ thuộc chức năng và cấu hình máy.

## Chức năng và thay đổi

| Hạng mục | Nội dung |
| --- | --- |
| **Sửa màu chữ** | Bỏ setter Foreground gây ghi đè màu chữ nút/điều hướng; bổ sung template ComboBox và màu popup. |
| **Tổng quan phần cứng** | CPU/nhân/luồng, RAM từng thanh, GPU/driver, máy/bo mạch/BIOS, ổ đĩa, phân vùng và cấu hình mạng; có xuất TXT. |
| **Kho phần mềm mở rộng** | 36 mục thuộc 8 nhóm, tìm kiếm và bộ chọn văn phòng/lập trình; cài gói qua WinGet theo lựa chọn. |
| **Truy cập Windows** | 38 trang thiết lập, 21 công cụ hệ thống, 12 tiện ích nhanh; tìm kiếm và yêu thích đồng bộ theo ID. |
| **Tiện ích thường dùng** | Báo cáo pin, thông tin mạng, SHA-256, mở thư mục, chụp màn hình và công cụ trợ năng. |
| **Mở bộ cài Windows** | Mở trang tải chính thức và kiểm tra cấu trúc media/chữ ký Microsoft trước khi chạy setup.exe do người dùng chọn. |

## Sử dụng nhanh

1. Làm mới Tổng quan để đọc cấu hình, xem rõ mục nào không lấy được dữ liệu.
2. Tìm và chọn gói trong kho, hoặc lưu công cụ thường dùng vào Yêu thích.
3. Gắn ISO bằng Windows rồi chọn setup.exe nếu cần mở Windows Setup.

Cài phần mềm qua WinGet cần App Installer/WinGet hoạt động và kết nối mạng. Chỉ xác nhận những gói hoặc thay đổi bạn muốn thực hiện; xem kết quả và nhật ký sau khi thao tác. Các chức năng cần quản trị có thể hiện yêu cầu UAC.

## Lưu ý

- Windows Setup chỉ mở bộ cài đã kiểm tra; chưa cài lại Windows giống WinToHDD, chia phân vùng hoặc ghi boot. Người dùng tự tải và gắn ISO.
- Thông tin phần cứng có thể thiếu tùy quyền truy cập, thiết bị và nguồn dữ liệu Windows.
- Dọn file tạm chỉ thực hiện sau xác nhận; file đã xóa có thể không khôi phục được.
- Nhận diện phần mềm qua registry có thể thiếu bản portable hoặc bản theo tài khoản khác.
- Không kèm ISO, bộ cài hoặc bản quyền phần mềm bên thứ ba. Free/trial/commercial tuân theo điều khoản của nhà phát hành.

## Cập nhật và hỗ trợ

Ứng dụng kiểm tra phiên bản qua GitHub Releases theo SemVer. Cùng hoặc thấp hơn phiên bản đang dùng không được coi là bản cập nhật. Việc tải/cài cần xác nhận và đúng asset kiến trúc; không truy cập được nguồn cập nhật không đồng nghĩa đã dùng bản mới nhất.

Khi báo lỗi, ghi **v0.2.0**, Windows/build, kiến trúc và bước tái hiện. Xem lại log/báo cáo trước khi gửi vì có thể chứa thông tin máy hoặc đường dẫn cá nhân.

Xem [Release notes của bản này](RELEASE_NOTES.md) để sao chép nội dung khi tạo Release.

## Tác giả và giấy phép

**Triết Võ** — Trợ Lý Máy Tính. Điều khoản mã nguồn được quyết định bởi chủ dự án; bộ tài liệu này không tự cấp thêm giấy phép. Các thành phần tích hợp và phần mềm bên thứ ba giữ giấy phép/ghi công riêng.
