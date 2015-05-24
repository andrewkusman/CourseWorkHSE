﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    class Calc
    {
        //Главная функция, которая отправляет решение
        public static void SendSolution(VkApi _vk, Message message)
        {
            string solution;
            try
            {
                solution = MainCalc(message.Body).ToString();
            }
            catch
            {
                solution = "Mistake in incoming expression, please try again";
            }
            var rand = new Random();
            var captcha = rand.Next(10000, 1000000).ToString();
            var randomNumber = rand.Next(1000, 1000000);
            _vk.Messages.Send(
                Convert.ToInt64(message.UserId),
                false,
                "This message was send by Bot. \n\r" +
                DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " + 
                randomNumber + "\n\r" + "Solution: \n\r" + 
                solution,
                "",
                null,
                null,
                false,
                null,
                null,
                captcha
            );
        }
        //Получаем приоритет
        static private byte PrioGet(char s)
        {
            switch (s)
            {
                case '(': return 0;
                case ')': return 1;
                case '+': return 2;
                case '-': return 3;
                case '*': return 4;
                case '/': return 4;
                case '^': return 5;
                default: return 6;
            }
        }

        //Метод возвращает true, если проверяемый символ - оператор
        private static bool IsOper(char с)
        {
            if (("+-/*^()".IndexOf(с) != -1))
                return true;
            return false;
        }

        //Метод возвращает true, если проверяемый символ - разделитель
        private static bool IsSepar(char c)
        {
            if ((" =".IndexOf(c) != -1))
                return true;
            return false;
        }


        //Метод mainCalc принимает выражение в виде строки и возвращает результат
        private static double MainCalc(string input)
        {
            string output = RpnConvert(input);
            double result = RpnCalc(output);
            return result;
        }

        //Метод, преобразующий входную строку с выражением в постфиксную запись
        private static string RpnConvert(string input)
        {
            string output = string.Empty;
            Stack<char> operStack = new Stack<char>();
            char[] tmp = new char[input.Length];
            string sTmp = string.Empty;


            for (int i = 0; i < input.Length; i++)
            {
                if (IsSepar(input[i]))
                    continue;

                if (Char.IsDigit(input[i]))
                {
                    while (!IsSepar(input[i]) && !IsOper(input[i]))
                    {
                        output += input[i];
                        i++;
                        if (i == input.Length) break;
                    }
                    output += " ";
                    i--;
                }
                if (IsOper(input[i]))
                {
                    if (input[i] == '(')
                        operStack.Push(input[i]);
                    else if (input[i] == ')')  //Если символ - закрывающая скобка, то выписываем
                    {                           // все операторы до открывающей скобки в строку
                        char s = operStack.Pop();
                        while (s != '(')
                        {
                            output += s.ToString() + ' ';
                            s = operStack.Pop();
                        }
                    }
                    else
                    {
                        if (operStack.Count > 0)
                            if (PrioGet(input[i]) <= PrioGet(operStack.Peek()))
                                output += operStack.Pop().ToString() + " ";
                        operStack.Push(char.Parse(input[i].ToString()));
                    }
                }
                sTmp = "";
                tmp = operStack.ToArray();
                for (int j = tmp.Length - 1; j >= 0; j--)
                {
                    sTmp += tmp[j] + " ";
                }

            }
            while (operStack.Count > 0) //Выкидываем из стека все оставшиеся там операторы в строку
                output += operStack.Pop() + " ";
            return output;
        }


        //Метод, вычисляющий значение выражения, уже преобразованного в постфиксную запись
        private static double RpnCalc(string input)
        {
            double result = 0;
            Stack<double> temp = new Stack<double>();
            double[] tmp = new double[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (IsSepar(input[i]))
                {
                    continue;
                }
                if (Char.IsDigit(input[i]))
                {
                    string a = string.Empty;
                    while (!IsSepar(input[i]) && !IsOper(input[i]))
                    {
                        a += input[i];
                        i++;
                        if (i == input.Length) break;
                    }
                    temp.Push(double.Parse(a)); //Записываем в стек
                    i--;
                }
                else if (IsOper(input[i])) //Если оператор, то берем 2 последних значения из стека и проводим операцию
                {
                    double a = temp.Pop();
                    double b = temp.Pop();
                    switch (input[i])
                    {
                        case '+': result = b + a; break;
                        case '-': result = b - a; break;
                        case '*': result = b * a; break;
                        case '/': result = b / a; break;
                        case '^': result = double.Parse(Math.Pow(double.Parse(b.ToString()), double.Parse(a.ToString())).ToString()); break;
                    }
                    temp.Push(result); //Результат вычисления записываем обратно в стек
                }
                tmp = temp.ToArray();
                //if (!Char.IsDigit(input[i + 1]) && i < input.Length)
                //{
                //    Console.Write(input[i]);
                //} 
            }
            return temp.Peek();
        }
    }
    //Класс для получения гороскопа
    class Content
    {
        public string content = null;
    }

    class Anekdot
    {
        //Метод, получающий анекдот с сайта
        public static string GetAnekdote()
        {
            var req =
                    (HttpWebRequest)
                        WebRequest.Create("http://rzhunemogu.ru/RandJSON.aspx?CType=1");
            var resp = (HttpWebResponse)req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(1251));
            var content = sr.ReadToEnd();
            sr.Close();
            Content contenta = null;
            //Возможная проблема у сайта с анекдотами
            try
            {
                contenta = JsonConvert.DeserializeObject<Content>(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(">> Can't get anekdote");
                return null;
            }
            return contenta.content;
        }
        //Метод, посылающий анекдот пользователю, который запросил его
        public static void SendAnekdote(VkApi _vk, long id)
        {
            var text = GetAnekdote();
            var random = new Random();
            var randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            var captcha = randomNumber.ToString();
            if (text == null)//ошибка, если не пришел ответ с сервера
            {
                _vk.Messages.Send(
                    id,
                    false,
                    "This message was send by Bot. \n\r" +
                    DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " + 
                    "Message Id: " + randomNumber + "\n\r" +
                    "Text of message: \n\rI think you have made a mistake in message or server is not avalible now, please try again.",
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
    //Класс, для получения ответа с ВК
    class Person
    {
        public long Id;
        public string first_name;
        public string last_name;
        public int hidden;
    }
    //Класс для получения ответа с ВК
    class Response
    {
        public Person response;
    }
    class Horoscope
    {
        private static string date = null;//Если не получим ответа от сервера, то передадим null
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
            foreach (var i in incStr.Split(' '))//Понимаем, гороскоп на сегодня или на завтра
            {
                try
                {
                    if (incStr.Split(' ').Contains("Завтра") || (incStr.Split(' ').Contains("завтра")))
                    {
                        tmp =
                            horoscopes[i.ToLower()].Substring(0,
                                horoscopes[i.ToLower()].Length - 5) + "_zavtra.html";//Меняем строку запроса на сервер
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
        //Получаем сам гороскоп
        public static string GetHoroscope(string incStr, out string horoName)
        {
            var horoscope = "No Horoscope";
            var pattern = "(<div class=\"goroskop\">(.+)</div>)";
            string horoname = null;
            var horoLink = GetHorolink(incStr, out horoname);
            horoName = horoname;
            if (horoLink == null)
            {
                horoscope = null;//Возвращаем null, если не получили ответ или он был неправильным
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
        //Метод, посылающий гороскоп
        public static void SendHoroscope(Message incStr, VkApi _vk)
        {

            var random = new Random();
            var randomNumber = random.Next(1000, 1000000); //рандом для каптчи и для айди сообщений
            var captcha = randomNumber.ToString();
            string horoName = null;
            var text = GetHoroscope(incStr.Body, out horoName);
            var i = Convert.ToInt64(incStr.UserId);
            if (text == null)//Если где-то произошла ошибка, то посылаем ее обратно
            {
                _vk.Messages.Send(
                    i,
                    false,
                    "This message was send by Bot. \n\r" + 
                    DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " + 
                    randomNumber + "\n\r" +
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
                    "This message was send by Bot. \n\r" +
                    DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " +
                    randomNumber + "\n\r" + "Text of message: \n\rYour horoscope for \""+ horoName + "\" " + date +" is: " + text,
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
        private static string errText = "Can't resend empty message";
        private static string SplitStr(string text) //Функция, возвращающая текст, заключенный в " "
        {                                           //из исходного сообщения
            try
            {
                text += " ";
                var flag = false;
                string result;
                var tmp1 = 0;
                var tmp2 = 0;
                for (var i = 1; i < text.Length; i++)
                {
                    if (text[i - 1] != '\"' || i < 1) 
                        continue;
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

        public static List<long> GetReciepentIdList(string text) //Функция, возвращающая лист айди пользователей
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
                        if (!listOfId.Contains(Convert.ToInt64(nickname)))
                        {
                            listOfId.Add(Convert.ToInt64(nickname));
                        }
                    }
                    else
                    {
                        try
                        {
                            var req =
                                (HttpWebRequest)
                                    WebRequest.Create("http://api.vk.com/method/users.get?user_ids=" + nickname +
                                                      "&v=5.30");
                            var resp = (HttpWebResponse) req.GetResponse();
                            var sr = new StreamReader(resp.GetResponseStream());
                            string content = sr.ReadToEnd();
                            sr.Close();
                            content = content.Replace('[', ' ');
                            content = content.Replace(']', ' ');
                            Response contenta = JsonConvert.DeserializeObject<Response>(content);
                            listOfId.Add(contenta.response.Id);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(">>" + e);
                            return null;
                        }
                    }
                }
            }
            return listOfId;
        }

        public static void ReSendMessage(Message messageText, VkApi _vk)//главный метод, делающий пересыл сообщений
        {
            var random = new Random();
            var reciepentIdList = GetReciepentIdList(messageText.Body);
            int randomNumber; //рандом для каптчи и для айди сообщений
            string captcha = null;
            var text = SplitStr(messageText.Body);

            if (reciepentIdList!= null && reciepentIdList.Count > 0 && reciepentIdList.Count <= 10)
            {
                foreach (var i in reciepentIdList)
                {
                    if (string.IsNullOrEmpty(text))//Если текст пустой, то говорим, что не пересылаем пустые тексты
                    {
                        randomNumber = random.Next(1000, 1000000);
                        captcha = randomNumber.ToString();
                        _vk.Messages.Send(
                            Convert.ToInt64(messageText.UserId),
                            false,
                            errText,
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
                        randomNumber = random.Next(1000, 1000000);
                        captcha = randomNumber.ToString();
                        _vk.Messages.Send(
                            i,
                            false,
                            "This message was send by someone who wants you to know something ;). \n\r" +
                            DateTime.Now.ToString("HH:mm:ss") + " :: "
                            + randomNumber + "\n\r" + "Text of message: \n\r" + text,
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
            else if (reciepentIdList!= null && (reciepentIdList.Count > 10 || reciepentIdList.Count <= 0))//Если не нашли пользователей
            {                                                                                             //которым надо перелать сообщение
                randomNumber = random.Next(1000, 1000000);
                captcha = randomNumber.ToString();
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
            else//любая другая ошибка, скорее всего, если получили ошибку от сервера вк при запросе
            {
                randomNumber = random.Next(1000, 1000000);
                captcha = randomNumber.ToString();
                _vk.Messages.Send(
                    Convert.ToInt64(messageText.UserId),
                    false,
                    "Maybe you have made a mistake in link?",
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
    //Класс для добавления в друзья и отслыки помощи при добавлении
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
        private static string text = "This message was send by Bot. \n\r" +
                                     DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " +
                                     randomNumber + "\n\r" + "Бот для ВК. v.0.2.3 \n\r" +
                                     "Доступные функции: \n\r" +
                                     "1)Гороскоп на сегодня или на завтра. Напишите: Гороскоп на сегодня/завтра \"знак зодиака\"\n\r" +
                                     "2)Переслать текст сообщения от имени бота. Напишите: Переслать \"текст сообщения в кавычках\" и ссылку/cсылки на страницы получателей(не больше 10 за 1 раз)\n\r" +
                                     "3)Переслать анекдот кому-то от имени бота. Напишите: Переслать анекдот и ссылку/cсылки на страницы получателей(не больше 10 за 1 раз)\n\r" + 
                                     "4)Получение анекдота. Напишите: Анекдот" +
                                     "5)Переслать гороскоп. Напишите: Переслать анекдот и ссылку/cсылки на страницы получателей(не больше 10 за 1 раз)\n\r" +
                                     "6)Посчитать простой пример. Напишите: Посчитать \"2+2\" (тригонометрические функции пока что не поддерживаются)" + 
                                     "Сделал Андрей Кусачев. https://vk.com/kusandre";
        static Random rand = new Random();
        static string captcha = rand.Next(10000, 1000000).ToString();
        public static void HelpSend(VkApi _vk, long id)//Отсылка помощи
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

    class Interpretator
    {
        public static void Interpretate(ReadOnlyCollection<Message> message, VkApi _vk)
        {
            var random = new Random();
            var randomNumber = random.Next(1000, 1000000);
            foreach (var i in message)
            {
                if (i.Body.ToLower().Contains("переслать") && !Convert.ToBoolean(i.ReadState))
                {
                    if (i.Body.ToLower().Contains("анекдот"))
                    {
                        i.Body += " \"" + Anekdot.GetAnekdote() + "\"";
                        Console.WriteLine(">> " + i.Body);
                        ReSender.ReSendMessage(i, _vk);
                        Console.WriteLine(">> Anekdote was send to another person");
                        _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                    }
                    else if (i.Body.ToLower().Contains("гороскоп"))
                    {
                        string horo;
                        string text = Horoscope.GetHoroscope(i.Body, out horo);
                        i.Body = i.Body.Substring(9) + " \" Гороскоп " + horo + " " + text + "\"";
                        ReSender.ReSendMessage(i, _vk);
                        _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                    }
                    else
                    {
                        ReSender.ReSendMessage(i, _vk);
                        Console.WriteLine(">> Message was ReSend");
                        _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                    }
                }
                else if (i.Body.ToLower().Contains("гороскоп") && !Convert.ToBoolean(i.ReadState))
                {
                    Horoscope.SendHoroscope(i, _vk);
                    Console.WriteLine(">> Horoscope was Send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (i.Body.ToLower().Contains("помощь") && !i.Body.ToLower().Contains("переслать") &&
                         !Convert.ToBoolean(i.ReadState))
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
                else if (i.Body.ToLower().Contains("посчитать") && !Convert.ToBoolean(i.ReadState))
                {
                    Calc.SendSolution(_vk, i);
                    Console.WriteLine(">> Solution was send");
                    _vk.Messages.MarkAsRead(Convert.ToInt64(i.Id));
                }
                else if (!Convert.ToBoolean(i.ReadState))
                {
                    var rand = new Random();
                    var captcha = rand.Next(10000, 1000000).ToString();
                    _vk.Messages.Send(
                        Convert.ToInt64(i.UserId),
                        false,
                        "This message was send by Bot. \n\r" +
                        DateTime.Now.ToString("HH:mm:ss \n\r") + " :: " +
                        randomNumber + "\n\r" + "Type \"Помощь\" to get information about Bot",
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
            Interpretator.Interpretate(testMessage, _vk);
            
        }//Каждые 3 секунды проверяем входящие сообщения

        private static void RefreshFriends(object source, ElapsedEventArgs e)//Каждые 10 сек проверяем запросы в друзья
        {
            AddFriends.AddToFriends(_vk);
        }

        static void Main(string[] args)
        {
            const int appid = 4915376;
            const string email = "89263014118";
            var password = Console.ReadLine();
            var mess = Settings.Messages;
            var friends = Settings.Friends;

            _vk = new VkApi();
            
            try
            {
                _vk.Authorize(appid, email, password, mess | friends);
                Console.WriteLine(_vk.AccessToken);
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
            _timerNew.Elapsed += RefreshFriends;
            _timer.Start();
            _timer.Elapsed += OnTimedEvent;

        Exit:
            Console.ReadKey();
        }
    }
}
