using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace CSDiscordService
{
    public class PlainTextInputFormatter : TextInputFormatter
    {
        private static readonly UTF8Encoding _utf8Encoder = new(false, true);

        public PlainTextInputFormatter()
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add("text/plain");
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        protected override object GetDefaultValueForType(Type modelType)
        {
            return null;
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return new List<string>() { "text/plain" };
        }

        public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var content = await context.HttpContext.Request.BodyReader.ReadAsync();
            var encoded = _utf8Encoder.GetString(content.Buffer);
            return await InputFormatterResult.SuccessAsync(encoded);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var content = await context.HttpContext.Request.BodyReader.ReadAsync();
            var encoded = _utf8Encoder.GetString(content.Buffer);
            return await InputFormatterResult.SuccessAsync(encoded);
        }
    }
}
