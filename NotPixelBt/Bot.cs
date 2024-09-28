using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V127.HeapProfiler;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotPixelBt
{
    public class Bot
    {
        public int Clicks;
        public bool Loged;
        public ChromeDriver Driver;

        public Bot() 
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            Clicks = 0;
            Loged = false;
            Driver = new ChromeDriver(options);
        }

        public void StartBot()
        {
            Console.WriteLine("Запуск страницы авторизации");

            Driver.Navigate().GoToUrl("https://web.telegram.org/a/");

            Loged = IsLoged();

            while (Loged == false)
                Loged = IsLoged();
            if (Loged)
                Logger.LogInfo("Авторизация пройдена");

            Console.WriteLine("Запустите приложение, кликните на пиксель и нажмите enter для продолжения");
            Console.ReadLine();
            Logger.LogInfo("Запуск бота");

            Initilize();
        }
        public void RestartBot()
        {
            try
            {
                Logger.LogWarning("Бот крашнулся идет перезапуск");
                Logger.LogInfo("Обновление страницы");
                Driver.Navigate().Refresh();

                WebDriverWait webDriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(120));

                IWebElement chatScroll = webDriverWait.Until(x => x.FindElement(By.Id("LeftColumn-main")));
                IWebElement NotPixelChat = null;

                Logger.LogInfo("Поиск чата");
                while (NotPixelChat == null)
                {
                    Thread.Sleep(2000);
                    List<IWebElement> chats = chatScroll.FindElements(By.TagName("a")).Where(x => x.Text != "").ToList();
                    foreach (IWebElement chat in chats)
                    {
                        string chatTitle = chat.FindElement(By.TagName("h3")).Text;
                        if (chatTitle.Contains("Not Pixel"))
                        {
                            NotPixelChat = chat;
                            NotPixelChat.Click();
                            break;
                        }
                    }
                    if (NotPixelChat != null)
                    {
                        Logger.LogInfo("Чат найден");
                        break;
                    }


                    chats.Last().Click();
                    chats.Last().SendKeys(Keys.PageDown);

                    Thread.Sleep(2000);
                }

                Logger.LogInfo("Запуск приложения");

                IWebElement messageWindow = webDriverWait.Until(x => x.FindElement(By.Id("MiddleColumn")));
                List<IWebElement> buttons = messageWindow.FindElements(By.TagName("button")).ToList();
                IWebElement goButton = buttons.First(x => x.Text == "start");

                goButton.Click();

                Logger.LogInfo("Ожидание полной загрузки");

                Thread.Sleep(10000);

                Initilize();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                RestartBot();
            }
        }
        private void Initilize()
        {
            try
            {
                WebDriverWait webDriverWait = new WebDriverWait(Driver,TimeSpan.FromSeconds(120));
                webDriverWait.Until(x => x.SwitchTo().Frame(0));

                Logger.LogInfo("Захват холста");
                IWebElement gameCanvas = webDriverWait.Until(x => x.FindElement(By.Id("canvasHolder")));

                gameCanvas.Click();
                
                IWebElement energyButton = webDriverWait.Until(x => x.FindElement(By.ClassName("_button_text_hqiqj_171") ));
                
                Console.WriteLine("Нажмите Enter что бы выйти");

                ConsoleKeyInfo cki = new ConsoleKeyInfo();
                while (true)
                {
                    Thread.Sleep(500);
                    if (Console.KeyAvailable == true)
                    {
                        cki = Console.ReadKey(true);

                        if (cki.Key == ConsoleKey.Enter)
                        {
                            DisableBot();
                            break;
                        }
                    }
                    Thread.Sleep(100);
                    if (energyButton.Text != "No energy")
                    {
                        PaintPixel(gameCanvas, energyButton);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                RestartBot();
            }
            
        }

        private void PaintPixel(IWebElement gameCanvas, IWebElement energyButton)
        {
            int height = int.Parse(gameCanvas.GetAttribute("height"));
            int width = int.Parse(gameCanvas.GetAttribute("width"));
            
            Random rnd = new Random();
            int ofsetX = rnd.Next(1,width);
            int ofsetY = rnd.Next(1,height)*-1;
            
            Actions actions = new Actions(Driver);
            actions.MoveToElement(gameCanvas,ofsetX,ofsetY).DoubleClick();
            
            //gameCanvas.Click();
            energyButton.Click();
            Clicks++;
            Logger.LogInfo($"Раскрашен пиксель X:{ofsetX} Y:{ofsetY}\n" +
                $"Сделано кликов {Clicks}");
        }

        private void DisableBot()
        {
            Driver.Quit();
            Console.WriteLine($"Собрано {Clicks}");
        }
        private bool IsLoged()
        {
            IWebElement element = Driver.FindElement(By.TagName("h1"));

            if (element.Text == "Log in to Telegram by QR Code") return false;

            return true;
        }
        public void ScrollTo(int xPosition = 0, int yPosition = 0)
        {
            
            var js = String.Format("window.scrollTo({0}, {1})", xPosition, yPosition);
            Driver.ExecuteScript(js);
        }
    }
}
