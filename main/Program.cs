using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Timers;
using System.Text;
using System.Text.RegularExpressions;
using FluentNUnit;
using Newtonsoft.Json;
using RazorEngine;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;
using Encoding = System.Text.Encoding;


namespace main
{
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

        private static string GetHorolink(string incStr)
        {

            Dictionary<string, string> _horoscopes = new Dictionary<string, string>();
            _horoscopes.Add("овен", "http://astroscope.ru/horoskop/ejednevniy_goroskop/aries.html");
            _horoscopes.Add("весы", "http://astroscope.ru/horoskop/ejednevniy_goroskop/libra.html");
            _horoscopes.Add("телец", "http://astroscope.ru/horoskop/ejednevniy_goroskop/taurus.html");
            _horoscopes.Add("скорпион", "http://astroscope.ru/horoskop/ejednevniy_goroskop/scorpio.html");
            _horoscopes.Add("близнецы", "http://astroscope.ru/horoskop/ejednevniy_goroskop/gemini.html");
            _horoscopes.Add("стрелец", "http://astroscope.ru/horoskop/ejednevniy_goroskop/sagittarius.html");
            _horoscopes.Add("рак", "http://astroscope.ru/horoskop/ejednevniy_goroskop/cancer.html");
            _horoscopes.Add("козерог", "http://astroscope.ru/horoskop/ejednevniy_goroskop/capricorn.html");
            _horoscopes.Add("лев", "http://astroscope.ru/horoskop/ejednevniy_goroskop/leo.html");
            _horoscopes.Add("водолей", "http://astroscope.ru/horoskop/ejednevniy_goroskop/aquarius.html");
            _horoscopes.Add("дева", "http://astroscope.ru/horoskop/ejednevniy_goroskop/virgo.html");
            _horoscopes.Add("рыба", "http://astroscope.ru/horoskop/ejednevniy_goroskop/pisces.html");
            if (incStr.Split(' ').Contains("Завтра") || (incStr.Split(' ').Contains("завтра")))
            {
                return
                    _horoscopes[incStr.Split(' ')[1].ToLower()].Substring(0,
                        _horoscopes[incStr.Split(' ')[1].ToLower()].Length - 5) + "_zavtra.html";
            }
            else
            {
                return _horoscopes[incStr.Split(' ')[1].ToLower()];
            }

        }
        private static string GetHoroscope(string incStr)
        {
            string horoscope = "No Horoscope";
            string pattern = "(<div class=\"goroskop\">(.+)</div>)";
            string horoLink = GetHorolink(incStr);
            var req =
                            (HttpWebRequest)
                                WebRequest.Create(horoLink);
            var resp = (HttpWebResponse)req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251));
            string content = sr.ReadToEnd();
            sr.Close();
            MatchCollection matches = Regex.Matches(content, pattern);
            foreach (Match match in matches.Cast<Match>().Where(match => matches.Count != 0))
            {
                horoscope = match.Groups[2].Value;
            }
            return horoscope;
        }

        public static void SendHoroscope(Message incStr, VkApi _vk)
        {

            Random random = new Random();
            int randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            string captcha = randomNumber.ToString();
            string text = GetHoroscope(incStr.Body);
            var i = Convert.ToInt64(incStr.UserId);
            _vk.Messages.Send(
               i,
               false,
               "This message was send by Bot. \n\rTime of sending: " +
               DateTime.Now.ToString("HH:mm:ss \n\r") +
               "Message Id: " + randomNumber + "\n\r" + "Text of message: \n\rYour horoscope for today is: " + text,
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

    class Task
    {
        private static string SplitStr(string text) //Функция, возвращающая текст, заключенный в " "
        {                                           //из исходного сообщения
            bool flag = false;
            string result = null;
            int tmp1 = 0;
            int tmp2 = 0;
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i - 1] != '\"' || i < 1) continue;
                if (!flag)
                {
                    tmp1 = i;
                    flag = true;
                }
                else tmp2 = i - 1;
            }
            result = text.Substring(tmp1, tmp2 - tmp1);
            return result;
        }

        private static List<long> GetReciepentIdList(string text) //Функция, возвращающая лист айди пользователей
        {                                                         //кому переслать сообщение
            string[] message = text.Split(' ');
            List<long> listOfId = new List<long>();
            foreach (string i in message)
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
                    if (nickname.Contains("id"))
                    {
                        nickname = nickname.Substring(2);
                        listOfId.Add(Convert.ToInt64(nickname));
                    }
                    else
                    {
                        var req =
                            (HttpWebRequest)
                                WebRequest.Create("http://api.vk.com/method/users.get?user_ids=" + nickname + "&v=5.30");
                        var resp = (HttpWebResponse)req.GetResponse();
                        var sr = new StreamReader(resp.GetResponseStream());
                        string content = sr.ReadToEnd();
                        sr.Close();
                        content = content.Replace('[', ' ');
                        content = content.Replace(']', ' ');
                        Response contenta = JsonConvert.DeserializeObject<Response>(content);
                        listOfId.Add(contenta.response.Id);
                    }
                }
            }
            return listOfId;
        }

        public static void ReSendMessage(Message messageText, VkApi _vk)//главный метод, делающий пересыл сообщений
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            string captcha = randomNumber.ToString();
            if (GetReciepentIdList(messageText.Body).Count > 0 && GetReciepentIdList(messageText.Body).Count <= 10)
            {
                foreach (var i in GetReciepentIdList(messageText.Body))
                {
                    string text = SplitStr(messageText.Body);
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
    class Program
    {
        private static Timer _timer;
        private static VkApi _vk;
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine(">> Time: " + DateTime.Now.ToString("HH:mm:ss"));
            int count = 0;
            ReadOnlyCollection<Message> testMessage = _vk.Messages.Get(MessageType.Received, out count);
            foreach (Message i in testMessage)
            {
                if (i.Body.Contains("Переслать") && !Convert.ToBoolean(i.ReadState))
                {
                    Task.ReSendMessage(i, _vk);
                    Console.WriteLine(">> Message was ReSend");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (i.Body.Contains("Гороскоп") && !Convert.ToBoolean(i.ReadState))
                {
                    Horoscope.SendHoroscope(i, _vk);
                    Console.WriteLine(">> Horoscope was Send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
            }
        }

        static void Main(string[] args)
        {
            int appid = 4875368;
            string email = "89998132952";
            string password = "T511baa927nkk365fmt315z";
            Settings mess = Settings.Messages;
            Settings friends = Settings.Friends;



            _vk = new VkApi();
            try
            {
                _vk.Authorize(appid, email, password, mess | friends);
                Console.WriteLine("Authorization successfull");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                goto Exit;
            }

            _timer = new Timer(5000);
            _timer.Start();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

        Exit:
            Console.ReadKey();

            //the end

        }
    }
}
