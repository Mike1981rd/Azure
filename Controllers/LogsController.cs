using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        private readonly string _logDirectory;
        // Thread-safe file writing using SemaphoreSlim (allows async operations)
        private static readonly SemaphoreSlim _fileWriteSemaphore = new SemaphoreSlim(1, 1);
        // Dictionary to track semaphores per file to allow parallel writes to different files
        private static readonly Dictionary<string, SemaphoreSlim> _fileSemaphores = new();
        private static readonly object _semaphoreCreationLock = new object();

        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "frontend");
            
            // Ensure directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        [HttpPost("frontend")]
        [AllowAnonymous] // Allow frontend to send logs without authentication
        public async Task<IActionResult> LogFrontendErrors([FromBody] FrontendLogRequest request)
        {
            try
            {
                if (request?.Logs == null || !request.Logs.Any())
                {
                    return BadRequest("No logs provided");
                }

                var today = DateTime.Now.ToString("yyyyMMdd");
                
                // Group logs by file to minimize lock contention
                var logsByFile = new Dictionary<string, List<(FrontendLogEntry log, string formatted)>>();
                
                // Process and group log entries
                foreach (var log in request.Logs)
                {
                    // Skip logging errors about log endpoint itself to prevent infinite loops
                    if (log.Type == "network" && log.Message?.Contains("/api/logs/frontend") == true)
                    {
                        continue;
                    }
                    
                    // Determine file based on type
                    string fileName = log.Type switch
                    {
                        "network" => $"frontend-network-{today}.log",
                        "exception" => $"frontend-exceptions-{today}.log",
                        "promise" => $"frontend-exceptions-{today}.log",
                        _ => $"frontend-errors-{today}.log"
                    };

                    var filePath = Path.Combine(_logDirectory, fileName);
                    
                    // Format log entry
                    var logEntry = FormatLogEntry(log);
                    
                    // Group by file
                    if (!logsByFile.ContainsKey(filePath))
                    {
                        logsByFile[filePath] = new List<(FrontendLogEntry, string)>();
                    }
                    logsByFile[filePath].Add((log, logEntry));
                }
                
                // Write to files with proper synchronization
                var writeTasks = new List<Task>();
                foreach (var kvp in logsByFile)
                {
                    writeTasks.Add(WriteLogsToFileAsync(kvp.Key, kvp.Value));
                }
                
                // Wait for all writes to complete
                await Task.WhenAll(writeTasks);
                
                // Log to backend logger (this is already thread-safe)
                foreach (var logGroup in logsByFile.Values)
                {
                    foreach (var (log, _) in logGroup)
                    {
                        switch (log.Level)
                        {
                            case "error":
                                _logger.LogError("Frontend Error: {Message} | Type: {Type} | URL: {Url}", 
                                    log.Message, log.Type, log.Url);
                                break;
                            case "warn":
                                _logger.LogWarning("Frontend Warning: {Message} | Type: {Type} | URL: {Url}", 
                                    log.Message, log.Type, log.Url);
                                break;
                            default:
                                _logger.LogInformation("Frontend Log: {Message} | Type: {Type} | URL: {Url}", 
                                    log.Message, log.Type, log.Url);
                                break;
                        }
                    }
                }

                return Ok(new { message = $"Logged {request.Logs.Count} entries" });
            }
            catch (Exception ex)
            {
                // Don't log the full exception to avoid noise, just log a warning
                _logger.LogWarning("Error processing frontend logs: {Message}", ex.Message);
                // Return OK anyway to prevent frontend from retrying
                return Ok(new { message = "Logs received", warning = "Some logs may not have been saved" });
            }
        }
        
        private async Task WriteLogsToFileAsync(string filePath, List<(FrontendLogEntry log, string formatted)> logs)
        {
            // Get or create a semaphore for this specific file
            SemaphoreSlim fileSemaphore = GetOrCreateFileSemaphore(filePath);
            
            // Wait to acquire the semaphore (only one writer per file at a time)
            await fileSemaphore.WaitAsync();
            try
            {
                // Combine all logs for this file into a single write operation
                var combinedLogs = string.Join("", logs.Select(l => l.formatted));
                
                // Use FileStream with FileShare.Read to allow reading while writing
                using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true))
                using (var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(combinedLogs);
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we don't want to fail the entire request
                _logger.LogWarning("Failed to write to log file {FilePath}: {Message}", filePath, ex.Message);
            }
            finally
            {
                // Always release the semaphore
                fileSemaphore.Release();
            }
        }
        
        private SemaphoreSlim GetOrCreateFileSemaphore(string filePath)
        {
            lock (_semaphoreCreationLock)
            {
                if (!_fileSemaphores.ContainsKey(filePath))
                {
                    _fileSemaphores[filePath] = new SemaphoreSlim(1, 1);
                }
                return _fileSemaphores[filePath];
            }
        }

        private string FormatLogEntry(FrontendLogEntry log)
        {
            var entry = new List<string>
            {
                "================================================================================",
                $"[{log.Timestamp}] [{log.Level.ToUpper()}] [{log.Type.ToUpper()}]",
                $"URL: {log.Url}",
                $"Message: {log.Message}"
            };

            if (!string.IsNullOrEmpty(log.UserAgent))
            {
                entry.Add($"User Agent: {log.UserAgent}");
            }

            if (log.NetworkDetails != null)
            {
                entry.Add("Network Details:");
                entry.Add($"  Method: {log.NetworkDetails.Method}");
                entry.Add($"  URL: {log.NetworkDetails.Url}");
                entry.Add($"  Status: {log.NetworkDetails.Status} {log.NetworkDetails.StatusText}");
                
                if (log.NetworkDetails.RequestBody != null)
                {
                    entry.Add($"  Request Body: {JsonSerializer.Serialize(log.NetworkDetails.RequestBody)}");
                }
                
                if (log.NetworkDetails.ResponseBody != null)
                {
                    var responseStr = JsonSerializer.Serialize(log.NetworkDetails.ResponseBody);
                    if (responseStr.Length > 1000)
                    {
                        responseStr = responseStr.Substring(0, 1000) + "... (truncated)";
                    }
                    entry.Add($"  Response Body: {responseStr}");
                }
            }

            if (!string.IsNullOrEmpty(log.Stack))
            {
                entry.Add("Stack Trace:");
                entry.Add(log.Stack);
            }

            if (log.Details != null)
            {
                var detailsStr = JsonSerializer.Serialize(log.Details, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                entry.Add($"Additional Details: {detailsStr}");
            }

            entry.Add("");
            
            return string.Join(Environment.NewLine, entry) + Environment.NewLine;
        }
    }

    public class FrontendLogRequest
    {
        public List<FrontendLogEntry> Logs { get; set; } = new();
    }

    public class FrontendLogEntry
    {
        public string Timestamp { get; set; } = "";
        public string Level { get; set; } = "info";
        public string Type { get; set; } = "console";
        public string Message { get; set; } = "";
        public object? Details { get; set; }
        public string? Url { get; set; }
        public string? UserAgent { get; set; }
        public string? Stack { get; set; }
        public NetworkDetails? NetworkDetails { get; set; }
    }

    public class NetworkDetails
    {
        public string? Method { get; set; }
        public string? Url { get; set; }
        public int? Status { get; set; }
        public string? StatusText { get; set; }
        public object? RequestBody { get; set; }
        public object? ResponseBody { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}