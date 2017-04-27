using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace CSDiscordService
{
    public class PlainTextInputFormatter : TextInputFormatter
    {
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
            var reader = context.ReaderFactory.Invoke(context.HttpContext.Request.Body, Encoding.UTF8);
            return await InputFormatterResult.SuccessAsync(await reader.ReadToEndAsync());
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var reader = context.ReaderFactory.Invoke(context.HttpContext.Request.Body, encoding);
            return await InputFormatterResult.SuccessAsync(await reader.ReadToEndAsync());
        }
    }
}
