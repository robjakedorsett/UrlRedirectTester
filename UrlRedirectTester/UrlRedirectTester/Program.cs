// See https://aka.ms/new-console-template for more information

using UrlRedirectTester;

if (args.Any())
{

    await UrlTesterService.TestUrlsAsync(args[0], args[1]);

}
else
{
    Console.WriteLine("Please Enter Base Url for the OLD Urls");
    Console.WriteLine();
    var baseUrl = Console.ReadLine();

    Console.WriteLine("Please Enter the CSV File Name");
    Console.WriteLine();
    var fileName = Console.ReadLine();

    if (baseUrl == null || fileName == null)
        Console.WriteLine("Base url and file name are both required.");
    else
        await UrlTesterService.TestUrlsAsync(baseUrl, fileName);
}