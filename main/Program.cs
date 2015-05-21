using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

namespace main
{
    class Content
    {
        public string content = null;
    }

    internal class Anekdot
    {
        private static string GetAnekdote()
        {
            var req =
                    (HttpWebRequest)
                        WebRequest.Create("http://rzhunemogu.ru/RandJSON.aspx?CType=1");
            var resp = (HttpWebResponse)req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251));
            var content = sr.ReadToEnd();
            sr.Close();
            var contenta = JsonConvert.DeserializeObject<Content>(content);
            return contenta.content;
        }

        public static void SendAnekdote(VkApi _vk, long id)
        {
            var text = GetAnekdote();
            var random = new Random();
            var randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            var captcha = randomNumber.ToString();
            if (text == null)
            {
                _vk.Messages.Send(
                    id,
                    false,
                    "This message was send by Bot. \n\rTime of sending: " +
                    DateTime.Now.ToString("HH:mm:ss \n\r") +
                    "Message Id: " + randomNumber + "\n\r" +
                    "Text of message: \n\rI think you have made a mistake in message, please try again.",
                    "",
                    null,
                    null,
                    false,
                    null,
                    null,
                    captcha
                    );
                Console.WriteLine(">> Message sent back cuz of mistake in it " + id);
            }
            else
            {
                _vk.Messages.Send(
                    id,
                    false,
                    text,
                    "",
                    null,
                    null,
                    false,
                    null,
                    null,
                    captcha
                    );
                Console.WriteLine(">> Id sent to: " + id);
            }
        }
    }

    class Person
    {
        public long Id;
        public string first_name;
        public string last_name;
        public int hidden;
    }
    class Response
    {
        public Person response;
    }
    class Horoscope
    {
        private static string date = null;
        private static string GetHorolink(string incStr, out string horo)
        {

            var horoscopes = new Dictionary<string, string>
            {
                {"овен", "http://astroscope.ru/horoskop/ejednevniy_goroskop/aries.html"},
                {"весы", "http://astroscope.ru/horoskop/ejednevniy_goroskop/libra.html"},
                {"телец", "http://astroscope.ru/horoskop/ejednevniy_goroskop/taurus.html"},
                {"скорпион", "http://astroscope.ru/horoskop/ejednevniy_goroskop/scorpio.html"},
                {"близнецы", "http://astroscope.ru/horoskop/ejednevniy_goroskop/gemini.html"},
                {"стрелец", "http://astroscope.ru/horoskop/ejednevniy_goroskop/sagittarius.html"},
                {"рак", "http://astroscope.ru/horoskop/ejednevniy_goroskop/cancer.html"},
                {"козерог", "http://astroscope.ru/horoskop/ejednevniy_goroskop/capricorn.html"},
                {"лев", "http://astroscope.ru/horoskop/ejednevniy_goroskop/leo.html"},
                {"водолей", "http://astroscope.ru/horoskop/ejednevniy_goroskop/aquarius.html"},
                {"дева", "http://astroscope.ru/horoskop/ejednevniy_goroskop/virgo.html"},
                {"рыба", "http://astroscope.ru/horoskop/ejednevniy_goroskop/pisces.html"}
            };
            string tmp = null;
            string horoTmp = null;
            foreach (var i in incStr.Split(' '))
            {
                try
                {
                    if (incStr.Split(' ').Contains("Завтра") || (incStr.Split(' ').Contains("завтра")))
                    {
                        tmp =
                            horoscopes[i.ToLower()].Substring(0,
                                horoscopes[i.ToLower()].Length - 5) + "_zavtra.html";
                        horoTmp = i;
                        date = "for tomorrow";
                    }
                    else
                    {
                        tmp = horoscopes[i.ToLower()];
                        horoTmp = i;
                        date = "for today";
                    }
                }
                catch
                {
                    continue;
                }
            }
            horo = horoTmp;
            return tmp;
        }
        private static string GetHoroscope(string incStr, out string horoName)
        {
            var horoscope = "No Horoscope";
            var pattern = "(<div class=\"goroskop\">(.+)</div>)";
            string horoname = null;
            var horoLink = GetHorolink(incStr, out horoname);
            horoName = horoname;
            if (horoLink == null)
            {
                horoscope = null;
                return horoscope;
            }
            else
            {
                var req =
                    (HttpWebRequest)
                        WebRequest.Create(horoLink);
                var resp = (HttpWebResponse) req.GetResponse();
                var sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251));
                var content = sr.ReadToEnd();
                sr.Close();
                var matches = Regex.Matches(content, pattern);
                foreach (var match in matches.Cast<Match>().Where(match => matches.Count != 0))
                {
                    horoscope = match.Groups[2].Value;
                }
                return horoscope;
            }
        }

        public static void SendHoroscope(Message incStr, VkApi _vk)
        {

            var random = new Random();
            var randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            var captcha = randomNumber.ToString();
            string horoName = null;
            var text = GetHoroscope(incStr.Body, out horoName);
            var i = Convert.ToInt64(incStr.UserId);
            if (text == null)
            {
                _vk.Messages.Send(
                    i,
                    false,
                    "This message was send by Bot. \n\rTime of sending: " +
                    DateTime.Now.ToString("HH:mm:ss \n\r") +
                    "Message Id: " + randomNumber + "\n\r" +
                    "Text of message: \n\rI think you have made a mistake in message, please try again.",
                    "",
                    null,
                    null,
                    false,
                    null,
                    null,
                    captcha
                    );
                Console.WriteLine(">> Message sent back cuz of mistake in it " + i);
            }
            else
            {
                _vk.Messages.Send(
                    i,
                    false,
                    "This message was send by Bot. \n\rTime of sending: " +
                    DateTime.Now.ToString("HH:mm:ss \n\r") +
                    "Message Id: " + randomNumber + "\n\r" + "Text of message: \n\rYour horoscope for \""+ horoName + "\" " + date +" is: " + text,
                    "",
                    null,
                    null,
                    false,
                    null,
                    null,
                    captcha
                    );
                Console.WriteLine(">> Id sent to: " + i);
            }
        }
    }
    class ReSender
    {
        private static string SplitStr(string text) //Функция, возвращающая текст, заключенный в " "
        {                                           //из исходного сообщения
            try
            {
                text += " ";
                var flag = false;
                string result = null;
                var tmp1 = 0;
                var tmp2 = 0;
                for (var i = 1; i < text.Length; i++)
                {
                    if (text[i - 1] != '\"' || i < 1) continue;
                    else if (!flag)
                    {
                        tmp1 = i;
                        flag = true;
                    }
                    else tmp2 = i - 1;
                }
                result = text.Substring(tmp1, tmp2 - tmp1);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static List<long> GetReciepentIdList(string text) //Функция, возвращающая лист айди пользователей
        {                                                         //кому переслать сообщение
            var message = text.Split(' ');
            var listOfId = new List<long>();
            foreach (var i in message)
            {
                if (i.Contains("vk.com"))
                {
                    string nickname = null;
                    foreach (var a in i.Split('/'))
                    {
                        if (!a.Contains("http") && !a.Contains("vk.com"))
                        {
                            nickname = a;
                        }
                    }
                    if (nickname.Contains("id") && !listOfId.Contains(Convert.ToInt64(nickname)))
                    {
                        nickname = nickname.Substring(2);
                        listOfId.Add(Convert.ToInt64(nickname));
                        Console.WriteLine(">> Added id: " + nickname);
                    }
                    else
                    {
                        var req =
                            (HttpWebRequest)
                                WebRequest.Create("http://api.vk.com/method/users.get?user_ids=" + nickname + "&v=5.30");
                        var resp = (HttpWebResponse)req.GetResponse();
                        var sr = new StreamReader(resp.GetResponseStream());
                        var content = sr.ReadToEnd();
                        sr.Close();
                        content = content.Replace('[', ' ');
                        content = content.Replace(']', ' ');
                        var contenta = JsonConvert.DeserializeObject<Response>(content);
                        if (!listOfId.Contains(Convert.ToInt64(contenta.response.Id)))
                        {
                            listOfId.Add(contenta.response.Id);
                            Console.WriteLine(">> Added id: " + contenta.response.Id);
                        }
                    }
                }
            }
            foreach (var i in listOfId)
            {
                Console.WriteLine(">> id: " + i);
            }
            return listOfId;
            
        }

        public static void ReSendMessage(Message messageText, VkApi _vk)//главный метод, делающий пересыл сообщений
        {
            var random = new Random();
            var ReciepentIdList = GetReciepentIdList(messageText.Body);
            var randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            var captcha = randomNumber.ToString();
            if (ReciepentIdList.Count > 0 && ReciepentIdList.Count <= 10)
            {
                foreach (var i in ReciepentIdList)
                {
                    var text = SplitStr(messageText.Body);
                    if (text == null || text == " " || text == "")
                    {
                        _vk.Messages.Send(
                            Convert.ToInt64(messageText.UserId),
                            false,
                            "There was a mistake in your message, please put your text into the \" \" and use full " +
                            "link, like: https://vk.com/1",
                            "",
                            null,
                            null,
                            false,
                            null,
                            null,
                            captcha
                            );
                        Console.WriteLine(">> Message was ReSend Back " + i);
                    }
                    else
                    {
                        _vk.Messages.Send(
                            i,
                            false,
                            "This message was send by ANONIMUS. \n\rTime of sending: " +
                            DateTime.Now.ToString("HH:mm:ss \n\r") +
                            "Message Id: " + randomNumber + "\n\r" + "Text of message: " + text,
                            "",
                            null,
                            null,
                            false,
                            null,
                            null,
                            captcha
                            );
                        Console.WriteLine(">> Id sent to: " + i);
                    }
                }
            }
            else if (GetReciepentIdList(messageText.Body).Count <= 10 || GetReciepentIdList(messageText.Body).Count <= 0)
            {
                _vk.Messages.Send(
                    Convert.ToInt64(messageText.UserId),
                    false,
                    "The amount of recievers has to be more than 0 and less than 11",
                    "",
                    null,
                    null,
                    false,
                    null,
                    null,
                    captcha
                );
            }

        }
    }
    class AddFriends
    {
        public static void AddToFriends(VkApi _vk)
        {
            var friendRequestsId = _vk.Friends.GetRequests(4, null);
            foreach (var i in friendRequestsId)
            {
                _vk.Friends.Add(i);
                Help.HelpSend(_vk, i);
                Console.WriteLine(">> Added id: " + i);
            }
        }
    }
    class Help
    {
        private static Random random = new Random();
        private static int randomNumber = random.Next(1000, 1000000); 
        private static string text = "This message was send by Bot. \n\rTime of sending: " +
                                     DateTime.Now.ToString("HH:mm:ss \n\r") +
                                     "Message Id: " + randomNumber + "\n\r" + "Бот для ВК. v.0.0.1 \n\r" +
                                     "Доступные функции: \n\r" +
                                     "1)Гороскоп на сегодня или на завтра. Напишите: Гороскоп на сегодня/завтра \"знак зодиака\"\n\r" +
                                     "2)Переслать текст сообщения от имени бота. Напишите: Переслать \"текст сообщения в кавычках(Смайлики пока что не поддержиапются)\" и ссылку/cсылки на страницы получателей(не больше 10 за 1 раз)\n\r";
        static Random rand = new Random();
        static string captcha = rand.Next(10000, 1000000).ToString();
        public static void HelpSend(VkApi _vk, long id)
        {
            _vk.Messages.Send(
                            id,
                            false,
                            text,
                            "",
                            null,
                            null,
                            false,
                            null,
                            null,
                            captcha
                            );
            Console.WriteLine(">> Help was send" + id);
        }
    }
    class Program
    {
        private static Timer _timer;
        private static Timer _timerNew;
        private static VkApi _vk;
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Console.WriteLine(">> Time: " + DateTime.Now.ToString("HH:mm:ss"));
            var count = 100;
            var testMessage = _vk.Messages.Get(MessageType.Received, out count);
            var random = new Random();
            var randomNumber = random.Next(1000, 1000000);
            foreach (var i in testMessage)
            {
                if (i.Body.ToLower().Contains("переслать") && !Convert.ToBoolean(i.ReadState))
                {
                    ReSender.ReSendMessage(i, _vk);
                    Console.WriteLine(">> Message was ReSend");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (i.Body.ToLower().Contains("гороскоп") && !Convert.ToBoolean(i.ReadState))
                {
                    Horoscope.SendHoroscope(i, _vk);
                    Console.WriteLine(">> Horoscope was Send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (i.Body.ToLower().Contains("помощь") && !i.Body.ToLower().Contains("переслать") && !Convert.ToBoolean(i.ReadState))
                {
                    Help.HelpSend(_vk, Convert.ToInt64(i.UserId));
                    Console.WriteLine(">> Help was Send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (i.Body.ToLower().Contains("анекдот") && !i.Body.ToLower().Contains("переслать") &&
                         !Convert.ToBoolean(i.ReadState))
                {
                    Anekdot.SendAnekdote(_vk, Convert.ToInt64(i.UserId));
                    Console.WriteLine(">> Anekdote was send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if(!Convert.ToBoolean(i.ReadState))
                {
                    var rand = new Random();
                    var captcha = rand.Next(10000, 1000000).ToString();
                    _vk.Messages.Send(
                       Convert.ToInt64(i.UserId),
                       false,
                       "This message was send by Bot. \n\rTime of sending: " +
                       DateTime.Now.ToString("HH:mm:ss \n\r") +
                       "Message Id: " + randomNumber + "\n\r" +"Type \"Помощь\" to get information about Bot",
                       "",
                       null,
                       null,
                       false,
                       null,
                       null,
                       captcha
                   );
                    Console.WriteLine(">> Unknow message has been handled");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
            }
        }

        private static void RefreshFriends(object source, ElapsedEventArgs e)
        {
            AddFriends.AddToFriends(_vk);
        }

        static void Main(string[] args)
        {
            var appid = 4915376;
            var email = "89263014118";
            var password = Console.ReadLine();
            var mess = Settings.Messages;
            var friends = Settings.Friends;
            
            

            
            _vk = new VkApi();
            

            try
            {
                VkAuthorization vkAuth = null;
                //string ololo = vkAuth.ExpiresIn;
                _vk.Authorize(appid, email, password, mess | friends);
                Console.WriteLine(_vk.AccessToken);
                //Console.WriteLine(ololo);
                Console.WriteLine("Authorization successfull");
            }
            catch (VkApiAuthorizationException e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                goto Exit;
            }

            _timer = new Timer(2500);
            _timerNew = new Timer(10000);
            _timerNew.Start();
            _timerNew.Elapsed += new ElapsedEventHandler(RefreshFriends);
            _timer.Start();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

        Exit:
            Console.ReadKey();


        }
    }
}
