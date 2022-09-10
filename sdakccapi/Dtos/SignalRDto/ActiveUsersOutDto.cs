using sdakccapi.Models.Entities;

namespace sdakccapi.Dtos.SignalRDto
{
    public class ActiveUsersOutDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicUrl { get; set; }
        public string userName { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        public ActiveUsersOutDto()
        {

        }
        public ActiveUsersOutDto(ActiveUsers activeUsers)
        {
            UserId = activeUsers?.UserId;
            FirstName = activeUsers?.User?.FirstName;
            LastName = activeUsers?.User?.Lastname;
            ProfilePicUrl = activeUsers?.User?.ProfilePicUrl;
            userName = activeUsers?.User?.UserName;
            FullName = $"{FirstName} {LastName}";
            
            CreatedTime = activeUsers.CreatedTime;
            DateModified = activeUsers.DateModified;

           
        }

        public ActiveUsersOutDto(ConversationMembers activeUsers)
        {
            UserId = activeUsers?.UserId;
            FirstName = activeUsers?.Users?.FirstName;
            LastName = activeUsers?.Users?.Lastname;
            ProfilePicUrl = activeUsers?.Users?.ProfilePicUrl;
            userName = activeUsers?.Users?.UserName;
            FullName = $"{FirstName} {LastName}";

            CreatedTime = activeUsers.CreatedDate;
            DateModified = activeUsers.DateModified;


        }

    }
}
