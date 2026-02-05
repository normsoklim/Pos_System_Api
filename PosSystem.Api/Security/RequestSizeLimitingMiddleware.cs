using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PosSystem.Api.Security
{
    public class RequestSizeLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _maxRequestBodySize;

        public RequestSizeLimitingMiddleware(RequestDelegate next, long maxRequestBodySize = 10 * 1024 * 1024) // 10MB default
        {
            _next = next;
            _maxRequestBodySize = maxRequestBodySize;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check content length header if available
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > _maxRequestBodySize)
            {
                context.Response.StatusCode = 413; // Request Entity Too Large
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Request too large",
                    message = $"Request body exceeds the maximum allowed size of {_maxRequestBodySize / (1024 * 1024)} MB"
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

              // For chunked requests or when Content-Length is not specified, 
              // we'll wrap the request body stream to monitor its size
              if (context.Request.Body.CanRead)
              {
                  var originalBody = context.Request.Body;
                  
                  try
                  {
                      // Check if we need to wrap the stream for size monitoring
                      if (context.Request.ContentLength == null)
                      {
                          var monitoredStream = new SizeLimitingStream(originalBody, _maxRequestBodySize);
                          context.Request.Body = monitoredStream;
                      }
                      else
                      {
                          // If Content-Length is specified, check it first before proceeding
                          if (context.Request.ContentLength > _maxRequestBodySize)
                          {
                              context.Response.StatusCode = 413; // Request Entity Too Large
                              context.Response.ContentType = "application/json";
                              
                              var response = new
                              {
                                  error = "Request too large",
                                  message = $"Request body exceeds the maximum allowed size of {_maxRequestBodySize / (1024 * 1024)} MB"
                              };
                              
                              await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                              return;
                          }
                      }
                      
                      await _next(context);
                      
                      // Check if the stream exceeded the limit during processing
                      if (context.Request.Body is SizeLimitingStream sizeStream && sizeStream.IsSizeExceeded)
                      {
                          context.Response.StatusCode = 413; // Request Entity Too Large
                          context.Response.ContentType = "application/json";
                          
                          var response = new
                          {
                              error = "Request too large",
                              message = $"Request body exceeds the maximum allowed size of {_maxRequestBodySize / (1024 * 1024)} MB"
                          };
                          
                          await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                          return;
                      }
                  }
                  finally
                  {
                      // Restore original body stream
                      context.Request.Body = originalBody;
                  }
              }
             else
             {
                 await _next(context);
             }
        }
    }

    // Custom stream wrapper to monitor size
    public class SizeLimitingStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly long _maxSize;
        private long _currentSize = 0;
        
        public bool IsSizeExceeded { get; private set; } = false;

        public SizeLimitingStream(Stream innerStream, long maxSize)
        {
            _innerStream = innerStream;
            _maxSize = maxSize;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position 
        { 
            get => _innerStream.Position; 
            set => _innerStream.Position = value; 
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _currentSize += count;
            if (_currentSize > _maxSize)
            {
                IsSizeExceeded = true;
                throw new InvalidOperationException($"Request body exceeds the maximum allowed size of {_maxSize / (1024 * 1024)} MB");
            }
            
            _innerStream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _currentSize += count;
            if (_currentSize > _maxSize)
            {
                IsSizeExceeded = true;
                throw new InvalidOperationException($"Request body exceeds the maximum allowed size of {_maxSize / (1024 * 1024)} MB");
            }
            
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }
    }
}