using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BidOneGateway.Api.Helpers;

public static class IdempotencyHelper
{
    /// <summary>
    /// Generates a unique signature for a request based on the idempotency key,
    /// operation and hash of the request body.
    /// </summary>
    /// <returns>A unique string signature, or null if the key is missing.</returns>
    public static async Task<string?> GenerateIdempotencySignature(HttpRequest req, string httpMethod, string path)
    {
        if (!req.Headers.TryGetValue("Idempotency-Key", out var keyValues))
        {
            return null;
        }
        var idempotencyKey = keyValues.First();
        
        req.Body.Position = 0;
        using var reader = new StreamReader(req.Body, leaveOpen: true);
        var bodyAsString = await reader.ReadToEndAsync();
        req.Body.Position = 0;
        
        using var sha256 = SHA256.Create();
        var bodyHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(bodyAsString));
        var bodyHash = Convert.ToBase64String(bodyHashBytes);
        
        return $"{idempotencyKey}:{httpMethod}:{path}:{bodyHash}";
    }
}