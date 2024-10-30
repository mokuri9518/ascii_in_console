using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

Console.BackgroundColor = ConsoleColor.Black;
int width = Math.Min(900, Console.LargestWindowWidth);
int height = Math.Min(450, Console.LargestWindowHeight);

Console.SetWindowSize(width, height);
Console.Title = "ascii art";
const string dir = @"./data";
Process.Start("powershell.exe", "-command \"Set-ItemProperty 'HKCU:Console' -Name 'FontSize' -Value 1;\"");
if (!Directory.Exists(dir))
{
    Directory.CreateDirectory(dir);
    Console.WriteLine("create ./data success");
}

if (!File.Exists("ascii.json"))
{
    File.WriteAllText("ascii.json",
        """
        {
          "pictures": [
            {
              "file": "example.png",
              "colors": {
                "fore": "White",
                "background": "Black"
              },
              "delay": 0
            },
            {
              "file": "example2.png",
              "colors": {
                "fore": "Yellow",
                "background": "Blue"
              },
              "delay": 5
            }
          ]
        }
        
        """);
    Console.Beep();
    Console.WriteLine("create ascii.json success");
    Console.WriteLine("please set the ascii.json");
    Console.WriteLine("press...");
    Console.ReadKey();
}

READFILE:
var photoConfig = File.ReadAllText("ascii.json");
if (string.IsNullOrEmpty(photoConfig))
{
    Console.Beep();
    Console.WriteLine("please set the ascii.json");
    Console.WriteLine("press...");
    Console.ReadKey();
    goto READFILE;
}

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true // 忽略屬性名稱的大小寫
};
// 反序列化 JSON
var config = JsonSerializer.Deserialize<Dictionary<string, List<PictureConfig>>>(photoConfig);

if (config == null )
{
    Console.WriteLine("Failed to load configuration.");
    return;
}


//Console.SetWindowSize(150, 100);
if (config != null && config.TryGetValue("pictures", out var pictures))
    foreach (var picture in pictures)
{
    try
    {
        // 設定顏色
        if (picture.colors != null)
        {
            if (!string.IsNullOrEmpty(picture.colors.fore))
                Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), picture.colors.fore, true);

            if (!string.IsNullOrEmpty(picture.colors.background))
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), picture.colors.background, true);
        }
        // 加載圖片
        Bitmap pika = new Bitmap(Path.Combine(dir, picture.file));
        Bitmap graypika = new Bitmap(pika.Width, pika.Height);

        for (int y = 0; y < pika.Height; y++)
        {
            for (int x = 0; x < pika.Width; x++)
            {
                Color origin = pika.GetPixel(x, y);
                int R = origin.R;
                int G = origin.G;
                int B = origin.B;
                int grayScale = (Math.Max(Math.Max(R, G), B) + Math.Min(R, Math.Min(G, B))) / 2;
                graypika.SetPixel(x, y, Color.FromArgb(origin.A, grayScale, grayScale, grayScale));

                if (x * 2 % 4 == 0 && y * 2.5 % 5 == 0)
                {
                    if (origin.A <= 128)
                    {
                        Console.Write("  ");
                        continue;
                    }
                    if (R + G + B > 128 * 5)
                        Console.Write("■");
                    else if (R + G + B > 128 * 4)
                        Console.Write("■");
                    else if (R + G + B > 128 * 3.4)
                        Console.Write('●');
                    else if (R + G + B > 128 * 3.11)
                        Console.Write("◆");
                    else if (R + G + B > 128 * 2.5)
                        Console.Write("▲");
                    else if (R + G + B > 128 * 1)
                        Console.Write("88");
                    else if (R + G + B > 64)
                        Console.Write("[]");
                    else if (R + G + B > 10)
                        Console.Write("||");
                    else
                        Console.Write("::");
                }
            }
            if (y * 2.5 % 5 == 0)
            {
                Console.WriteLine();
                if (picture.delay.HasValue)
                {
                        if (picture.delay.Value < 0)
                            throw new Exception($"delay cannot be minus (delay: {picture.delay.Value})");
                    Thread.Sleep(picture.delay.Value);
                    
                }
            }
        }
        graypika.Dispose();
    }
    catch (Exception e)
    {
        Console.Beep();
            if (e.Message.Contains("Parameter is not valid."))
                Console.WriteLine($"Error processing:{picture.file} not found");


            else
                Console.WriteLine($"Error processing {picture.file}: {e.Message}");
            Console.WriteLine("press...");
            Console.ReadKey();
        }
        Console.WriteLine("\f");
}
Console.WriteLine("Process done\npress...");
Console.ReadKey();

Thread.Sleep(-1);


public class PictureConfig
{
    public string file { get; set; }
    public Colors? colors { get; set; }
    public int? delay { get; set; }
}

public class Colors
{
    public string? fore { get; set; }
    public string? background { get; set; }
}
