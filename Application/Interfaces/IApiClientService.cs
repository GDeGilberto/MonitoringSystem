namespace Application.Interfaces
{
    /// <summary>
    /// Service to make authenticated HTTP requests to API endpoints
    /// </summary>
    public interface IApiClientService
    {
        /// <summary>
        /// Makes a GET request to the specified endpoint with API key authentication
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint (relative URL)</param>
        /// <returns>The deserialized response</returns>
        Task<T> GetAsync<T>(string endpoint);
        
        /// <summary>
        /// Makes a POST request to the specified endpoint with API key authentication
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint (relative URL)</param>
        /// <param name="request">The request body</param>
        /// <returns>The deserialized response</returns>
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
        
        /// <summary>
        /// Makes a PUT request to the specified endpoint with API key authentication
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint (relative URL)</param>
        /// <param name="request">The request body</param>
        /// <returns>The deserialized response</returns>
        Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
    }
}