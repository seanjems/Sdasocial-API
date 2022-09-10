using sdakccapi.Models.Entities;

namespace sdakccapi.Dtos.SignalRDto
{
    public class OnlineUsersOutDto
    {
        public long conversationId { get; set; }
        public List<ActiveUsersOutDto> ActiveMembersList { get; set; }
    }
}
