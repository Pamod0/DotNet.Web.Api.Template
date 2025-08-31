using Microsoft.AspNetCore.SignalR;

namespace ASP.NET_Core_Identity.Hubs
{
    public class NotificationHub : Hub
    {
        // This method allows clients to join a department-specific group.
        public async Task JoinDepartmentGroup(string departmentId)
        {
            // Add the current connection to a group with the departmentId as its name.
            await Groups.AddToGroupAsync(Context.ConnectionId, departmentId);
        }

        // Sends a notification to all clients in the specified departmentId group
        public async Task SendNotificationToDepartment(string departmentId, object notificationData)
        {
            // Sends the message to all clients connected to the hub.
            await Clients.Group(departmentId).SendAsync("ReceiveNotification", notificationData);
        }

        // Sends a notification to a specific user
        public async Task SendNotificationToUser(string userId, object notificationData)
        {
            // Sends the message to a specific user identified by userId.
            await Clients.User(userId).SendAsync("ReceiveNotification", notificationData);
        }
    }
}
