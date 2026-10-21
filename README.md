# CafeManager - Hệ Thống Quản Lý Quán Cà Phê

Ứng dụng web quản lý quán cà phê được xây dựng trên nền tảng **ASP.NET Core MVC** kết hợp với **Entity Framework Core** và cơ sở dữ liệu **SQLite**.

---

## 1. Tính Năng Chính

- **Xác thực và phân quyền người dùng (Cookie Authentication):**
  - Quản lý (Manager): Toàn quyền thêm, sửa, xóa món ăn, quản lý sơ đồ bàn, thanh toán và xem báo cáo hóa đơn.
  - Thu ngân (Cashier): Mở bàn, gọi món, áp dụng giảm giá, xử lý thanh toán và in hóa đơn.
  - Nhân viên phục vụ (Staff): Xem sơ đồ bàn, tạo đơn và thêm món cho khách.
- **Sơ đồ bàn trực quan:** Quản lý trạng thái bàn theo thời gian thực (Bàn trống / Đang phục vụ).
- **Quản lý thực đơn (Menu):** Phân loại theo danh mục (Cà phê, Trà, Sinh tố, Bánh), tìm kiếm nhanh theo tên, bật/tắt trạng thái còn bán.
- **Quy trình bán hàng (Order & POS):**
  - Tạo đơn theo bàn hoặc mang đi.
  - Thêm, điều chỉnh số lượng hoặc xóa món khỏi đơn.
  - Tự động tính tổng tiền, chiết khấu giảm giá.
  - Hỗ trợ các phương thức thanh toán: Tiền mặt, Thẻ, Chuyển khoản ngân hàng.
- **In hóa đơn & Lịch sử:** Giao diện hóa đơn chuẩn in nhiệt/A5 và theo dõi toàn bộ lịch sử các đơn hàng đã thanh toán.

---

## 2. Công Nghệ Sử Dụng

- **Backend:** .NET (ASP.NET Core MVC)
- **Cơ sở dữ liệu:** SQLite thông qua Entity Framework Core
- **Bảo mật:** Cookie Authentication, mã hóa mật khẩu bằng BCrypt
- **Frontend:** Razor Pages (HTML5/CSS3), Bootstrap 5, Bootstrap Icons

---

## 3. Cấu Trúc Dự Án

```text
CafeManager/
├── Controllers/          # Điều hướng và xử lý logic nghiệp vụ
│   ├── AccountController.cs  # Đăng nhập, đăng xuất, kiểm tra quyền
│   ├── HomeController.cs     # Trang chủ điều hướng
│   ├── MenuController.cs     # Quản lý thực đơn món ăn
│   └── OrderController.cs    # Sơ đồ bàn, gọi món, thanh toán, hóa đơn
├── Data/                 # Cấu hình Entity Framework và DbContext
│   └── AppDbContext.cs
├── Models/               # Định nghĩa các bảng dữ liệu
│   ├── CafeTable.cs
│   ├── Category.cs
│   ├── MenuItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── User.cs
├── Views/                # Giao diện người dùng Razor View
│   ├── Account/
│   ├── Menu/
│   ├── Order/
│   └── Shared/
├── Program.cs            # Khởi tạo ứng dụng, middleware và seed dữ liệu ban đầu
├── CafeManager.csproj    # File cấu hình dự án và dependencies
└── .gitignore            # Loại trừ file build và database
```

---

## 4. Hướng Dẫn Cài Đặt và Khởi Chạy

### Yêu Cầu Môi Trường
- .NET SDK (phiên bản 8.0 trở lên)
- Git

### Các Bước Thực Hiện

1. **Clone repository về máy:**
   ```bash
   git clone https://github.com/nguyenngocthang236-dot/desingai.git
   cd desingai
   ```

2. **Khôi phục các gói phụ thuộc (Dependencies):**
   ```bash
   dotnet restore
   ```

3. **Khởi chạy ứng dụng:**
   ```bash
   dotnet run
   ```

4. **Truy cập hệ thống:**
   Mở trình duyệt web và điều hướng tới địa chỉ:
   ```text
   http://localhost:5000  hoặc  https://localhost:5001
   ```
   *Lưu ý: Cơ sở dữ liệu SQLite (`cafe.db`) và các dữ liệu ban đầu sẽ được tự động khởi tạo trong lần chạy đầu tiên.*

---

## 5. Tài Khoản Đăng Nhập Mặc Định

Hệ thống được khởi tạo sẵn các tài khoản thử nghiệm tương ứng với từng vai trò:

| Vai trò | Tên đăng nhập | Mật khẩu | Phạm vi quyền hạn |
| :--- | :--- | :--- | :--- |
| **Quản lý (Manager)** | `admin` | `admin123` | Quản lý món ăn, bán hàng, thanh toán, xem lịch sử |
| **Thu ngân (Cashier)** | `cashier` | `123456` | Bán hàng, tính tiền, in hóa đơn, xem lịch sử |
| **Nhân viên (Staff)** | `staff` | `123456` | Xem sơ đồ bàn, gọi món cho khách |

---

## 6. Giấy Phép

Dự án phục vụ mục đích học tập và phát triển ứng dụng quản lý bán hàng.
