using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using sdakccapi.Controllers;
using sdakccapi.Controllers.SignalR;
using sdakccapi.Dtos.SignalRDto;
using sdakccapi.Models.Entities;
using System.Globalization;

namespace sdakccapi.Hubs
{
    public class ChatHub:Hub
    {
       // private readonly string _botUser;
        //private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly AuthorizationController _authorizationController;
        private readonly ActiveUsersController _activeUsersController;
        private readonly ConversationsController _conversationsController;
        private readonly ChatsController _chatsController;


        public ChatHub( AuthorizationController authorizationController, ActiveUsersController activeUsersController,ConversationsController conversationsController, ChatsController chatsController)
        {
            //_connections = connections;
            _authorizationController = authorizationController;
            _activeUsersController = activeUsersController;
            _conversationsController = conversationsController;
            _chatsController = chatsController;
        }
        public override async Task<Task> OnDisconnectedAsync(Exception exception)
        {
           
            //update hubgroups
            var currentUser = _authorizationController.GetCurrentUser(Context);

            var conversationList = await _conversationsController.GetAllConversationsForUser(currentUser.UserId);

            foreach (var connection in conversationList)
            {
                //add to hub groups
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{connection.ConversationId}");
            }

            //updateOnlineUsers
            var listOfwork = _activeUsersController.GetAffectedConversationsWithNewLists(currentUser.UserId);

            //send new onlineuser to ui
            foreach (var connection in listOfwork)
            {

                await Clients.Group($"{connection.conversationId}").SendAsync("ReceiveUsers", connection.ActiveMembersList);

            }

            return base.OnDisconnectedAsync(exception);
        }


        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task StartConnection()
        {
            //getUser details from context and update active users
            
            bool conectionExists = _activeUsersController.ConnectionExists(Context.ConnectionId);

            if (!conectionExists)
            {
                var currentUser = _authorizationController.GetCurrentUser(Context);
                if (currentUser == null) return;

                
                //add it to db
                var newUser = new ActiveUsers()
                {
                    ConnectionId = Context.ConnectionId,
                    CreatedTime = DateTime.Now,
                    UserId = currentUser.UserId,

                };
                //add to db

                 await _activeUsersController.PostActiveUsers(newUser);

               

                var listOfwork = _activeUsersController.GetAffectedConversationsWithNewLists(newUser.UserId);

                //send new onlineuser to ui
                foreach (var connection in listOfwork)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, Context.ConnectionId);
                    await Clients.Group(Context.ConnectionId).SendAsync("ReceiveUsers", connection.ActiveMembersList);
                    //reset temp group
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.ConnectionId);
                }

                //update hubgroups
                var conversationList = await _conversationsController.GetAllConversationsForUser(currentUser.UserId);

                foreach (var connection in conversationList)
                {
                    //add to hub groups
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{connection.ConversationId}");
                }

            }

            //await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            ////save the connection
            //_connections[Context.ConnectionId] = userConnection;

            //await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,$"{userConnection.User} has joined {userConnection.Room}");
        }



        public async Task SendMessage( MessageFromUiDto message)
        {
            //get current user
            var currentUser = _authorizationController.GetCurrentUser(Context);

            var converExists = _conversationsController.ConversationExists(currentUser.UserId, message.ReceiverId);

            var connection = await _conversationsController.GetConversationOrCreate(currentUser.UserId, message.ReceiverId, Context.ConnectionId);

            if (!converExists)
            {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{connection.Id }");
            }


            //send message
            if (connection !=null){
                //await Clients.Group(userConnectionDto.Room)
                //    .SendAsync("ReceiveMessage", userConnectionDto.User, message);
                var CreatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                var senderId = currentUser.UserId;
                var receiverId = message.ReceiverId;
                await Clients.Group($"{connection.Id}").SendAsync("ReceiveMessage",senderId,receiverId ,message.MessageText, CreatedAt);


                //save a copy of the message in the db;
                var chatBackup = new Chats()
                {
                    conversationId = connection.Id,
                    CreatedDate = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    isDeleted = false,
                    UserId = senderId,
                    TextMessage = message.MessageText,
                    
                };
                _chatsController.PostChatsPerUser(chatBackup);
            }

        }

        
        public async Task RefreshChatHeads()
        {
            //get current user
         
            var chats = await _conversationsController.GetAllChatHeadsPerUser(Context);

            //use temp group to target sender device.
            if (chats.Count()>0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.ConnectionId);
                await Clients.Group(Context.ConnectionId).SendAsync("ReceiveChatHeads", chats);
                //reset temp group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.ConnectionId);
               
            }

        }
    }
}
