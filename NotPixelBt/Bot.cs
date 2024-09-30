using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace NotPixelBt
{
    public class Bot
    {
        public int Clicks;
        public bool Loged;
        public SessionMode SessionMode;
        
        public LocalStorageManager StorageManager;
        public CanvasPixel CanvasPixel;
        public ChromeDriver Driver;

        public Bot()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            Clicks = 0;
            Loged = false;
            Driver = new ChromeDriver(options);
            StorageManager = new LocalStorageManager(Driver);
            SetSessionMode();
        }

        public void StartAplicationLogIn()
        {
            Console.WriteLine("Запуск страницы авторизации");

            Driver.Navigate().GoToUrl("https://web.telegram.org/a/");

            switch (SessionMode)
            {
                case SessionMode.NewSession:
                    LogInCheck();
                    Console.WriteLine("Введите имя для сессии");
                    StorageManager.SaveCurrentStorageToJson(Console.ReadLine());
                    break;
                    
                case SessionMode.LoadSession:
                    Console.WriteLine("Выберите сессию");
                    for (int i = 0; i < StorageManager.AvailableStorages.Count; i++)
                    {
                        Console.WriteLine($"{i} - {Path.GetFileName(StorageManager.AvailableStorages[i])}");
                    }
                    int index = int.Parse(Console.ReadLine());
                    StorageManager.SetCurrentLocalStorage(index);
                    Driver.Navigate().Refresh();
                    break;
                    
                case SessionMode.AnonymousSession:
                    LogInCheck();
                    break;
            }
        }


        public void RestartAplication()
        {
            try
            {
                Logger.LogWarning("Идет перезапуск приложения");
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

                RunClicker();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                RestartAplication();
            }

        }
        public void RunClicker()
        {
            try
            {
                WebDriverWait webDriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(120));

                Logger.LogInfo("Поиск фрейма");
                webDriverWait.Until(x => x.SwitchTo().Frame(0));

                Logger.LogInfo("Захват холста");
                IWebElement gameCanvas = webDriverWait.Until(x => x.FindElement(By.Id("canvasHolder")));

                gameCanvas.Click();

                Logger.LogInfo("Захват кнопки");
                IWebElement energyButton = webDriverWait.Until(x => x.FindElement(By.ClassName("_button_text_hqiqj_171")));

                while (true)
                {
                    Thread.Sleep(500);

                    if (energyButton.Text != "No energy")
                    {
                        PaintPixel(gameCanvas, energyButton);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                RestartAplication();
            }
        }

        private void PaintPixel(IWebElement gameCanvas, IWebElement energyButton)
        {
            int height = int.Parse(gameCanvas.GetAttribute("height"));
            int width = int.Parse(gameCanvas.GetAttribute("width"));

            if (CanvasPixel == null)
            {
                Random rnd = new Random();
                int ofsetX = rnd.Next(1, width);
                int ofsetY = rnd.Next(1, height) * -1;
                CanvasPixel = new CanvasPixel(ofsetX, ofsetY);
            }
            else
            {
                int ofsetX = CanvasPixel.X + 20;
                int ofsetY = CanvasPixel.Y - 20;

                if (ofsetX > width)
                    ofsetX = 50;
                if (ofsetY < height * -1)
                    ofsetY = -50;

                CanvasPixel = new CanvasPixel(ofsetX, ofsetY);
            }

            Actions actions = new Actions(Driver);
            actions.MoveToElement(gameCanvas, CanvasPixel.X, CanvasPixel.Y).Click().Build().Perform();

            //gameCanvas.Click();
            energyButton.Click();
            Clicks++;
            Logger.LogInfo($"Раскрашен пиксель X:{CanvasPixel.X} Y:{CanvasPixel.Y}\n" +
                $"Сделано кликов {Clicks}");
        }

        private void DisableBot()
        {
            Driver.Quit();
            Logger.LogInfo("Бот выключен");
            Logger.LogInfo($"Собрано {Clicks}");
        }
        private bool IsLoged()
        {
            IWebElement element = Driver.FindElement(By.TagName("h1"));

            if (element.Text == "Log in to Telegram by QR Code") return false;

            return true;
        }
        private void LogInCheck()
        {
            Loged = IsLoged();

            while (Loged == false)
                Loged = IsLoged();
            if (Loged)
                Logger.LogInfo("Авторизация пройдена");
        }
        private void SetSessionMode()
        {
            Console.WriteLine("Выберите режим новой сессии");
            Console.WriteLine
                (
                "1 - Новая Сессия \n" +
                "2 - Загрузить Сессию \n" +
                "Любая клавиша - Анонимная сессия"
                );
            string mode = Console.ReadLine();

            switch (mode)
            {
                case "1":
                    Logger.LogInfo("Выбран режим новой сессии");
                    SessionMode = SessionMode.NewSession; break;
                    
                case "2":
                    Logger.LogInfo("Выбран режим загрузки сессии");
                    SessionMode = SessionMode.LoadSession; break;
                
                default :
                    Logger.LogInfo("Выбран режим анонимной сессии");
                    SessionMode = SessionMode.AnonymousSession; break;
            }
        }
    }
}
