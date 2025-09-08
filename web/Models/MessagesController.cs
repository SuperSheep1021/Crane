using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace web.Models 
{

    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        // POST: api/messages
        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] ClientMessageRequest request)
        {
            try
            {
                // 验证请求
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 处理消息
                var result = await _messageService.ProcessClientMessageAsync(request);

                return Ok(new
                {
                    success = true,
                    messageId = result.MessageId,
                    processedAt = result.ProcessedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // GET: api/messages/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageStatus(string id)
        {
            try
            {
                var status = await _messageService.GetMessageStatusAsync(id);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }

    public class ClientMessageRequest
    {
        public string conversationId;
        public string senderId;
        public string msgType="text";
        public string content;

        public Dictionary<string, object> metadata { get; set; } = new Dictionary<string, object>();
    }

    public class MessageProcessingResult
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Response { get; set; }
    }

}
