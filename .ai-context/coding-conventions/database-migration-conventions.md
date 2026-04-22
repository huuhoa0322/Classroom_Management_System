# Database & EF Core Migration Conventions

> **Scope:** Áp dụng cho quá trình thiết kế, cấu hình cơ sở dữ liệu và quản lý EF Core Migrations trong dự án Classroom Management System (CLS).
> **Target Database:** PostgreSQL 15+ (thông qua Supabase).
> **ORM:** Entity Framework Core 10.

---

## 1. Entity & Property Naming Conventions

### 1.1 Class & Property Names (C# Level)
- **Entity Class:** `PascalCase`, **danh từ số ít**. VD: `Student`, `ClassSession`, `RenewalAlert`.
- **Properties:** `PascalCase`. VD: `FullName`, `CreatedAt`.
- **Primary Key:** Bắt buộc sử dụng tên `Id` với kiểu dữ liệu `int` (Identity/Serial) cho mọi Entity kế thừa từ `BaseEntity`.

### 1.2 Table & Column Names (Database Level)
Toàn bộ database schema phải tuân thủ chuẩn của PostgreSQL là **`snake_case`**.
- **Table:** `snake_case`, **số nhiều**. VD: `students`, `class_sessions`, `renewal_alerts`.
- **Column:** `snake_case`. VD: `full_name`, `created_at`, `class_id`.

---

## 2. EF Core Configuration (Fluent API)

Tuyệt đối **KHÔNG** sử dụng Data Annotations (như `[Table]`, `[Column]`, `[Required]`) trên các Entity classes. Mọi cấu hình phải được thực hiện thông qua **Fluent API**.

### 2.1 IEntityTypeConfiguration
Mỗi Entity phải có một file cấu hình riêng biệt kế thừa từ `IEntityTypeConfiguration<T>` và được đặt trong thư mục `CLS.DAL/Configurations/`.

```csharp
// Ví dụ mẫu trong CLS.DAL/Configurations/ClassSessionConfiguration.cs
public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        // 1. Table mapping
        builder.ToTable("class_sessions");
        
        // 2. Primary Key
        builder.HasKey(x => x.Id);
        
        // 3. Properties mapping
        builder.Property(x => x.StartTime)
               .HasColumnName("start_time")
               .IsRequired();
               
        // 4. Relationships
        builder.HasOne(x => x.Teacher)
               .WithMany(t => t.Sessions)
               .HasForeignKey(x => x.TeacherId)
               .HasConstraintName("fk_class_sessions_teachers")
               .OnDelete(DeleteBehavior.Restrict); // Bắt buộc định nghĩa rõ OnDelete
    }
}
```

### 2.2 Quy tắc thiết lập quan hệ (Relationships)
- Luôn định nghĩa rõ `OnDelete` behavior (ví dụ: `DeleteBehavior.Restrict` hoặc `DeleteBehavior.Cascade`). Tuyệt đối không để EF Core tự quyết định default cascade delete để tránh mất dữ liệu không mong muốn.
- Đặt tên Foreign Key Constraints rõ ràng theo chuẩn `fk_[bảng_hiện_tại]_[bảng_tham_chiếu]`.

---

## 3. Base Entity & Audit Fields

Mọi Entity chính trong hệ thống phải kế thừa từ `BaseEntity` (đã được định nghĩa trong `CLS.DAL/Entities/BaseEntity.cs`).

- `BaseEntity` cung cấp sẵn các trường: `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`.
- Các trường Audit (`CreatedAt`, `UpdatedAt`) sẽ được **tự động gán giá trị** thông qua việc override hàm `SaveChangesAsync` trong `AppDbContext`. Không được gán tay các giá trị này ở Service layer.
- Hệ thống áp dụng **Global Soft Delete**. Khi xóa, Service layer chỉ việc set `IsDeleted = true`. `AppDbContext` sẽ tự động ghi nhận `DeletedAt`.
- `BaseEntityConfiguration<T>` đã cấu hình `HasQueryFilter(x => !x.IsDeleted)` nên mọi truy vấn thông thường sẽ tự động bỏ qua các bản ghi đã bị soft-delete.

---

## 4. EF Core Migration Workflow

Vì cấu trúc dự án là Modular Monolith phân tách Layer (`CLS.Server`, `CLS.BLL`, `CLS.DAL`), các lệnh Migration cần phải chỉ định rõ `project` và `startup-project`.

### 4.1 Thêm Migration mới

```bash
dotnet ef migrations add <MeaningfulName> --project CLS.DAL --startup-project CLS.Server
```

**Quy tắc đặt tên `<MeaningfulName>`:**
Sử dụng PascalCase, mô tả rõ hành động. 
- Tạo bảng mới: `Add[EntityName]Table` (VD: `AddStudentTable`)
- Sửa cột: `Alter[EntityName][PropertyName]` (VD: `AlterStudentPhoneNumberLength`)
- Thêm index: `Add[EntityName][IndexName]Index` (VD: `AddStudentEmailIndex`)

### 4.2 Cập nhật Database

```bash
dotnet ef database update --project CLS.DAL --startup-project CLS.Server
```

### 4.3 Xóa Migration cuối cùng (khi chưa update db)

```bash
dotnet ef migrations remove --project CLS.DAL --startup-project CLS.Server
```

### 4.4 Quy tắc chung về file Migrations
1. **Không sửa tay:** Tuyệt đối không chỉnh sửa thủ công các file `.cs` do lệnh `migrations add` sinh ra. Nếu có lỗi cấu hình, hãy `migrations remove`, sửa lại cấu hình bằng Fluent API, rồi chạy lại `migrations add`.
2. **Review trước khi update:** Luôn đọc nhanh file Migration vừa được tạo ra để đảm bảo EF Core hiểu đúng ý đồ thiết kế (nhất là các thao tác Drop Column, Alter Column có khả năng gây mất data).
3. **Source Control:** Cần phải commit cả thư mục `Migrations` lên Git cùng với các thay đổi code tương ứng.

---

## 5. Seed Data

Nếu cần khởi tạo dữ liệu mẫu hoặc dữ liệu danh mục ban đầu (VD: Roles mặc định, Admin user đầu tiên), hãy cấu hình thông qua phương thức `HasData` bên trong các class `IEntityTypeConfiguration<T>`.

```csharp
builder.HasData(
    new Role { Id = 1, Name = AppRoles.Admin, Description = "System Administrator", CreatedAt = DateTime.UtcNow },
    new Role { Id = 2, Name = AppRoles.Teacher, Description = "Teacher", CreatedAt = DateTime.UtcNow }
);
```
