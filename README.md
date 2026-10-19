# Trợ Lý Máy Tính

**Bộ công cụ tất cả trong một cho Windows — cài phần mềm, tinh chỉnh, dọn dẹp, sửa lỗi và truy cập nhanh các tiện ích.**

> Trạng thái: đang lập kế hoạch. Tài liệu này mô tả định hướng dự án; repository GitHub chưa được tạo; chưa có mã nguồn ứng dụng hoặc bản cài đặt. Toàn bộ chức năng bên dưới là dự kiến.

## Giới thiệu

Trợ Lý Máy Tính hướng tới một ứng dụng C# dành cho người dùng Việt Nam và quốc tế, hỗ trợ giao diện tiếng Việt và tiếng Anh. Ứng dụng dễ dùng cho người dùng phổ thông và có chế độ nâng cao dành cho kỹ thuật viên. Ứng dụng hỗ trợ thiết lập máy mới, bảo trì Windows và tổ chức các công cụ thường dùng.

- **Tên ứng dụng:** Trợ Lý Máy Tính
- **Tên repository dự kiến:** `tro-ly-may-tinh`
- **Ngôn ngữ README:** Tiếng Việt
- **Ngôn ngữ giao diện dự kiến:** Tiếng Việt và tiếng Anh
- **Tác giả khởi xướng:** Triết Võ
- **Tài khoản GitHub:** [trietpko2002](https://github.com/trietpko2002)
- **Công nghệ dự kiến:** C#, WPF, .NET, MVVM
- **Nền tảng mục tiêu:** Windows 11; các phiên bản Windows khác sẽ được xác định sau khi kiểm thử.

## Các mô-đun dự kiến

| Mô-đun | Chức năng |
| --- | --- |
| Tổng quan | Cấu hình máy, phiên bản Windows, dung lượng ổ đĩa và trạng thái hệ thống |
| Phần mềm | Cài theo danh mục, cài hàng loạt, kiểm tra phiên bản, cập nhật và gỡ phần mềm |
| Ngôn ngữ và bộ gõ | Tiện ích theo ngôn ngữ; với tiếng Việt, hỗ trợ cài bộ gõ và hướng dẫn Telex/VNI, Unicode |
| Thiết lập máy mới | Chọn bộ phần mềm và cấu hình theo nhu cầu |
| Tinh chỉnh | Explorer, thanh tác vụ, giao diện, thông báo, nguồn điện và khởi động |
| Dọn dẹp | Quét file tạm, cache được hỗ trợ, thùng rác; xem trước trước khi xóa |
| Phân tích dung lượng | Tìm file lớn, thư mục lớn và file trùng |
| Sửa chữa | Tích hợp công cụ kiểm tra, sửa lỗi hệ thống và mạng |
| Driver | Xem thông tin thiết bị, sao lưu và khôi phục driver |
| Sao lưu | Lưu cấu hình, lịch sử thay đổi và hỗ trợ khôi phục |
| Windows | Chọn ISO, đọc phiên bản và chuẩn bị triển khai Windows |
| AIO | Truy cập công cụ Windows, tiện ích portable, tìm kiếm và yêu thích |
| Tự động hóa | Lưu quy trình cài đặt và bảo trì để sử dụng lại |
| Nhật ký | Tiến trình, kết quả, lỗi và báo cáo thao tác |

## Giao diện và trải nghiệm

- Cho phép chọn tiếng Việt hoặc tiếng Anh trong Cài đặt; lưu lựa chọn để dùng cho các lần mở sau.
- Nội dung dễ hiểu, hiển thị đầy đủ dấu tiếng Việt và hỗ trợ thay đổi kích thước màn hình.
- Chế độ sáng/tối và điều hướng bằng bàn phím.
- Trang chủ có các thao tác thường dùng và ô tìm kiếm.
- Chế độ cơ bản và chế độ kỹ thuật viên.
- Mỗi phần mềm hiển thị biểu tượng, mô tả, nhà phát hành và phiên bản đã cài.
- Các nút Cài đặt, Mở và Cập nhật thay đổi theo trạng thái thực tế.
- Trang Giới thiệu hiển thị tác giả, phiên bản, liên kết hỗ trợ và giấy phép thành phần.

## Thiết kế cập nhật GitHub

Bộ cập nhật ứng dụng sẽ đọc GitHub Releases của repository chính thức. Đây là chức năng dự kiến, chưa được triển khai.

1. So sánh phiên bản theo Semantic Versioning.
2. Chỉ đề nghị cập nhật khi bản phát hành phù hợp mới hơn bản đang chạy.
3. Phiên bản bằng hoặc thấp hơn không hiện lời mời cập nhật và không tự hạ phiên bản.
4. Phân biệt kênh ổn định và thử nghiệm; bỏ qua bản nháp.
5. Chọn đúng kiến trúc, kiểu đóng gói và yêu cầu hệ điều hành.
6. Không có gói tương thích thì không tự tải.
7. Khi lỗi mạng hoặc giới hạn API, hiển thị chưa thể kiểm tra; không khẳng định đang dùng bản mới nhất.
8. Xác minh tính toàn vẹn và nguồn phát hành của gói cập nhật trước khi thực thi.
9. Hỗ trợ thông báo, tự tải hoặc cài khi đóng app theo lựa chọn người dùng.
10. Nếu cập nhật thất bại, giữ bản đang dùng khi có thể và ghi nhật ký.

Cập nhật phần mềm bên thứ ba dùng nguồn phù hợp với từng phần mềm, độc lập với bộ cập nhật Trợ Lý Máy Tính. Không lấy phiên bản của app để suy ra phiên bản của các phần mềm khác.

## Nguyên tắc thao tác hệ thống

- Chỉ yêu cầu quyền quản trị khi tác vụ cần.
- Tinh chỉnh lưu giá trị trước đó để hoàn tác khi khả thi.
- Dọn dẹp hiển thị phạm vi và danh sách trước khi xóa.
- Thao tác phân vùng, boot và cài Windows phải có bước xem trước, xác nhận rõ ràng và kiểm thử riêng.
- Không tự tắt cơ chế bảo vệ hệ thống hoặc chạy tối ưu không có mô tả.
- Không đóng gói bộ cài, bộ gõ hoặc tiện ích bên thứ ba khi chưa đáp ứng giấy phép phân phối.
- Chỉ dùng nguồn tải chính thức hoặc nguồn tin cậy đã được xác minh.
- Không cung cấp công cụ vượt bản quyền hay bộ cài Windows chỉnh sửa không rõ nguồn.

## Lộ trình

### Giai đoạn 1 — Nền tảng

- [ ] Khởi tạo dự án C# WPF và cấu trúc MVVM.
- [ ] Giao diện tiếng Việt và tiếng Anh, bộ chọn ngôn ngữ, điều hướng và trang Giới thiệu.
- [ ] Tổng quan máy và kho công cụ truy cập nhanh.
- [ ] Danh mục phần mềm, bộ gõ và hàng đợi cài đặt.
- [ ] Kiểm tra cập nhật GitHub theo đúng phiên bản và kiến trúc.
- [ ] Nhật ký, tiến trình và xử lý lỗi.

### Giai đoạn 2 — Bảo trì

- [ ] Dọn dẹp có xem trước.
- [ ] Tinh chỉnh có hoàn tác.
- [ ] Công cụ sửa chữa Windows và mạng.
- [ ] Sao lưu cấu hình và driver.
- [ ] Quy trình thiết lập máy mới.

### Giai đoạn 3 — Triển khai

- [ ] Chọn ISO và đọc các bản Windows bên trong.
- [ ] Kiểm tra điều kiện đĩa, phân vùng và khởi động.
- [ ] Thiết kế môi trường triển khai và phương án phục hồi.
- [ ] Kiểm thử cài Windows trên máy ảo trước khi hỗ trợ máy thật.
- [ ] Tự động hóa và xuất báo cáo.

## Tải xuống và phát triển

Chưa có bản tải xuống hoặc hướng dẫn build vì mã nguồn chưa được khởi tạo. Hướng dẫn sẽ được bổ sung cùng dự án chạy được. Không coi các mục trong lộ trình là chức năng đã hoàn thành.

## Đóng góp và phản hồi

Khi repository được tạo, có thể dùng Issues để báo lỗi hoặc đề xuất tính năng. Khi báo lỗi, ghi phiên bản app, phiên bản Windows, bước tái hiện và nhật ký đã loại bỏ dữ liệu cá nhân.

## Giấy phép

Chưa chọn giấy phép cho dự án. Việc công khai repository không đồng nghĩa với cấp quyền sử dụng, sửa đổi hoặc phân phối mã nguồn. Giấy phép và thông báo thư viện bên thứ ba sẽ được bổ sung trước khi phát hành.

