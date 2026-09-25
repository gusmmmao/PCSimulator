using System;
using System.IO;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string text = File.ReadAllText(@"c:\Users\gusmao\Projetos\JogoFenecan\PCSimulator\Assets\_PCSimulator\Scripts\UI\MainMenuManager.cs");
        // Remove verbatim strings
        text = Regex.Replace(text, @"@\""""([^\""""]|\""""\"""")*\""""", "");
        // Remove normal strings
        text = Regex.Replace(text, @"\""([^\""\\\\]|\\\\.)*\""", "");
        // Remove line comments
        text = Regex.Replace(text, @"//.*", "");
        // Remove block comments
        text = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        int open = 0, close = 0;
        foreach (char c in text) {
            if (c == '{') open++;
            if (c == '}') close++;
        }
        Console.WriteLine($"Open: {open}, Close: {close}");
    }
}
