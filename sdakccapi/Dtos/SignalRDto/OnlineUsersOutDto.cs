using sdakccapi.Models.Entities;

namespace sdakccapi.Dtos.SignalRDto
{
    public class OnlineUsersOutDto
    {
        public string ConnectionId { get; set; }
        public List<string> ActiveUserIds { get; set; }
    }
}
