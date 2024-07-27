Console.Title = "Text Replacement Console App";

var directoryPath = args.Length > 0 ? args[0] : @"c:\temp";
var searchText = args.Length > 1 ? args[1] : "FooBarFoo";
var replaceText = args.Length > 2 ? args[2] : "FooBarFoo-1";

Console.WriteLine($"""
    Searching all code in '{directoryPath}'.
    Replacing '{searchText}' with '{replaceText}' ...
    Start Time ..: {DateTime.UtcNow.ToLocalTime()}
    """);

var res = Common.ReplaceTextInFiles(directoryPath, searchText, replaceText);

Console.WriteLine($"""
    End Time ....: {DateTime.UtcNow.ToLocalTime()}
    Text replacement completed.
    {res}
    """);

// Console.ReadKey();
