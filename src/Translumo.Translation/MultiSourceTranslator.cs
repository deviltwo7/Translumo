using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Translation.Exceptions;

namespace Translumo.Translation
{
    public sealed class MultiSourceTranslator : ITranslator
    {
        private readonly IReadOnlyList<ITranslator> _translators;
        private readonly ILogger _logger;

        public MultiSourceTranslator(IEnumerable<ITranslator> translators, ILogger logger)
        {
            if (translators == null)
            {
                throw new ArgumentNullException(nameof(translators));
            }

            _translators = translators.ToList();
            if (_translators.Count == 0)
            {
                throw new ArgumentException("Multi-source translator requires at least one translator.", nameof(translators));
            }

            _logger = logger;
        }

        public async Task<string> TranslateTextAsync(string sourceText)
        {
            var results = new List<string>();
            Exception lastException = null;

            foreach (var translator in _translators)
            {
                try
                {
                    var result = await translator.TranslateTextAsync(sourceText);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        results.Add(result);
                        var best = GetBestResult(results);
                        if (!string.IsNullOrWhiteSpace(best))
                        {
                            return best;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger?.LogWarning(ex, "Multi-source translator failed: {Translator}", translator.GetType().Name);
                }
            }

            if (results.Count > 0)
            {
                return results[0];
            }

            throw new TranslationException("All translators failed.", lastException);
        }

        private static string GetBestResult(IEnumerable<string> results)
        {
            var matches = results
                .Where(result => !string.IsNullOrWhiteSpace(result))
                .Select(result => result.Trim())
                .GroupBy(result => result, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (matches == null)
            {
                return null;
            }

            return matches.Count() >= 2 ? matches.Key : null;
        }
    }
}
