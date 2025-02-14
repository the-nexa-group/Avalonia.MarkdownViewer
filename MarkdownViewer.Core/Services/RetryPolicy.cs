using System;
using System.Threading.Tasks;

namespace MarkdownViewer.Core.Services
{
    public class RetryPolicy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _delay;
        private readonly Action<Exception, int>? _onRetry;

        public RetryPolicy(
            int maxRetries = 3,
            int delayMilliseconds = 1000,
            Action<Exception, int>? onRetry = null
        )
        {
            _maxRetries = maxRetries;
            _delay = TimeSpan.FromMilliseconds(delayMilliseconds);
            _onRetry = onRetry;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            var attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return await action();
                }
                catch (Exception ex) when (attempts <= _maxRetries)
                {
                    _onRetry?.Invoke(ex, attempts);
                    await Task.Delay(_delay);
                }
            }
        }
    }
}
