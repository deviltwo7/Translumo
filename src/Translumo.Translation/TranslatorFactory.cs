using System;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Dispatching;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Deepl;
using Translumo.Translation.Google;
using Translumo.Translation.Papago;
using Translumo.Translation.Yandex;

namespace Translumo.Translation
{
    public class TranslatorFactory
    {
        private readonly LanguageService _languageService;
        private readonly IActionDispatcher _actionDispatcher;
        private readonly ILogger _logger;

        public TranslatorFactory(LanguageService languageService, IActionDispatcher actionDispatcher, ILogger<TranslatorFactory> logger)
        {
            this._languageService = languageService;
            this._actionDispatcher = actionDispatcher;
            this._logger = logger;
        }

        public ITranslator CreateTranslator(TranslationConfiguration translatorConfiguration)
        {
            if (translatorConfiguration.Translator == Translators.MultiSource)
            {
                var translators = new[]
                {
                    CreateSingleTranslator(Translators.Deepl, translatorConfiguration),
                    CreateSingleTranslator(Translators.Google, translatorConfiguration),
                    CreateSingleTranslator(Translators.Yandex, translatorConfiguration),
                    CreateSingleTranslator(Translators.Papago, translatorConfiguration)
                };

                return new MultiSourceTranslator(translators, _logger);
            }

            return CreateSingleTranslator(translatorConfiguration.Translator, translatorConfiguration);
        }

        private ITranslator CreateSingleTranslator(Translators translator, TranslationConfiguration translatorConfiguration)
        {
            switch (translator)
            {
                case Translators.Deepl:
                    return new DeepLTranslator(translatorConfiguration, _languageService, _logger);
                case Translators.Yandex:
                    return new YandexTranslator(translatorConfiguration, _languageService, _actionDispatcher, _logger);
                case Translators.Papago:
                    return new PapagoTranslator(translatorConfiguration, _languageService, _logger);
                case Translators.Google:
                    return new GoogleTranslator(translatorConfiguration, _languageService, _logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(translator), translator, "Unknown translator");
            }
        }
    }
}
