using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnvixRegger.Engine
{
    class MainMethods
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Регистрация на сайте Onvix
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="pass">Пароль пользователя</param>
        /// <param name="email">Email пользователя</param>
        /// <param name="captcha">Символы капчи</param>
        /// <param name="codeNum">Номер кода регистрации</param>
        /// <param name="promo">Коды регистрации</param>
        /// <param name="url">Зеркало сайта</param>
        public async void Onvix_Reg(string login, string pass, string email, string captcha, int codeNum, string[] promo, string url)
        {       
            using (HttpClient client = new HttpClient())
            {

                var values = new Dictionary<string, string>
                {
                    { "user[promo]", promo[codeNum]},
                    { "user[name]", login},
                    { "user[email]", email},
                    { "user[password]", pass},
                    { "user[captcha]", captcha}
                };

                var content = new FormUrlEncodedContent(values);

                var post = await client.PostAsync(url+"users", content);
            }
        }

        /// <summary>
        /// Проверить сайт на доступность
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>True - К сайту удалось подключиться. False - не удалось.</returns>
        public bool CheckSiteOnUrl(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                if (client.GetAsync(url).Result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Получить зеркала сайта Onvix
        /// </summary>
        /// <returns>Список зеркал</returns>
        public async Task<List<string>> GetUrlsFromSite()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string result = await client.GetAsync($"{Settings.BaseSite}/onvix/urls.php").Result.Content.ReadAsStringAsync();

                    return result.Split(';').ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Сгенерировать случайную последовательность букв и цифр
        /// </summary>
        /// <param name="length">Длина</param>
        /// <returns>Случайную строку символов из букв и цифр</returns>
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
