using LeanCloud;
using LeanCloud.Push;
using LeanCloud.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using web.Models;


namespace web.Models 
{
    public class MessageService
    {
        /// <summary>
        /// 处理客户端发送的消息
        /// </summary>
        public async Task<MessageProcessingResult> ProcessClientMessageAsync(ClientMessageRequest request)
        {
            try
            {
                // 1. 验证发送者
                if (!await ValidateSenderAsync(request.SenderId))
                {
                    Console.WriteLine("ValidateSender：false");
                    return new MessageProcessingResult
                    {
                        Success = false,
                        Response = "无效的发送者"
                    };
                }
                Console.WriteLine("ValidateSender：true");
                // 2. 保存消息到 LeanCloud
                var messageId = await SaveMessageToLeanCloudAsync(request);
                Console.WriteLine(string.Format("saveMessageToLeanCloud__{0}", messageId ));

                // 3. 根据消息类型处理消息
                var response = await ProcessMessageByTypeAsync(request);
                Console.WriteLine(string.Format("ProcessMessageByTypeAsync__{0}", messageId));

                // 4. 更新消息状态为已处理
                await MarkMessageAsProcessedAsync(messageId);
                Console.WriteLine(string.Format("MarkMessageAsProcessedAsync__{0}", messageId));


                //// 5. 可选：发送推送通知给相关用户
                //if (request.Type == "urgent")
                //{
                //    await SendNotificationAsync(request);
                //}

                return new MessageProcessingResult
                {
                    Success = true,
                    MessageId = messageId,
                    ProcessedAt = DateTime.UtcNow,
                    Response = response
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理消息时出错: {ex.Message}");
                return new MessageProcessingResult
                {
                    Success = false,
                    Response = $"处理失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 验证消息发送者
        /// </summary>
        private async Task<bool> ValidateSenderAsync(string senderId)
        {
            try
            {
                AVQuery<AVUser> query = new AVQuery<AVUser>().WhereEqualTo("objectId", senderId);
                AVUser user = await query.FirstAsync();
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存消息到 LeanCloud
        /// </summary>
        private async Task<string> SaveMessageToLeanCloudAsync(ClientMessageRequest request)
        {
            LCMessage message = new LCMessage
            {
                SenderId = request.SenderId,
                Content = request.Content,
                Type = request.MsgType,
                IsProcessed = false
            };

            // 添加元数据
            foreach (KeyValuePair<string ,object> item in request.metadata)
            {
                message[item.Key] = item.Value;
            }

            await message.SaveAsync();
            return message.ObjectId;
        }

        /// <summary>
        /// 根据消息类型处理消息
        /// </summary>
        private async Task<string> ProcessMessageByTypeAsync(ClientMessageRequest request)
        {
            switch (request.MsgType.ToLower())
            {
                case "text":
                    return await ProcessTextMessageAsync(request);

                case "command":
                    return await ProcessCommandMessageAsync(request);

                case "location":
                    return await ProcessLocationMessageAsync(request);

                case "image":
                    return await ProcessImageMessageAsync(request);

                default:
                    return $"收到 {request.MsgType} 类型消息";
            }
        }

        /// <summary>
        /// 处理文本消息
        /// </summary>
        private async Task<string> ProcessTextMessageAsync(ClientMessageRequest request)
        {
            // 这里可以添加自然语言处理或关键词检测
            if (request.Content.Contains("帮助"))
            {
                return "这是帮助信息：...";
            }
            Console.WriteLine($"已收到您的消息: {request.Content}");
            // 默认响应
            return $"已收到您的消息: {request.Content}";
        }

        /// <summary>
        /// 处理命令消息
        /// </summary>
        private async Task<string> ProcessCommandMessageAsync(ClientMessageRequest request)
        {
            var command = request.Content.ToLower();

            switch (command)
            {
                case "status":
                    return "系统运行正常";

                case "users":
                    var count = await GetUserCountAsync();
                    return $"当前用户数: {count}";

                default:
                    return $"未知命令: {command}";
            }
        }

        /// <summary>
        /// 处理位置消息
        /// </summary>
        private async Task<string> ProcessLocationMessageAsync(ClientMessageRequest request)
        {
            if (request.metadata.TryGetValue("latitude", out var latObj) &&
                request.metadata.TryGetValue("longitude", out var lngObj))
            {
                // 处理位置信息
                return $"已收到位置信息: {latObj}, {lngObj}";
            }

            return "位置信息不完整";
        }

        /// <summary>
        /// 处理图片消息
        /// </summary>
        private async Task<string> ProcessImageMessageAsync(ClientMessageRequest request)
        {
            if (request.metadata.TryGetValue("url", out var urlObj))
            {
                // 处理图片
                return $"已收到图片: {urlObj}";
            }

            return "图片信息不完整";
        }

        /// <summary>
        /// 标记消息为已处理
        /// </summary>
        private async Task MarkMessageAsProcessedAsync(string messageId)
        {
            var message = AVObject.CreateWithoutData("AllMessage", messageId) as LCMessage;
            message.IsProcessed = true;
            await message.SaveAsync();
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        private async Task SendNotificationAsync(ClientMessageRequest request)
        {
            //try
            //{
            //    var push = new AVPush();
            //    push.Alert = $"新消息来自用户: {request.SenderId}";
            //    push.Data = new Dictionary<string, object>
            //{
            //    { "type", "new_message" },
            //    { "senderId", request.SenderId },
            //    { "timestamp", DateTime.UtcNow }
            //};

            //    // 发送给管理员或其他相关用户
            //    push.Query = new AVQuery<AVInstallation>()
            //        .WhereEqualTo("channels", "admin");

            //    await push.SendAsync();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"发送通知失败: {ex.Message}");
            //}
        }

        /// <summary>
        /// 获取消息状态
        /// </summary>
        public async Task<object> GetMessageStatusAsync(string messageId)
        {
            try
            {
                var query = new AVQuery<LCMessage>()
                    .WhereEqualTo("objectId", messageId);

                var message = await query.FirstAsync();

                if (message == null)
                    return new { found = false };

                return new
                {
                    found = true,
                    messageId = message.ObjectId,
                    senderId = message.SenderId,
                    content = message.Content,
                    type = message.Type,
                    isProcessed = message.IsProcessed,
                    createdAt = message.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// 获取用户数量
        /// </summary>
        private async Task<int> GetUserCountAsync()
        {
            var query = new AVQuery<AVUser>();
            return await query.CountAsync();
        }
    }
}
