#nullable enable
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

#pragma warning disable IDE0060 // Remove unused parameter
namespace CSDiscordService.Eval.ResultModels
{
    public class ConsoleLikeStringWriter : StringWriter
    {
        private ConsoleKeyInfo _readKeyValue = new('a', ConsoleKey.A, false, false, false);
        private string _readLineValue = $"line{Environment.NewLine}";


        public ConsoleColor BackgroundColor { get; set; } = Console.BackgroundColor;
        public int BufferHeight { get; set; }
        public int BufferWidth { get; set; }

        [SupportedOSPlatform("windows")]
        public bool CapsLock => Console.CapsLock;
        public int CursorLeft { get; set; }
        public int CursorSize { get; set; }
        public int CursorTop { get; set; }
        [SupportedOSPlatform("windows")]
        public bool CursorVisible { get; set; }
        public TextWriter Error { get; set; }
        public ConsoleColor ForegroundColor { get; set; } = Console.ForegroundColor;
        public TextWriter In { get; set; } = new StringWriter();
        public Encoding InputEncoding { get; set; } = Encoding.UTF8;
        public TextWriter Out { get; set; }
        public Encoding OutEncoding { get; set; }
        [SupportedOSPlatform("windows")]
        public string Title { get; set; } = "This is a Console, I promise!";
        public bool TreateControlCAsInput { get; set; } = Console.TreatControlCAsInput;
        public int WindowHeight { get; set; }
        public int WindowLeft { get; set; }
        public int WindowTop { get; set; }
        public int WindowWidth { get; set; }

        public event ConsoleCancelEventHandler? CancelKeyPress { add { } remove { } }

        public ConsoleLikeStringWriter(StringBuilder builder) : base(builder)
        {
            Error = this;
            Out = this;
            OutEncoding = this.Encoding;
        }

        public void SetReadKeyValue(ConsoleKeyInfo value)
        {
            _readKeyValue = value;
        }

        public void SetReadLineValue(string line)
        {
            _readLineValue = $"{line}{Environment.NewLine}";
        }

        public void Beep() { }

        public void Beep(int a, int b) { }

        public void Clear()
        {
            base.GetStringBuilder().Clear();
        }
        public (int Left, int Top) GetCursorPosition()
        {
            return (CursorLeft, CursorTop);
        }

        public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) { }

        public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor) { }

        public Stream OpenStandardError() => new MemoryStream();

        public Stream OpenStandardError(int bufferSize) => new MemoryStream(bufferSize);

        public Stream OpenStandardInput() => new MemoryStream();

        public Stream OpenStandardInput(int bufferSize) => new MemoryStream(bufferSize);

        public Stream OpenStandardOutput() => new MemoryStream();

        public Stream OpenStandardOutput(int bufferSize) => new MemoryStream(bufferSize);

        public int Read() => -1;

        public ConsoleKeyInfo ReadKey() => _readKeyValue;

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (intercept)
            {
                Write(_readKeyValue.KeyChar);
            }
            return ReadKey();
        }

        public string ReadLine() => _readLineValue;

        public void ResetColor()
        {
            ForegroundColor = Console.ForegroundColor;
            BackgroundColor = Console.BackgroundColor;
        }

        public void SetBufferSize(int width, int height)
        {
            BufferWidth = width;
            BufferHeight = height;
        }

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public void SetError(TextWriter wr)
        {
            Error = wr;
        }

        public void SetIn(TextWriter wr)
        {
            In = wr;
            InputEncoding = wr.Encoding;
        }

        public void SetOut(TextWriter wr)
        {
            Out = wr;
            OutEncoding = wr.Encoding;
        }

        public void SetWindowPosition(int left, int top)
        {
            WindowLeft = left;
            WindowTop = top;
        }

        public void SetWindowSize(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
        }

    }
}

#pragma warning restore IDE0060 // Remove unused parameter