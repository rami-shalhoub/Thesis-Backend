using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using Pgvector;
namespace Backend.services.AI
{
    public class DeepseekService : IDeepseekService
    {
        private readonly HttpClient _httpClient;
        private readonly DeepseekConfig _config;
        private readonly ISourceService _sourceService;

        public DeepseekService(
            HttpClient httpClient,
            IOptions<DeepseekConfig> config,
            ISourceService sourceService)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _sourceService = sourceService;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_config.ApiUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }

        public async Task<ChatResponseDTO> GetCompletionAsync(string prompt, List<MessageDTO> conversationHistory)
        {
            try
            {
                //* Prepare the messages for the API
                var messages = new List<object>
                {
                    new { role = "system", content = _config.SystemPrompt }
                };

                //* Add conversation history
                foreach (var message in conversationHistory)
                {
                    messages.Add(new { role = "user", content = message.Prompt });
                    messages.Add(new { role = "assistant", content = message.Response });
                }

                //* Add the current prompt
                messages.Add(new { role = "user", content = prompt });

                //* Prepare the request payload
                var requestData = new
                {
                    model = _config.Model,
                    messages,
                    max_tokens = _config.MaxTokens,
                    temperature = _config.Temperature
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                //* Send the request
                var response = await _httpClient.PostAsync("/chat/completions", content);
                response.EnsureSuccessStatusCode();

                //* Parse the response
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                //* Extract the AI's response
                string aiResponse = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                //* Extract sources from the response
                var sources = await _sourceService.ExtractSourcesAsync(aiResponse);
                sources = await _sourceService.ValidateSourcesAsync(sources);
                sources = await _sourceService.FormatSourcesAsync(sources);

                //* Create the response DTO
                var chatResponse = new ChatResponseDTO
                {
                    Response = aiResponse,
                    Sources = sources,
                    Timestamp = DateTime.Now
                };

                return chatResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error calling DeepSeek API: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ExtractLegalTopicsAsync(string prompt)
        {
            try
            {
                //* Prepare the messages for the API
                var messages = new List<object>
                {
                    new { role = "system", content = "You are a legal topic classifier. Extract the main legal topics from the user's query. Return only a comma-separated list of legal topics (e.g., 'Contract Law, Tort Law, Employment Law'). Be specific and concise." }
                };

                //* Add the prompt
                messages.Add(new { role = "user", content = prompt });

                //* Prepare the request payload
                var requestData = new
                {
                    model = _config.Model,
                    messages,
                    max_tokens = 100,
                    temperature = 0.3
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                //* Send the request
                var response = await _httpClient.PostAsync("/chat/completions", content);
                response.EnsureSuccessStatusCode();

                //* Parse the response
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                //* Extract the topics
                string topics = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                //* Clean up the response (remove any extra text)
                topics = topics.Trim();
                if (topics.Contains("\n"))
                {
                    topics = topics.Split('\n')[0];
                }

                return topics;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error extracting legal topics: {ex.Message}");
                return "UK Law"; // Default topic
            }
        }

        public async Task<string> GenerateSessionTitleAsync(string prompt)
        {
            try
            {
                //* Prepare the messages for the API
                var messages = new List<object>
                {
                    new { role = "system", content = "Generate a concise title (maximum 50 characters) for a legal conversation based on the user's query. The title should reflect the main legal topic or question. Return only the title, with no additional text or explanation." }
                };

                //* Add the prompt
                messages.Add(new { role = "user", content = prompt });

                //* Prepare the request payload
                var requestData = new
                {
                    model = _config.Model,
                    messages,
                    max_tokens = 50,
                    temperature = 0.7
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                //* Send the request
                var response = await _httpClient.PostAsync("/chat/completions", content);
                response.EnsureSuccessStatusCode();

                //* Parse the response
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                //* Extract the title
                string title = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                //* Clean up the title
                title = title.Trim();
                if (title.Contains("\n"))
                {
                    title = title.Split('\n')[0];
                }

                // Ensure the title is not too long
                if (title.Length > 50)
                {
                    title = title.Substring(0, 47) + "...";
                }

                return title;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error generating session title: {ex.Message}");
                return "UK Law Chat Session"; // Default title
            }
        }

        public async Task<string> GenerateSummaryAsync(List<MessageDTO> conversationHistory)
        {
            try
            {
                //* Take the last 4 messages (or fewer if there aren't 4)
                var recentMessages = conversationHistory
                    .OrderByDescending(m => m.SequenceNumber)
                    .Take(4)
                    .OrderBy(m => m.SequenceNumber)
                    .ToList();

                if (recentMessages.Count == 0)
                {
                    return "No conversation history to summarize.";
                }

                //* Format the conversation history
                var formattedHistory = new StringBuilder();
                foreach (var message in recentMessages)
                {
                    formattedHistory.AppendLine($"User: {message.Prompt}");
                    formattedHistory.AppendLine($"Assistant: {message.Response}");
                    formattedHistory.AppendLine();
                }

                //* Prepare the messages for the API
                var messages = new List<object>
                {
                    new { role = "system", content = "Summarize this legal conversation in 1-2 sentences, focusing on key legal topics discussed." }
                };

                //* Add the conversation history
                messages.Add(new { role = "user", content = $"Conversation: {formattedHistory}\nSummary:" });

                //* Prepare the request payload
                var requestData = new
                {
                    model = _config.Model,
                    messages,
                    max_tokens = 200,
                    temperature = 0.5
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                //* Send the request
                var response = await _httpClient.PostAsync("/chat/completions", content);
                response.EnsureSuccessStatusCode();

                //* Parse the response
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                //* Extract the summary
                string summary = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                return summary.Trim();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error generating summary: {ex.Message}");
                return "Discussion about UK legal matters."; // Default summary
            }
        }

        //TODO fix this method, the embedding vectore is not beind generated        
        public async Task<Pgvector.Vector> GenerateEmbeddingAsync(string text)
        {
            try
            {
                var httpClient = new HttpClient();

                //* Use OpenAI API key if available, otherwise fall back to the regular API key
                string apiKey = !string.IsNullOrEmpty(_config.OpenAIApiKey) ? _config.OpenAIApiKey : _config.ApiKey ?? string.Empty;
                Console.WriteLine($"Using API key for embeddings: {(string.IsNullOrEmpty(apiKey) ? "No API key provided" : "API key provided")}");

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                //* Prepare the request payload
                var requestData = new
                {
                    input = text,
                    model = "text-embedding-ada-002"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                //* Send the request to OpenAI embedding API
                var response = await httpClient.PostAsync(
                    "https://api.openai.com/v1/embeddings",
                    content);

                //* Log the response status
                Console.WriteLine($"Embedding API response status: {response.StatusCode}");

                //* Ensure the request was successful
                response.EnsureSuccessStatusCode();

                //* Parse the response
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Embedding API response: {responseBody.Substring(0, Math.Min(responseBody.Length, 100))}...");

                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                //* Extract the embedding as an array of floats
                var embeddingArray = new List<float>();
                var embeddingElement = responseObject
                    .GetProperty("data")[0]
                    .GetProperty("embedding");

                foreach (var item in embeddingElement.EnumerateArray())
                {
                    embeddingArray.Add(item.GetSingle());
                }

                var vector = new Pgvector.Vector(embeddingArray.ToArray());
                Console.WriteLine($"Successfully created embedding vector with {embeddingArray.Count} dimensions");

                return vector;
            }
            catch (HttpRequestException ex)
            {
                //* Log the HTTP exception details
                Console.WriteLine($"HTTP error generating embedding: {ex.Message}");
                Console.WriteLine($"Status code: {ex.StatusCode}");

                // //* Generate a random embedding instead of zeros for testing
                // var random = new Random();
                // var randomEmbedding = new float[1536];
                // for (int i = 0; i < randomEmbedding.Length; i++)
                // {
                //     randomEmbedding[i] = (float)((random.NextDouble() * 2) - 1); // Values between -1 and 1
                // }

                // Console.WriteLine("Using random embedding vector as fallback");
                // return new Pgvector.Vector(randomEmbedding);
                return new Pgvector.Vector(new float[1536]);
            }
            catch (Exception ex)
            {
                //* Log the general exception
                Console.WriteLine($"Error generating embedding: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // //* Generate a random embedding instead of zeros for testing
                // var random = new Random();
                // var randomEmbedding = new float[1536];
                // for (int i = 0; i < randomEmbedding.Length; i++)
                // {
                //     randomEmbedding[i] = (float)((random.NextDouble() * 2) - 1); // Values between -1 and 1
                // }

                // Console.WriteLine("Using random embedding vector as fallback");
                // return new Pgvector.Vector(randomEmbedding);
                return new Pgvector.Vector(new float[1536]);
            }
        }
    }
}
