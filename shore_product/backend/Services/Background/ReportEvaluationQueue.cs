using System.Threading.Channels;

namespace ProductApi.Services.Background
{
    public class ReportEvaluationQueue
    {
        private readonly Channel<Guid> _queue;

        public ReportEvaluationQueue()
        {
            _queue = Channel.CreateBounded<Guid>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public async ValueTask EnqueueAsync(Guid reportId, CancellationToken ct) 
            => await _queue.Writer.WriteAsync(reportId, ct);

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct) 
            => _queue.Reader.ReadAllAsync(ct);
    }
}
