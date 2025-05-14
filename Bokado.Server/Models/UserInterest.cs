    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    namespace Bokado.Server.Models
    {
        [Table("user_interests")]
        public class UserInterest
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int UserInterestId { get; set; }

            public int UserId { get; set; }
            public int InterestId { get; set; }

            public User User { get; set; }
            public Interest Interest { get; set; }
        }
    }