using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Кожен юзер підключається до своєї персональної групи "user_{id}"
        // Це дозволяє надсилати сповіщення конкретному юзеру з будь-якого сервісу
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId > 0)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnDisconnectedAsync(exception);
        }

        private int GetUserId()
        {
            try
            {
                var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(token)) return 0;

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
                return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
            }
            catch { return 0; }
        }
    }
}
