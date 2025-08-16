    using Microsoft.AspNetCore.Http;
    using Nop.Core.Domain.Topics;
    using Nop.Core.Events;
    using Nop.Services.Common;
    using Nop.Services.Events;

    namespace Nop.Plugin.Zimaltec.CustomPages.EventConsumers;

    // Escuchamos inserción y actualización de Topic
    public class TopicTemplateFlagConsumer :
        IConsumer<EntityInsertedEvent<Topic>>,
        IConsumer<EntityUpdatedEvent<Topic>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericAttributeService _ga;
        private const string GA_KEY = "CustomPages.IsTemplate";
        private const string FORM_KEY = "cp-isTemplate";

        public TopicTemplateFlagConsumer(IHttpContextAccessor httpContextAccessor,
            IGenericAttributeService ga)
        {
            _httpContextAccessor = httpContextAccessor;
            _ga = ga;
        }

        public Task HandleEventAsync(EntityInsertedEvent<Topic> eventMessage)
        {
            return SaveFromFormAsync(eventMessage.Entity);
        }

        public Task HandleEventAsync(EntityUpdatedEvent<Topic> eventMessage)
        {
            return SaveFromFormAsync(eventMessage.Entity);
        }

        private async Task SaveFromFormAsync(Topic topic)
        {
            var form = _httpContextAccessor.HttpContext?.Request?.HasFormContentType == true
                ? _httpContextAccessor.HttpContext.Request.Form
                : null;

            if (form == null)
                return; // No venimos de una pantalla con form (p.ej. import jobs)

            // El hidden siempre envía algo. Si el checkbox está marcado, vendrá "true".
            var raw = form[FORM_KEY].ToString();
            var isTemplate =
                string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(raw, "on",   StringComparison.OrdinalIgnoreCase) ||
                raw == "1";

            await _ga.SaveAttributeAsync(topic, GA_KEY, isTemplate);
        }
    }