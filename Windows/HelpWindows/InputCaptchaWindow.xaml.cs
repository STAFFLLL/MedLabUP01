﻿using MedLab.Windows.LaboratoryAssistantWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MedLab.Windows.HelpWindows
{
    /// <summary>
    /// Логика взаимодействия для InputCaptchaWindow.xaml
    /// </summary>
    public partial class InputCaptchaWindow : Window
    {
        private MedLabEntities medLabEntities = new MedLabEntities();
        private MainWindow _mainWindow;
        private User user;

        public InputCaptchaWindow(MainWindow mainWindow, User users)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            GenerateCaptcha();
            user = users;
        }
        public InputCaptchaWindow()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        /// <summary>
        /// Блокирует приложение на 5 секунд при неверной капче и записывает событие в историю
        /// </summary>
        /// <remarks>
        /// Последовательность действий:
        /// 1. Показывает сообщение о блокировке
        /// 2. Отключает интерфейс на 5 секунд
        /// 3. По истечении времени показывает сообщение о разблокировке
        /// 4. Записывает событие в историю с причиной "Ошибки ввода данных"
        /// 5. Закрывает текущее окно и завершает работу приложения
        /// </remarks>
        public async void BlockApplication()
        {
            MessageBox.Show("Капча не верна! Приложение заблокировано на 10 секунд!");
            IsEnabled = false;
            await Task.Delay(10000);
            IsEnabled = true;
            MessageBox.Show("Приложение разблокировано!");
            _mainWindow.Show();
            History newHistory = new History()
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now,
                IdReason = 3,
                IdUser = user.Id,
                Ip = HelperIP.GetIp()
            };
            medLabEntities.Histories.Add(newHistory);
            medLabEntities.SaveChanges();
            Application.Current.Shutdown();
            this.Close();
        }

        private string textCaptcha;
        /// <summary>
        /// Блокирует приложение на 5 секунд при неверной капче и записывает событие в историю
        /// </summary>
        /// <remarks>
        /// Последовательность действий:
        /// 1. Показывает сообщение о блокировке
        /// 2. Отключает интерфейс на 5 секунд
        /// 3. По истечении времени показывает сообщение о разблокировке
        /// 4. Записывает событие в историю с причиной "Неверная капча"
        /// 5. Закрывает текущее окно и завершает работу приложения
        /// </remarks>
        public void GenerateCaptcha()
        {
            textCaptcha = string.Empty;
            var allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            string[] ar = allowchar.Split(a);
            string temp = string.Empty;
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];

                textCaptcha += temp;
            }
            TextCaptch.Text = textCaptcha;
        }

        private void changeCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void NextInputCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (textCaptcha != InputTextCaptcha.Text)
            {
                MessageBox.Show("Капча не верна!");
                BlockApplication();
            }
            else
            {
                _mainWindow.Show();
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainWindow.Show();
        }
    }
}
