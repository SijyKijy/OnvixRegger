using OnvixRegger.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OnvixRegger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainMethods api = new MainMethods();
        private BinaryFormatter formatter = new BinaryFormatter();
        private static Random random = new Random((int)DateTime.Now.Ticks);

        private string[] promo;
        private string url;


        public MainWindow()
        {
            InitializeComponent();
            RunAsync();
        }

        /// <summary>
        /// Старт программы
        /// </summary>
        private async void RunAsync()
        {
            if(CheckUrls())
            {
                await GetCodes();

                statusUrlL.Content = url;

                #region десериализация
                try
                {
                    if (File.Exists("save.dat")) // Десериализация //
                    {
                        using (FileStream fs = new FileStream("save.dat", FileMode.Open))
                        {
                            SaveData save = (SaveData)formatter.Deserialize(fs);

                            Login.Text = save.Login;
                            Password.Text = save.Password;
                            Email.Text = save.Email;
                            SaveDataCheck.IsChecked = true;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Файл сохранения повреждён. Удалите или перезапишите файл.");
                }
                #endregion
            }
            else
            {
                statusUrlL.Content = "Error!";

                AuthButton.IsEnabled = false;
                FastReg.IsEnabled = false;

                MessageBox.Show("Валидные зеркала не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получение специальный кодов регистрации onvix'a с сайта
        /// </summary>
        async Task GetCodes()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var site = await client.GetAsync($"{Settings.BaseSite}/onvix/code.php").Result.Content.ReadAsStringAsync();

                    if (site.ToLower().Contains("nothing") || site.ToLower().Contains("error"))
                    {
                        throw new Exception();
                    }
                    promo = site.Split(';');
                    CodesCount.Maximum = promo.Length;
                    StatusLabel.Content = "OK!";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                }
                catch (Exception)
                {
                    StatusLabel.Content = "ERROR!";
                    StatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Ошибка соединения с базой");
                    AuthButton.IsEnabled = false;
                    FastReg.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Проверка заполнения полей регистрация
        /// </summary>
        private void Registration()
        {
            #region Проверка на заполнение полей данный
            if (Login.Text == "" || Login.Text == "Login")
            {
                MessageBox.Show("Введите логин.");
                Login.Text = "Login";
                return;
            }

            if (Password.Text == "" || Password.Text == "Password")
            {
                MessageBox.Show("Введите пароль.");
                Password.Text = "Password";
                return;
            }

            if (Email.Text == "" || Email.Text == "Email")
            {
                MessageBox.Show("Введите email.");
                Email.Text = "Email";
                return;
            }

            if (CaptchaBox.Text == "")
            {
                MessageBox.Show("Введите капчу.");
                return;
            }
            #endregion

            try
            {
                if(api.Onvix_Reg(Login.Text, Password.Text, Email.Text, CaptchaBox.Text, (int)CodesCount.Value, promo, url))
                {
                    MessageBox.Show("Вы прекрасны!" + Environment.NewLine + "Email и пароль скопированы в буфер обмена.","Всё супер!",MessageBoxButton.OK,MessageBoxImage.Information);
                    Clipboard.SetText($"{Email.Text}:{Password.Text}");
                }
                else
                {
                    MessageBox.Show("Перезапустите программу кнопкой FastReboot.","Ошибка!",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! "+ Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// Регистрация путём заполнения полей
        /// </summary>
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            Registration();
        }

        /// <summary>
        /// Регистрация путём рандомной генерации данных
        /// </summary>
        private void FastReg_Click(object sender, RoutedEventArgs e)
        {
            Login.Text = api.RandomString(random.Next(8, 18)).ToLower();
            Email.Text = api.RandomString(random.Next(8, 18)).ToLower() + "@obos.ru";
            Password.Text = api.RandomString(random.Next(8, 18)).ToLower();
            Registration();

        }

        /// <summary>
        /// Кнопка редиректа на сайт Temp-Mail.org
        /// </summary>
        private void TempMailB_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://temp-mail.org");
        }

        /// <summary>
        /// Кнопка для быстрой перезагрузки приложения
        /// </summary>
        private void FastReboot_Click(object sender, RoutedEventArgs e)
        {
            SaveData();

            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка. Возможно вы переместили программу." + Environment.NewLine + ex.Message);
                Environment.Exit(0);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Проверка и установление валидного зеркала onvix'a.
        /// </summary>
        private bool CheckUrls()
        {
            List<string> urls = api.GetUrlsFromSite(); // Получаем список зеркал с сайта

            if (urls == null) // Если по какой-то причине не получили, берём дефолтный список
            {
                urls = new List<string>() { "https://onvix.tv/", "https://onvix.co/" };
            }

            foreach (string url in urls)
            {
                if (api.CheckSiteOnUrl(url)) // Получаем ответ от каждого из зеркал
                {
                    this.url = url;

                    BitmapImage _image = new BitmapImage(); // Обновляем капчу под новый (валидный) url
                    _image.BeginInit();
                    _image.CacheOption = BitmapCacheOption.None;
                    _image.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    _image.CacheOption = BitmapCacheOption.OnLoad;
                    _image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    _image.UriSource = new Uri(url + "rucaptcha", UriKind.RelativeOrAbsolute);
                    _image.EndInit();
                    capImg.Source = _image;

                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Сохранение данных из полей ввода в файл путём бинарной сериализации
        /// </summary>
        private void SaveData()
        {
            if (SaveDataCheck.IsChecked == true) // Сериализация //
            {
                SaveData save = new SaveData();
                save.Login = Login.Text;
                save.Password = Password.Text;
                save.Email = Email.Text;

                using (FileStream fs = new FileStream("save.dat", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, save); // сериализуем весь объект save
                }
            }
        }

        #region Очистка полей ввода при клике
        private void ClearMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (Login.Text == "Login" && sender == Login)
                {
                    Login.Text = "";
                }

                if (Email.Text == "Email" && sender == Email)
                {
                    Email.Text = "";
                }

                if (Password.Text == "Password" && sender == Password)
                {
                    Password.Text = "";
                }
            }
        }
        #endregion

        private void MainW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }
    }
}
