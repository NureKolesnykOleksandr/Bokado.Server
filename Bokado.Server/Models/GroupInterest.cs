using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bokado.Server.Models
{
    [Table("group_interests")]
    public class GroupInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupInterestId { get; set; }

        public int GroupId { get; set; }
        public int InterestId { get; set; }

        public Group Group { get; set; }
        public Interest Interest { get; set; }
    }
}
