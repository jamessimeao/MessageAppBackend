using Microsoft.AspNetCore.SignalR;

namespace Message.SignalR
{
    public class HubFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMessageAsync
        (
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object>> next
        )
        {
            return next(invocationContext);
        }
    }
}
