namespace CLS.BLL.DTOs;

/// <summary>
/// Lightweight DTO chứa thông tin cần thiết để generate JWT claims.
/// Dùng thay cho User Entity trong JwtService để:
///   1. Tránh dependency vào DAL layer từ JwtService
///   2. Chỉ expose dữ liệu tối thiểu cần cho token
///
/// Mapping: AuthService sẽ map User entity → JwtUserDto trước khi gọi JwtService.
/// </summary>
/// <param name="Id">User ID — sẽ được đưa vào claim 'sub'.</param>
/// <param name="Email">Email — claim 'email'.</param>
/// <param name="Role">Role (Admin/Teacher/Parent) — claim 'role'.</param>
/// <param name="FullName">Tên đầy đủ — claim 'name'.</param>
public record JwtUserDto(int Id, string Email, string Role, string FullName);
