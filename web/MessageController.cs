using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;
    
    public MessageController(MessageService messageService)
    {
        _messageService = messageService;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
    {
        var result = await _messageService.SendMessageToClientAsync(
            request.TargetClientId, 
            request.MessageContent);
        
        if (result)
        {
            return Ok(new { success = true, message = "消息发送成功" });
        }
        else
        {
            return BadRequest(new { success = false, message = "消息发送失败" });
        }
    }
}

public class MessageRequest
{
    public string TargetClientId { get; set; }
    public string MessageContent { get; set; }
}