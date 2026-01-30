using System;
using System.Collections.Generic;
using PosSystem.Domain.Entities;
namespace PosSystem.Domain.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}