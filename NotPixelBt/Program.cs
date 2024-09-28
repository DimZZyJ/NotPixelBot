using NotPixelBt;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

internal class Program
{
    private static void Main(string[] args)
    {
       Bot bot = new Bot();
       bot.StartBot();
       
       //bot.RestartBot();
    }
}