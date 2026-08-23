using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Data.Db.Identity;

[Table("users", Schema = "identity")]
[Index(nameof(EntraObjectId), IsUnique = true)]
public sealed class UserDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public UserId Id { get; set; }

    public required Guid EntraObjectId { get; set; }

    public required Email Email { get; set; }

    public required DisplayName DisplayName { get; set; }
}
