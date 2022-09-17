using sdakccapi.Models.Entities;

namespace sdakccapi.Dtos.SignalRDto
{
    public class ChatOut
    {
       

        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public ChatOut()
        {

        }
        public ChatOut(Chats chats)
        {
            SenderId = chats.UserId;
            Message = chats.TextMessage;
            CreatedAt = chats.CreatedDate;

                
        }
    }
}
