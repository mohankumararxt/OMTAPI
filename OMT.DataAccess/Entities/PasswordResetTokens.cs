using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class PasswordResetTokens
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string GuId { get; set; }
        public DateTime LinkSentDate { get; set; }
        public DateTime? ResetDate { get; set; }
        public bool IsUsed { get; set; }

    }
}
