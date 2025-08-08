using HLM_Web_APi.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HLM_Web_APi.Services
{
    public class MenuRepository
    {
        private readonly string _conn;
        public MenuRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }
        private SqlConnection GetConn() => new SqlConnection(_conn);

        public async Task<List<MenuDto>> GetMenusForUserAsync(int userId)
        {
            var list = new List<MenuDto>();
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_GetMenusByUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new MenuDto
                {
                    MenuId = rdr.GetInt32(rdr.GetOrdinal("MenuId")),
                    Title = rdr.GetString(rdr.GetOrdinal("Title")),
                    Icon = rdr.IsDBNull(rdr.GetOrdinal("Icon")) ? null : rdr.GetString(rdr.GetOrdinal("Icon")),
                    Link = rdr.IsDBNull(rdr.GetOrdinal("Link")) ? null : rdr.GetString(rdr.GetOrdinal("Link")),
                    SortOrder = rdr.GetInt32(rdr.GetOrdinal("SortOrder"))
                });
            }
            return list;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var list = new List<RoleDto>();
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_GetAllRoles", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new RoleDto { RoleId = rdr.GetInt32(0), RoleName = rdr.GetString(1) });
            }
            return list;
        }

        public async Task<List<MenuDto>> GetAllMenusAsync()
        {
            var list = new List<MenuDto>();
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_GetAllMenus", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new MenuDto
                {
                    MenuId = rdr.GetInt32(rdr.GetOrdinal("MenuId")),
                    Title = rdr.GetString(rdr.GetOrdinal("Title")),
                    Icon = rdr.IsDBNull(rdr.GetOrdinal("Icon")) ? null : rdr.GetString(rdr.GetOrdinal("Icon")),
                    Link = rdr.IsDBNull(rdr.GetOrdinal("Link")) ? null : rdr.GetString(rdr.GetOrdinal("Link")),
                    SortOrder = rdr.GetInt32(rdr.GetOrdinal("SortOrder"))
                });
            }
            return list;
        }

        public async Task<List<PermissionDto>> GetRolePermissionsAsync(int roleId)
        {
            var list = new List<PermissionDto>();
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_GetRolePermissions", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new PermissionDto
                {
                    PermissionId = rdr.GetInt32(rdr.GetOrdinal("PermissionId")),
                    PermissionKey = rdr.GetString(rdr.GetOrdinal("PermissionKey")),
                    Description = rdr.IsDBNull(rdr.GetOrdinal("Description")) ? null : rdr.GetString(rdr.GetOrdinal("Description")),
                    IsGranted = rdr.GetInt32(rdr.GetOrdinal("IsGranted")) == 1
                });
            }
            return list;
        }

        public async Task<bool> UpdateRolePermissionAsync(int roleId, string permissionKey, bool isGranted)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_UpdateRolePermission", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@PermissionKey", permissionKey);
            cmd.Parameters.AddWithValue("@IsGranted", isGranted ? 1 : 0);
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0; // stored proc uses transactions and either commits or throws
        }

        public async Task<bool> SetMenuActiveAsync(int menuId, bool isActive)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_SetMenuActive", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@MenuId", menuId);
            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows >= 0;
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.sp_IsUserInRole", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) == 1;
        }
    }
}
