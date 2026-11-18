using System;
using System.Web;
using UniManage.Models;

namespace UniManage.Helpers
{
    public static class AuditHelper
    {
        public static void Log(string userId, string action, string entity, string entityId, string details, HttpRequestBase request)
        {
            using (var db = new ApplicationDbContext())
            {
                var log = new AuditLog
                {
                    PerformedById = userId,
                    Action = action,
                    EntityAffected = entity,
                    EntityId = entityId,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = request.UserHostAddress,
                    UserAgent = request.UserAgent
                };
                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
        }
    }
}
