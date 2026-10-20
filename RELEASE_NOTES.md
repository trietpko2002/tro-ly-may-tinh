# Trợ Lý Máy Tính v0.2.0 — Chữ rõ hơn, thông tin máy đầy đủ hơn

## Có gì trong phiên bản này?

- **Sửa màu chữ:** Bỏ setter Foreground gây ghi đè màu chữ nút/điều hướng; bổ sung template ComboBox và màu popup.
- **Tổng quan phần cứng:** CPU/nhân/luồng, RAM từng thanh, GPU/driver, máy/bo mạch/BIOS, ổ đĩa, phân vùng và cấu hình mạng; có xuất TXT.
- **Kho phần mềm mở rộng:** 36 mục thuộc 8 nhóm, tìm kiếm và bộ chọn văn phòng/lập trình; cài gói qua WinGet theo lựa chọn.
- **Truy cập Windows:** 38 trang thiết lập, 21 công cụ hệ thống, 12 tiện ích nhanh; tìm kiếm và yêu thích đồng bộ theo ID.
- **Tiện ích thường dùng:** Báo cáo pin, thông tin mạng, SHA-256, mở thư mục, chụp màn hình và công cụ trợ năng.
- **Mở bộ cài Windows:** Mở trang tải chính thức và kiểm tra cấu trúc media/chữ ký Microsoft trước khi chạy setup.exe do người dùng chọn.

## Tải và sử dụng

Tải `TroLyMayTinh-win-x64.exe` trong **Assets** dành cho Windows x64 rồi mở để sử dụng. Nếu Release có bản ARM64, chọn `TroLyMayTinh-win-arm64.exe` đúng kiến trúc máy.

**Đây là bản ứng dụng EXE để sử dụng trực tiếp. Không cần build, Visual Studio hay .NET SDK.** Bản EXE self-contained đã bao gồm runtime .NET cho ứng dụng; các công cụ ngoài như WinGet/QEMU vẫn có yêu cầu riêng theo chức năng.

## Lưu ý

- Windows Setup chỉ mở bộ cài đã kiểm tra; chưa cài lại Windows giống WinToHDD, chia phân vùng hoặc ghi boot. Người dùng tự tải và gắn ISO.
- Thông tin phần cứng có thể thiếu tùy quyền truy cập, thiết bị và nguồn dữ liệu Windows.
- Cài gói qua WinGet cần App Installer/WinGet hoạt động và kết nối mạng; một số thao tác yêu cầu quyền quản trị.
- Dọn file tạm có xác nhận trước khi xóa; file đã xóa có thể không khôi phục được.
- Không kèm ISO Windows hay bộ cài bên thứ ba. Điều khoản sử dụng thuộc từng nhà phát hành.

## Báo lỗi

Ghi phiên bản **v0.2.0**, Windows/build, kiến trúc máy, các bước tái hiện và thông báo lỗi. Xem lại dữ liệu cá nhân trong nhật ký trước khi chia sẻ.

**Tác giả: Triết Võ** · **Giao diện: Tiếng Việt / English · Sáng / Tối**
