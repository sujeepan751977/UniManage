using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; }
        public virtual ApplicationUser Receiver { get; set; }

        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public bool IsDeletedBySender { get; set; } = false;

        public bool IsDeletedByReceiver { get; set; } = false;

        public bool IsStarred { get; set; } = false;
    }
}
