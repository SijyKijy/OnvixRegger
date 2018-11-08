using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool Onvix_Reg(string login, string pass, string email, string captcha, int codeNum, string[] promo, string url)
        {
            using (HttpRequest client = new HttpRequest
            {
                UserAgent = "Mozilla/5.0 (Linux; Android 5.0; SM-G900P Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Mobile Safari/537.36"
            })
            {
                RequestParams content = new RequestParams()
                {
                    ["user[promo]"] = promo[codeNum],
                    ["user[name]"] = login,
                    ["user[email]"] = email,
                    ["user[password]"] = pass,
                    ["user[captcha]"] = captcha
                };

                HttpResponse post = client.Post(url + "users", content);

                content = new RequestParams()
                {
                    ["user[email]"] = email,
                    ["user[password]"] = pass,
                    ["user[remember_me]"] = "true",
                    ["mobile"] = "true",
                    ["redirect_to"] = "/m/"
                };

                try
                {
                    return client.Post(url + "users/sign_in.json", content).IsOK;
                }
                catch (HttpException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Проверить сайт на доступность
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>True - К сайту удалось подключиться. False - не удалось.</returns>
        public bool CheckSiteOnUrl(string url)
        {
            using (HttpRequest client = new HttpRequest
            {
                UserAgent = "Mozilla/5.0 (Linux; Android 5.0; SM-G900P Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Mobile Safari/537.36"
            })
            {
                if (client.Get(url).StatusCode == HttpStatusCode.OK)
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
        public List<string> GetUrlsFromSite()
        {
            using (HttpRequest client = new HttpRequest
            {
                UserAgent = "Mozilla/5.0 (Linux; Android 5.0; SM-G900P Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Mobile Safari/537.36"
            })
                try
                {
                    string result = client.Get($"{Settings.BaseSite}/onvix/urls.php").ToString();

                    return result.Split(';').ToList();
                }
                catch (Exception)
                {
                    return null;
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
