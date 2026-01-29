using System.Text;
using KShell;

History.Initialize();

var cursor = new History.Cursor();

while (true)
{
    Console.Write("$ ");

    var input = new StringBuilder();
    var enterPressed = false;
    var previousStroke = ConsoleKey.None;
    while (!enterPressed)
    {
        var keyInfo = Console.ReadKey(true);
        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter:
                enterPressed = true;
                Console.Write('\n');
                break;

            case ConsoleKey.Backspace:
                if (input.Length == 0)
                    break;

                input.Remove(input.Length - 1, 1);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write(' ');
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

                cursor = new History.Cursor();

                break;

            case ConsoleKey.Tab:
                if (input.Length == 0)
                    break;

                Autocomplete.PrintMatches(input, previousStroke);

                cursor = new History.Cursor();

                break;

            case ConsoleKey.UpArrow:
                if (!cursor.Active)
                {
                    cursor.Active = true;
                    cursor.Prefix = input.ToString();
                    cursor.Index = History.All.Count;
                }

                var upOption = History.All
                    .Select((value, index) => (value, index))
                    .LastOrDefault(o =>
                        o.index < cursor.Index && o.value.StartsWith(cursor.Prefix));

                if (upOption.value is null)
                    break;

                Console.Write($"\r\e[2K$ {upOption.value}");

                input.Clear();
                input.Append(upOption.value);

                cursor.Index = upOption.index;

                break;

            case ConsoleKey.DownArrow:
                if (!cursor.Active)
                {
                    Console.Write('\a');
                    break;
                }

                var downOption = History.All
                    .Select((value, index) => (value, index))
                    .FirstOrDefault(o =>
                        o.index > cursor.Index && o.value.StartsWith(cursor.Prefix));

                if (downOption.value is null)
                    break;

                Console.Write($"\r\e[2K$ {downOption.value}");

                input.Clear();
                input.Append(downOption.value);

                cursor.Index = downOption.index;

                break;

            default:
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }

                cursor = new History.Cursor();

                break;
        }

        previousStroke = keyInfo.Key;
    }

    var command = input.ToString();
    if (command.Trim().Length == 0)
        continue;

    cursor = new History.Cursor();

    History.Add(command);
    await CommandRunner.RunAsync(command);
}