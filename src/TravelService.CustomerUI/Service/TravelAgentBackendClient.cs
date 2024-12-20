using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.RealtimeConversation;
using static TravelService.CustomerUI.Components.Layout.MainLayout;
using static TravelService.CustomerUI.Components.Pages.Booking.Booking;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TravelService.CustomerUI.Clients.Backend;
#pragma warning disable OPENAI002

public class TravelAgentBackendClient(HttpClient http)
{
   public async Task<string> TriggerMultiAgentOrchestrationAsync(string request, string sessionId, string userId, AssistantType assistantType)
   {
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/ChatAssistant")
      {
         Content = new StringContent(JsonSerializer.Serialize(new { userQuery = request }), Encoding.UTF8, "application/json")
      };
      httpRequest.Headers.Add("Session-Id", sessionId);
      httpRequest.Headers.Add("User-Id", userId);
      httpRequest.Headers.Add("Agent-Type", assistantType.ToString());
      var response = await http.SendAsync(httpRequest);
      return await response.Content.ReadAsStringAsync();
   }

   public async Task<string> TriggerRealTimeAgentAsync(string sessionId, string functionCallId, string userId, string userPrompt)
   {
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/realtime")
      {
         Content = new StringContent(JsonConvert.SerializeObject(new { SessionId = sessionId, FunctionCallId = functionCallId, userQuery = userPrompt }), Encoding.UTF8, "application/json")
      };

      httpRequest.Headers.Add("Session-Id", sessionId);
      httpRequest.Headers.Add("User-Id", userId);
      httpRequest.Headers.Add("Agent-Type", "Realtime");

      var response = await http.SendAsync(httpRequest);
      var json = await response.Content.ReadAsStringAsync();
      return json;
   }

   public async Task<BookingDetailsResult> GetBookingMessagesAsync(string sessionId, string userId)
   {
      var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/chat/{sessionId}")
      {
         Headers =
               {
                   { "User-Id", userId }
               }
      };

      var response = await http.SendAsync(httpRequest);
      var json = await response.Content.ReadAsStringAsync();
      var result = JsonConvert.DeserializeObject<BookingDetailsResult>(json);
      return result;
   }

   public async Task<List<SessionSummary>> GetBookingMessagesByUserIdAsync(string userId)
   {
      var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/chat")
      {
         Headers =
               {
                   { "User-Id", userId }
               }
      };

      var response = await http.SendAsync(httpRequest);
      var json = await response.Content.ReadAsStringAsync();
      var result = JsonConvert.DeserializeObject<List<SessionSummary>>(json);
      return result;
   }
}

public record BookingDetailsResult(string SessionId, string CustomerFullName, string? LongSummary, ICollection<BookingDetailsResultMessage> Messages, ICollection<string> AgentMessages);

public record BookingDetailsResultMessage(string MessageId, DateTime CreatedAt, bool IsCustomerMessage, string MessageText);
