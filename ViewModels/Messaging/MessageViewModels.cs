using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels.Messaging
{
    public class InboxViewModel
    {
        public List<MessageSummaryVM> Messages { get; set; } = new List<MessageSummaryVM>();
        public int UnreadCount { get; set; }
        public string CurrentFolder { get; set; } = "Inbox";
    }

    public class MessageSummaryVM
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string PreviewText { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string SenderAvatar { get; set; }
    }

    public class ComposeMessageViewModel
    {
        [Required]
        [Display(Name = "Recipient")]
        public string ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        public int? RelatedCourseId { get; set; }
    }

    public class MessageThreadViewModel
    {
        public string Subject { get; set; }
        public List<MessageDetailVM> Messages { get; set; } = new List<MessageDetailVM>();
    }

    public class MessageDetailVM
    {
        public string SenderName { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
