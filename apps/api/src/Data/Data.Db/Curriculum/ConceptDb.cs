using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Milese.Common.Types.ValueTypes.Curriculum;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Data.Db.Curriculum;

[Table("concepts", Schema = "curriculum")]
public sealed class ConceptDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ConceptId Id { get; set; }

    public required SubjectId SubjectId { get; set; }

    public required ConceptTitle Title { get; set; }

    public required SortOrder Order { get; set; }

    public SubjectDb? Subject { get; set; }
}
