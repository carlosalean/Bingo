using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public enum UserRole
{
    Host,
    Player,
    Guest
}