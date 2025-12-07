using BlazorApp.Models;

namespace BlazorApp.Services
{
    public interface IMessageService
    {
        Task<List<MessageDto>> GetInboxAsync();
        Task<List<MessageDto>> GetSentAsync();
        Task<List<MessageDto>> GetUnreadAsync();
        Task<MessageDto> GetMessageAsync(int id);
        Task<List<MessageDto>> GetConversationAsync(string otherUserId);
        Task<int> GetUnreadCountAsync();
        Task<bool> SendMessageAsync(CreateMessageDto model);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> DeleteMessageAsync(int id);
        Task<List<MessageDto>> GetInboxMessagesAsync();
        Task<List<MessageDto>> GetSentMessagesAsync();
    }

    public class MessageService : IMessageService
    {
        private readonly IApiClient _apiClient;

        public MessageService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<MessageDto>> GetInboxAsync()
        {
            var response = await _apiClient.GetAsync<List<MessageDto>>("api/messages/inbox");
            return response.Data ?? new List<MessageDto>();
        }

        public async Task<List<MessageDto>> GetSentAsync()
        {
            var response = await _apiClient.GetAsync<List<MessageDto>>("api/messages/sent");
            return response.Data ?? new List<MessageDto>();
        }

        public async Task<List<MessageDto>> GetUnreadAsync()
        {
            var response = await _apiClient.GetAsync<List<MessageDto>>("api/messages/unread");
            return response.Data ?? new List<MessageDto>();
        }

        public async Task<MessageDto> GetMessageAsync(int id)
        {
            var response = await _apiClient.GetAsync<MessageDto>($"api/messages/{id}");
            return response.Data;
        }

        public async Task<List<MessageDto>> GetConversationAsync(string otherUserId)
        {
            var response = await _apiClient.GetAsync<List<MessageDto>>($"api/messages/conversation/{otherUserId}");
            return response.Data ?? new List<MessageDto>();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var response = await _apiClient.GetAsync<int>("api/messages/unread/count");
            return response.Data;
        }

        public async Task<bool> SendMessageAsync(CreateMessageDto model)
        {
            var response = await _apiClient.PostAsync<MessageDto>("api/messages", model);
            return response.Success;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var response = await _apiClient.PutAsync<object>($"api/messages/{id}/read", new { });
            return response.Success;
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/messages/{id}");
            return response.Success;
        }

        public async Task<List<MessageDto>> GetInboxMessagesAsync()
        {
            return await GetInboxAsync();
        }

        public async Task<List<MessageDto>> GetSentMessagesAsync()
        {
            return await GetSentAsync();
        }
    }
}
