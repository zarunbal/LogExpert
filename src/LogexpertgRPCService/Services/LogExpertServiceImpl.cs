using Grpc.Core;

using LogExpert.Grpc;

namespace LogexpertGRPCService.Services
{
    public class LogExpertServiceImpl : LogExpertService.LogExpertServiceBase
    {
        public override Task<LogReply> SendLog(LogRequest request, ServerCallContext context)
        {
            return Task.FromResult(new LogReply { Result = "Hello " + request.Message });
        }

        public override Task<FilesReply> LoadFiles(FileNames fileNames, ServerCallContext context)
        {
            return Task.FromResult(new FilesReply { Success = true });
        }

        public override Task<FilesReply> NewWindow(FileNames fileNames, ServerCallContext context)
        {
            return Task.FromResult(new FilesReply { Success = true });
        }

        public override Task<FilesReply> NewWindowOrLockedWindow(FileNames fileNames, ServerCallContext context)
        {
            return Task.FromResult(new FilesReply { Success = true });
        }

        //public override void WindowClosed()
        //{

        //}
    }
}
