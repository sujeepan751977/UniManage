using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Models;
using UniManage.ViewModels.Messaging;
using System.Data.Entity;

namespace UniManage.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Messages/Inbox
        public ActionResult Inbox(string folder = "Inbox")
        {
            var userId = User.Identity.GetUserId();
            var query = db.Messages.AsQueryable();

            switch (folder.ToLower())
            {
                case "sent":
                    query = query.Where(m => m.SenderId == userId && !m.IsDeletedBySender);
                    break;
                case "starred":
                    query = query.Where(m => (m.ReceiverId == userId || m.SenderId == userId) && m.IsStarred);
                    break;
                case "trash":
                    query = query.Where(m => (m.ReceiverId == userId && m.IsDeletedByReceiver) || (m.SenderId == userId && m.IsDeletedBySender));
                    break;
                default:
                    folder = "Inbox";
                    query = query.Where(m => m.ReceiverId == userId && !m.IsDeletedByReceiver);
                    break;
            }

            var messages = query
                .OrderByDescending(m => m.SentAt)
                .ToList()
                .Select(m => new MessageSummaryVM
                {
                    MessageId = m.MessageId,
                    SenderName = folder.ToLower() == "sent" ? (m.Receiver?.FullName ?? "Unknown") : (m.Sender?.FullName ?? "System"),
                    Subject = m.Subject,
                    PreviewText = m.Content.Length > 50 ? m.Content.Substring(0, 47) + "..." : m.Content,
                    Timestamp = m.SentAt,
                    IsRead = m.IsRead
                }).ToList();

            var model = new InboxViewModel
            {
                Messages = messages,
                UnreadCount = db.Messages.Count(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeletedByReceiver),
                CurrentFolder = folder
            };

            return View(model);
        }

        // GET: Messages/Compose
        public ActionResult Compose(string receiverId, string subject)
        {
            var model = new ComposeMessageViewModel
            {
                ReceiverId = receiverId,
                Subject = subject
            };
            
            if (!string.IsNullOrEmpty(receiverId))
            {
                var user = db.Users.Find(receiverId);
                if (user != null) model.ReceiverName = user.FullName;
            }

            return View(model);
        }

        // POST: Messages/Compose
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Compose(ComposeMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string targetId = model.ReceiverId;
                
                // Resolve email to ID if needed
                if (targetId.Contains("@"))
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == targetId);
                    if (user != null)
                    {
                        targetId = user.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("ReceiverId", "Recipient email not found in system.");
                        return View(model);
                    }
                }

                var message = new Message
                {
                    SenderId = User.Identity.GetUserId(),
                    ReceiverId = targetId,
                    Subject = model.Subject,
                    Content = model.Content,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                db.Messages.Add(message);
                db.SaveChanges();

                TempData["Success"] = "Message sent successfully!";
                return RedirectToAction("Inbox");
            }

            return View(model);
        }

        // GET: Messages/Read/5
        public ActionResult Read(int id)
        {
            var userId = User.Identity.GetUserId();
            var message = db.Messages.Include(m => m.Sender).FirstOrDefault(m => m.MessageId == id && m.ReceiverId == userId);
            
            if (message == null) return HttpNotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                db.SaveChanges();
            }

            return View(message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
